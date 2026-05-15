namespace QidiTagForge

open System
open PCSC
open PCSC.Utils


// TODO: what about using MifareCard nugget?
module Rfid =
    type private RfidKey =
        {
            Name: string
            Bytes: byte array
        }

    /// MIFARE Classic block used for the QIDI material/color payload.
    let private dataBlock = 4uy

    let private pcscVendorCommandClass = 0xFFuy
    let private loadKeyInstruction = 0x82uy
    let private authenticateInstruction = 0x86uy
    let private volatileKeyStructure = 0x00uy
    let private keySlot = 0x00uy
    let private mifareClassicKeyLength = 0x06uy
    let private authenticateCommandLength = 0x05uy
    let private authenticateVersion = 0x01uy
    let private mifareKeyTypeA = 0x60uy

    /// PC/SC APDU prefix for loading a 6-byte MIFARE key into the reader.
    let private loadKeyCommandPrefix =
        [| pcscVendorCommandClass; loadKeyInstruction; volatileKeyStructure; keySlot; mifareClassicKeyLength |]

    let private createAuthenticateBlockCommand block =
        [|
            pcscVendorCommandClass
            authenticateInstruction
            0x00uy
            0x00uy
            authenticateCommandLength
            authenticateVersion
            0x00uy
            block
            mifareKeyTypeA
            keySlot
        |]

    /// Candidate MIFARE keys tried in order when authenticating the data block.
    let private keysToTry =
        [
            {
                Name = "NDEF formatting key"
                Bytes = [| 0xD3uy; 0xF7uy; 0xD3uy; 0xF7uy; 0xD3uy; 0xF7uy |]
            }
            {
                Name = "Factory default key"
                Bytes = [| 0xFFuy; 0xFFuy; 0xFFuy; 0xFFuy; 0xFFuy; 0xFFuy |]
            }
        ]

    /// Decodes APDU SW1/SW2 status bytes into success or a human-readable failure.
    let private decodeStatus status =
        match status with
        | [| 0x90uy; 0x00uy |] -> Ok ()
        | [| 0x63uy; 0x00uy |] -> Error "Authentication or operation failed"
        | [| 0x69uy; 0x82uy |] -> Error "Security status not satisfied"
        | [| 0x69uy; 0x86uy |] -> Error "Command not allowed"
        | [| 0x6Auy; 0x81uy |] -> Error "Function not supported"
        | [| 0x6Auy; 0x82uy |] -> Error "File or application not found"
        | [| 0x6Auy; 0x86uy |] -> Error "Incorrect command parameters"
        | [| 0x6Buy; 0x00uy |] -> Error "Wrong parameters"
        | [| 0x6Duy; 0x00uy |] -> Error "Instruction not supported"
        | [| 0x6Euy; 0x00uy |] -> Error "Class not supported"
        | [| 0x67uy; 0x00uy |] -> Error "Wrong length"
        | [| sw1; sw2 |] -> Error $"Reader returned status %02X{sw1} %02X{sw2}"
        | _ -> Error "Reader returned an invalid status response"

    /// Raises an InvalidOperationException when APDU status bytes represent failure.
    let private ensureSuccess message status =
        match decodeStatus status with
        | Ok () -> ()
        | Error detail -> raise (InvalidOperationException($"{message}: {detail}"))

    /// Sends an APDU command and returns the full reader response.
    let private transmit (reader: ICardReader) command =
        let sendPci = SCardPCI.GetPci(reader.Protocol)
        let response = Array.zeroCreate<byte> 258
        match reader.Transmit(sendPci, command, response) with
        | r when r < 0 -> raise (InvalidOperationException(enum<SCardError> r |> SCardHelper.StringifyError ))
        | r -> response |> Array.take r

    /// Sends an APDU command and returns only the trailing SW1/SW2 status bytes.
    let private transmitStatus (reader: ICardReader) command =
        let response = transmit reader command
        match response.Length with
        | r when r < 2 -> raise (InvalidOperationException("Reader returned an incomplete response."))
        | r -> response |> Array.skip (r - 2)

    /// Runs an action with the first available reader and disposes PC/SC resources after.
    let private withConnection texts action =
        let context = ContextFactory.Instance.Establish(SCardScope.System)
        use context = context

        match context.GetReaders() |> Array.tryHead with
        | Some readerName ->
            use reader = context.ConnectReader(readerName, SCardShareMode.Shared, SCardProtocol.Any)
            action reader
        | None ->
            raise (InvalidOperationException(texts.NoReader))

    /// Loads a MIFARE key into the reader's volatile key slot.
    let private loadKey reader key =
        Array.append loadKeyCommandPrefix key
        |> transmitStatus reader
        |> decodeStatus
        |> Result.isOk

    /// Authenticates a MIFARE block with the key currently loaded in the reader.
    let private authenticateBlock reader block =
        createAuthenticateBlockCommand block
        |> transmitStatus reader
        |> decodeStatus
        |> Result.isOk

    /// Finds the first configured key that can authenticate the target block.
    let private findWorkingKey (texts: Texts) reader block =
        keysToTry
        |> List.tryFind (fun key -> loadKey reader key.Bytes && authenticateBlock reader block)
        |> Option.defaultWith (fun () -> raise (InvalidOperationException texts.NoKey))

    /// Ensures the target block is authenticated before read/write commands.
    let private ensureAuthenticated texts reader block =
        let key = findWorkingKey texts reader block

        if not (loadKey reader key.Bytes && authenticateBlock reader block) then
            raise (InvalidOperationException(texts.AuthFailed))

    /// Writes material and color values to the configured RFID data block.
    let writeTag texts (materialValue: MaterialId) (colorValue: ColorId) =
        withConnection texts (fun reader ->
            ensureAuthenticated texts reader dataBlock

            Array.concat [ [| materialValue; colorValue; 1uy |]; Array.zeroCreate<byte> 13 ]
            |> Array.append [| 0xFFuy; 0xD6uy; 0x00uy; dataBlock; 0x10uy |]
            |> transmitStatus reader
            |> ensureSuccess texts.WriteFailed)

    /// Reads material and color values from the configured RFID data block.
    let readTag texts =
        let materialValue, colorValue =
            withConnection texts (fun reader ->
                ensureAuthenticated texts reader dataBlock

                [| 0xFFuy; 0xB0uy; 0x00uy; dataBlock; 0x10uy |]
                |> transmit reader)
            |> fun response ->
                if response.Length < 18 then
                    raise (InvalidOperationException(texts.ReadFailed))

                response
                |> Array.skip (response.Length - 2)
                |> ensureSuccess texts.ReadFailed

                response[0], response[1]

        match materialValue, colorValue with
        | 0uy, 0uy -> { Material = texts.EmptyTag; ColorHex = "#FFFFFF"; ColorName = "" }
        | mat, col ->
            let color =
                Database.colors
                |> Map.tryFind col
            { Material =
                Database.materials
                |> Map.tryFind mat
                |> Option.map _.Name
                |> Option.defaultValue texts.Unknown
              ColorHex =
                match color with
                | Some c -> c.Hex
                | None -> "#FFFFFF"
              ColorName =
                color
                |> Option.map _.Name
                |> Option.defaultValue texts.Unknown }
