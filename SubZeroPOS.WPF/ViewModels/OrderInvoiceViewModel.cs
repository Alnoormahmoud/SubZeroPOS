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

        [ObservableProperty]
        private string backButtonLabel = "الرئيسية";

        public event Action? NewOrderRequested;
        public event Action? PrintRequested;

        // Whoever navigates here sets this - checkout flow sends back to the
        // dashboard, Previous Orders sends back to itself. Defaults to
        // dashboard if nothing sets it.
        private Action? _backAction;

        public void SetOrder(OrderInvoiceDto invoice, Action? backAction = null, string backLabel = "الرئيسية")
        {
            Order = invoice;
            _backAction = backAction;
            BackButtonLabel = backLabel;
        }

        [RelayCommand]
        private void NewOrder() => NewOrderRequested?.Invoke();

        [RelayCommand]
        private void GoBack() => _backAction?.Invoke();

        [RelayCommand]
        private void Print() => PrintRequested?.Invoke();
    }
}
