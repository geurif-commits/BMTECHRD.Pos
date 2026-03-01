using System;
using System.Threading.Tasks;
using BMTECHRD.Pos.App.Models;
using BMTECHRD.Pos.App.Services;

namespace BMTECHRD.Pos.App.ViewModels.Admin;

public sealed class BusinessSettingsViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly Guid _businessId;

    public string Name { get; set; } = string.Empty;
    public bool EnableItbis { get; set; }
    public decimal ItbisRate { get; set; } = 0.18m;
    public bool EnableTip { get; set; }
    public decimal TipRate { get; set; } = 0.10m;
    public bool EnableFiscalReceipt { get; set; }
    public bool EnableElectronicInvoice { get; set; }

    public string? InfoMessage { get; set; }

    public RelayCommand LoadCommand { get; }
    public RelayCommand SaveCommand { get; }

    public BusinessSettingsViewModel(ApiClient api, Guid businessId)
    {
        _api = api;
        _businessId = businessId;

        LoadCommand = new RelayCommand(async _ => await LoadAsync());
        SaveCommand = new RelayCommand(async _ => await SaveAsync());
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        Error = null;

        try
        {
            var settings = await _api.GetBusinessSettingsAsync(_businessId);
            if (settings == null)
            {
                Error = "No se pudo cargar configuración del negocio.";
                return;
            }

            Name = settings.Name;
            EnableItbis = settings.EnableItbis;
            ItbisRate = settings.ItbisRate;
            EnableTip = settings.EnableTip;
            TipRate = settings.TipRate;
            EnableFiscalReceipt = settings.EnableFiscalReceipt;
            EnableElectronicInvoice = settings.EnableElectronicInvoice;

            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(EnableItbis));
            OnPropertyChanged(nameof(ItbisRate));
            OnPropertyChanged(nameof(EnableTip));
            OnPropertyChanged(nameof(TipRate));
            OnPropertyChanged(nameof(EnableFiscalReceipt));
            OnPropertyChanged(nameof(EnableElectronicInvoice));
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            Error = "Nombre del negocio requerido.";
            return;
        }

        if (EnableItbis && (ItbisRate < 0 || ItbisRate > 1))
        {
            Error = "ITBIS debe estar entre 0 y 1 (ej: 0.18).";
            return;
        }

        if (EnableTip && (TipRate < 0 || TipRate > 1))
        {
            Error = "Propina debe estar entre 0 y 1 (ej: 0.10).";
            return;
        }

        Error = null;
        InfoMessage = null;
        OnPropertyChanged(nameof(InfoMessage));

        IsBusy = true;
        try
        {
            var ok = await _api.UpdateBusinessSettingsAsync(new BusinessSettingsModel
            {
                BusinessId = _businessId,
                Name = Name.Trim(),
                EnableItbis = EnableItbis,
                ItbisRate = ItbisRate,
                EnableTip = EnableTip,
                TipRate = TipRate,
                EnableFiscalReceipt = EnableFiscalReceipt,
                EnableElectronicInvoice = EnableElectronicInvoice
            });

            if (!ok)
            {
                Error = "No se pudo guardar la configuración.";
                return;
            }

            InfoMessage = "Configuración guardada correctamente.";
            OnPropertyChanged(nameof(InfoMessage));
        }
        finally
        {
            IsBusy = false;
        }
    }
}
