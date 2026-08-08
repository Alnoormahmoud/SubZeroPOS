using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubZeroPOS.Core.Interfaces;
using SubZeroPOS.WPF.Session;

namespace SubZeroPOS.WPF.ViewModels
{
    public partial class MainDashboardViewModel : ObservableObject
    {
        private readonly IOrderService _orderService;
        private readonly IExpenseService _expenseService;

        public MainDashboardViewModel(IOrderService orderService, IExpenseService expenseService)
        {
            _orderService = orderService;
            _expenseService = expenseService;
        }

        public string WelcomeMessage => $"مرحباً، {CurrentSession.FullName}";
        public bool IsManager => CurrentSession.IsManager;

        [ObservableProperty]
        private string todaysSales = "0.000";

        [ObservableProperty]
        private string todaysExpenses = "0.000";

        [ObservableProperty]
        private string todaysOrderCount = "0";

        public event Action? LogoutRequested;
        public event Action? NewOrderRequested;
        public event Action? ExpensesRequested;
        public event Action? ReportsRequested;
        public event Action? ShiftRequested;

        // Called every time the dashboard becomes visible (Loaded fires on every
        // navigation back to it), so numbers always reflect the latest orders/expenses.
        public async Task RefreshStatsAsync()
        {
            var today = DateTime.Today;

            var orders = await _orderService.GetOrdersByDateAsync(today);
            var completedOrders = orders.Where(o => o.StatusCode == "Completed").ToList();

            TodaysSales = completedOrders.Sum(o => o.TotalAmount).ToString("0.000");
            TodaysOrderCount = completedOrders.Count.ToString();

            var expenses = await _expenseService.GetExpensesByDateRangeAsync(today, today);
            TodaysExpenses = expenses.Sum(e => e.Amount).ToString("0.000");
        }

        [RelayCommand]
        private void Logout()
        {
            CurrentSession.Clear();
            LogoutRequested?.Invoke();
        }

        [RelayCommand]
        private void NewOrder() => NewOrderRequested?.Invoke();

        [RelayCommand]
        private void OpenExpenses() => ExpensesRequested?.Invoke();

        [RelayCommand]
        private void OpenReports() => ReportsRequested?.Invoke();

        [RelayCommand]
        private void OpenShift() => ShiftRequested?.Invoke();
    }
}
