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

        [ObservableProperty]
        private bool shouldAutoPrint;

        public event Action? NewOrderRequested;
        public event Action? PrintRequested;
        public event Action? AutomaticPrintRequested;

        private Action? _backAction;

        public void SetOrder(
            OrderInvoiceDto invoice,
            Action? backAction = null,
            string backLabel = "الرئيسية",
            bool autoPrint = false)
        {
            Order = invoice;

            _backAction = backAction;

            BackButtonLabel = backLabel;

            ShouldAutoPrint = autoPrint;
        }

        [RelayCommand]
        private void NewOrder()
        {
            NewOrderRequested?.Invoke();
        }

        [RelayCommand]
        private void GoBack()
        {
            _backAction?.Invoke();
        }

        // Manual printing.
        [RelayCommand]
        private void Print()
        {
            if (Order is null)
                return;

            PrintRequested?.Invoke();
        }

        // Automatic printing after confirming a new order.
        public void RequestAutomaticPrint()
        {
            if (Order is null)
                return;
            // Run only if at least one output option is enabled.
            if (!ShouldAutoPrint && !Order!.OpenPdfAfterPrinting)
                return;

            AutomaticPrintRequested?.Invoke();

            // Prevent printing twice.
            ShouldAutoPrint = false;
         }
    }
}