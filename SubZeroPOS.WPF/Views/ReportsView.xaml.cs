using System.Windows;
using System.Windows.Controls;
using SubZeroPOS.WPF.ViewModels;

namespace SubZeroPOS.WPF.Views
{
    public partial class ReportsView : UserControl
    {
        public ReportsView()
        {
            InitializeComponent();
            Loaded += ReportsView_Loaded;
        }

        private async void ReportsView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ReportsViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }
    }
}
