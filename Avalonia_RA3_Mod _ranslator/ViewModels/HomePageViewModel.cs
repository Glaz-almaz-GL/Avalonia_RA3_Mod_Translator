using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia_RA3_Mod__ranslator.Managers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Huskui.Avalonia.Models;
using RA3_Mod_Translater;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Avalonia_RA3_Mod__ranslator.ViewModels.Pages
{
    public partial class HomePageViewModel : ObservableObject
    {
        public HomePageViewModel()
        {
            InitializeTranslationSettings();
        }


        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanStartTranslation))]
        private string _selectedModFile = string.Empty;

        [ObservableProperty]
        private int _translationProgress = 0;

        [ObservableProperty]
        private string _progressText = string.Empty;

        [ObservableProperty]
        private string _originalText = string.Empty;

        [ObservableProperty]
        private string _translatedText = string.Empty;

        [ObservableProperty]
        private bool _isTranslationInProgress = false;

        // Свойства для выбора переводчика
        [ObservableProperty]
        private Dictionary<string, string> _availableTranslators = new();

        [ObservableProperty]
        private KeyValuePair<string, string> _selectedTranslator;

        [ObservableProperty]
        private string _selectedTranslatorDescription = string.Empty;

        // Свойства для выбора языков
        [ObservableProperty]
        private Dictionary<string, string> _supportedSourceLanguages = new();

        [ObservableProperty]
        private Dictionary<string, string> _supportedTargetLanguages = new();

        [ObservableProperty]
        private KeyValuePair<string, string> _selectedSourceLanguage;

        [ObservableProperty]
        private KeyValuePair<string, string> _selectedTargetLanguage;

        public bool CanStartTranslation => !string.IsNullOrEmpty(SelectedModFile) && File.Exists(SelectedModFile);

        private void InitializeTranslationSettings()
        {
            // Доступные переводчики
            AvailableTranslators = new Dictionary<string, string>
            {
                { "1", "Microsoft Translator (Рекомендуется)" },
                { "2", "Yandex Translator (Быстрый)" },
                { "3", "Google Translator" },
                { "4", "Google Translator 2" }
            };

            // Поддерживаемые языки
            SupportedSourceLanguages = new Dictionary<string, string>
            {
                { "auto", "Автоопределение" },
                { "en", "Английский" },
                { "ru", "Русский" },
                { "es", "Испанский" },
                { "fr", "Французский" },
                { "de", "Немецкий" },
                { "it", "Итальянский" },
                { "pt", "Португальский" },
                { "zh", "Китайский" },
                { "ja", "Японский" },
                { "ko", "Корейский" },
                { "ar", "Арабский" }
            };

            SupportedTargetLanguages = new Dictionary<string, string>
            {
                { "ru", "Русский" },
                { "en", "Английский" },
                { "es", "Испанский" },
                { "fr", "Французский" },
                { "de", "Немецкий" },
                { "it", "Итальянский" },
                { "pt", "Португальский" },
                { "zh", "Китайский" },
                { "ja", "Японский" },
                { "ko", "Корейский" },
                { "ar", "Арабский" }
            };

            // Установка значений по умолчанию
            SelectedTranslator = AvailableTranslators.FirstOrDefault(x => x.Key == "2"); // Yandex Translator по умолчанию
            SelectedSourceLanguage = SupportedSourceLanguages.FirstOrDefault(x => x.Key == "auto");
            SelectedTargetLanguage = SupportedTargetLanguages.FirstOrDefault(x => x.Key == "ru");

            UpdateTranslatorDescription();
        }

        private void UpdateTranslatorDescription()
        {
            SelectedTranslatorDescription = SelectedTranslator.Key switch
            {
                "1" => "Лучшее качество перевода",
                "2" => "Самый быстрый перевод",
                "3" => "Нестабильный, но быстрый",
                "4" => "Альтернативный Google",
                _ => "Выберите переводчик"
            };
        }

        partial void OnSelectedTranslatorChanged(KeyValuePair<string, string> value)
        {
            UpdateTranslatorDescription();
        }

        [RelayCommand]
        private async Task SelectModFile()
        {
            var file = await DialogsManager.OpenModStrFileDialogAsync();

            if (file != null)
            {
                SelectedModFile = file.Path.AbsolutePath;
            }
        }

        [RelayCommand]
        private void UpdateTranslationSettings()
        {
            GrowlsManager.ShowSuccesMsg($"Переводчик: {SelectedTranslator.Value}\nЯзык: {SelectedSourceLanguage.Value} → {SelectedTargetLanguage.Value}", "Настройки обновлены");
        }

        [RelayCommand]
        private async Task StartTranslation()
        {
            if (!CanStartTranslation) return;

            IsTranslationInProgress = true;
            ProgressText = "Начинаем перевод...";
            TranslationProgress = 0;

            try
            {
                await PerformTranslation();
                ProgressText = "Перевод завершен успешно!";
            }
            catch (Exception ex)
            {
                ProgressText = "Ошибка при переводе";
                TranslationProgress = 0;
                GrowlsManager.ShowErrorMsg($"Ошибка перевода: {ex.Message}");
            }
            finally
            {
                IsTranslationInProgress = false;
            }
        }

        private async Task PerformTranslation()
        {
            var stopWatch = Stopwatch.StartNew();

            try
            {
                OriginalText = await File.ReadAllTextAsync(SelectedModFile);

                string translationOutputPath = Path.Combine(
                    Path.GetDirectoryName(SelectedModFile)!,
                    Path.GetFileNameWithoutExtension(SelectedModFile) + "_translated.str");

                // Устанавливаем callbacks для прогресса
                APITranslateManager.ProgressCallback = (message) =>
                {
                    Dispatcher.UIThread.Post(() => ProgressText = message);
                };

                APITranslateManager.ProgressUpdateCallback = (current, total) =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (total > 0)
                        {
                            TranslationProgress = (int)((double)current / total * 100);
                            ProgressText = $"Прогресс: {current}/{total} ({TranslationProgress}%)";
                        }
                    });
                };

                // Используем выбранные настройки
                string translator = SelectedTranslator.Key;
                string sourceLanguage = SelectedSourceLanguage.Key;
                string targetLanguage = SelectedTargetLanguage.Key;

                TranslatedText = await APITranslateManager.TranslateAllQuotedTextAsync(
                    OriginalText,
                    translator,
                    targetLanguage,
                    sourceLanguage);

                stopWatch.Stop();

                string elapsedTime = $"{stopWatch.Elapsed:mm\\:ss\\.fff}";
                GrowlsManager.ShowInfoMsg($"Перевод завершен! Время: {elapsedTime}");

                await File.WriteAllTextAsync(translationOutputPath, TranslatedText);
                GrowlsManager.ShowInfoMsg($"Файл сохранен: {translationOutputPath}");
            }
            catch (Exception ex)
            {
                stopWatch.Stop();
                GrowlsManager.ShowErrorMsg(ex);
            }
            finally
            {
                // Очищаем callbacks
                APITranslateManager.ProgressCallback = null;
                APITranslateManager.ProgressUpdateCallback = null;
            }
        }
    }
}