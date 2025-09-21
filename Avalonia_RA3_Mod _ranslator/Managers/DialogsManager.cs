using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Huskui.Avalonia.Controls;
using Splat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avalonia_RA3_Mod__ranslator.Managers
{
    public static class DialogsManager
    {
        private static TopLevel? _topLevel;

        // Статический метод для инициализации
        public static void Initialize(TopLevel topLevel)
        {
            _topLevel = topLevel;
        }

        public static async Task<IStorageFile?> OpenSingleFileDialogAsync(string title, IReadOnlyList<string>? allowedExtensions = null)
        {
            if (_topLevel == null)
            {
                return null;
            }

            var options = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false
            };

            // Если указаны разрешённые расширения, добавляем фильтр
            if (allowedExtensions != null && allowedExtensions.Count > 0)
            {
                options.FileTypeFilter = new[] { new FilePickerFileType(title) { Patterns = allowedExtensions } };
            }

            var files = await _topLevel.StorageProvider.OpenFilePickerAsync(options);

            if (files.Count >= 1)
            {
                return files[0];
            }

            return null;
        }

        public static async Task<IReadOnlyCollection<IStorageFile>?> OpenMultiplyFilesDialogAsync(string title, IReadOnlyList<string>? allowedExtensions = null)
        {
            if (_topLevel == null)
            {
                return null;
            }

            var options = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = true
            };

            // Если указаны разрешённые расширения, добавляем фильтр
            if (allowedExtensions != null && allowedExtensions.Count > 0)
            {
                options.FileTypeFilter = new[] { new FilePickerFileType(title) { Patterns = allowedExtensions } };
            }

            var files = await _topLevel.StorageProvider.OpenFilePickerAsync(options);

            if (files.Count >= 1)
            {
                return files;
            }

            return null;
        }

        // Дополнительные удобные методы для часто используемых расширений
        public static async Task<IStorageFile?> OpenTextFileDialogAsync(string title = "Выберите текстовый файл")
        {
            var extensions = new[] { "*.txt", "*.str", "*.csv" };
            return await OpenSingleFileDialogAsync(title, extensions);
        }

        public static async Task<IStorageFile?> OpenModStrFileDialogAsync(string title = "Выберите файл модификации")
        {
            var extensions = new[] { "*.str" };
            return await OpenSingleFileDialogAsync(title, extensions);
        }

        public static async Task<IStorageFile?> OpenAllFilesDialogAsync(string title = "Выберите файл")
        {
            var extensions = new[] { "*.*" };
            return await OpenSingleFileDialogAsync(title, extensions);
        }

        public static async Task<IReadOnlyCollection<IStorageFile>?> OpenMultipleTextFilesDialogAsync(string title = "Выберите текстовые файлы")
        {
            var extensions = new[] { "*.txt", "*.str", "*.csv" };
            return await OpenMultiplyFilesDialogAsync(title, extensions);
        }
    }
}
