using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia_RA3_Mod__ranslator.ViewModels.Pages;
using Huskui.Avalonia.Controls;

namespace Avalonia_RA3_Mod__ranslator.Views.Pages
{

    public partial class AboutPage : Page
    {
        public AboutPage()
        {
            InitializeComponent();
            DataContext = new AboutPageViewModel();
        }
    }
}