using Avalonia.Controls;
using Avalonia_RA3_Mod__ranslator.ViewModels.Pages;
using Huskui.Avalonia.Controls;
using System.IO;

namespace Avalonia_RA3_Mod__ranslator.Views.Pages
{
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
            DataContext = new HomePageViewModel(); // Устанавливаем ViewModel как DataContext
        }

        private void ModStrTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && DataContext is HomePageViewModel viewModel && !string.IsNullOrEmpty(textBox.Text))
            {
                viewModel.SelectedModFile = textBox.Text;
            }
        }
    }
}