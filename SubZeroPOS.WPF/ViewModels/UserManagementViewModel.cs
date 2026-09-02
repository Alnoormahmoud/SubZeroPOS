using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubZeroPOS.Core.Entities;
using SubZeroPOS.Core.Interfaces;

namespace SubZeroPOS.WPF.ViewModels
{
    public partial class UserManagementViewModel : ObservableObject
    {
        private const int PageSize = 20;

        private readonly IUserService _userService;
        private List<User> _allUsers = new();

        public UserManagementViewModel(IUserService userService)
        {
            _userService = userService;
        }

        public ObservableCollection<User> Users { get; } = new(); // current page only
        public ObservableCollection<Role> Roles { get; } = new();

        [ObservableProperty] private string newFullName = string.Empty;
        [ObservableProperty] private string newUsername = string.Empty;
        [ObservableProperty] private string newPassword = string.Empty;
        [ObservableProperty] private Role? selectedRole;
        [ObservableProperty] private string statusMessage = string.Empty;
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private int currentPage = 1;
        [ObservableProperty] private int totalPages = 1;
        [ObservableProperty] private int totalUserCount;

        [ObservableProperty]
        private User? selectedUser;

        [ObservableProperty]
        private string editFullName = string.Empty;

        [ObservableProperty]
        private string editUsername = string.Empty;

        [ObservableProperty]
        private string editPassword = string.Empty;

        [ObservableProperty]
        private Role? editSelectedRole;

        public string PageInfoText => $"الصفحة {CurrentPage} من {TotalPages}";
        public bool CanGoNext => CurrentPage < TotalPages;
        public bool CanGoPrevious => CurrentPage > 1;

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

        partial void OnSelectedUserChanged(User? value)
        {
            if (value is null)
                return;

            EditFullName = value.FullName;
            EditUsername = value.Username;
            EditPassword = string.Empty;
            EditSelectedRole = Roles.FirstOrDefault(
                r => r.RoleId == value.RoleId);
        }
        [RelayCommand]
        private async Task UpdateUserAsync()
        {
            StatusMessage = string.Empty;

            if (SelectedUser is null)
            {
                StatusMessage = "اختر مستخدماً أولاً";
                return;
            }

            if (string.IsNullOrWhiteSpace(EditFullName))
            {
                StatusMessage = "الاسم الكامل مطلوب";
                return;
            }

            if (string.IsNullOrWhiteSpace(EditUsername))
            {
                StatusMessage = "اسم المستخدم مطلوب";
                return;
            }

            if (EditSelectedRole is null)
            {
                StatusMessage = "الرجاء اختيار الصلاحية";
                return;
            }

            IsBusy = true;

            try
            {
                var result =
                    await _userService.UpdateUserAsync(
                        SelectedUser.UserId,
                        EditFullName,
                        EditUsername,
                        EditSelectedRole.RoleId,
                        string.IsNullOrWhiteSpace(EditPassword)
                            ? null
                            : EditPassword);

                if (!result.Success)
                {
                    StatusMessage =
                        result.ErrorMessage ?? "تعذر تحديث المستخدم";

                    return;
                }

                StatusMessage = "تم تحديث المستخدم بنجاح";

                EditPassword = string.Empty;

                await ReloadUsersAsync();

                SelectedUser = null;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void SelectUserForEdit(User user)
        {
            SelectedUser = user;
        }
        private async Task ReloadUsersAsync()
        {
            _allUsers = await _userService.GetAllUsersAsync();
            TotalUserCount = _allUsers.Count;
            CurrentPage = 1;
            UpdatePagedUsers();
        }

        private void UpdatePagedUsers()
        {
            TotalPages = Math.Max(1, (int)Math.Ceiling(_allUsers.Count / (double)PageSize));
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;

            var pageItems = _allUsers.Skip((CurrentPage - 1) * PageSize).Take(PageSize);

            Users.Clear();
            foreach (var u in pageItems)
                Users.Add(u);

            OnPropertyChanged(nameof(PageInfoText));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanGoPrevious));
        }

        [RelayCommand]
        private void NextPage()
        {
            if (!CanGoNext) return;
            CurrentPage++;
            UpdatePagedUsers();
        }

        [RelayCommand]
        private void PreviousPage()
        {
            if (!CanGoPrevious) return;
            CurrentPage--;
            UpdatePagedUsers();
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
