using System.Windows;
using System.Windows.Controls;
using SubZeroPOS.WPF.ViewModels;

namespace SubZeroPOS.WPF.Views
{
    public partial class MyAccountView : UserControl
    {
        public MyAccountView()
        {
            InitializeComponent();

            Loaded +=
                MyAccountView_Loaded;
        }


        private async void MyAccountView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            if (DataContext
                is MyAccountViewModel vm)
            {
                await vm.InitializeAsync();
            }
        }


        private void ChangePassword_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (DataContext
                is not MyAccountViewModel vm)
            {
                return;
            }


            vm.CurrentPassword =
                CurrentPasswordBox.Password;


            vm.NewPassword =
                NewPasswordBox.Password;


            vm.ConfirmPassword =
                ConfirmPasswordBox.Password;


            if (vm.ChangePasswordCommand
                .CanExecute(null))
            {
                vm.ChangePasswordCommand
                    .Execute(null);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}