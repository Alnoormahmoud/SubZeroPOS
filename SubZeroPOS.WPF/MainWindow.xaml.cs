using System.Windows;
using SubZeroPOS.WPF.ViewModels;
using SubZeroPOS.WPF.Views;

namespace SubZeroPOS.WPF
{
    public partial class MainWindow : Window
    {
        private readonly LoginView _loginView;
        private readonly LoginViewModel _loginViewModel;

        public MainWindow(LoginView loginView, LoginViewModel loginViewModel)
        {
            InitializeComponent();

            _loginView = loginView;
            _loginViewModel = loginViewModel;

            _loginView.DataContext = _loginViewModel;
            _loginViewModel.LoginSucceeded += OnLoginSucceeded;

            RootContent.Children.Add(_loginView);
        }

        private void OnLoginSucceeded()
        {
            // TODO: replace with the real MainDashboardView once it's built.
            // For now, this confirms the full pipeline works:
            // TextBox -> LoginViewModel -> AuthService -> SubZeroDbContext -> SQL Server -> back.
            MessageBox.Show(
                $"تم تسجيل الدخول بنجاح\nالمستخدم: {Session.CurrentSession.FullName}\nالصلاحية: {Session.CurrentSession.RoleName}",
                "نجاح",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
