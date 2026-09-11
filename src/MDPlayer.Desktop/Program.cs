using Avalonia;

namespace MDPlayer.Desktop;
internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Shared existence marker lets the installer wait for every document process to close.
        using var running = OperatingSystem.IsWindows() ? new Mutex(false, "MDPlayer.Running") : null;
        App.StartupArguments = args;
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().LogToTrace();
}
