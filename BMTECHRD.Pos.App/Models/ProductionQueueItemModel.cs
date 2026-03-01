using System;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BMTECHRD.Pos.App.Models;

public sealed class ProductionQueueItemModel : INotifyPropertyChanged
{
    public Guid OrderItemId { get; set; }
    public Guid OrderId { get; set; }
    public Guid TableId { get; set; }
    public int TableNumber { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }

    private string _status = string.Empty;
    public string Status
    {
        get => _status;
        set
        {
            if (_status == value) return;
            _status = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanStart));
            OnPropertyChanged(nameof(CanDone));
            OnPropertyChanged(nameof(StatusLabel));
        }
    }

    public string Area { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public string TimeLabel => CreatedAt.ToString("hh:mm tt", CultureInfo.InvariantCulture);

    public bool CanStart => string.Equals(Status, "SENT", StringComparison.OrdinalIgnoreCase);
    public bool CanDone => string.Equals(Status, "IN_PROGRESS", StringComparison.OrdinalIgnoreCase);

    public string StatusLabel
    {
        get
        {
            return Status switch
            {
                "SENT" => "Pendiente",
                "IN_PROGRESS" => "En proceso",
                "DONE" => "Listo",
                "CANCELLED" => "Cancelado",
                _ => Status
            };
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
