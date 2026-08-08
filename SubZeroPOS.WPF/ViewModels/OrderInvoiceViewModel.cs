using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubZeroPOS.Core.DTOs;

namespace SubZeroPOS.WPF.ViewModels
{
    public partial class OrderInvoiceViewModel : ObservableObject
    {
        [ObservableProperty]
        private OrderInvoiceDto? order;

        public event Action? NewOrderRequested;
        public event Action? BackToDashboardRequested;
        public event Action? PrintRequested;

        public void SetOrder(OrderInvoiceDto invoice)
        {
            Order = invoice;
        }

        [RelayCommand]
        private void NewOrder() => NewOrderRequested?.Invoke();

        [RelayCommand]
        private void BackToDashboard() => BackToDashboardRequested?.Invoke();

        [RelayCommand]
        private void Print() => PrintRequested?.Invoke();
    }
}
