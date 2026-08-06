using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubZeroPOS.WPF.Session;

namespace SubZeroPOS.WPF.ViewModels
{
    public partial class MainDashboardViewModel : ObservableObject
    {
        public string WelcomeMessage => $"مرحباً، {CurrentSession.FullName}";
        public bool IsManager => CurrentSession.IsManager;

        // Placeholder stats - wire these to real queries once OrderService/ExpenseService exist
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

   
    }
}
