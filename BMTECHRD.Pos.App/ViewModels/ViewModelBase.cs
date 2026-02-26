using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace BMTECHRD.Pos.App.ViewModels;
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy == value) return;
            _isBusy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LoadingText));
        }
    }

    public string LoadingText => IsBusy ? "Cargando..." : string.Empty;

    private string? _error;
    public string? Error { get => _error; set { _error = value; OnPropertyChanged(); OnPropertyChanged(nameof(ErrorVisible)); } }
    public bool ErrorVisible => !string.IsNullOrEmpty(Error);
}
