using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using MDPlayer.Rendering;
using Xunit;

namespace MDPlayer.Tests;
public sealed class AccessibilityTests
{
    [AvaloniaFact]
    public void AccessibilityExposesWholeDocumentAndOffscreenBlocksWithoutCreatingTextControls()
    {
        var reader = new MarkdownDocumentView();
        var parsed = new MarkdownParser().Parse(string.Concat(Enumerable.Repeat("# Heading\n\nAn accessible paragraph.\n\n", 100)), 1);
        reader.SetDocument(parsed);
        var window = new Window { Content = new ScrollViewer { Content = reader }, Width = 600, Height = 400 };
        window.Show();
        var peer = ControlAutomationPeer.CreatePeerForElement(reader)!;
        Assert.Equal(parsed.PlainText, Assert.IsAssignableFrom<IValueProvider>(peer).Value);
        Assert.True(((IValueProvider)peer).IsReadOnly);
        Assert.Equal(parsed.Blocks.Count, peer.GetChildren()!.Count);
        Assert.Contains("Heading 1", peer.GetChildren()![0].GetName());
        Assert.True(peer.GetChildren()![^1].IsOffscreen());
        Assert.NotEqual(peer.GetChildren()![0].GetAutomationId(), peer.GetChildren()![1].GetAutomationId());
        window.Close();
    }
}
