using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubZeroPOS.Core.Entities;
using SubZeroPOS.Core.Interfaces;
using SubZeroPOS.WPF.Session;

namespace SubZeroPOS.WPF.ViewModels
{
    public partial class ShiftViewModel : ObservableObject
    {
        private readonly IShiftService _shiftService;
        private ShiftClosing? _openShift;

        public ShiftViewModel(IShiftService shiftService)
        {
            _shiftService = shiftService;
        }

        [ObservableProperty]
        private bool isShiftOpen;
         

        [ObservableProperty]
        private string openingCashText = "0";

        [ObservableProperty]
        private string actualCashText = string.Empty;

        [ObservableProperty]
        private string closeNotes = string.Empty;

        [ObservableProperty]
        private string openedByName = string.Empty;

        [ObservableProperty]
        private string openedAtText = string.Empty;

        [ObservableProperty]
        private string openingCashDisplay = "0";

        [ObservableProperty]
        private string expectedCashDisplay = "0";

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private bool isBusy;

        // Closing summary - shown as a popup right after closing
        [ObservableProperty]
        private bool showClosingSummary;

        [ObservableProperty]
        private string summaryOrderCount = "0";

        [ObservableProperty]
        private string summaryCashOrderCount = "0";

        [ObservableProperty]
        private string summaryBankOrderCount = "0";

        [ObservableProperty]
        private string summaryTotalSales = "0";

        [ObservableProperty]
        private string summaryCashSales = "0";

        [ObservableProperty]
        private string summaryBankSales = "0";

        [ObservableProperty]
        private string summaryExpected = "0";

        [ObservableProperty]
        private string summaryActual = "0";

        [ObservableProperty]
        private string summaryDifference = "0";

        [ObservableProperty]
        private bool summaryHasShortage;

        [ObservableProperty]
        private string summaryOpeningCashDisplay = "0";

        [ObservableProperty]
        private string summaryTotalExpensesDisplay = "0";

        public event Action? BackRequested;

        public async Task InitializeAsync()
        {
            IsBusy = true;
            try
            {
                _openShift = await _shiftService.GetOpenShiftAsync();
                IsShiftOpen = _openShift != null;

                if (_openShift != null)
                {
                    OpenedByName = _openShift.User?.FullName ?? "";
                    OpenedAtText = _openShift.OpenedAt.ToString("yyyy-MM-dd hh:mm");
                    OpeningCashDisplay = _openShift.OpeningCash.ToString("#,##0");

                    var expected = await _shiftService.CalculateExpectedCashAsync(_openShift.ShiftId);
                    ExpectedCashDisplay = expected.ToString("#,##0");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task OpenShiftAsync()
        {
            if (_openShift != null)
            {
                StatusMessage = "الوردية مفتوحة بالفعل";
                return;
            }

            if (!decimal.TryParse(OpeningCashText, out var opening) || opening < 0)
            {
                StatusMessage = "الرجاء إدخال مبلغ افتتاحي صحيح";
                return;
            }

            IsBusy = true;
            try
            {
                await _shiftService.OpenShiftAsync(CurrentSession.UserId, opening);
                StatusMessage = "تم فتح الوردية بنجاح";
                await InitializeAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }
        [RelayCommand]
        private async Task CloseShiftAsync()
        {
            if (_openShift is null)
                return;

            if (!decimal.TryParse(ActualCashText, out var actual) || actual < 0)
            {
                StatusMessage = "الرجاء إدخال المبلغ الفعلي في الدرج";
                return;
            }

            IsBusy = true;

            try
            {
                var closedShiftId = _openShift.ShiftId;

                var (success, error) =
                    await _shiftService.CloseShiftAsync(            
                        closedShiftId,
                        actual,
                        CloseNotes);

                if (!success)
                {
                    StatusMessage = error ?? "حدث خطأ";
                    return;
                }

                // ==========================================
                // Build closing summary
                // ==========================================

                var summary =
                    await _shiftService.GetShiftSummaryAsync(closedShiftId);

                SummaryOrderCount =
                    summary.TotalOrderCount.ToString();

                SummaryCashOrderCount =
                    summary.CashOrderCount.ToString();

                SummaryBankOrderCount =
                    summary.BankOrderCount.ToString();

                SummaryTotalSales =
                    summary.TotalSales.ToString("#,##0");

                SummaryCashSales =
                    summary.CashSales.ToString("#,##0");

                SummaryBankSales =
                    summary.BankSales.ToString("#,##0");

                SummaryExpected =
                    (summary.Shift?.ExpectedCash ?? 0)
                    .ToString("#,##0");

                SummaryActual =
                    (summary.Shift?.ActualCash ?? 0)
                    .ToString("#,##0");

                var difference =
                    (summary.Shift?.ActualCash ?? 0)
                    - (summary.Shift?.ExpectedCash ?? 0);

                SummaryDifference =
                    difference.ToString("#,##0");

                SummaryHasShortage = difference < 0;

                SummaryOpeningCashDisplay = (summary.Shift?.OpeningCash ?? 0).ToString("#,##0");


                SummaryTotalExpensesDisplay =   summary.TotalExpenses.ToString("#,##0");

                // ==========================================
                // IMPORTANT:
                // Immediately clear the current shift
                // ==========================================

                _openShift = null;
                IsShiftOpen = false;

                OpenedByName = string.Empty;
                OpenedAtText = string.Empty;
                OpeningCashDisplay = "0";
                ExpectedCashDisplay = "0";

                ActualCashText = string.Empty;
                CloseNotes = string.Empty;

                StatusMessage = "تم إغلاق الوردية بنجاح";

                // ==========================================
                // Show summary
                // ==========================================

                ShowClosingSummary = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void CloseSummary() => ShowClosingSummary = false;

        [RelayCommand]
        private void Back() => BackRequested?.Invoke();
    }
}