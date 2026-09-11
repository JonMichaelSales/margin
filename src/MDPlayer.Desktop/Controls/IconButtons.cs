using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace MDPlayer.Desktop.Controls;

public static class IconButtons
{
    public static void Set(Button button, string kind, string label, bool compact = false, string? shortcut = null)
    {
        var icon = new AppIcon { Kind = kind, VerticalAlignment = VerticalAlignment.Center };
        icon.Bind(TemplatedControl.ForegroundProperty, button.GetObservable(TemplatedControl.ForegroundProperty));
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 7, VerticalAlignment = VerticalAlignment.Center };
        panel.Children.Add(icon);
        panel.Children.Add(new TextBlock { Text = label, IsVisible = !compact, VerticalAlignment = VerticalAlignment.Center });
        button.Content = panel;
        AutomationProperties.SetName(button, label);
        ToolTip.SetTip(button, shortcut is null ? label : $"{label} ({shortcut})");
    }

    public static void Update(Button button, string kind, string label, bool compact)
    {
        if (button.Content is not StackPanel panel) { Set(button, kind, label, compact); return; }
        ((AppIcon)panel.Children[0]).Kind = kind;
        var text = (TextBlock)panel.Children[1]; text.Text = label; text.IsVisible = !compact;
        AutomationProperties.SetName(button, label);
        ToolTip.SetTip(button, label);
    }
}
