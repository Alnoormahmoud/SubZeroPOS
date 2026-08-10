using System;
using System.Collections.ObjectModel;
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

        public SettingsViewModel(IRestaurantSettingsService settingsService)
        {
            _settingsService = settingsService;

            Currencies = new ObservableCollection<CurrencyOption>
            {
                new() { Code = "SDG", Symbol = "ج.س" }, // Sudanese Pound - default
                new() { Code = "USD", Symbol = "$" },
                new() { Code = "SAR", Symbol = "ر.س" },
                new() { Code = "EGP", Symbol = "ج.م" },
                new() { Code = "AED", Symbol = "د.إ" }
            };
        }

        public ObservableCollection<CurrencyOption> Currencies { get; }

        [ObservableProperty]
        private string restaurantName = string.Empty;

        [ObservableProperty]
        private string phone = string.Empty;

        [ObservableProperty]
        private string address = string.Empty;

        [ObservableProperty]
        private string invoiceFooterPrimary = string.Empty;

        [ObservableProperty]
        private string invoiceFooterSecondary = string.Empty;

        [ObservableProperty]
        private string currencySymbol = string.Empty;

        [ObservableProperty]
        private CurrencyOption? selectedCurrency;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private bool isBusy;

        public event Action? BackRequested;

        public async Task InitializeAsync()
        {
            IsBusy = true;
            try
            {
                var settings = await _settingsService.GetSettingsAsync();
                _settingsId = settings.SettingsId;
                RestaurantName = settings.RestaurantName;
                Phone = settings.Phone ?? string.Empty;
                Address = settings.Address ?? string.Empty;
                InvoiceFooterPrimary = settings.InvoiceFooterPrimary ?? string.Empty;
                InvoiceFooterSecondary = settings.InvoiceFooterSecondary ?? string.Empty;
                CurrencySymbol = settings.CurrencySymbol;

                SelectedCurrency = Currencies.Count > 0
                    ? (System.Linq.Enumerable.FirstOrDefault(Currencies, c => c.Code == settings.CurrencyCode) ?? Currencies[0])
                    : null;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(RestaurantName))
            {
                StatusMessage = "الرجاء إدخال اسم المطعم";
                return;
            }

            IsBusy = true;
            try
            {
                await _settingsService.SaveSettingsAsync(new RestaurantSettings
                {
                    SettingsId = _settingsId,
                    RestaurantName = RestaurantName,
                    Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone,
                    Address = string.IsNullOrWhiteSpace(Address) ? null : Address,
                    InvoiceFooterPrimary = string.IsNullOrWhiteSpace(InvoiceFooterPrimary) ? null : InvoiceFooterPrimary,
                    InvoiceFooterSecondary = string.IsNullOrWhiteSpace(InvoiceFooterSecondary) ? null : InvoiceFooterSecondary,
                    CurrencyCode = SelectedCurrency?.Code ?? "SDG",
                    CurrencySymbol = SelectedCurrency?.Symbol ?? "ج.س"
                });

                StatusMessage = "تم حفظ الإعدادات بنجاح";
            }
            catch (Exception ex)
            {
                StatusMessage = $"حدث خطأ: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void Back() => BackRequested?.Invoke();
    }
}
