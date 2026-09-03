using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubZeroPOS.Core.Interfaces;
using SubZeroPOS.WPF.Session;

namespace SubZeroPOS.WPF.ViewModels
{
    public partial class MyAccountViewModel : ObservableObject
    {
        private readonly IUserService _userService;


        public MyAccountViewModel(
            IUserService userService)
        {
            _userService = userService;
        }


        // ==========================================
        // Account information
        // ==========================================

        [ObservableProperty]
        private string fullName = string.Empty;


        [ObservableProperty]
        private string username = string.Empty;


        [ObservableProperty]
        private string roleName = string.Empty;


        // ==========================================
        // Password
        // ==========================================

        [ObservableProperty]
        private string currentPassword =
            string.Empty;


        [ObservableProperty]
        private string newPassword =
            string.Empty;


        [ObservableProperty]
        private string confirmPassword =
            string.Empty;


        // ==========================================
        // Status
        // ==========================================

        [ObservableProperty]
        private string statusMessage =
            string.Empty;


        [ObservableProperty]
        private bool isBusy;


        public event Action? BackRequested;
        public event Action? PasswordChangedSuccessfully;

        // ==========================================
        // Initialize
        // ==========================================

        public async Task InitializeAsync()
        {
            StatusMessage = string.Empty;

            IsBusy = true;

            try
            {
                var user =
                    await _userService.GetUserByIdAsync(
                        CurrentSession.UserId);


                if (user is null)
                {
                    StatusMessage =
                        "تعذر تحميل بيانات المستخدم.";

                    return;
                }


                FullName =
                    user.FullName;


                Username =
                    user.Username;


                RoleName =
                    user.Role.RoleNameAr;
            }
            finally
            {
                IsBusy = false;
            }
        }


        // ==========================================
        // Save account information
        // ==========================================

        [RelayCommand]
        private async Task SaveAccountAsync()
        {
            StatusMessage =
                string.Empty;


            if (string.IsNullOrWhiteSpace(
                    FullName))
            {
                StatusMessage =
                    "الرجاء إدخال الاسم الكامل.";

                return;
            }


            if (string.IsNullOrWhiteSpace(
                    Username))
            {
                StatusMessage =
                    "الرجاء إدخال اسم المستخدم.";

                return;
            }


            IsBusy = true;

            try
            {
                var result =
                    await _userService
                        .UpdateMyAccountAsync(
                            CurrentSession.UserId,
                            FullName,
                            Username);


                if (!result.Success)
                {
                    StatusMessage =
                        result.ErrorMessage ??
                        "حدث خطأ أثناء تحديث الحساب.";

                    return;
                }

        

                // Update the current session immediately.
                CurrentSession.FullName =
                    FullName.Trim();


                StatusMessage =
                    "تم تحديث بيانات الحساب بنجاح.";

            }
            finally
            {
                IsBusy = false;
            }
        }


        // ==========================================
        // Change password
        // ==========================================

        [RelayCommand]
        private async Task ChangePasswordAsync()
        {
            StatusMessage =
                string.Empty;


            if (string.IsNullOrWhiteSpace(
                    CurrentPassword))
            {
                StatusMessage =
                    "الرجاء إدخال كلمة المرور الحالية.";

                return;
            }


            if (string.IsNullOrWhiteSpace(
                    NewPassword))
            {
                StatusMessage =
                    "الرجاء إدخال كلمة المرور الجديدة.";

                return;
            }


            if (NewPassword.Length < 4)
            {
                StatusMessage =
                    "كلمة المرور الجديدة يجب أن تكون 4 أحرف على الأقل.";

                return;
            }


            if (NewPassword !=
                ConfirmPassword)
            {
                StatusMessage =
                    "كلمتا المرور الجديدتان غير متطابقتين.";

                return;
            }


            IsBusy = true;

            try
            {
                var result =
                    await _userService
                        .ChangePasswordAsync(
                            CurrentSession.UserId,
                            CurrentPassword,
                            NewPassword);


                if (!result.Success)
                {
                    StatusMessage =
                        result.ErrorMessage ??
                        "حدث خطأ أثناء تغيير كلمة المرور.";

                    return;
                }

 ;
                // Clear the password fields after successful change.
         

                PasswordChangedSuccessfully?.Invoke();



                StatusMessage =
                    "تم تغيير كلمة المرور بنجاح.";
            }
            finally
            {
                IsBusy = false;
            }
        }


        // ==========================================
        // Back
        // ==========================================

        [RelayCommand]
        private void Back()
        {
            BackRequested?.Invoke();
        }
    }
}