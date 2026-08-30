using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubZeroPOS.Core.Entities;
using SubZeroPOS.Core.Interfaces;

namespace SubZeroPOS.WPF.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IRestaurantSettingsService _settingsService;

        private int _settingsId;


        public SettingsViewModel(
            IRestaurantSettingsService settingsService)
        {
            _settingsService = settingsService;


            // ==========================================
            // Currency options
            // ==========================================

            Currencies =
                new ObservableCollection<CurrencyOption>
                {
                    new()
                    {
                        Code = "SDG",
                        Symbol = "ج.س"
                    },

                    new()
                    {
                        Code = "USD",
                        Symbol = "$"
                    },

                    new()
                    {
                        Code = "SAR",
                        Symbol = "ر.س"
                    },

                    new()
                    {
                        Code = "EGP",
                        Symbol = "ج.م"
                    },

                    new()
                    {
                        Code = "AED",
                        Symbol = "د.إ"
                    }
                };


            // ==========================================
            // Date formats
            // ==========================================

            DateFormats =
                new ObservableCollection<DateFormatOption>
                {
                    new()
                    {
                        FormatString = "yyyy-MM-dd HH:mm"
                    },

                    new()
                    {
                        FormatString = "dd/MM/yyyy hh:mm"
                    },

                    new()
                    {
                        FormatString = "dd-MM-yyyy hh:mm tt"
                    },

                    new()
                    {
                        FormatString = "MMM dd, yyyy HH:mm"
                    },

                    new()
                    {
                        FormatString = "yyyy/MM/dd hh:mm"
                    },

                    new()
                    {
                        FormatString = "dd/MM/yyyy hh:mm tt"
                    },

                    new()
                    {
                        FormatString = "MM/dd/yyyy hh:mm"
                    },

                    new()
                    {
                        FormatString = "dd MMMM yyyy - hh:mm"
                    },

                    new()
                    {
                        FormatString = "yyyy-MM-dd"
                    },

                    new()
                    {
                        FormatString = "dd/MM/yyyy"
                    },

                    new()
                    {
                        FormatString = "hh:mm - dd/MM/yyyy"
                    }
                };


            // ==========================================
            // Receipt paper sizes
            // ==========================================

            ReceiptPaperSizes =
                new ObservableCollection<ReceiptPaperSizeOption>
                {
                    new()
                    {
                        DisplayName = "58 mm",
                        WidthMillimeters = 58
                    },

                    new()
                    {
                        DisplayName = "80 mm",
                        WidthMillimeters = 80
                    }
                };
        }


        // ==============================================
        // Collections
        // ==============================================

        public ObservableCollection<CurrencyOption>
            Currencies
        {
            get;
        }


        public ObservableCollection<DateFormatOption>
            DateFormats
        {
            get;
        }


        public ObservableCollection<ReceiptPaperSizeOption>
            ReceiptPaperSizes
        {
            get;
        }


        // ==============================================
        // Restaurant information
        // ==============================================

        [ObservableProperty]
        private string restaurantName = string.Empty;


        [ObservableProperty]
        private string phone = string.Empty;


        [ObservableProperty]
        private string address = string.Empty;


        // ==============================================
        // Invoice footer
        // ==============================================

        [ObservableProperty]
        private string invoiceFooterPrimary = string.Empty;


        [ObservableProperty]
        private string invoiceFooterSecondary = string.Empty;


        // ==============================================
        // Currency
        // ==============================================

        [ObservableProperty]
        private CurrencyOption? selectedCurrency;


        // ==============================================
        // Date format
        // ==============================================

        [ObservableProperty]
        private DateFormatOption? selectedDateFormat;


        // ==============================================
        // Paper size
        // ==============================================

        [ObservableProperty]
        private ReceiptPaperSizeOption?
            selectedReceiptPaperSize;


        // ==============================================
        // Invoice display settings
        // ==============================================

        [ObservableProperty]
        private bool showCashierNameOnInvoice = true;


        [ObservableProperty]
        private bool showLogoOnInvoice = true;


        [ObservableProperty]
        private bool showOrderNumberOnInvoice = true;


        [ObservableProperty]
        private bool showCustomerNameOnInvoice = true;


        // ==============================================
        // Printing behavior
        // ==============================================

        [ObservableProperty]
        private bool autoPrintReceipt;


        [ObservableProperty]
        private bool openPdfAfterPrinting;


        // ==============================================
        // UI state
        // ==============================================

        [ObservableProperty]
        private string statusMessage = string.Empty;


        [ObservableProperty]
        private bool isBusy;


        // ==============================================
        // Navigation event
        // ==============================================

        public event Action? BackRequested;


        // ==============================================
        // Load settings
        // ==============================================

        public async Task InitializeAsync()
        {
            IsBusy = true;

            try
            {
                var settings =
                    await _settingsService.GetSettingsAsync();


                _settingsId =
                    settings.SettingsId;


                // Restaurant information
                RestaurantName =
                    settings.RestaurantName;

                Phone =
                    settings.Phone
                    ?? string.Empty;

                Address =
                    settings.Address
                    ?? string.Empty;


                // Invoice footer
                InvoiceFooterPrimary =
                    settings.InvoiceFooterPrimary
                    ?? string.Empty;

                InvoiceFooterSecondary =
                    settings.InvoiceFooterSecondary
                    ?? string.Empty;


                // Currency
                SelectedCurrency =
                    Currencies.FirstOrDefault(
                        c => c.Code == settings.CurrencyCode)
                    ?? Currencies[0];


                // Date format
                SelectedDateFormat =
                    DateFormats.FirstOrDefault(
                        d => d.FormatString
                             == settings.DateFormat)
                    ?? DateFormats[0];


                // Invoice display
                ShowCashierNameOnInvoice =
                    settings.ShowCashierNameOnInvoice;

                ShowLogoOnInvoice =
                    settings.ShowLogoOnInvoice;

                ShowOrderNumberOnInvoice =
                    settings.ShowOrderNumberOnInvoice;

                ShowCustomerNameOnInvoice =
                    settings.ShowCustomerNameOnInvoice;


                // Paper size
                SelectedReceiptPaperSize =
                    ReceiptPaperSizes.FirstOrDefault(
                        p => p.WidthMillimeters
                             == settings.ReceiptPaperWidthMm)
                    ?? ReceiptPaperSizes.Last();


                // Printing behavior
                AutoPrintReceipt =
                    settings.AutoPrintReceipt;

                OpenPdfAfterPrinting =
                    settings.OpenPdfAfterPrinting;


                StatusMessage = string.Empty;
            }
            catch (Exception ex)
            {
                StatusMessage =
                    $"حدث خطأ أثناء تحميل الإعدادات: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }


        // ==============================================
        // Save settings
        // ==============================================

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(RestaurantName))
            {
                StatusMessage =
                    "الرجاء إدخال اسم المطعم";

                return;
            }


            IsBusy = true;


            try
            {
                await _settingsService.SaveSettingsAsync(
                    new RestaurantSettings
                    {
                        SettingsId = _settingsId,


                        // Restaurant information
                        RestaurantName =
                            RestaurantName.Trim(),

                        Phone =
                            string.IsNullOrWhiteSpace(Phone)
                                ? null
                                : Phone.Trim(),

                        Address =
                            string.IsNullOrWhiteSpace(Address)
                                ? null
                                : Address.Trim(),


                        // Invoice footer
                        InvoiceFooterPrimary =
                            string.IsNullOrWhiteSpace(
                                InvoiceFooterPrimary)
                                ? null
                                : InvoiceFooterPrimary.Trim(),

                        InvoiceFooterSecondary =
                            string.IsNullOrWhiteSpace(
                                InvoiceFooterSecondary)
                                ? null
                                : InvoiceFooterSecondary.Trim(),


                        // Currency
                        CurrencyCode =
                            SelectedCurrency?.Code
                            ?? "SDG",

                        CurrencySymbol =
                            SelectedCurrency?.Symbol
                            ?? "ج.س",


                        // Date format
                        DateFormat =
                            SelectedDateFormat?.FormatString
                            ?? "yyyy-MM-dd HH:mm",


                        // Invoice display
                        ShowCashierNameOnInvoice =
                            ShowCashierNameOnInvoice,

                        ShowLogoOnInvoice =
                            ShowLogoOnInvoice,

                        ShowOrderNumberOnInvoice =
                            ShowOrderNumberOnInvoice,

                        ShowCustomerNameOnInvoice =
                            ShowCustomerNameOnInvoice,


                        // Paper size
                        ReceiptPaperWidthMm =
                            SelectedReceiptPaperSize
                                ?.WidthMillimeters
                            ?? 80,


                        // Printing behavior
                        AutoPrintReceipt =
                            AutoPrintReceipt,

                        OpenPdfAfterPrinting =
                            OpenPdfAfterPrinting
                    });


                StatusMessage =
                    "تم حفظ الإعدادات بنجاح";
            }
            catch (Exception ex)
            {
                StatusMessage =
                    $"حدث خطأ: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }


        // ==============================================
        // Back navigation
        // ==============================================

        [RelayCommand]
        private void Back()
        {
            BackRequested?.Invoke();
        }
    }
}