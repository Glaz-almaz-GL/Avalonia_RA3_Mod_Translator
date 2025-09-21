using Avalonia_RA3_Mod__ranslator.Managers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Avalonia_RA3_Mod__ranslator.ViewModels.Pages
{
    public partial class AboutPageViewModel : ObservableObject
    {
        private readonly Stopwatch _uptimeStopwatch;

        public AboutPageViewModel()
        {
            _uptimeStopwatch = new Stopwatch();
            _uptimeStopwatch.Start();

            // Инициализация системной информации
            UpdateSystemInfo();
        }

        // Информация о приложении
        public string AppName => "RA3 Mod Translator";
        public string AppVersion => Assembly.GetExecutingAssembly()?.GetName().Version?.ToString() ?? "1.0.0.0";
        public string License => "MIT License";
        public string AppStatus => "Стабильная версия";

        // Информация о разработчике
        public string DeveloperName => "Глазов Михаил";
        public string DeveloperCompany => "Независимый разработчик";
        public string DeveloperContact => "glazalmazgl@gmail.com";
        public string GitHubUrl => "https://github.com/Glaz-almaz-GL/RA3-Mod-Translater";

        // Системная информация
        [ObservableProperty]
        private string _osInfo;

        [ObservableProperty]
        private string _architecture;

        [ObservableProperty]
        private string _dotNetVersion;

        [ObservableProperty]
        private string _clrVersion;

        [ObservableProperty]
        private string _availableMemory;

        [ObservableProperty]
        private string _totalMemory;

        [ObservableProperty]
        private string _uptime;

        [ObservableProperty]
        private string _processorCount;

        [ObservableProperty]
        private string _processorArchitecture;

        private void UpdateSystemInfo()
        {
            try
            {
                // Операционная система
                OsInfo = RuntimeInformation.OSDescription;

                // Архитектура
                Architecture = RuntimeInformation.OSArchitecture.ToString();

                // Версия .NET
                DotNetVersion = Environment.Version.ToString();
                ClrVersion = RuntimeInformation.FrameworkDescription;

                // Время работы
                var uptime = _uptimeStopwatch.Elapsed;
                Uptime = $"{uptime.Days} дн. {uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";

                // Количество ядер
                ProcessorCount = Environment.ProcessorCount.ToString();
                ProcessorArchitecture = RuntimeInformation.ProcessArchitecture.ToString();
            }
            catch (Exception ex)
            {
                OsInfo = "Не удалось получить информацию";
                Architecture = "Неизвестно";
                DotNetVersion = Environment.Version.ToString();
                ClrVersion = "Неизвестно";
                AvailableMemory = "Неизвестно";
                TotalMemory = "Неизвестно";
                Uptime = "Неизвестно";
                ProcessorCount = "Неизвестно";
                ProcessorArchitecture = "Неизвестно";

                // Логируем ошибку если нужно
                Console.WriteLine($"Ошибка получения системной информации: {ex.Message}");
            }
        }

        [RelayCommand]
        private void OpenGitHub()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = GitHubUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                GrowlsManager.ShowErrorMsg($"Не удалось открыть ссылку: {ex.Message}");
            }
        }

        [RelayCommand]
        private void OpenEmail()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"mailto:{DeveloperContact}",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                GrowlsManager.ShowErrorMsg($"Не удалось открыть почтовый клиент: {ex.Message}");
            }
        }

        [RelayCommand]
        private void CheckForUpdates()
        {
            GrowlsManager.ShowInfoMsg("Проверка обновлений...");
            // Здесь будет логика проверки обновлений
        }

        [RelayCommand]
        private void RefreshSystemInfo()
        {
            UpdateSystemInfo();
            GrowlsManager.ShowInfoMsg("Информация обновлена");
        }

        [RelayCommand]
        private void OpenDocumentation()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/d3ara1n/RA3-Mod-Translator/wiki",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                GrowlsManager.ShowErrorMsg($"Не удалось открыть документацию: {ex.Message}");
            }
        }

        [RelayCommand]
        private void ReportBug()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/Glaz-almaz-GL/RA3-Mod-Translater/issues/new",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                GrowlsManager.ShowErrorMsg($"Не удалось открыть страницу ошибок: {ex.Message}");
            }
        }
    }
}