using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BMTECHRD.Pos.App.Models;
using BMTECHRD.Pos.App.Services;

namespace BMTECHRD.Pos.App.ViewModels.Admin;

public sealed class ReportsAdminViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly Guid _businessId;
    private readonly Guid _actorUserId;

    private DateTime _fromDate = DateTime.Today.AddDays(-7);
    public DateTime FromDate { get => _fromDate; set { _fromDate = value; OnPropertyChanged(); } }

    private DateTime _toDate = DateTime.Today;
    public DateTime ToDate { get => _toDate; set { _toDate = value; OnPropertyChanged(); } }

    private int _topLimit = 10;
    public int TopLimit { get => _topLimit; set { _topLimit = value > 50 ? 50 : value; OnPropertyChanged(); } }

    private DailySalesReportModel? _dailyReport;
    public DailySalesReportModel? DailyReport { get => _dailyReport; set { _dailyReport = value; OnPropertyChanged(); } }

    public ObservableCollection<SalesByProductModel> TopProducts { get; } = new();
    public ObservableCollection<SalesByUserModel> TopUsers { get; } = new();

    private decimal _totalSales;
    public decimal TotalSales { get => _totalSales; set { _totalSales = value; OnPropertyChanged(); } }

    private decimal _averageDaily;
    public decimal AverageDaily { get => _averageDaily; set { _averageDaily = value; OnPropertyChanged(); } }

    private decimal _paymentsTotal;
    public decimal PaymentsTotal { get => _paymentsTotal; set { _paymentsTotal = value; OnPropertyChanged(); } }

    private decimal _paymentsCash;
    public decimal PaymentsCash { get => _paymentsCash; set { _paymentsCash = value; OnPropertyChanged(); } }

    private decimal _paymentsCard;
    public decimal PaymentsCard { get => _paymentsCard; set { _paymentsCard = value; OnPropertyChanged(); } }

    private decimal _paymentsTransfer;
    public decimal PaymentsTransfer { get => _paymentsTransfer; set { _paymentsTransfer = value; OnPropertyChanged(); } }

    private decimal _paymentsMixed;
    public decimal PaymentsMixed { get => _paymentsMixed; set { _paymentsMixed = value; OnPropertyChanged(); } }

    public RelayCommand LoadReportsCommand { get; }
    public RelayCommand SetRange7DaysCommand { get; }
    public RelayCommand SetRangeTodayCommand { get; }
    public RelayCommand SetRangeMonthCommand { get; }

    public ReportsAdminViewModel(ApiClient api, Guid businessId, Guid actorUserId)
    {
        _api = api;
        _businessId = businessId;
        _actorUserId = actorUserId;

        LoadReportsCommand = new RelayCommand(async _ => await LoadReportsAsync());
        SetRange7DaysCommand = new RelayCommand(_ => { FromDate = DateTime.Today.AddDays(-7); ToDate = DateTime.Today; });
        SetRangeTodayCommand = new RelayCommand(_ => { FromDate = DateTime.Today; ToDate = DateTime.Today; });
        SetRangeMonthCommand = new RelayCommand(_ => { FromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); ToDate = DateTime.Today; });

        _ = LoadReportsAsync();
    }

    public async Task LoadReportsAsync()
    {
        // Validación
        if (FromDate > ToDate)
        {
            Error = "La fecha 'Desde' no puede ser posterior a 'Hasta'";
            return;
        }

        IsBusy = true;
        Error = null;
        OnPropertyChanged(nameof(IsBusy));

        try
        {
            // Cargar en paralelo
            var dailyTask = _api.GetDailySalesAsync(_businessId, FromDate, ToDate);
            var productsTask = _api.GetSalesByProductAsync(_businessId, FromDate, ToDate, TopLimit);
            var usersTask = _api.GetSalesByUserAsync(_businessId, FromDate, ToDate);

            await Task.WhenAll(dailyTask, productsTask, usersTask);

            // Asignar resultados
            DailyReport = await dailyTask;
            var products = await productsTask;
            var users = await usersTask;

            TopProducts.Clear();
            foreach (var p in products)
                TopProducts.Add(p);

            TopUsers.Clear();
            foreach (var u in users)
                TopUsers.Add(u);

            // Calcular KPIs
            if (DailyReport != null)
            {
                TotalSales = DailyReport.Items.Sum(x => x.Total);
                AverageDaily = DailyReport.Items.Count > 0 ? TotalSales / DailyReport.Items.Count : 0;
                PaymentsTotal = DailyReport.Payments.Total;
                PaymentsCash = DailyReport.Payments.Cash;
                PaymentsCard = DailyReport.Payments.Card;
                PaymentsTransfer = DailyReport.Payments.Transfer;
                PaymentsMixed = DailyReport.Payments.Mixed;
            }
        }
        catch (HttpRequestException)
        {
            Error = "No fue posible cargar reportes. Verifica la conexión con el servidor.";
        }
        catch (TaskCanceledException)
        {
            Error = "Tiempo de espera agotado al cargar reportes.";
        }
        catch (Exception ex)
        {
            Error = $"Error al cargar reportes: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(IsBusy));
        }
    }
}
