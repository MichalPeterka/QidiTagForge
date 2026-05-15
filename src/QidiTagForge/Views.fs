namespace QidiTagForge

open System
open System.Threading.Tasks
open Avalonia
open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Threading

module Views =
    let private brush (hex: HexColor) =
        SolidColorBrush(Avalonia.Media.Color.Parse(hex)) :> IBrush

    let private lightTextColors: Set<HexColor> =
        set [ "#FAFAFA"; "#D9E3ED"; "#99DEFF"; "#CEC0FE"; "#CADE4B"; "#E2DFCD"; "#CAC59F"; "#FFFFFF" ]

    let private foregroundFor (hex: HexColor) =
        if lightTextColors.Contains hex then Brushes.Black :> IBrush else Brushes.White :> IBrush

    let private label text =
        TextBlock.create [
            TextBlock.text text
            TextBlock.textAlignment TextAlignment.Center
            TextBlock.fontWeight FontWeight.Bold
            TextBlock.horizontalAlignment HorizontalAlignment.Center
            TextBlock.margin (Thickness(0.0, 8.0, 0.0, 4.0))
        ]

    let private titleBar (texts: Texts) =
        DockPanel.create [
            DockPanel.lastChildFill true
            DockPanel.children [
                TextBlock.create [
                    TextBlock.text texts.Description
                    TextBlock.textAlignment TextAlignment.Center
                    TextBlock.fontSize 13.0
                    TextBlock.textWrapping TextWrapping.Wrap
                    TextBlock.horizontalAlignment HorizontalAlignment.Center
                    TextBlock.verticalAlignment VerticalAlignment.Center
                ]
            ]
        ]

    let private card (background: string option) (child: Types.IView) =
        Border.create [
            Border.margin (Thickness(20.0, 8.0, 20.0, 0.0))
            Border.padding (Thickness 12.0)
            Border.cornerRadius (CornerRadius 8.0)
            Border.borderBrush Brushes.Black
            Border.borderThickness (Thickness 2.0)
            match background with
            | Some color -> Border.background (brush color)
            | None -> ()
            Border.child child
        ]

    let private materialKeys =
        [| for KeyValue(value, _) in Database.materials -> value |]

    let private materialName texts (value: MaterialId) =
        Database.materials
        |> Map.tryFind value
        |> Option.map _.Name
        |> Option.defaultValue texts.Unknown

    let private colorValues =
        [ for KeyValue(value, color) in Database.colors -> value, color ]

    let private materialSelector (texts: Texts) (selectedMaterial: IWritable<MaterialId option>) =
        ComboBox.create [
            ComboBox.width 220.0
            ComboBox.horizontalAlignment HorizontalAlignment.Center
            ComboBox.horizontalContentAlignment HorizontalAlignment.Center
            ComboBox.placeholderText texts.Material
            ComboBox.dataItems [| for KeyValue(_, material) in Database.materials -> material.Name |]
            ComboBox.selectedIndex (
                selectedMaterial.Current
                |> Option.bind (fun selected ->
                    Database.materials
                    |> Map.tryFind selected
                    |> Option.map _.Index)
                |> Option.defaultValue -1
            )
            ComboBox.onSelectedIndexChanged (fun index ->
                materialKeys
                |> Array.tryItem index
                |> selectedMaterial.Set)
        ]

    let private colorSelector (selectedColor: IWritable<ColorId option>) =
        StackPanel.create [
            StackPanel.horizontalAlignment HorizontalAlignment.Center
            StackPanel.children (
                colorValues
                |> List.chunkBySize 8
                |> List.map (fun rowColors ->
                    StackPanel.create [
                        StackPanel.orientation Orientation.Horizontal
                        StackPanel.horizontalAlignment HorizontalAlignment.Center
                        StackPanel.children (
                            rowColors
                            |> List.map (fun (value, color) ->
                                Button.create [
                                    Button.width 40.0
                                    Button.height 40.0
                                    Button.margin (Thickness 3.0)
                                    Button.background (brush color.Hex)
                                    Button.borderBrush (
                                        match selectedColor.Current with
                                        | Some selected when selected = value -> Brushes.Black :> IBrush
                                        | _ -> Brushes.Transparent :> IBrush
                                    )
                                    Button.borderThickness (Thickness 2.0)
                                    Button.content ""
                                    Button.onClick (fun _ -> selectedColor.Set(Some value))
                                ])
                        )
                    ])
            )
        ]

    let private colorPill width height radius (text: string) (hex: HexColor) =
        Border.create [
            Border.width width
            Border.height height
            Border.cornerRadius (CornerRadius radius)
            Border.background (brush hex)
            Border.borderBrush Brushes.Black
            Border.borderThickness (Thickness 2.0)
            Border.horizontalAlignment HorizontalAlignment.Center
            Border.child (
                TextBlock.create [
                    TextBlock.text text
                    TextBlock.textAlignment TextAlignment.Center
                    TextBlock.fontWeight FontWeight.Bold
                    TextBlock.foreground (foregroundFor hex)
                    TextBlock.horizontalAlignment HorizontalAlignment.Center
                    TextBlock.verticalAlignment VerticalAlignment.Center
                ]
            )
        ]

    let private writeButton
        (texts: Texts)
        (selectedMaterial: IWritable<byte option>)
        (selectedColor: IWritable<byte option>)
        showError
        setStatus =

        Button.create [
            Button.height 35.0
            Button.width 220.0
            Button.margin (Thickness(40.0, 14.0, 40.0, 0.0))
            Button.content texts.Write
            Button.horizontalAlignment HorizontalAlignment.Center
            Button.horizontalContentAlignment HorizontalAlignment.Center
            Button.verticalContentAlignment VerticalAlignment.Center
            Button.background (brush "#28A745")
            Button.foreground Brushes.White
            Button.onClick (fun _ ->
                match selectedMaterial.Current, selectedColor.Current with
                | Some materialValue, Some colorValue ->
                    Task.Run(fun () ->
                        try
                            Rfid.writeTag texts materialValue colorValue
                            Dispatcher.UIThread.Post(fun () -> setStatus texts.Done)
                        with ex ->
                            Dispatcher.UIThread.Post(fun () -> showError ex.Message))
                    |> ignore
                | _ -> showError texts.SelectValid)
        ]

    let private writeCard
        (texts: Texts)
        selectedMaterial
        selectedColor
        selectedPreviewText
        selectedPreviewHex
        showError
        setStatus =
        card None (
            StackPanel.create [
                StackPanel.spacing 8.0
                StackPanel.children [
                    label texts.Material
                    materialSelector texts selectedMaterial

                    label texts.Color
                    colorSelector selectedColor

                    label texts.SelectionPreview
                    colorPill 220.0 40.0 20.0 selectedPreviewText selectedPreviewHex

                    writeButton texts selectedMaterial selectedColor showError setStatus
                ]
            ]
        )

    let private readCard (texts: Texts) (tagDisplay: TagDisplay) statusText tagColorText readAndShow =
        card (Some "#E9ECEF") (
            StackPanel.create [
                StackPanel.spacing 8.0
                StackPanel.children [
                    Button.create [
                        Button.height 35.0
                        Button.width 220.0
                        Button.margin (Thickness(40.0, 0.0, 40.0, 0.0))
                        Button.content texts.Read
                        Button.horizontalAlignment HorizontalAlignment.Center
                        Button.horizontalContentAlignment HorizontalAlignment.Center
                        Button.verticalContentAlignment VerticalAlignment.Center
                        Button.background (brush "#007BFF")
                        Button.foreground Brushes.White
                        Button.onClick (fun _ -> readAndShow ())
                    ]
                    TextBlock.create [
                        TextBlock.text texts.TagInfo
                        TextBlock.textAlignment TextAlignment.Center
                        TextBlock.fontWeight FontWeight.Bold
                        TextBlock.horizontalAlignment HorizontalAlignment.Center
                    ]
                    colorPill 200.0 50.0 25.0 tagColorText tagDisplay.ColorHex
                    TextBlock.create [
                        TextBlock.text statusText
                        TextBlock.textAlignment TextAlignment.Center
                        TextBlock.textWrapping TextWrapping.Wrap
                        TextBlock.horizontalAlignment HorizontalAlignment.Center
                        TextBlock.margin (Thickness(20.0, 4.0, 20.0, 0.0))
                    ]
                ]
            ]
        )

    let rootView (window: Window) =
        Component(fun ctx ->
            let selectedMaterial = ctx.useState<MaterialId option> (materialKeys |> Array.tryHead)
            let selectedColor = ctx.useState<ColorId option> (colorValues |> List.tryHead |> Option.map fst)
            let tagDisplay = ctx.useState { Material = "---"; ColorHex = "#FFFFFF"; ColorName = "---" }
            let status = ctx.useState ""
            let texts = Text.english

            window.Title <- texts.Title

            let setStatus message =
                status.Set message

            let showError message =
                setStatus $"%s{texts.Error}: %s{message}"

            let readAndShow () =
                Task.Run(fun () ->
                    try
                        let result = Rfid.readTag texts
                        Dispatcher.UIThread.Post(fun () ->
                            tagDisplay.Set result
                            setStatus texts.Idle)
                        true
                    with ex ->
                        Dispatcher.UIThread.Post(fun () -> showError ex.Message)
                        false)
                |> ignore

            let selectedPreviewText, selectedPreviewHex =
                match selectedMaterial.Current, selectedColor.Current with
                | Some material, Some color ->
                    let selectedColor = Database.colors |> Map.tryFind color
                    let selectedColorName =
                        selectedColor
                        |> Option.map _.Name
                        |> Option.defaultValue texts.Unknown
                    let selectedColorHex =
                        selectedColor
                        |> Option.map _.Hex
                        |> Option.defaultValue "#FFFFFF"
                    $"%s{materialName texts material} - %s{selectedColorName}", selectedColorHex
                | Some material, None ->
                    $"%s{materialName texts material} - %s{texts.NoColor}", "#FFFFFF"
                | None, Some color ->
                    let selectedColor = Database.colors |> Map.tryFind color
                    let selectedColorName = selectedColor |> Option.map _.Name |> Option.defaultValue texts.Unknown
                    let selectedColorHex = selectedColor |> Option.map _.Hex |> Option.defaultValue "#FFFFFF"
                    $"%s{texts.Material} - %s{selectedColorName}", selectedColorHex
                | None, None -> texts.NoColor, "#FFFFFF"

            let tagColorText =
                if String.IsNullOrWhiteSpace tagDisplay.Current.ColorName then
                    "---"
                else
                    $"%s{tagDisplay.Current.Material} - %s{tagDisplay.Current.ColorName}"

            let statusText =
                if String.IsNullOrWhiteSpace status.Current then texts.Idle else status.Current

            DockPanel.create [
                DockPanel.lastChildFill true
                DockPanel.children [
                    StackPanel.create [
                        StackPanel.margin (Thickness 20.0)
                        StackPanel.spacing 8.0
                        StackPanel.children [
                            titleBar texts
                            writeCard texts selectedMaterial selectedColor selectedPreviewText selectedPreviewHex showError setStatus
                            readCard texts tagDisplay.Current statusText tagColorText readAndShow
                        ]
                    ]
                ]
            ])
