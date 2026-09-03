using Microsoft.Extensions.DependencyInjection;
using SubZeroPOS.Core.Interfaces;
using SubZeroPOS.Data.Services;
using SubZeroPOS.WPF.ViewModels;
using SubZeroPOS.WPF.Views;
using System.ComponentModel;
using System.Windows;
 
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
        private readonly ShiftView _shiftView;
        private readonly ShiftViewModel _shiftViewModel;
        private readonly IShiftService _shiftService;
        private readonly IServiceProvider _serviceProvider;
        private readonly MyAccountView _myAccountView;
        private readonly MyAccountViewModel _myAccountViewModel;


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
            ReportsViewModel reportsViewModel,
            ShiftView shiftView,
            ShiftViewModel shiftViewModel,
            IShiftService shiftService,
            IServiceProvider serviceProvider,
            MyAccountView myAccountView,
            MyAccountViewModel myAccountViewModel)
        {
            InitializeComponent();

            Closing += MainWindow_Closing;

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
            _shiftView = shiftView;
            _shiftViewModel = shiftViewModel;
            _shiftService = shiftService;
            _serviceProvider = serviceProvider;
            _myAccountView = myAccountView;
            _myAccountViewModel = myAccountViewModel;

            _loginView.DataContext = _loginViewModel;
            _loginViewModel.LoginSucceeded += async () => await OnLoginSucceededAsync();
            _dashboardView.DataContext = _dashboardViewModel;
            _dashboardViewModel.LogoutRequested += ShowLogin;
            _dashboardViewModel.NewOrderRequested += ShowOrderEntry;
            _dashboardViewModel.ExpensesRequested += ShowExpenses;
            _dashboardViewModel.SettingsRequested += ShowSettings;
            _dashboardViewModel.UserManagementRequested += ShowUserManagement;
            _dashboardViewModel.MenuManagementRequested += ShowMenuManagement;
            _dashboardViewModel.PreviousOrdersRequested += ShowPreviousOrders;
            _dashboardViewModel.ShiftRequested += ShowShift;

   

            _orderEntryView.DataContext = _orderEntryViewModel;
            _orderEntryView.BackRequested += ShowDashboard;
            _orderEntryViewModel.CancelOrderRequested += ShowDashboard;
            _orderEntryViewModel.OrderCompleted += invoice =>
            {
                _orderInvoiceViewModel.SetOrder(invoice, ShowDashboard, "الرئيسية", autoPrint: invoice.AutoPrintReceipt);
                ShowInvoice();
            };


            _expenseView.DataContext = _expenseViewModel;
            _expenseViewModel.BackRequested += ShowDashboard;

            _orderInvoiceView.DataContext = _orderInvoiceViewModel;
            _orderInvoiceViewModel.NewOrderRequested += ShowOrderEntry;

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
                _orderInvoiceViewModel.SetOrder(invoice, ShowPreviousOrders, "رجوع للطلبات السابقة");
                ShowInvoice();
            };

            _reportsView.DataContext = _reportsViewModel;
            _reportsViewModel.BackRequested += ShowDashboard;
            _dashboardViewModel.ReportsRequested += ShowReports;

            _shiftView.DataContext = _shiftViewModel;
            _shiftViewModel.BackRequested += ShowDashboard;

            _myAccountView.DataContext = _myAccountViewModel;
            _myAccountViewModel.BackRequested += ShowDashboard;

            _dashboardViewModel.MyAccountRequested += ShowMyAccount;
            // Placeholder handlers for screens not built yet.

            ShowLogin();
        }

        private void ShowMyAccount()
        {
            RootContent.Children.Clear();

            RootContent.Children.Add(_myAccountView);
        }

        private bool _allowClose;
        private bool _isCheckingClose;

        private async void MainWindow_Closing(
     object? sender,
     CancelEventArgs e)
        {
            if (_allowClose)
                return;

            // Cancel the close immediately.
            e.Cancel = true;

            // Prevent multiple checks if the user clicks X repeatedly.
            if (_isCheckingClose)
                return;

            _isCheckingClose = true;

            try
            {
                var openShift =
                    await _shiftService.GetOpenShiftAsync();

                if (openShift != null)
                {
                    MessageBox.Show(
                        "لا يمكن إغلاق البرنامج قبل إغلاق الوردية الحالية يرجى إغلاق الوردية أولاً.",
                        "وردية مفتوحة",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                // No open shift → allow the window to close.
                _allowClose = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"حدث خطأ أثناء التحقق من الوردية:\n{ex.Message}",
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                _isCheckingClose = false;
            }
        }
        private async System.Threading.Tasks.Task OnLoginSucceededAsync()
        {
            ShowDashboard();

            var openShift = await _shiftService.GetOpenShiftAsync();
            if (openShift == null)
            {

                var result = MessageBox.Show(
                    "لا توجد وردية مفتوحة حالياً. هل تريد فتح وردية الآن؟",
                    "فتح وردية",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    ShowShift();
                    return;
                }
            }

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

        private void ShowShift()
        {
            RootContent.Children.Clear();
            RootContent.Children.Add(_shiftView);
        }
    }
}
