using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubZeroPOS.Core.Interfaces;
using SubZeroPOS.WPF.Session;
using System.IO;

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

        [ObservableProperty]
        private bool rememberMe;

        // Stores only the username locally (never the password) so the login
        // screen can pre-fill it next time. One file per Windows user account.
        private static readonly string RememberedUserFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SubZeroPOS", "remembered_user.txt");

        public void LoadRememberedUsername()
        {
            try
            {
                if (File.Exists(RememberedUserFilePath))
                {
                    Username = File.ReadAllText(RememberedUserFilePath).Trim();
                    RememberMe = true;
                }
            }
            catch
            {
                // Non-critical - just skip pre-filling if anything goes wrong.
            }
        }

        private void SaveOrClearRememberedUsername()
        {
            try
            {
                if (RememberMe)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(RememberedUserFilePath)!);
                    File.WriteAllText(RememberedUserFilePath, Username);
                }
                else if (File.Exists(RememberedUserFilePath))
                {
                    File.Delete(RememberedUserFilePath);
                }
            }
            catch
            {
                // Non-critical.
            }
        }

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

                SaveOrClearRememberedUsername();

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
