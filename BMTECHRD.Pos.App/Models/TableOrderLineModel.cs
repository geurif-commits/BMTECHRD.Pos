using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BMTECHRD.Pos.App.Models;

public sealed class TableOrderLineModel : INotifyPropertyChanged
{
    private int _quantity;

    public System.Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity == value) return;
            _quantity = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LineTotal));
        }
    }

    public decimal LineTotal => UnitPrice * Quantity;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
