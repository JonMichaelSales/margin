using CommunityToolkit.Mvvm.ComponentModel;
using MDPlayer.Core;

namespace MDPlayer.Desktop.ViewModels;

public sealed class DocumentViewModel : ObservableObject
{
    public DocumentSession Session { get; } = new();
    private bool _busy;
    private string _status = "Ready when you are";
    private ReadingPreferences _reading;
    public bool IsBusy { get => _busy; set => SetProperty(ref _busy, value); }
    public string Status { get => _status; set => SetProperty(ref _status, value); }
    public ReadingPreferences Reading { get => _reading; set => SetProperty(ref _reading, value.Sanitize()); }
    public DocumentViewModel(ReadingPreferences reading) => _reading = reading.Sanitize();
}
