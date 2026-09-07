using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SubZeroPOS.Core.Entities;
using SubZeroPOS.Core.Interfaces;
using SubZeroPOS.Data.Services;
using System;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SubZeroPOS.WPF.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IRestaurantSettingsService _settingsService;
        private readonly IBackupService _backupService;

        private int _settingsId;


        public SettingsViewModel(IRestaurantSettingsService settingsService, IBackupService backupService)
        {
            _settingsService = settingsService;
            _backupService = backupService;


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
            // 12-hour format with Arabic AM/PM indicator
            // ==========================================

            DateFormats =
                new ObservableCollection<DateFormatOption>
                {
                    new()
                    {
                        FormatString = "yyyy-MM-dd hh:mm tt"
                    },

                    new()
                    {
                        FormatString = "dd/MM/yyyy hh:mm tt"
                    },

                    new()
                    {
                        FormatString = "dd-MM-yyyy hh:mm tt"
                    },

                    new()
                    {
                        FormatString = "MMM dd, yyyy hh:mm tt"
                    },

                    new()
                    {
                        FormatString = "yyyy/MM/dd hh:mm tt"
                    },

                    new()
                    {
                        FormatString = "MM/dd/yyyy hh:mm tt"
                    },

                    new()
                    {
                        FormatString = "dd MMMM yyyy - hh:mm tt"
                    },

                    new()
                    {
                        FormatString = "hh:mm tt - dd/MM/yyyy"
                    },

                    new()
                    {
                        FormatString = "hh:mm tt - dd-MM-yyyy"
                    },

                    new()
                    {
                        FormatString = "dd MMMM yyyy - hh:mm tt"
                    },

                    // Date only
                    new()
                    {
                        FormatString = "yyyy-MM-dd"
                    },

                    new()
                    {
                        FormatString = "dd/MM/yyyy"
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



            // ==========================================
            // receipt copy count options
            // ==========================================
            ReceiptCopyOptions =
                new ObservableCollection<int>
                {
                    1, 2, 3, 4, 5
                };

            // ==========================================
            // Load installed printers
            // ==========================================
            LoadPrinters();
        }
        public ObservableCollection<ThemeOption> Themes { get; } =
            new()
            {
        new ThemeOption
        {
            Value = ThemeManager.SubZeroDark,
            DisplayName = "الوضع الداكن"
        },

        new ThemeOption
        {
            Value = ThemeManager.Light,
            DisplayName = "الوضع الفاتح"
        },

        new ThemeOption
        {
            Value = ThemeManager.BlueDark,
            DisplayName = "الأزرق الداكن"
        }
            };

        [ObservableProperty]
        private ThemeOption? selectedTheme;
 

        // ==============================================
        // Collections
        // ==============================================

        public ObservableCollection<CurrencyOption>
            Currencies
        {
            get;
        }

        // ==============================================
        // Receipt copy count options
        // ==============================================
        public ObservableCollection<int>
            ReceiptCopyOptions
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

        public ObservableCollection<string> Printers { get; } = new();

        private void LoadPrinters()
        {
            Printers.Clear();

            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                Printers.Add(printer);
            }
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

        [ObservableProperty]
        private string backupStatusMessage = string.Empty;

        [ObservableProperty]
        private bool isBackupRunning;

        [ObservableProperty]
        private int selectedReceiptCopyCount = 1;

        [ObservableProperty]
        private string selectedPrinter = string.Empty;

        [ObservableProperty]
        private bool weeklyBackupEnabled;

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


                // ==========================================
                // Restaurant information
                // ==========================================

                RestaurantName =
                    settings.RestaurantName;

                Phone =
                    settings.Phone
                    ?? string.Empty;

                Address =
                    settings.Address
                    ?? string.Empty;


                // ==========================================
                // Invoice footer
                // ==========================================

                InvoiceFooterPrimary =
                    settings.InvoiceFooterPrimary
                    ?? string.Empty;

                InvoiceFooterSecondary =
                    settings.InvoiceFooterSecondary
                    ?? string.Empty;


                // ==========================================
                // Currency
                // ==========================================

                SelectedCurrency =
                    Currencies.FirstOrDefault(
                        c => c.Code == settings.CurrencyCode)
                    ?? Currencies[0];


                // ==========================================
                // Date format
                // ==========================================

                SelectedDateFormat =
                    DateFormats.FirstOrDefault(
                        d => d.FormatString
                             == settings.DateFormat)
                    ?? DateFormats[0];


                // ==========================================
                // Invoice display
                // ==========================================

                ShowCashierNameOnInvoice =
                    settings.ShowCashierNameOnInvoice;

                ShowLogoOnInvoice =
                    settings.ShowLogoOnInvoice;

                ShowOrderNumberOnInvoice =
                    settings.ShowOrderNumberOnInvoice;

                ShowCustomerNameOnInvoice =
                    settings.ShowCustomerNameOnInvoice;


                // ==========================================
                // Paper size
                // ==========================================

                SelectedReceiptPaperSize =
                    ReceiptPaperSizes.FirstOrDefault(
                        p => p.WidthMillimeters
                             == settings.ReceiptPaperWidthMm)
                    ?? ReceiptPaperSizes.Last();


                // ==========================================
                // Printing behavior
                // ==========================================

                AutoPrintReceipt =
                    settings.AutoPrintReceipt;

                OpenPdfAfterPrinting =
                    settings.OpenPdfAfterPrinting;

                SelectedReceiptCopyCount =
                    settings.ReceiptCopies;


                SelectedPrinter =
    Printers.FirstOrDefault(
        p => p == settings.PrinterName)
    ?? Printers.FirstOrDefault()
    ?? string.Empty;

                SelectedTheme =
                Themes.FirstOrDefault(t => t.Value == settings.Theme)
                ?? Themes.First();

                weeklyBackupEnabled =
    settings.WeeklyBackupEnabled;

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


                        // ==========================================
                        // Restaurant information
                        // ==========================================

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


                        // ==========================================
                        // Invoice footer
                        // ==========================================

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


                        // ==========================================
                        // Currency
                        // ==========================================

                        CurrencyCode =
                            SelectedCurrency?.Code
                            ?? "SDG",

                        CurrencySymbol =
                            SelectedCurrency?.Symbol
                            ?? "ج.س",


                        // ==========================================
                        // Date format
                        // ==========================================

                        DateFormat =
                            SelectedDateFormat?.FormatString
                            ?? "yyyy-MM-dd hh:mm tt",


                        // ==========================================
                        // Invoice display
                        // ==========================================

                        ShowCashierNameOnInvoice =
                            ShowCashierNameOnInvoice,

                        ShowLogoOnInvoice =
                            ShowLogoOnInvoice,

                        ShowOrderNumberOnInvoice =
                            ShowOrderNumberOnInvoice,

                        ShowCustomerNameOnInvoice =
                            ShowCustomerNameOnInvoice,


                        // ==========================================
                        // Paper size
                        // ==========================================

                        ReceiptPaperWidthMm =
                            SelectedReceiptPaperSize
                                ?.WidthMillimeters
                            ?? 80,


                        // ==========================================
                        // Printing behavior
                        // ==========================================

                        AutoPrintReceipt =
                            AutoPrintReceipt,

                        OpenPdfAfterPrinting =
                            OpenPdfAfterPrinting,

                        ReceiptCopies =
                            SelectedReceiptCopyCount,

                        PrinterName =
                            SelectedPrinter
                            ?? string.Empty,

                        Theme =
    SelectedTheme?.Value
    ?? ThemeManager.SubZeroDark


                    });
                ThemeManager.ApplyTheme(
    SelectedTheme?.Value
    ?? ThemeManager.SubZeroDark);

                WeeklyBackupEnabled =    weeklyBackupEnabled;

                SubZeroPOS.WPF.Session.CurrencyHolder.Symbol = SelectedCurrency?.Symbol ?? "ج.س";

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

         [RelayCommand]
        private async Task BackupNowAsync()
        {
            try
            {
                IsBackupRunning = true;

                BackupStatusMessage =
                    "جاري إنشاء النسخة الاحتياطية...";

                Directory.CreateDirectory(BackupDirectory);

                var timestamp =
                    DateTime.Now.ToString("yyyy-MM-dd_HHmmss");

                var backupPath =
                    Path.Combine(
                        BackupDirectory,
                        $"SubZeroPOS_{timestamp}.bak");

                var result =
                    await _backupService.CreateBackupAsync(
                        backupPath);

                BackupStatusMessage =
                    result.Message;

                if (result.Success)
                {
                    MessageBox.Show(
                        result.Message,
                        "النسخ الاحتياطي",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        result.Message,
                        "خطأ",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                BackupStatusMessage =
                    $"فشل النسخ الاحتياطي: {ex.Message}";

                MessageBox.Show(
                    BackupStatusMessage,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsBackupRunning = false;
            }
        }
        [RelayCommand]
        private async Task RestoreBackupAsync()
        {
            var openDialog = new OpenFileDialog
            {
                Title = "اختيار النسخة الاحتياطية",
                Filter = "Backup Files (*.bak)|*.bak",
                Multiselect = false
            };

            if (openDialog.ShowDialog() != true)
                return;

            var result = MessageBox.Show(
                "تحذير!\n\n" +
                "استعادة النسخة الاحتياطية ستستبدل البيانات الحالية " +
                "بالبيانات الموجودة في النسخة الاحتياطية.\n\n" +
                "هل تريد المتابعة؟",
                "تأكيد الاستعادة",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                IsBackupRunning = true;

                BackupStatusMessage =
                    "جاري استعادة قاعدة البيانات...";

                var backupDirectory = BackupDirectory;

                Directory.CreateDirectory(backupDirectory);

                var temporaryBackupPath =
                    Path.Combine(
                        backupDirectory,
                        "RestoreTemp.bak");

                File.Copy(
                    openDialog.FileName,
                    temporaryBackupPath,
                    true);

                var restoreResult =
                    await _backupService.RestoreBackupAsync(
                        temporaryBackupPath);

                BackupStatusMessage =
                    restoreResult.Message;

                if (restoreResult.Success)
                {
                    MessageBox.Show(
                        restoreResult.Message,
                        "استعادة قاعدة البيانات",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        restoreResult.Message,
                        "خطأ",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }

                try
                {
                    File.Delete(temporaryBackupPath);
                }
                catch
                {
                    // Ignore cleanup failure.
                }
            }
            catch (Exception ex)
            {
                BackupStatusMessage =
                    $"فشل الاستعادة: {ex.Message}";

                MessageBox.Show(
                    BackupStatusMessage,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsBackupRunning = false;
            }
        }

        private static string BackupDirectory =>
    Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.CommonApplicationData),
        "SubZeroPOS",
        "Backups");
    }
}