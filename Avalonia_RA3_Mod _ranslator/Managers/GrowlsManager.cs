using Huskui.Avalonia.Controls;
using Huskui.Avalonia.Models;
using System;

namespace Avalonia_RA3_Mod__ranslator.Managers
{
    public static class GrowlsManager
    {
        private static AppWindow? _appWindow;

        // Статический метод для инициализации
        public static void Initialize(AppWindow appWindow)
        {
            _appWindow = appWindow;
        }

        public static void ShowInfoMsg(string msg, string title = "")
        {
            ShowGrowlMsg(GrowlLevel.Information, string.IsNullOrWhiteSpace(title) ? "Информация" : title, msg);
        }

        public static void ShowSuccesMsg(string msg, string title = "")
        {
            ShowGrowlMsg(GrowlLevel.Success, string.IsNullOrWhiteSpace(title) ? "Успех" : title, msg);
        }

        public static void ShowWarningMsg(string warnMsg)
        {
            ShowGrowlMsg(GrowlLevel.Warning, "Предупреждение", warnMsg);
        }

        public static void ShowErrorMsg(Exception ex, string title = "")
        {
            ShowGrowlMsg(GrowlLevel.Danger, string.IsNullOrWhiteSpace(title) ? "Ошибка" : title, ex.Message + (ex.InnerException?.Message ?? ""));
        }

        public static void ShowErrorMsg(string errMsg)
        {
            ShowGrowlMsg(GrowlLevel.Danger, "Ошибка", errMsg);
        }

        public static void ShowGrowlMsg(GrowlLevel growlLevel, string title, string content)
        {
            if (_appWindow == null)
            {
                return;
            }

            GrowlItem growlItem = new GrowlItem
            {
                Level = growlLevel,
                Title = title,
                Content = content
            };

            _appWindow.PopGrowl(growlItem);
        }
    }
}