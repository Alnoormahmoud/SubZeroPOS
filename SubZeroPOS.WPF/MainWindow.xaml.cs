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

        public MainWindow(
            LoginView loginView,
            LoginViewModel loginViewModel,
            MainDashboardView dashboardView,
            MainDashboardViewModel dashboardViewModel)
        {
            InitializeComponent();

            _loginView = loginView;
            _loginViewModel = loginViewModel;
            _dashboardView = dashboardView;
            _dashboardViewModel = dashboardViewModel;

            _loginView.DataContext = _loginViewModel;
            _loginViewModel.LoginSucceeded += ShowDashboard;

            _dashboardView.DataContext = _dashboardViewModel;
            _dashboardViewModel.LogoutRequested += ShowLogin;

            // Placeholder handlers for nav tiles until those screens are built.
            _dashboardViewModel.NewOrderRequested += () =>
                MessageBox.Show("شاشة الطلب الجديد - قيد الإنشاء", "قريباً");
            _dashboardViewModel.ExpensesRequested += () =>
                MessageBox.Show("شاشة المصروفات - قيد الإنشاء", "قريباً");
            _dashboardViewModel.ReportsRequested += () =>
                MessageBox.Show("شاشة التقارير - قيد الإنشاء", "قريباً");
            _dashboardViewModel.ShiftRequested += () =>
                MessageBox.Show("شاشة الوردية - قيد الإنشاء", "قريباً");

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
    }
}
