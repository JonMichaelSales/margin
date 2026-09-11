using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace MDPlayer.Desktop.Controls;

// Production geometry reconstructed from the Margin asset concepts, on a 24 DIP grid.
public sealed class AppIcon : TemplatedControl
{
    public static readonly StyledProperty<string> KindProperty = AvaloniaProperty.Register<AppIcon, string>(nameof(Kind), "information");
    public string Kind { get => GetValue(KindProperty); set => SetValue(KindProperty, value); }
    private static readonly Dictionary<string, Geometry> Shapes = new Dictionary<string, string>
    {
        ["brand"] = "M17 3 H7 Q5 3 5 5 V19 Q5 21 7 21 H17 Q19 21 19 19 V7 L18 6 M8 6 V18",
        ["open"] = "M3 19 V6 H9 L11 8 H21 V11 M3 19 L6 11 H22 L19 19 Z",
        ["save"] = "M4 3 H17 L21 7 V21 H3 V3 Z M7 3 V9 H16 V3 M7 21 V14 H17 V21",
        ["save-as"] = "M10 21 H3 V3 H17 L21 7 V10 M7 3 V9 H16 V3 M13 19 L19 13 L22 16 L16 22 H13 Z",
        ["read"] = "M12 6 Q7 3 2 5 V20 Q7 18 12 21 Q17 18 22 20 V5 Q17 3 12 6 V21",
        ["edit"] = "M4 16 L16 4 L20 8 L8 20 L3 21 Z M14 6 L18 10",
        ["split"] = "M3 4 H21 V20 H3 Z M12 4 V20",
        ["outline"] = "M3 5 H5 M8 5 H21 M5 12 H7 M10 12 H21 M7 19 H9 M12 19 H21",
        ["typography"] = "M2 19 L7 5 L12 19 M4 14 H10 M15 11 Q21 9 21 14 V19 M21 14 H17 Q14 14 15 17 Q16 20 21 17",
        ["find"] = "M17 10 A7 7 0 1 1 3 10 A7 7 0 1 1 17 10 M15 15 L22 22",
        ["focus"] = "M3 9 V3 H9 M15 3 H21 V9 M21 15 V21 H15 M9 21 H3 V15",
        ["exit-focus"] = "M3 9 H9 V3 M15 3 V9 H21 M21 15 H15 V21 M9 21 V15 H3",
        ["appearance"] = "M21 13 C23 5 17 2 12 2 C6 2 2 6 2 12 C2 18 7 22 12 22 C17 22 15 18 14 17 C13 15 16 14 19 15 Q21 15 21 13 M7 8 h.1 M12 6 h.1 M17 9 h.1 M6 13 h.1",
        ["undo"] = "M8 4 L3 9 L8 14 M3 9 H14 A6 6 0 0 1 14 21 H10",
        ["redo"] = "M16 4 L21 9 L16 14 M21 9 H10 A6 6 0 0 0 10 21 H14",
        ["bold"] = "M6 3 V21 H13 C21 21 21 12 13 12 H6 M6 3 H12 C20 3 20 12 12 12",
        ["italic"] = "M10 3 H20 M4 21 H14 M15 3 L9 21",
        ["heading"] = "M5 3 V21 M19 3 V21 M5 12 H19",
        ["quote"] = "M10 4 Q3 7 3 14 H10 V21 H3 V14 M21 4 Q14 7 14 14 H21 V21 H14 V14",
        ["list"] = "M3 5 h.1 M3 12 h.1 M3 19 h.1 M8 5 H21 M8 12 H21 M8 19 H21",
        ["code"] = "M7 6 L2 12 L7 18 M17 6 L22 12 L17 18 M14 3 L10 21",
        ["link"] = "M10 7 L13 4 A5 5 0 0 1 20 11 L17 14 M7 10 L4 13 A5 5 0 0 0 11 20 L14 17 M8 16 L16 8",
        ["close"] = "M5 5 L19 19 M19 5 L5 19",
        ["previous"] = "M15 4 L7 12 L15 20",
        ["next"] = "M9 4 L17 12 L9 20",
        ["expand"] = "M4 8 L12 16 L20 8",
        ["collapse"] = "M8 4 L16 12 L8 20",
        ["information"] = "M22 12 A10 10 0 1 1 2 12 A10 10 0 1 1 22 12 M12 11 V17 M12 7 h.1",
        ["warning"] = "M12 2 L23 21 H1 Z M12 9 V14 M12 18 h.1",
        ["error"] = "M22 12 A10 10 0 1 1 2 12 A10 10 0 1 1 22 12 M8 8 L16 16 M16 8 L8 16",
        ["success"] = "M22 12 A10 10 0 1 1 2 12 A10 10 0 1 1 22 12 M6 12 L10 16 L18 8",
        ["unsaved"] = "M16 12 A4 4 0 1 1 8 12 A4 4 0 1 1 16 12 Z",
        ["external-change"] = "M10 21 H3 V3 H15 L19 7 V9 M14 3 V8 H19 M12 15 A5 5 0 0 1 21 13 M21 10 V14 H17 M22 18 A5 5 0 0 1 13 20 M13 23 V19 H17"
    }.ToDictionary(x => x.Key, x => Geometry.Parse(x.Value));

    public static IReadOnlyCollection<string> Kinds => Shapes.Keys;
    static AppIcon() { AffectsRender<AppIcon>(KindProperty, ForegroundProperty); }
    public AppIcon() { Width = 20; Height = 20; IsHitTestVisible = false; }
    public override void Render(DrawingContext context)
    {
        if (Foreground is null || !Shapes.TryGetValue(Kind, out var geometry)) return;
        var size = Math.Min(Bounds.Width, Bounds.Height);
        using var transform = context.PushTransform(Matrix.CreateScale(size / 24, size / 24) * Matrix.CreateTranslation((Bounds.Width - size) / 2, (Bounds.Height - size) / 2));
        context.DrawGeometry(Kind == "unsaved" ? Foreground : null, new Pen(Foreground, Kind == "brand" ? 1.5 : 1.8, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round), geometry);
    }
}
