using System.Windows.Input;
using BMTECHRD.Pos.App.Core;
using BMTECHRD.Pos.App.Helpers;

namespace BMTECHRD.Pos.App.ViewModels;

public class DeviceModeChangerViewModel : ViewModelBase
{
    private DeviceMode _currentMode;
    private DeviceMode _selectedMode;
    private bool _isLoading;

    public DeviceMode CurrentMode
    {
        get => _currentMode;
        set
        {
            if (_currentMode == value) return;
            _currentMode = value;
            OnPropertyChanged();
        }
    }

    public DeviceMode SelectedMode
    {
        get => _selectedMode;
        set
        {
            if (_selectedMode == value) return;
            _selectedMode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSave));
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading == value) return;
            _isLoading = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSave));
        }
    }

    public bool CanSave => SelectedMode != CurrentMode && !IsLoading;

    public ICommand SelectServerCommand { get; }
    public ICommand SelectCashierCommand { get; }
    public ICommand SelectKitchenCommand { get; }
    public ICommand SelectBarCommand { get; }
    public ICommand SelectFloorCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public event System.Action<DeviceMode>? OnSaveRequested;
    public event System.Action? OnCancelRequested;

    public DeviceModeChangerViewModel()
    {
        SelectServerCommand = new RelayCommand(_ => SelectedMode = DeviceMode.Server);
        SelectCashierCommand = new RelayCommand(_ => SelectedMode = DeviceMode.Cashier);
        SelectKitchenCommand = new RelayCommand(_ => SelectedMode = DeviceMode.Kitchen);
        SelectBarCommand = new RelayCommand(_ => SelectedMode = DeviceMode.Bar);
        SelectFloorCommand = new RelayCommand(_ => SelectedMode = DeviceMode.Floor);

        SaveCommand = new RelayCommand(_ => HandleSave(), _ => CanSave);
        CancelCommand = new RelayCommand(_ => OnCancelRequested?.Invoke());
    }

    public void Initialize(DeviceMode currentMode)
    {
        CurrentMode = currentMode;
        SelectedMode = currentMode;
    }

    private void HandleSave()
    {
        IsLoading = true;
        OnSaveRequested?.Invoke(SelectedMode);
    }
}
