using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Markup.Xaml.MarkupExtensions;
using MDPlayer.Desktop.Controls;

namespace MDPlayer.Desktop.Views;

public sealed class AboutWindow : Window
{
    public AboutWindow()
    {
        Title = "About Margin"; Width = 480; SizeToContent = SizeToContent.Height; CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var stack = new StackPanel { Margin = new Thickness(36), Spacing = 14, HorizontalAlignment = HorizontalAlignment.Stretch };
        var symbol = new AppIcon { Kind = "brand", Width = 64, Height = 76, HorizontalAlignment = HorizontalAlignment.Center };
        symbol[!TemplatedControl.ForegroundProperty] = new DynamicResourceExtension("AccentBlueBrush");
        stack.Children.Add(symbol);
        var wordmark = new TextBlock { Text = "Margin", FontFamily = new FontFamily("Georgia"), FontSize = 44, FontWeight = FontWeight.SemiBold, HorizontalAlignment = HorizontalAlignment.Center };
        wordmark[!TextBlock.ForegroundProperty] = new DynamicResourceExtension("AccentBlueBrush");
        stack.Children.Add(wordmark);
        stack.Children.Add(new TextBlock { Text = "A little room to read.", HorizontalAlignment = HorizontalAlignment.Center });
        var version = typeof(App).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "Development";
        stack.Children.Add(new TextBlock { Text = $"Version {version}\nJon Sales", TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap });
        stack.Children.Add(new TextBlock { Text = "Built with Avalonia, AvaloniaEdit, AvaloniaSkinManager, Markdig and CommunityToolkit.Mvvm.\n\nThird-party licenses accompany distributed builds.", TextWrapping = TextWrapping.Wrap, FontSize = 12 });
        var close = new Button { Content = "Close", HorizontalAlignment = HorizontalAlignment.Center };
        close.Click += (_, _) => Close(); stack.Children.Add(close); Content = stack;
        KeyDown += (_, e) => { if (e.Key == Key.Escape) { Close(); e.Handled = true; } };
    }
}
