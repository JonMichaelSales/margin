using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using MDPlayer.Desktop;
using MDPlayer.Desktop.Controls;
using MDPlayer.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MDPlayer.Tests;

public sealed class IconIntegrationTests
{
    [AvaloniaFact]
    public void CompactActionsKeepAccessibleNamesAndExitFocusLabel()
    {
        var window = ((App)Application.Current!).CreateWindow(); window.Show(); window.Width = 720; window.Height = 480;
        try
        {
            Dispatcher.UIThread.RunJobs();
            var appearance = window.FindControl<Button>("AppearanceButton")!;
            Assert.Equal("Appearance", AutomationProperties.GetName(appearance));
            Assert.NotNull(ToolTip.GetTip(appearance));
            Assert.False(((StackPanel)appearance.Content!).Children.OfType<TextBlock>().Single().IsVisible);
            foreach (var name in new[] { "OpenButton", "ReadButton", "EditButton", "SplitButton" })
                Assert.True(((StackPanel)window.FindControl<Button>(name)!.Content!).Children.OfType<TextBlock>().Single().IsVisible);
            var focus = window.FindControl<Button>("FocusButton")!;
            focus.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal("Exit focus", AutomationProperties.GetName(focus));
            Assert.True(((StackPanel)focus.Content!).Children.OfType<TextBlock>().Single().IsVisible);
            Assert.Equal("exit-focus", ((StackPanel)focus.Content!).Children.OfType<AppIcon>().Single().Kind);
            Assert.NotNull(window.FindControl<ScrollViewer>("EmptyPanel"));
            var defaultApp = window.FindControl<Button>("DefaultAppButton")!;
            Assert.Equal(OperatingSystem.IsWindows(), defaultApp.IsVisible);
            Assert.Contains(".md", ToolTip.GetTip(defaultApp)?.ToString());
        }
        finally { window.Close(); }
    }

    [Fact]
    public void WindowsDefaultAppLinkTargetsMarginsPerUserRegistration()
    {
        Assert.Equal("ms-settings:defaultapps?registeredAppUser=Margin", PlatformIntegration.WindowsDefaultAppsUri.AbsoluteUri);
    }

    [AvaloniaFact]
    public void ExistingButtonIconsFollowAllSkinChangesWithoutReplacement()
    {
        var app = (App)Application.Current!; var window = app.CreateWindow(); window.Show();
        var appearance = app.Services.GetRequiredService<IAppearanceService>(); appearance.BeginPreview();
        try
        {
            var button = window.FindControl<Button>("OpenButton")!;
            var icon = ((StackPanel)button.Content!).Children.OfType<AppIcon>().Single();
            foreach (var skin in appearance.Catalog)
            {
                appearance.Preview(new(false, skin.Name)); Dispatcher.UIThread.RunJobs();
                Assert.Equal(((ISolidColorBrush)button.Foreground!).Color, ((ISolidColorBrush)icon.Foreground!).Color);
                Assert.Same(icon, ((StackPanel)button.Content!).Children.OfType<AppIcon>().Single());
            }
        }
        finally { appearance.Cancel(); window.Close(); }
    }

    [AvaloniaFact]
    public void EveryProductionGlyphRenders()
    {
        var panel = new WrapPanel(); foreach (var kind in AppIcon.Kinds) panel.Children.Add(new AppIcon { Kind = kind, Foreground = Brushes.Black });
        var window = new Window { Width = 800, Height = 300, Content = panel }; window.Show();
        try { Dispatcher.UIThread.RunJobs(); AvaloniaHeadlessPlatform.ForceRenderTimerTick(); Dispatcher.UIThread.RunJobs(); Assert.NotNull(window.CaptureRenderedFrame()); }
        finally { window.Close(); }
    }
}
