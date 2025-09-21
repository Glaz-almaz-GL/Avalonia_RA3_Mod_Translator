using Avalonia.Controls;
using Avalonia_RA3_Mod__ranslator.Models;
using Avalonia_RA3_Mod__ranslator.Views.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using Huskui.Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Avalonia_RA3_Mod__ranslator.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private Page? _currentPage;

        public Page? CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public ObservableCollection<NavigationItem> NavigationItems { get; }

        public MainViewModel()
        {
            // Создаем страницы заранее
            var homePage = new HomePage();
            //var settingsPage = new SettingsPage();
            var aboutPage = new AboutPage();

            NavigationItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem()
                {
                    Title = "Главная",
                    Description = "Основная страница приложения",
                    Icon = Symbol.Home,
                    IsNew = false,
                    IsUpdated = false,
                    NavigationPage = homePage
                },
                //new NavigationItem()
                //{
                //    Title = "Настройки",
                //    Description = "Параметры приложения",
                //    Icon = Symbol.Settings,
                //    IsNew = false,
                //    IsUpdated = false,
                //    NavigationPage = settingsPage
                //},
                new NavigationItem()
                {
                    Title = "О программе",
                    Description = "Информация о приложении",
                    Icon = Symbol.Info,
                    IsNew = false,
                    IsUpdated = false,
                    NavigationPage = aboutPage
                }
            };

            CurrentPage = homePage;
        }

        [RelayCommand]
        public void NavigateToItem(NavigationItem item)
        {
            if (item?.NavigationPage != null && item.NavigationPage != CurrentPage)
            {
                CurrentPage = item.NavigationPage;
            }
        }

        [RelayCommand]
        private void OpenGitHub()
        {
            OpenUrl("https://github.com/Glaz-almaz-GL");
        }

        [RelayCommand]
        private void OpenEmail()
        {
            OpenUrl("mailto:glazalmazgl@gmail.com");
        }

        [RelayCommand]
        private void OpenRA3ModsSite()
        {
            OpenUrl("https://www.moddb.com/games/cc-red-alert-3/mods");
        }

        private static void OpenUrl(string url)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Не удалось открыть ссылку: {ex.Message}");
                // Здесь можно показать сообщение пользователю через Growl/MessageBox
            }
        }
    }
}