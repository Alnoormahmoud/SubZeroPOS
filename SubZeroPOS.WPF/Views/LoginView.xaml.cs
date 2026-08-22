using System.Windows.Controls;
using System.Windows.Input;
using SubZeroPOS.WPF.ViewModels;

namespace SubZeroPOS.WPF.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            Loaded += LoginView_Loaded;

        }

        private void LoginView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.LoadRememberedUsername();
            }
            Keyboard.Focus(PasswordBox);

        }

        private void LoginButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                // PasswordBox.Password can't be bound directly for security reasons,
                // so we pass it explicitly into the RelayCommand here.
                vm.LoginCommand.Execute(PasswordBox.Password);
            }
        }
    }
}
