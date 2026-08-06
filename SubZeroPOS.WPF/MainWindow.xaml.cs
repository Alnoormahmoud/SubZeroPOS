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

        public MainWindow(
            LoginView loginView,
            LoginViewModel loginViewModel,
            MainDashboardView dashboardView,
            MainDashboardViewModel dashboardViewModel,
            OrderEntryView orderEntryView,
            OrderEntryViewModel orderEntryViewModel)
        {
            InitializeComponent();

            _loginView = loginView;
            _loginViewModel = loginViewModel;
            _dashboardView = dashboardView;
            _dashboardViewModel = dashboardViewModel;
            _orderEntryView = orderEntryView;
            _orderEntryViewModel = orderEntryViewModel;

            _loginView.DataContext = _loginViewModel;
            _loginViewModel.LoginSucceeded += ShowDashboard;

            _dashboardView.DataContext = _dashboardViewModel;
            _dashboardViewModel.LogoutRequested += ShowLogin;
            _dashboardViewModel.NewOrderRequested += ShowOrderEntry;

            _orderEntryView.DataContext = _orderEntryViewModel;
            _orderEntryView.BackRequested += ShowDashboard;
            _orderEntryViewModel.OrderCompleted += ShowDashboard;

            // Placeholder handlers for screens not built yet.
            _dashboardViewModel.ExpensesRequested += () =>
                MessageBox.Show("شاشة المصروفات - قيد الإنشاء", "قريباً");
            _dashboardViewModel.ReportsRequested += () =>
                MessageBox.Show("شاشة التقارير - قيد الإنشاء", "قريباً");
 

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
    }
}
