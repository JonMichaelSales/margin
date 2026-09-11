using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaThemeManager.Extensions;
using AvaloniaThemeManager.Theme;
using MDPlayer.Core;
using MDPlayer.Infrastructure;
using MDPlayer.Rendering;
using MDPlayer.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MDPlayer.Desktop;

public partial class App : Application
{
    public static string[] StartupArguments { get; set; } = [];
    public IServiceProvider Services { get; private set; } = null!;
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        services.AddThemeManagerServices();
        services.AddSingleton<IUserPreferencesStore>(CreatePreferencesStore());
        services.AddSingleton<IThemeSelectionStore, ThemeSelectionStore>();
        services.AddSingleton<IDocumentFileService, DocumentFileService>();
        services.AddSingleton<IMarkdownParser, MarkdownParser>();
        services.AddSingleton<IAppearanceService, AppearanceService>();
        services.AddSingleton<IPlatformIntegration, PlatformIntegration>();
        Services = services.BuildServiceProvider();
        Services.GetRequiredService<IAppearanceService>().Initialize();
        if (TryGetFeature(typeof(IActivatableLifetime)) is IActivatableLifetime activation)
            activation.Activated += (_, args) =>
            {
                if (args is FileActivatedEventArgs files)
                    foreach (var file in files.Files)
                        if (file.TryGetLocalPath() is { } path) Dispatcher.UIThread.Post(() => OpenActivatedFile(path));
            };
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnLastWindowClose;
            var paths = StartupArguments.Where(x => !x.StartsWith("--") && !x.StartsWith("-psn_")).ToArray();
            desktop.MainWindow = CreateWindow(paths.FirstOrDefault());
            foreach (var path in paths.Skip(1)) CreateWindow(path).Show();
            desktop.Exit += (_, _) => (Services as IDisposable)?.Dispose();
        }
        base.OnFrameworkInitializationCompleted();
    }
    private void OpenActivatedFile(string path)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.Windows.OfType<MainWindow>().FirstOrDefault(x => x.Session.FilePath is null) is { } empty)
        { _ = empty.OpenDocumentAsync(path); empty.Activate(); }
        else CreateWindow(path).Show();
    }
    protected virtual IUserPreferencesStore CreatePreferencesStore() => new UserPreferencesStore();
    public MainWindow CreateWindow(string? path = null) => new(Services, path);
}
