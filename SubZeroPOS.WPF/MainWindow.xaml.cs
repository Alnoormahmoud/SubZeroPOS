using System.Windows;
using SubZeroPOS.WPF.ViewModels;
using SubZeroPOS.WPF.Views;

namespace SubZeroPOS.WPF
{
    public partial class MainWindow : Window
    {
        private readonly LoginView _loginView;
        private readonly LoginViewModel _loginViewModel;
        private readonly MainDashboardView _dashboardView;
        private readonly MainDashboardViewModel _dashboardViewModel;
        private readonly OrderEntryView _orderEntryView;
        private readonly OrderEntryViewModel _orderEntryViewModel;
        private readonly ExpenseView _expenseView;
        private readonly ExpenseViewModel _expenseViewModel;
        private readonly OrderInvoiceView _orderInvoiceView;
        private readonly OrderInvoiceViewModel _orderInvoiceViewModel;
        private readonly SettingsView _settingsView;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly UserManagementView _userManagementView;
        private readonly UserManagementViewModel _userManagementViewModel;
        private readonly MenuManagementView _menuManagementView;
        private readonly MenuManagementViewModel _menuManagementViewModel;
        private readonly PreviousOrdersView _previousOrdersView;
        private readonly PreviousOrdersViewModel _previousOrdersViewModel;
        private readonly ReportsView _reportsView;
        private readonly ReportsViewModel _reportsViewModel;

        public MainWindow(
            LoginView loginView,
            LoginViewModel loginViewModel,
            MainDashboardView dashboardView,
            MainDashboardViewModel dashboardViewModel,
            OrderEntryView orderEntryView,
            OrderEntryViewModel orderEntryViewModel,
            ExpenseView expenseView,
            ExpenseViewModel expenseViewModel,
            OrderInvoiceView orderInvoiceView,
            OrderInvoiceViewModel orderInvoiceViewModel,
            SettingsView settingsView,
            SettingsViewModel settingsViewModel,
            UserManagementView userManagementView,
            UserManagementViewModel userManagementViewModel,
            MenuManagementView menuManagementView,
            MenuManagementViewModel menuManagementViewModel,
            PreviousOrdersView previousOrdersView,
            PreviousOrdersViewModel previousOrdersViewModel,
            ReportsView reportsView,
            ReportsViewModel reportsViewModel)
        {
            InitializeComponent();

            _loginView = loginView;
            _loginViewModel = loginViewModel;
            _dashboardView = dashboardView;
            _dashboardViewModel = dashboardViewModel;
            _orderEntryView = orderEntryView;
            _orderEntryViewModel = orderEntryViewModel;
            _expenseView = expenseView;
            _expenseViewModel = expenseViewModel;
            _orderInvoiceView = orderInvoiceView;
            _orderInvoiceViewModel = orderInvoiceViewModel;
            _settingsView = settingsView;
            _settingsViewModel = settingsViewModel;
            _userManagementView = userManagementView;
            _userManagementViewModel = userManagementViewModel;
            _menuManagementView = menuManagementView;
            _menuManagementViewModel = menuManagementViewModel;
            _previousOrdersView = previousOrdersView;
            _previousOrdersViewModel = previousOrdersViewModel;
            _reportsView = reportsView;
            _reportsViewModel = reportsViewModel;

            _loginView.DataContext = _loginViewModel;
            _loginViewModel.LoginSucceeded += ShowDashboard;

            _dashboardView.DataContext = _dashboardViewModel;
            _dashboardViewModel.LogoutRequested += ShowLogin;
            _dashboardViewModel.NewOrderRequested += ShowOrderEntry;
            _dashboardViewModel.ExpensesRequested += ShowExpenses;
            _dashboardViewModel.SettingsRequested += ShowSettings;
            _dashboardViewModel.UserManagementRequested += ShowUserManagement;
            _dashboardViewModel.MenuManagementRequested += ShowMenuManagement;
            _dashboardViewModel.PreviousOrdersRequested += ShowPreviousOrders;

            _orderEntryView.DataContext = _orderEntryViewModel;
            _orderEntryView.BackRequested += ShowDashboard;
            _orderEntryViewModel.CancelOrderRequested += ShowDashboard;
            _orderEntryViewModel.OrderCompleted += invoice =>
            {
                _orderInvoiceViewModel.SetOrder(invoice);
                ShowInvoice();
            };

            _expenseView.DataContext = _expenseViewModel;
            _expenseViewModel.BackRequested += ShowDashboard;

            _orderInvoiceView.DataContext = _orderInvoiceViewModel;
            _orderInvoiceViewModel.NewOrderRequested += ShowOrderEntry;
            _orderInvoiceViewModel.BackToDashboardRequested += ShowDashboard;

            _settingsView.DataContext = _settingsViewModel;
            _settingsViewModel.BackRequested += ShowDashboard;

            _userManagementView.DataContext = _userManagementViewModel;
            _userManagementViewModel.BackRequested += ShowDashboard;

            _menuManagementView.DataContext = _menuManagementViewModel;
            _menuManagementViewModel.BackRequested += ShowDashboard;

            _previousOrdersView.DataContext = _previousOrdersViewModel;
            _previousOrdersViewModel.BackRequested += ShowDashboard;
            _previousOrdersViewModel.ViewInvoiceRequested += invoice =>
            {
                _orderInvoiceViewModel.SetOrder(invoice);
                ShowInvoice();
            };

            _reportsView.DataContext = _reportsViewModel;
            _reportsViewModel.BackRequested += ShowDashboard;
            _dashboardViewModel.ReportsRequested += ShowReports;

            // Placeholder handlers for screens not built yet.

            ShowLogin();
        }

        private void ShowLogin()
        {
            RootContent.Children.Clear();
            RootContent.Children.Add(_loginView);
        }

        private void ShowDashboard()
        {
            RootContent.Children.Clear();
            RootContent.Children.Add(_dashboardView);
        }

        private void ShowOrderEntry()
        {
            RootContent.Children.Clear();
            RootContent.Children.Add(_orderEntryView);
        }

        private void ShowExpenses()
        {
            RootContent.Children.Clear();
            RootContent.Children.Add(_expenseView);
        }

        private void ShowInvoice()
        {
            RootContent.Children.Clear();
            RootContent.Children.Add(_orderInvoiceView);
        }

        private void ShowSettings()
        {
            RootContent.Children.Clear();
            RootContent.Children.Add(_settingsView);
        }

        private void ShowUserManagement()
        {
            RootContent.Children.Clear();
            RootContent.Children.Add(_userManagementView);
        }

        private void ShowMenuManagement()
        {
            RootContent.Children.Clear();
            RootContent.Children.Add(_menuManagementView);
        }

        private void ShowPreviousOrders()
        {
            RootContent.Children.Clear();
            RootContent.Children.Add(_previousOrdersView);
        }

        private void ShowReports()
        {
            RootContent.Children.Clear();
            RootContent.Children.Add(_reportsView);
        }
    }
}
