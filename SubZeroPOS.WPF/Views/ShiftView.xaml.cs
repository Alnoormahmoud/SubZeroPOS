using System.Windows;
using System.Windows.Controls;
using SubZeroPOS.WPF.ViewModels;

namespace SubZeroPOS.WPF.Views
{
    public partial class ShiftView : UserControl
    {
        public ShiftView()
        {
            InitializeComponent();
            Loaded += ShiftView_Loaded;
        }

        private async void ShiftView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ShiftViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }
    }
}