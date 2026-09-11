using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.MarkupExtensions;
using MDPlayer.Desktop.Controls;

namespace MDPlayer.Desktop.Views;

public sealed class PromptWindow : Window
{
    public PromptWindow(string title, string message, params string[] choices)
    {
        Title = title; Width = 480; SizeToContent = SizeToContent.Height; CanResize = false; WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var stack = new StackPanel { Margin = new Thickness(24), Spacing = 18 };
        var header = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        var symbol = new AppIcon { Kind = choices.Contains("Discard") ? "warning" : "information", VerticalAlignment = VerticalAlignment.Center };
        symbol[!TemplatedControl.ForegroundProperty] = new DynamicResourceExtension(choices.Contains("Discard") ? "WarningBrush" : "AccentBlueBrush");
        header.Children.Add(symbol);
        header.Children.Add(new TextBlock { Text = title, FontSize = 21, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        stack.Children.Add(header);
        stack.Children.Add(new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        var buttons = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right };
        Button? cancel = null;
        foreach (var choice in choices)
        {
            var button = new Button { Content = choice, Margin = new Thickness(4) };
            button.Click += (_, _) => Close(choice); buttons.Children.Add(button);
            if (choice == "Cancel") cancel = button;
        }
        stack.Children.Add(buttons); Content = stack;
        Opened += (_, _) => cancel?.Focus();
        KeyDown += (_, e) => { if (e.Key == Key.Escape) { Close("Cancel"); e.Handled = true; } };
    }
    public static Task<string?> Ask(Window owner, string title, string message, params string[] choices) => new PromptWindow(title, message, choices).ShowDialog<string?>(owner);
}
