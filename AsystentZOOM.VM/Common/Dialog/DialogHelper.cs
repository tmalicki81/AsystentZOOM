using AsystentZOOM.VM.Model;
using AsystentZOOM.VM.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
        public static T ShowMessagePanel<T>(
            string messageBoxText, string caption, ImageEnum icon,
            T defaultButton, IEnumerable<MsgBoxButtonVM<T>> buttons)
        {
            var arg = new MsgBoxVM<T>(
                messageBoxText,
                caption,
                icon,
                defaultButton,
                buttons);

            EventAggregator.Publish("MessagePanel_Show", arg);
            return arg.Result;
        }

        /// <summary>
        /// Wyświetlenie okna z wiadomością
        /// </summary>
        /// <param name="messageBoxText">Treść wiadomości</param>
        /// <param name="caption">Temat wiaddomości</param>
        /// <param name="button">Przyciski z odpowiedziami</param>
        /// <param name="icon">Ikona</param>
        /// <param name="defaultResult">Domyślna odpowiedź</param>
        /// <returns>Odpowiedź uzytkownika</returns>
        public static MessageBoxResult ShowMessageBox(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            var arg = new MessageBoxParameters
            {
                MessageBoxText = messageBoxText,
                Caption = caption,
                Button = button,
                Icon = icon,
                DefaultResult = defaultResult
            };
            EventAggregator.Publish("MessageBox_Show", arg);
            return arg.Result;
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
        /// Pobranie obiektu postępu
        /// </summary>
        /// <returns></returns>
        public static ProgressInfoVM GetProgressInfo()
            => SingletonVMFactory.Main.ProgressInfo;

        /// <summary>
        /// Wykonanie asynchroniczne metody
        /// </summary>
        /// <param name="operationName">Nazwa operacji</param>
        /// <param name="isIndeterminate">Czy nie można określić postępu wykonania operacji</param>
        /// <param name="taskName">Nazwa zadania</param>
        /// <param name="action">Metoda realizujaca operację</param>
        /// <param name="exception">Metoda obsługująca wyjątek</param>
        public static void RunAsync(
            string operationName, bool isIndeterminate, string taskName,
            Action<ProgressInfoVM> action,
            Action<Exception> exception = null)
        {
            Task.Run(() =>
            {
                var progress = GetProgressInfo();
                progress.PercentCompletted = 0;
                progress.ProgressBarVisibility = true;
                progress.IsIndeterminate = isIndeterminate;
                progress.OperationName = operationName;
                progress.TaskName = taskName;
                try
                {
                    action(progress);
                    progress.ProgressBarVisibility = false;
                }
                catch (Exception ex)
                {
                    progress.ProgressBarVisibility = false;
                    if (exception != null)
                        exception(ex);
                    else
                    {
                        //Clipboard.SetText(ex.ToString());
                        ShowMessageBox(ex.ToString(), "Błąd ", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    }
                }
            });
        }
    }
}