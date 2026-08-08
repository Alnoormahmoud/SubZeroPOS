using System.Windows;
using System.Windows.Controls;
using SubZeroPOS.WPF.ViewModels;

namespace SubZeroPOS.WPF.Views
{
    public partial class ExpenseView : UserControl
    {
        public ExpenseView()
        {
            InitializeComponent();
            Loaded += ExpenseView_Loaded;
        }

        private async void ExpenseView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ExpenseViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }
    }
}
