using GTranslate.Translators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RA3_Mod_Translater
{
    public static partial class APITranslateManager
    {
        private static ITranslator? Translator;
        private static readonly Dictionary<string, string> TranslationCache = new();

        // Делегат для передачи прогресса
        public static bool IsTranslationInProgress;
        public static Action<string>? ProgressCallback { get; set; }
        public static Action<int, int>? ProgressUpdateCallback { get; set; } // текущий, всего

        // Новые свойства для управления языками
        public static string TargetLanguage { get; set; } = "ru"; // По умолчанию русский
        public static string SourceLanguage { get; set; } = "auto"; // По умолчанию автоопределение

        public static async Task<string> TranslateAllQuotedTextAsync(string input, string translator, string targetLanguage = "ru", string sourceLanguage = "auto")
        {
            TargetLanguage = targetLanguage;
            SourceLanguage = sourceLanguage;

            try
            {
                IsTranslationInProgress = true;
                ProgressCallback?.Invoke($"Начинаем процесс перевода на {GetLanguageName(targetLanguage)}...");

                Translator = CreateTranslator(translator);

                var matches = QuotesRegex().Matches(input)
                                   .Cast<Match>()
                                   .ToList();

                matches.Sort((a, b) => b.Index.CompareTo(a.Index)); // с конца

                string result = input;

                int i = 1;
                int matchesCount = matches.Count;

                // Отправляем сообщение о начале
                ProgressCallback?.Invoke($"Обнаружено {matchesCount} фраз для перевода.");

                foreach (var match in matches)
                {
                    try
                    {
                        string originalText = match.Groups[1].Value;

                        if (string.IsNullOrWhiteSpace(originalText) || ProtectedPatterns().IsMatch(originalText))
                        {
                            continue;
                        }

                        string translatedText = await TranslatePreservingTagsAsync(originalText, targetLanguage, sourceLanguage);
                        string replacement = $"\"{translatedText}\"";

                        // Отправляем прогресс
                        ProgressCallback?.Invoke($"[{i}/{matchesCount}] Запрос: {originalText,-28} | Ответ: {replacement}");
                        ProgressUpdateCallback?.Invoke(i, matchesCount);

                        result = result.Substring(0, match.Index) +
                                 replacement +
                                 result.Substring(match.Index + match.Length);

                        i++;
                    }
                    catch (Exception ex)
                    {
                        ProgressCallback?.Invoke($"❌ Ошибка обработки фразы (позиция {match.Index}): {ex.Message}");
                        i++;
                    }
                }

                ProgressCallback?.Invoke("Перевод завершен успешно!");
                return result;
            }
            catch (Exception ex)
            {
                ProgressCallback?.Invoke($"❌ Критическая ошибка перевода: {ex.Message}");
                throw;
            }
            finally
            {
                IsTranslationInProgress = false;
            }
        }

        public static async Task<string> TranslatePreservingTagsAsync(string text, string targetLanguage = "ru", string sourceLanguage = "auto")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text)) return text;

                // Protect
                string afterUrls = ProtectSpecialPatternsManager.ProtectUrls(text);
                string afterRules = ProtectSpecialPatternsManager.ProtectSpecialPatterns(afterUrls);
                var (afterTags, tagInfos) = ProtectSpecialPatternsManager.ProtectTags(afterRules);
                string protectedText = ProtectSpecialPatternsManager.ProtectSquareBrackets(afterTags);

                // Translate
                string afterAmpersand = await TranslateAmpersandTermsAsync(protectedText, targetLanguage, sourceLanguage);
                string translatedText = await TranslateWithDelayAsync(afterAmpersand, targetLanguage, sourceLanguage);

                // Restore
                string restoredTags = ProtectSpecialPatternsManager.RestoreTags(translatedText, tagInfos);
                string restoredSquareBrackets = ProtectSpecialPatternsManager.RestoreSquareBrackets(restoredTags);
                string restoredRules = ProtectSpecialPatternsManager.RestoreSpecialPatterns(restoredSquareBrackets);
                string finalResult = ProtectSpecialPatternsManager.RestoreUrls(restoredRules);

                return finalResult;
            }
            catch (Exception ex)
            {
                ProgressCallback?.Invoke($"❌ Ошибка при переводе с сохранением тегов: {ex.Message}");
                return text;
            }
        }

        public static async Task<string> TranslateAmpersandTermsAsync(string text, string targetLanguage = "ru", string sourceLanguage = "auto")
        {
            try
            {
                var matches = Regex.Matches(text, @"&([a-zA-Z][a-zA-Z0-9]*)")
                                   .Cast<Match>()
                                   .ToList();

                if (matches.Count == 0) return text;

                var keywordsToTranslate = new HashSet<string>();
                foreach (Match m in matches)
                {
                    try
                    {
                        keywordsToTranslate.Add(m.Groups[1].Value);
                    }
                    catch (Exception ex)
                    {
                        ProgressCallback?.Invoke($"❌ Ошибка обработки ключевого слова: {ex.Message}");
                    }
                }

                var translationTasks = new Dictionary<string, Task<string>>();
                foreach (string kw in keywordsToTranslate)
                {
                    try
                    {
                        string cacheKey = $"{kw}_{targetLanguage}_{sourceLanguage}";
                        if (!TranslationCache.TryGetValue(cacheKey, out _))
                        {
                            translationTasks[kw] = TranslateWithDelayAsync(kw, targetLanguage, sourceLanguage);
                        }
                    }
                    catch (Exception ex)
                    {
                        ProgressCallback?.Invoke($"❌ Ошибка подготовки перевода для '{kw}': {ex.Message}");
                    }
                }

                if (translationTasks.Count > 0)
                {
                    try
                    {
                        var results = await Task.WhenAll(translationTasks.Values);
                        var newTranslations = keywordsToTranslate.Zip(results, (k, v) => new { k, v });
                        foreach (var item in newTranslations)
                        {
                            string cacheKey = $"{item.k}_{targetLanguage}_{sourceLanguage}";
                            TranslationCache[cacheKey] = item.v;
                        }
                    }
                    catch (Exception ex)
                    {
                        ProgressCallback?.Invoke($"❌ Ошибка выполнения переводов: {ex.Message}");
                    }
                }

                string result = text;
                foreach (Match m in matches)
                {
                    try
                    {
                        string keyword = m.Groups[1].Value;
                        string cacheKey = $"{keyword}_{targetLanguage}_{sourceLanguage}";
                        if (TranslationCache.TryGetValue(cacheKey, out string? translatedKeyword))
                        {
                            string replacement = "&" + translatedKeyword;
                            result = result.Replace(m.Value, replacement);
                        }
                    }
                    catch (Exception ex)
                    {
                        ProgressCallback?.Invoke($"❌ Ошибка замены ключевого слова '{m.Value}': {ex.Message}");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                ProgressCallback?.Invoke($"❌ Критическая ошибка перевода терминов с &: {ex.Message}");
                return text;
            }
        }

        public static async Task<string> TranslateWithDelayAsync(string text, string targetLanguage = "ru", string sourceLanguage = "auto")
        {
            try
            {
                string cacheKey = $"{text}_{targetLanguage}_{sourceLanguage}";
                if (TranslationCache.TryGetValue(cacheKey, out string? cached))
                    return cached;

                const int MaxRetries = 3;
                for (int attempt = 1; attempt <= MaxRetries; attempt++)
                {
                    try
                    {
                        await Task.Delay(attempt == 1 ? 50 : 100 * attempt);

                        if (Translator == null)
                        {
                            throw new InvalidOperationException("Переводчик не инициализирован");
                        }

                        // Используем sourceLanguage и targetLanguage
                        var result = sourceLanguage == "auto"
                            ? await Translator.TranslateAsync(text, targetLanguage)
                            : await Translator.TranslateAsync(text, targetLanguage, sourceLanguage);

                        // Кэшируем результат
                        TranslationCache[cacheKey] = result.Translation;
                        return result.Translation;
                    }
                    catch (Exception ex) when (attempt < MaxRetries)
                    {
                        ProgressCallback?.Invoke($"❌ Ошибка перевода '{text}' (попытка {attempt}): {ex.Message}");
                    }
                }

                ProgressCallback?.Invoke($"❌ Не удалось перевести: {text}");
                TranslationCache[cacheKey] = text;
                return text;
            }
            catch (Exception ex)
            {
                ProgressCallback?.Invoke($"❌ Критическая ошибка в TranslateWithDelayAsync: {ex.Message}");
                string cacheKey = $"{text}_{targetLanguage}_{sourceLanguage}";
                TranslationCache[cacheKey] = text;
                return text;
            }
        }

        [GeneratedRegex(@"^(\$\w+|&\w+|\[@[^\]]+\]|\{[^\}]+\}|%\w+%)+$")]
        private static partial Regex ProtectedPatterns();

        public static ITranslator CreateTranslator(string selectedTranslator)
        {
            try
            {
                return selectedTranslator switch
                {
                    "1" => new MicrosoftTranslator(),
                    "2" => new GoogleTranslator(),
                    "3" => new GoogleTranslator2(),
                    "4" => new YandexTranslator(),
                    _ => throw new ArgumentException($"Неизвестный переводчик: {selectedTranslator}")
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка создания переводчика '{selectedTranslator}': {ex.Message}", ex);
            }
        }

        [GeneratedRegex("\"(.*?)\"")]
        private static partial Regex QuotesRegex();

        // Вспомогательный метод для получения названия языка
        private static string GetLanguageName(string languageCode)
        {
            return languageCode switch
            {
                "ru" => "русский",
                "en" => "английский",
                "es" => "испанский",
                "fr" => "французский",
                "de" => "немецкий",
                "it" => "итальянский",
                "pt" => "португальский",
                "zh" => "китайский",
                "ja" => "японский",
                "ko" => "корейский",
                "ar" => "арабский",
                _ => languageCode
            };
        }

        // Метод для очистки кэша
        public static void ClearCache()
        {
            TranslationCache.Clear();
        }

        // Метод для получения поддерживаемых языков
        public static Dictionary<string, string> GetSupportedLanguages()
        {
            return new Dictionary<string, string>
            {
                { "auto", "Автоопределение" },
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
        }
    }
}