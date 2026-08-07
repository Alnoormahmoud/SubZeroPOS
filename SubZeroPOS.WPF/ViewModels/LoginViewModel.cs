using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubZeroPOS.Core.Interfaces;
using SubZeroPOS.WPF.Session;

namespace SubZeroPOS.WPF.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool isBusy;

        // Raised when login succeeds - MainWindow subscribes to this to switch screens
        public event Action? LoginSucceeded;

        [RelayCommand]
        private async Task LoginAsync(string password)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "الرجاء إدخال اسم المستخدم وكلمة المرور";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var result = await _authService.LoginAsync(Username, password);

                if (!result.Success)
                {
                    ErrorMessage = result.ErrorMessage ?? "فشل تسجيل الدخول";
                    return;
                }

                CurrentSession.UserId = result.UserId;
                CurrentSession.FullName = result.FullName;
                CurrentSession.RoleCode = result.RoleCode;
                CurrentSession.RoleNameAr = result.RoleNameAr;

                LoginSucceeded?.Invoke();
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
