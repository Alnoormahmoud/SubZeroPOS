using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubZeroPOS.Core.Entities;
using SubZeroPOS.Core.Interfaces;
using SubZeroPOS.WPF.Session;

namespace SubZeroPOS.WPF.ViewModels
{
    public partial class ExpenseViewModel : ObservableObject
    {
        private readonly IExpenseService _expenseService;

        public ExpenseViewModel(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        public ObservableCollection<ExpenseCategory> Categories { get; } = new();
        public ObservableCollection<Expense> RecentExpenses { get; } = new();

        [ObservableProperty]
        private ExpenseCategory? selectedCategory;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private string amountText = string.Empty;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private decimal todaysTotal;

        public event Action? BackRequested;

        public async Task InitializeAsync()
        {
            IsBusy = true;
            try
            {
                Categories.Clear();
                var categories = await _expenseService.GetExpenseCategoriesAsync();
                foreach (var c in categories)
                    Categories.Add(c);

                if (Categories.Count > 0)
                    SelectedCategory = Categories.First();

                await LoadTodaysExpensesAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadTodaysExpensesAsync()
        {
            var today = DateTime.Today;
            var expenses = await _expenseService.GetExpensesByDateRangeAsync(today, today);

            RecentExpenses.Clear();
            foreach (var e in expenses)
                RecentExpenses.Add(e);

            TodaysTotal = expenses.Sum(e => e.Amount);
        }

        [RelayCommand]
        private async Task SaveExpenseAsync()
        {
            StatusMessage = string.Empty;

            if (SelectedCategory is null)
            {
                StatusMessage = "الرجاء اختيار تصنيف المصروف";
                return;
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                StatusMessage = "الرجاء إدخال وصف المصروف";
                return;
            }

            if (!decimal.TryParse(AmountText, out var amount) || amount <= 0)
            {
                StatusMessage = "الرجاء إدخال مبلغ صحيح";
                return;
            }

            IsBusy = true;
            try
            {
                var expense = new Expense
                {
                    ExpenseCategoryId = SelectedCategory.ExpenseCategoryId,
                    Description = Description,
                    Amount = amount,
                    ExpenseDate = DateTime.Now,
                    EnteredByUserId = CurrentSession.UserId
                };

                await _expenseService.AddExpenseAsync(expense);

                Description = string.Empty;
                AmountText = string.Empty;
                StatusMessage = "تم حفظ المصروف بنجاح";

                await LoadTodaysExpensesAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"حدث خطأ أثناء الحفظ: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void Back()
        {
            BackRequested?.Invoke();
        }
    }
}
