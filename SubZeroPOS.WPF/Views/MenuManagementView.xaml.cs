using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SubZeroPOS.WPF.ViewModels;

namespace SubZeroPOS.WPF.Views
{
    public partial class MenuManagementView : UserControl
    {
        public MenuManagementView()
        {
            InitializeComponent();
            Loaded += MenuManagementView_Loaded;
        }

        private async void MenuManagementView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MenuManagementViewModel vm)
            {
                vm.ChooseImageFileRequested += ChooseImageFile;
                await vm.InitializeAsync();
            }
        }

        private string? ChooseImageFile()
        {
            var dialog = new OpenFileDialog
            {
                Title = "اختيار صورة الصنف",
                Filter = "كل صور الصور|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp;*.tiff;*.tif;*.ico|" +
                          "JPEG|*.jpg;*.jpeg|" +
                          "PNG|*.png|" +
                          "BMP|*.bmp|" +
                          "GIF|*.gif|" +
                          "WEBP|*.webp|" +
                          "كل الملفات|*.*"
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
