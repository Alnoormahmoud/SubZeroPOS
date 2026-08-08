using System.Windows;
using System.Windows.Controls;
using SubZeroPOS.WPF.ViewModels;

namespace SubZeroPOS.WPF.Views
{
    public partial class MainDashboardView : UserControl
    {
        public MainDashboardView()
        {
            InitializeComponent();
            Loaded += MainDashboardView_Loaded;
        }

        private async void MainDashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            // Loaded fires every time this view is re-added to the visual tree
            // (e.g. navigating back from Order Entry or Expenses), so the stat
            // cards always reflect the latest data rather than staying frozen
            // at whatever they showed on first login.
            if (DataContext is MainDashboardViewModel vm)
            {
                await vm.RefreshStatsAsync();
            }
        }
    }
}
