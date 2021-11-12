using AsystentZOOM.VM.Model;
using AsystentZOOM.VM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AsystentZOOM.VM.Common.Dialog
{
    /// <summary>
    /// Komunikacja z uzytkownikiem
    /// </summary>
    public static class DialogHelper
    {
        /// <summary>
        /// Wyświetlenie panelu z wiadomością
        /// </summary>
        /// <param name="messageBoxText">Treść wiadomości</param>
        /// <param name="caption">Temat wiaddomości</param>
        /// <param name="button">Przyciski z odpowiedziami</param>
        /// <param name="icon">Ikona</param>
        /// <param name="defaultResult">Domyślna odpowiedź</param>
        /// <returns>Odpowiedź uzytkownika</returns>
        public static async Task<T> ShowMessageBoxAsync<T>(
            string messageBoxText, string caption, ImageEnum icon,
            T defaultButton, IEnumerable<MsgBoxButtonVM<T>> buttons)
        {
            var arg = new MsgBoxVM<T>(
                messageBoxText,
                caption,
                icon,
                defaultButton,
                buttons);

            await Task.Run(() => EventAggregator.Publish("MessagePanel_Show", arg));
            return arg.Result;
        }

        /// <summary>
        /// Wyświetlenie panelu z wiadomością
        /// </summary>
        /// <param name="messageBoxText">Treść wiadomości</param>
        /// <param name="caption">Temat wiaddomości</param>
        /// <param name="icon">Ikona</param>
        public static async Task ShowMessageBoxAsync(string messageBoxText, string caption, ImageEnum icon)
        {
            await ShowMessageBoxAsync(
                messageBoxText, caption, icon, true,
                new MsgBoxButtonVM<bool>[] { new(true, "OK", ImageEnum.Ok) });
        }

        /// <summary>
        /// Otworzenie okna do otwierania pliku
        /// </summary>
        /// <param name="title">Tytuł okna</param>
        /// <param name="filter">Filtr</param>
        /// <param name="multiselect">Możliwość wybierania wielu plików</param>
        /// <param name="fileNames">Wybrane pliki</param>
        /// <returns>Czy zatwierdzono</returns>
        public static bool? ShowOpenFile(string title, string filter, bool multiselect, out string[] fileNames)
        {
            return ShowOpenFile(title, filter, multiselect, false, null, null, out fileNames);
        }

        /// <summary>
        /// Otworzenie okna do otwierania pliku
        /// </summary>
        /// <param name="title">Tytuł okna</param>
        /// <param name="filter">Filtr</param>
        /// <param name="multiselect">Możliwość wybierania wielu plików</param>
        /// <param name="AddExtension">Czy dodawać rozszerzenie pliku</param>
        /// <param name="DefaultExt">Domyślne rozszerzenie pliku</param>
        /// <param name="InitialDirectory">Przeszukiwany katalog</param>
        /// <param name="fileNames">Wybrane pliki</param>
        /// <returns></returns>
        public static bool? ShowOpenFile(string title, string filter, bool multiselect,
            bool AddExtension, string DefaultExt, string InitialDirectory,
            out string[] fileNames)
        {
            var arg = new OpenFileDialogParameters
            {
                Multiselect = multiselect,
                Title = title,
                Filter = filter,
                AddExtension = AddExtension,
                DefaultExt = DefaultExt,
                InitialDirectory = InitialDirectory
            };
            EventAggregator.Publish("OpenFile_Show", arg);
            fileNames = arg.FileNames;
            return arg.Result;
        }

        /// <summary>
        /// Otworzenie okna do zapisanie pliku
        /// </summary>
        /// <param name="title">Tytuł okna</param>
        /// <param name="filter">Filtr</param>
        /// <param name="fileNames">Wybrane pliki</param>
        /// <returns>Czy zapisano plik</returns>
        public static bool? ShowSaveFile(string title, string filter, ref string[] fileNames)
        {
            return ShowSaveFile(title, filter, false, null, null, ref fileNames);
        }

        /// <summary>
        /// Otworzenie okna do zapisanie pliku
        /// </summary>
        /// <param name="title">Tytuł okna</param>
        /// <param name="filter">Filtr</param>
        /// <param name="AddExtension"></param>
        /// <param name="DefaultExt">Domyślne rozszerzenie pliku</param>
        /// <param name="InitialDirectory">Katalog do zapisu</param>
        /// <param name="fileNames">Wybrane pliki</param>
        /// <returns>Czy zapisano plik</returns>
        public static bool? ShowSaveFile(string title, string filter,
            bool AddExtension, string DefaultExt, string InitialDirectory,
            ref string[] fileNames)
        {
            var arg = new SaveFileDialogParameters
            {
                Title = title,
                Filter = filter,
                AddExtension = AddExtension,
                DefaultExt = DefaultExt,
                InitialDirectory = InitialDirectory,
                FileName = fileNames.FirstOrDefault(),
                FileNames = fileNames,
            };
            EventAggregator.Publish("SaveFile_Show", arg);
            fileNames = arg.FileNames;
            return arg.Result;
        }

        private static readonly object _locker = new object();

        /// <summary>
        /// Wyświetlenie wiadomości na dolnym pasku
        /// </summary>
        /// <param name="message">Treść widomości</param>
        /// <param name="level">Poziom ważności</param>
        public static void ShowMessageBar(string message, MessageBarLevelEnum level = MessageBarLevelEnum.Information)
        {
            lock (_locker)
            {
                MainVM.Dispatcher.Invoke(() =>
                {
                    var main = SingletonVMFactory.Main;
                    main.MessageBarIsNew = false;
                    main.MessageBarText = message;
                    main.MessageBarIsNew = true;
                });
            }
        }

        /// <summary>
        /// Wykonanie asynchroniczne metody
        /// </summary>
        /// <param name="operationName">Nazwa operacji</param>
        /// <param name="isIndeterminate">Czy nie można określić postępu wykonania operacji</param>
        /// <param name="taskName">Nazwa zadania</param>
        /// <param name="action">Metoda realizujaca operację</param>
        /// <param name="exception">Metoda obsługująca wyjątek</param>
        public static async Task RunAsync(
            string operationName, bool isIndeterminate, string taskName,
            Action<IProgressInfoVM> action,
            Action<Exception> exception = null)
        {
            var progress = new ProgressInfoVM
            {
                PercentCompletted = 0,
                IsIndeterminate = isIndeterminate,
                OperationName = operationName,
                TaskName = taskName
            };
            EventAggregator.Publish("ProgressInfo_Show", progress);
            try
            {
                await Task.Run(() => action(progress));
                EventAggregator.Publish("ProgressInfo_Hide", progress);
            }
            catch (Exception ex)
            {
                EventAggregator.Publish("ProgressInfo_Hide", progress);
                if (exception != null)
                    exception(ex);
                else
                    await ShowMessageBoxAsync(ex.ToString(), "Błąd", ImageEnum.Error);
            }
        }
    }
}