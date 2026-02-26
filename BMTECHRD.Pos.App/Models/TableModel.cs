using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BMTECHRD.Pos.App.Models;

public class TableModel : INotifyPropertyChanged
{
    public Guid Id { get; set; }

    private int _number;
    public int Number { get => _number; set { _number = value; OnPropertyChanged(); } }

    private string? _status;
    public string? Status { get => _status; set { _status = value; OnPropertyChanged(); } }

    private string? _waiterName;
    public string? WaiterName { get => _waiterName; set { _waiterName = value; OnPropertyChanged(); } }

    private double _posX;
    public double PosX { get => _posX; set { _posX = value; OnPropertyChanged(); } }

    private double _posY;
    public double PosY { get => _posY; set { _posY = value; OnPropertyChanged(); } }

    public Guid? OpenedByWaiterId { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
