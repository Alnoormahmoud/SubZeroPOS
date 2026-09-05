using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubZeroPOS.Core.DTOs;
using SubZeroPOS.Core.Interfaces;

namespace SubZeroPOS.WPF.ViewModels
{
    // Chart-ready wrapper: bar size is pre-computed here (as plain numbers)
    // so the XAML only needs simple bindings, not runtime math/converters.
    public class DailySalesBar
    {
        public string DayLabel { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public double BarHeight { get; set; } // pixels, 0-120
    }

    public class TopItemBar
    {
        public string ItemName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public double BarWidth { get; set; } // pixels, 0-260
    }

    public class CategoryBar
    {
        public string CategoryNameAr { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int QuantitySold { get; set; }
        public double BarWidth { get; set; } // pixels, 0-260
    }

    public class OrderTypeBar
    {
        public string OrderTypeNameAr { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public string PercentageLabel { get; set; } = "0%";
        public double BarWidth { get; set; } // pixels, 0-260
    }

    public partial class ReportsViewModel : ObservableObject
    {
        private const double MaxDailyBarHeight = 120;
        private const double MaxHorizontalBarWidth = 260;

        private readonly IReportService _reportService;

        public ReportsViewModel(IReportService reportService)
        {
            _reportService = reportService;
        }

        public ObservableCollection<ExpenseByCategoryDto> ExpensesByCategory { get; } = new();
        public ObservableCollection<DailySalesBar> DailySalesChart { get; } = new();
        public ObservableCollection<TopItemBar> TopItemsChart { get; } = new();
        public ObservableCollection<CategoryBar> CategoriesChart { get; } = new();
        public ObservableCollection<OrderTypeBar> OrderTypeChart { get; } = new();

        [ObservableProperty]
        private DateTime fromDate = DateTime.Today.AddDays(-6);

        [ObservableProperty]
        private DateTime toDate = DateTime.Today;

        [ObservableProperty]
        private string totalIncome = "0";

        [ObservableProperty]
        private string totalExpenses = "0";

        [ObservableProperty]
        private string netProfit = "0";

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

       
                TotalIncome = $"{summary.TotalIncome:#,##0} {Session.CurrencyHolder.Symbol}";
                TotalExpenses = $"{summary.TotalExpenses:#,##0} {Session.CurrencyHolder.Symbol}";
                NetProfit = $"{summary.NetProfit:#,##0} {Session.CurrencyHolder.Symbol}";
                TotalOrders = summary.TotalOrders.ToString();

                ExpensesByCategory.Clear();
                foreach (var e in summary.ExpensesByCategory)
                    ExpensesByCategory.Add(e);

                // Daily sales trend bar chart
                DailySalesChart.Clear();
                var maxDaily = summary.DailySales.Count > 0 ? summary.DailySales.Max(d => d.Total) : 0;
                foreach (var d in summary.DailySales)
                {
                    DailySalesChart.Add(new DailySalesBar
                    {
                        DayLabel = d.Date.ToString("MM/dd"),
                        Total = d.Total,
                        BarHeight = maxDaily > 0 ? (double)(d.Total / maxDaily) * MaxDailyBarHeight : 0
                    });
                }



                // Top selling items (already sorted by revenue, top 8)
                TopItemsChart.Clear();
                var maxItemRevenue = summary.TopSellingItems.Count > 0 ? summary.TopSellingItems.Max(i => i.TotalRevenue) : 0;
                foreach (var i in summary.TopSellingItems)
                {
                    TopItemsChart.Add(new TopItemBar
                    {
                        ItemName = i.ItemName,
                        QuantitySold = i.QuantitySold,
                        TotalRevenue = i.TotalRevenue,
                        BarWidth = maxItemRevenue > 0 ? (double)(i.TotalRevenue / maxItemRevenue) * MaxHorizontalBarWidth : 0
                    });
                }

                // Sales by category
                CategoriesChart.Clear();
                var maxCategoryRevenue = summary.SalesByCategory.Count > 0 ? summary.SalesByCategory.Max(c => c.TotalRevenue) : 0;
                foreach (var c in summary.SalesByCategory)
                {
                    CategoriesChart.Add(new CategoryBar
                    {
                        CategoryNameAr = c.CategoryNameAr,
                        TotalRevenue = c.TotalRevenue,
                        QuantitySold = c.QuantitySold,
                        BarWidth = maxCategoryRevenue > 0 ? (double)(c.TotalRevenue / maxCategoryRevenue) * MaxHorizontalBarWidth : 0
                    });
                }

                // Order type breakdown (covers delivery rate)
                OrderTypeChart.Clear();
                var totalOrderCount = summary.OrderTypeBreakdown.Sum(t => t.OrderCount);
                var maxTypeRevenue = summary.OrderTypeBreakdown.Count > 0 ? summary.OrderTypeBreakdown.Max(t => t.TotalRevenue) : 0;
                foreach (var t in summary.OrderTypeBreakdown)
                {
                    var pct = totalOrderCount > 0 ? (double)t.OrderCount / totalOrderCount * 100 : 0;
                    OrderTypeChart.Add(new OrderTypeBar
                    {
                        OrderTypeNameAr = t.OrderTypeNameAr,
                        OrderCount = t.OrderCount,
                        TotalRevenue = t.TotalRevenue,
                        PercentageLabel = $"{pct:0}%",
                        BarWidth = maxTypeRevenue > 0 ? (double)(t.TotalRevenue / maxTypeRevenue) * MaxHorizontalBarWidth : 0
                    });
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SetTodayAsync()
        {
            FromDate = DateTime.Today;
            ToDate = DateTime.Today;
            await LoadReportAsync();
        }

        [RelayCommand]
        private async Task SetThisWeekAsync()
        {
            FromDate = DateTime.Today.AddDays(-6);
            ToDate = DateTime.Today;
            await LoadReportAsync();
        }

        [RelayCommand]
        private async Task SetThisMonthAsync()
        {
            FromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            ToDate = DateTime.Today;
            await LoadReportAsync();
        }

        [RelayCommand]
        private void Back() => BackRequested?.Invoke();
    }
}
