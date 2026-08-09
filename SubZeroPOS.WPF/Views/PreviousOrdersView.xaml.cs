using System.Windows;
using System.Windows.Controls;
using SubZeroPOS.WPF.ViewModels;

namespace SubZeroPOS.WPF.Views
{
    public partial class PreviousOrdersView : UserControl
    {
        public PreviousOrdersView()
        {
            InitializeComponent();
            Loaded += PreviousOrdersView_Loaded;
        }

        private async void PreviousOrdersView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PreviousOrdersViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }
    }
}
