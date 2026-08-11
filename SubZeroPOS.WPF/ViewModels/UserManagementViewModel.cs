using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubZeroPOS.Core.Entities;
using SubZeroPOS.Core.Interfaces;

namespace SubZeroPOS.WPF.ViewModels
{
    public partial class UserManagementViewModel : ObservableObject
    {
        private readonly IUserService _userService;

        public UserManagementViewModel(IUserService userService)
        {
            _userService = userService;
        }

        public ObservableCollection<User> Users { get; } = new();
        public ObservableCollection<Role> Roles { get; } = new();

        [ObservableProperty] private string newFullName = string.Empty;
        [ObservableProperty] private string newUsername = string.Empty;
        [ObservableProperty] private string newPassword = string.Empty;
        [ObservableProperty] private Role? selectedRole;
        [ObservableProperty] private string statusMessage = string.Empty;
        [ObservableProperty] private bool isBusy;

        public event Action? BackRequested;

        public async Task InitializeAsync()
        {
            IsBusy = true;
            try
            {
                await ReloadUsersAsync();

                Roles.Clear();
                foreach (var r in await _userService.GetRolesAsync())
                    Roles.Add(r);

                if (Roles.Count > 0) SelectedRole = Roles[0];
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ReloadUsersAsync()
        {
            Users.Clear();
            foreach (var u in await _userService.GetAllUsersAsync())
                Users.Add(u);
        }

        [RelayCommand]
        private async Task CreateUserAsync()
        {
            StatusMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(NewFullName) || string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewPassword))
            {
                StatusMessage = "الرجاء تعبئة جميع الحقول";
                return;
            }

            if (NewPassword.Length < 4)
            {
                StatusMessage = "كلمة المرور قصيرة جداً (٤ أحرف على الأقل)";
                return;
            }

            if (SelectedRole is null)
            {
                StatusMessage = "الرجاء اختيار الصلاحية";
                return;
            }

            IsBusy = true;
            try
            {
                var success = await _userService.CreateUserAsync(NewFullName, NewUsername, NewPassword, SelectedRole.RoleId);
                if (!success)
                {
                    StatusMessage = "اسم المستخدم موجود بالفعل";
                    return;
                }

                NewFullName = string.Empty;
                NewUsername = string.Empty;
                NewPassword = string.Empty;
                StatusMessage = "تم إضافة المستخدم بنجاح";

                await ReloadUsersAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ToggleActiveAsync(User user)
        {
            await _userService.SetUserActiveAsync(user.UserId, !user.IsActive);
            await ReloadUsersAsync();
        }

        [RelayCommand]
        private async Task DeleteUserAsync(User user)
        {
            var confirm = System.Windows.MessageBox.Show(
                $"هل أنت متأكد من حذف المستخدم \"{user.FullName}\" نهائياً؟",
                "تأكيد الحذف",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (confirm != System.Windows.MessageBoxResult.Yes) return;

            var (success, errorMessage) = await _userService.DeleteUserAsync(user.UserId);

            if (!success)
            {
                System.Windows.MessageBox.Show(errorMessage, "تعذر الحذف",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            await ReloadUsersAsync();
        }

        [RelayCommand]
        private void Back() => BackRequested?.Invoke();
    }
}
