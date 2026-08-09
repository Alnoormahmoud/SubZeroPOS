using System.Windows;
using System.Windows.Controls;
using SubZeroPOS.WPF.ViewModels;

namespace SubZeroPOS.WPF.Views
{
    public partial class MenuManagementView : UserControl
    {
        public MenuManagementView()
        {
            InitializeComponent();
            Loaded += MenuManagementView_Loaded;
        }

        private async void MenuManagementView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MenuManagementViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }
    }
}
