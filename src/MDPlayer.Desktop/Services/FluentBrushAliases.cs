using Avalonia;
using Avalonia.Media;

namespace MDPlayer.Desktop.Services;

internal static class FluentBrushAliases
{
    public static void Apply(Application app)
    {
        void Alias(string resource, params string[] keys) { foreach (var key in keys) app.Resources[key] = app.Resources[resource]; }
        Alias("AccentBlueBrush", "SliderThumbBackground", "SliderThumbBackgroundPointerOver", "SliderThumbBackgroundPressed", "SliderTrackValueFill", "SliderTrackValueFillPointerOver", "SliderTrackValueFillPressed", "TextControlBorderBrushFocused", "ComboBoxBorderBrushFocused", "SystemControlHighlightAccentBrush", "SystemControlHighlightAltAccentBrush");
        Alias("BorderBrush", "SliderTrackFill", "SliderTrackFillPointerOver", "SliderTrackFillPressed", "SliderTrackFillDisabled", "TextControlBorderBrush", "TextControlBorderBrushPointerOver", "TextControlBorderBrushDisabled", "ComboBoxBorderBrush", "ComboBoxBorderBrushPointerOver", "ComboBoxBorderBrushPressed", "ComboBoxBorderBrushDisabled", "ComboBoxDropDownBorderBrush", "MenuFlyoutPresenterBorderBrush", "ToolTipBorderBrush");
        Alias("BackgroundLightBrush", "SliderContainerBackground", "SliderContainerBackgroundDisabled", "TextControlBackground", "TextControlBackgroundPointerOver", "TextControlBackgroundFocused", "TextControlBackgroundDisabled", "ComboBoxBackground", "ComboBoxBackgroundPointerOver", "ComboBoxBackgroundPressed", "ComboBoxBackgroundDisabled", "ComboBoxDropDownBackground", "MenuFlyoutPresenterBackground", "ToolTipBackground");
        Alias("TextPrimaryBrush", "TextControlForeground", "TextControlForegroundFocused", "TextControlForegroundPointerOver", "ComboBoxForeground", "ComboBoxForegroundPointerOver", "ComboBoxForegroundPressed", "ComboBoxItemForeground", "ComboBoxItemForegroundSelected", "ComboBoxItemForegroundSelectedPointerOver", "MenuFlyoutItemForeground", "MenuFlyoutItemForegroundPointerOver", "ToolTipForeground");
        Alias("TextSecondaryBrush", "SliderThumbBackgroundDisabled", "SliderTrackValueFillDisabled", "SliderTickBarFill", "TextControlForegroundDisabled", "TextControlPlaceholderForeground", "TextControlPlaceholderForegroundFocused", "TextControlPlaceholderForegroundPointerOver", "TextControlPlaceholderForegroundDisabled", "ComboBoxForegroundDisabled", "ComboBoxPlaceholderForeground", "MenuFlyoutItemForegroundDisabled");
        Alias("SecondaryColorBrush", "ComboBoxItemBackgroundSelected", "ComboBoxItemBackgroundPointerOver", "ComboBoxItemBackgroundSelectedPointerOver", "ComboBoxItemBackgroundPressed", "MenuFlyoutItemBackgroundPointerOver", "MenuFlyoutItemBackgroundPressed", "ScrollBarThumbFill", "ScrollBarThumbFillPointerOver", "ScrollBarThumbFillPressed");
        var source = ((SolidColorBrush)app.Resources["BackgroundBrush"]!).Color;
        app.Resources["ScrollBarBackground"] = new SolidColorBrush(source, 0);
        app.Resources["ScrollBarBackgroundPointerOver"] = app.Resources["BackgroundLightBrush"];
    }
}
