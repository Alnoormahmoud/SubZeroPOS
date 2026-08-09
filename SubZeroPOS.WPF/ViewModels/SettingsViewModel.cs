using System;
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
        }

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
                await _settingsService.SaveSettingsAsync(new Core.Entities.RestaurantSettings
                {
                    SettingsId = _settingsId,
                    RestaurantName = RestaurantName,
                    Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone,
                    Address = string.IsNullOrWhiteSpace(Address) ? null : Address,
                    InvoiceFooterPrimary = string.IsNullOrWhiteSpace(InvoiceFooterPrimary) ? null : InvoiceFooterPrimary,
                    InvoiceFooterSecondary = string.IsNullOrWhiteSpace(InvoiceFooterSecondary) ? null : InvoiceFooterSecondary
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
