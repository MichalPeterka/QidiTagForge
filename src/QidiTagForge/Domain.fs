namespace QidiTagForge

type MaterialId = byte

type ColorId = byte

type HexColor = string

type Texts =
    {
        Title: string
        Description: string
        Material: string
        Color: string
        Write: string
        Read: string
        Done: string
        Error: string
        SelectValid: string
        NoColor: string
        NoMaterial: string
        TagInfo: string
        EmptyTag: string
        NoReader: string
        NoKey: string
        AuthFailed: string
        WriteFailed: string
        ReadFailed: string
        Unknown: string
        AutoDetect: string
        Idle: string
        SelectionPreview: string
    }

type FilamentColor =
    {
        Hex: HexColor
        Name: string
    }

type MaterialInfo =
    {
        Index: int
        Name: string
    }

type TagDisplay =
    {
        Material: string
        ColorHex: HexColor
        ColorName: string
    }
