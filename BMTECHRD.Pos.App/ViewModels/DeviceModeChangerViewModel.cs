using System;
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
            OnPropertyChanged(nameof(CanSave));
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

    // Bindings usados por DeviceModeChangerDialog.xaml
    public ICommand SelectModeCommand { get; }
    public ICommand SaveAndRestartCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action<DeviceMode>? OnSaveRequested;
    public event Action? OnCancelRequested;

    public DeviceModeChangerViewModel()
    {
        SelectModeCommand = new RelayCommand(p =>
        {
            if (p is DeviceMode mode) SelectedMode = mode;
            else if (p != null && Enum.TryParse(p.ToString(), out DeviceMode parsed)) SelectedMode = parsed;
        });

        SaveAndRestartCommand = new RelayCommand(_ => HandleSave(), _ => CanSave);
        CancelCommand = new RelayCommand(_ => OnCancelRequested?.Invoke());
    }

    public void Initialize(DeviceMode currentMode)
    {
        CurrentMode = currentMode;
        SelectedMode = currentMode;
        IsLoading = false;
    }

    private void HandleSave()
    {
        IsLoading = true;
        OnSaveRequested?.Invoke(SelectedMode);
    }
}
