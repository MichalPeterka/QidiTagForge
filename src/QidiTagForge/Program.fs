namespace QidiTagForge

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.FuncUI.Hosts
open Avalonia.Themes.Fluent

type MainWindow() as this =
    inherit HostWindow()

    do
        base.Width <- 500.0
        base.Height <- 700.0
        base.MinWidth <- 440.0
        base.MinHeight <- 640.0
        base.Content <- Views.rootView this

type App() =
    inherit Application()

    override this.Initialize() =
        this.RequestedThemeVariant <- Styling.ThemeVariant.Light
        this.Styles.Add(FluentTheme())

    override _.OnFrameworkInitializationCompleted() =
        let current = Application.Current

        if not (isNull current) then
            match current.ApplicationLifetime with
            | :? IClassicDesktopStyleApplicationLifetime as desktop ->
                desktop.MainWindow <- MainWindow()
            | _ -> ()

        base.OnFrameworkInitializationCompleted()

module Program =
    [<EntryPoint>]
    let main argv =
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .StartWithClassicDesktopLifetime(argv)
