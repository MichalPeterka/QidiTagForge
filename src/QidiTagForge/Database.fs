namespace QidiTagForge

module Database =
    let supportedFirmware = "" // TODO: add current FW version

    let materials: Map<MaterialId, MaterialInfo> =
        // The materials mapping from config files in supported printer FW version
        Map [
            1uy, { Index = 0; Name = "PLA" }
            2uy, { Index = 1; Name = "PLA Matte" }
            3uy, { Index = 2; Name = "PLA Metal" }
            4uy, { Index = 3; Name = "PLA Silk" }
            5uy, { Index = 4; Name = "PLA-CF" }
            6uy, { Index = 5; Name = "PLA-Wood" }
            7uy, { Index = 6; Name = "PLA Basic" }
            8uy, { Index = 7; Name = "PLA Matte Basic" }
            11uy, { Index = 8; Name = "ABS" }
            12uy, { Index = 9; Name = "ABS-GF" }
            13uy, { Index = 10; Name = "ABS-Metal" }
            14uy, { Index = 11; Name = "ABS-Odorless" }
            18uy, { Index = 12; Name = "ASA" }
            19uy, { Index = 13; Name = "ASA-AERO" }
            24uy, { Index = 14; Name = "UltraPA" }
            25uy, { Index = 15; Name = "PA12-CF" }
            26uy, { Index = 16; Name = "UltraPA-CF25" }
            30uy, { Index = 17; Name = "PAHT-CF" }
            31uy, { Index = 18; Name = "PAHT-GF" }
            32uy, { Index = 19; Name = "Support For PAHT" }
            33uy, { Index = 20; Name = "Support For PET/PA" }
            34uy, { Index = 21; Name = "PC/ABS-FR" }
            37uy, { Index = 22; Name = "PET-CF" }
            38uy, { Index = 23; Name = "PET-GF" }
            39uy, { Index = 24; Name = "PETG Basic" }
            40uy, { Index = 25; Name = "PETG-Though" }
            41uy, { Index = 26; Name = "PETG" }
            44uy, { Index = 27; Name = "PPS-CF" }
            45uy, { Index = 28; Name = "PETG Translucent" }
            47uy, { Index = 29; Name = "PVA" }
            49uy, { Index = 30; Name = "TPU-AERO" }
            50uy, { Index = 31; Name = "TPU" }
        ]

    let colors: Map<ColorId, FilamentColor> =
        // The colors mapping from config file in supported printer FW version
        Map [
            1uy, { Hex = "#FAFAFA"; Name = "White" }
            2uy, { Hex = "#060606"; Name = "Black" }
            3uy, { Hex = "#D9E3ED"; Name = "Gray" }
            4uy, { Hex = "#5CF30F"; Name = "Light Green" }
            5uy, { Hex = "#63E492"; Name = "Mint" }
            6uy, { Hex = "#2850FF"; Name = "Blue" }
            7uy, { Hex = "#FE98FE"; Name = "Pink" }
            8uy, { Hex = "#DFD628"; Name = "Yellow" }
            9uy, { Hex = "#228332"; Name = "Green" }
            10uy, { Hex = "#99DEFF"; Name = "Light Blue" }
            11uy, { Hex = "#1714B0"; Name = "Dark Blue" }
            12uy, { Hex = "#CEC0FE"; Name = "Lavender" }
            13uy, { Hex = "#CADE4B"; Name = "Lime" }
            14uy, { Hex = "#1353AB"; Name = "Royal Blue" }
            15uy, { Hex = "#5EA9FD"; Name = "Sky Blue" }
            16uy, { Hex = "#A878FF"; Name = "Violet" }
            17uy, { Hex = "#FE717A"; Name = "Rose" }
            18uy, { Hex = "#FF362D"; Name = "Red" }
            19uy, { Hex = "#E2DFCD"; Name = "Beige" }
            20uy, { Hex = "#898F9B"; Name = "Silver" }
            21uy, { Hex = "#6E3812"; Name = "Brown" }
            22uy, { Hex = "#CAC59F"; Name = "Khaki" }
            23uy, { Hex = "#F28636"; Name = "Orange" }
            24uy, { Hex = "#B87F2B"; Name = "Bronze" }
        ]
