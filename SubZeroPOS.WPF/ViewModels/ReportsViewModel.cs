using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubZeroPOS.Core.DTOs;
using SubZeroPOS.Core.Interfaces;

namespace SubZeroPOS.WPF.ViewModels
{
    public partial class ReportsViewModel : ObservableObject
    {
        private readonly IReportService _reportService;

        public ReportsViewModel(IReportService reportService)
        {
            _reportService = reportService;
        }

        public ObservableCollection<ExpenseByCategoryDto> ExpensesByCategory { get; } = new();

        [ObservableProperty]
        private DateTime fromDate = DateTime.Today.AddDays(-6);

        [ObservableProperty]
        private DateTime toDate = DateTime.Today;

        [ObservableProperty]
        private string totalIncome = "0.000";

        [ObservableProperty]
        private string totalExpenses = "0.000";

        [ObservableProperty]
        private string netProfit = "0.000";

        [ObservableProperty]
        private string totalOrders = "0";

        [ObservableProperty]
        private bool isBusy;

        public event Action? BackRequested;

        public async Task InitializeAsync() => await LoadReportAsync();

        [RelayCommand]
        private async Task LoadReportAsync()
        {
            IsBusy = true;
            try
            {
                var summary = await _reportService.GetSummaryAsync(FromDate, ToDate);

                TotalIncome = summary.TotalIncome.ToString("0.000");
                TotalExpenses = summary.TotalExpenses.ToString("0.000");
                NetProfit = summary.NetProfit.ToString("0.000");
                TotalOrders = summary.TotalOrders.ToString();

                ExpensesByCategory.Clear();
                foreach (var e in summary.ExpensesByCategory)
                    ExpensesByCategory.Add(e);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void SetToday()
        {
            FromDate = DateTime.Today;
            ToDate = DateTime.Today;
        }

        [RelayCommand]
        private void SetThisWeek()
        {
            FromDate = DateTime.Today.AddDays(-6);
            ToDate = DateTime.Today;
        }

        [RelayCommand]
        private void SetThisMonth()
        {
            FromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            ToDate = DateTime.Today;
        }

        [RelayCommand]
        private void Back() => BackRequested?.Invoke();
    }
}
