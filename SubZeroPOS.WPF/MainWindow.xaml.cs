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
            OrderInvoiceViewModel orderInvoiceViewModel)
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

            _loginView.DataContext = _loginViewModel;
            _loginViewModel.LoginSucceeded += ShowDashboard;

            _dashboardView.DataContext = _dashboardViewModel;
            _dashboardViewModel.LogoutRequested += ShowLogin;
            _dashboardViewModel.NewOrderRequested += ShowOrderEntry;
            _dashboardViewModel.ExpensesRequested += ShowExpenses;

            _orderEntryView.DataContext = _orderEntryViewModel;
            _orderEntryView.BackRequested += ShowDashboard;
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

            // Placeholder handlers for screens not built yet.
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
    }
}
