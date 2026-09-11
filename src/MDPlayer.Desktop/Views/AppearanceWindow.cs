using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using MDPlayer.Core;
using MDPlayer.Desktop.Services;

namespace MDPlayer.Desktop.Views;

public sealed class AppearanceWindow : Window
{
    private bool _committed;
    private readonly TextBlock _status = new() { TextWrapping = TextWrapping.Wrap };
    public AppearanceWindow(IAppearanceService appearance)
    {
        Title = "Appearance · Margin"; Width = 780; Height = 650; MinWidth = 540; MinHeight = 420; WindowStartupLocation = WindowStartupLocation.CenterOwner;
        appearance.BeginPreview();
        var choices = new List<(Button Button, ThemePreference Preference)>();
        var root = new DockPanel { Margin = new Thickness(24) };
        var header = new StackPanel { Spacing = 8, Margin = new Thickness(0, 0, 0, 20) };
        header.Children.Add(new TextBlock { Text = "A different mood. The same words.", FontSize = 24, FontWeight = FontWeight.SemiBold, TextWrapping = TextWrapping.Wrap });
        header.Children.Add(new TextBlock { Text = "Preview a skin. Your reading typography stays exactly as you set it.", TextWrapping = TextWrapping.Wrap });
        var system = new Button { Content = "Follow system · Paper Light / Dark", HorizontalAlignment = HorizontalAlignment.Left };
        choices.Add((system, new()));
        system.Click += (_, _) => TryPreview(new()); header.Children.Add(system); DockPanel.SetDock(header, Dock.Top); root.Children.Add(header);
        var footer = new DockPanel { Margin = new Thickness(0, 16, 0, 0) };
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var cancel = new Button { Content = "Cancel" }; cancel.Click += (_, _) => Close();
        var apply = new Button { Content = "Apply" }; apply.Classes.Add("accent");
        apply.Click += (_, _) => { try { appearance.Commit(); _committed = true; Close(); } catch (Exception ex) { _status.Text = ex.Message; } };
        actions.Children.Add(cancel); actions.Children.Add(apply); DockPanel.SetDock(actions, Dock.Right); footer.Children.Add(actions); footer.Children.Add(_status); DockPanel.SetDock(footer, Dock.Bottom); root.Children.Add(footer);
        var groups = new StackPanel { Spacing = 18 };
        foreach (var applicationSkins in new[] { true, false })
        {
            groups.Children.Add(new TextBlock { Text = applicationSkins ? "Margin" : "Included skins", FontSize = 16, FontWeight = FontWeight.SemiBold });
            var tiles = new WrapPanel();
            foreach (var choice in appearance.Catalog.Where(x => x.IsApplicationSkin == applicationSkins).OrderBy(x => x.Name))
            {
                var sample = new StackPanel { Spacing = 8 };
                sample.Children.Add(new TextBlock { Text = "Aa  ·  A quiet moment", Foreground = choice.Foreground, FontFamily = "Georgia", FontSize = 21 });
                sample.Children.Add(new Border { Height = 4, Background = choice.Accent, CornerRadius = new CornerRadius(2) });
                sample.Children.Add(new Border { Background = choice.Secondary, Padding = new Thickness(4), Child = new TextBlock { Text = choice.Name, Foreground = choice.Foreground, TextWrapping = TextWrapping.Wrap, FontSize = 12 } });
                var tile = new Button { Content = new Border { Background = choice.Background, Padding = new Thickness(12), Child = sample }, Width = 222, Margin = new Thickness(0, 0, 10, 10), Padding = new Thickness(2) };
                AutomationProperties.SetName(tile, choice.Name);
                var preference = new ThemePreference(false, choice.Name);
                choices.Add((tile, preference));
                tile.Click += (_, _) => TryPreview(preference); tiles.Children.Add(tile);
            }
            groups.Children.Add(tiles);
        }
        root.Children.Add(new ScrollViewer { Content = groups, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled });
        Content = root; UpdateStatus();
        Closing += (_, e) =>
        {
            if (_committed) return;
            try { appearance.Cancel(); } catch (Exception ex) { e.Cancel = true; _status.Text = ex.Message; }
        };
        Opened += (_, _) => { if (Screens.ScreenFromWindow(this) is { } screen) Height = Math.Min(Height, screen.WorkingArea.Height / RenderScaling - 40); };
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape) { Close(); e.Handled = true; return; }
            var current = choices.FindIndex(x => x.Button.IsKeyboardFocusWithin);
            if (current < 0 || e.Key is not (Key.Left or Key.Right or Key.Up or Key.Down)) return;
            var columns = Math.Max(1, (int)((Bounds.Width - 48) / 232));
            var delta = e.Key switch { Key.Left => -1, Key.Right => 1, Key.Up => -columns, _ => columns };
            choices[Math.Clamp(current + delta, 0, choices.Count - 1)].Button.Focus(); e.Handled = true;
        };
        void UpdateStatus()
        {
            _status.Text = appearance.Selection.FollowSystem ? "Preview: Follow system" : "Preview: " + appearance.Selection.SkinName;
            foreach (var choice in choices)
            {
                var selected = choice.Preference.FollowSystem ? appearance.Selection.FollowSystem : !appearance.Selection.FollowSystem && appearance.Selection.SkinName == choice.Preference.SkinName;
                choice.Button.Classes.Set("active", selected);
                AutomationProperties.SetHelpText(choice.Button, selected ? "Current preview" : "Press Enter to preview this appearance");
            }
        }
        void TryPreview(ThemePreference preference) { try { appearance.Preview(preference); UpdateStatus(); } catch (Exception ex) { _status.Text = ex.Message; } }
    }
}
