using System;
using System.Windows;
using System.Windows.Controls;
using SubZeroPOS.WPF.ViewModels;

namespace SubZeroPOS.WPF.Views
{
    public partial class OrderEntryView : UserControl
    {
        public event Action? BackRequested;

        public OrderEntryView()
        {
            InitializeComponent();
            Loaded += OrderEntryView_Loaded;
        }

        private async void OrderEntryView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is OrderEntryViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            BackRequested?.Invoke();
        }
    }
}
