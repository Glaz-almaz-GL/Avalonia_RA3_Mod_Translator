using Avalonia.Controls;
using Avalonia_RA3_Mod__ranslator.Managers;
using Avalonia_RA3_Mod__ranslator.Models;
using Avalonia_RA3_Mod__ranslator.ViewModels;
using CommunityToolkit.Mvvm.Input;
using Huskui.Avalonia.Controls;
using System.Diagnostics;

namespace Avalonia_RA3_Mod__ranslator.Views
{
    public partial class MainWindow : AppWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            // Инициализируем GrowlManager статически
            GrowlsManager.Initialize(this);
            DialogsManager.Initialize(this);
        }

        private void OnNavigationItemSelected(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel &&
                e.AddedItems.Count > 0 &&
                e.AddedItems[0] is NavigationItem selectedItem)
            {
                viewModel.NavigateToItem(selectedItem);
            }
        }
    }
}