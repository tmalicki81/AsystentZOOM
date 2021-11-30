using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Enums;
using AsystentZOOM.VM.FileRepositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsystentZOOM.Finisher.ViewModel
{
    public class FinisherVM : BaseVM
    {
        private ObservableCollection<TaskVM> _taskList = new();
        public ObservableCollection<TaskVM> TaskList
        {
            get => _taskList;
            set => SetValue(ref _taskList, value, nameof(TaskList));
        }

        public async Task ExecuteAllTask()
        {
            foreach (var task in TaskList)
            {
                await task.TaskExecute();
            }
        }

        public FinisherVM()
        {
            TaskList.Add(new TaskVM("Usuwanie tymczasowych plików", false, (t) =>
            {
                List<string> filetToDelete = new();
                filetToDelete.AddRange(Directory.GetFiles(MediaLocalFileRepositoryFactory.Meetings.RootDirectory, $"*.{nameof(FileExtensionEnum.TMP_MEETING)}"));
                filetToDelete.AddRange(Directory.GetFiles(MediaLocalFileRepositoryFactory.TimePiece.RootDirectory, $"*.{nameof(FileExtensionEnum.TMP_TIM)}"));

                int fileNumber = 0;
                int filesCount = filetToDelete.Count;
                int PercentCompletted;
                foreach (string file in filetToDelete)
                {
                    File.Delete(file);
                    PercentCompletted = (100 * fileNumber++ / filesCount);
                    Dispatcher.Invoke(() => t.PercentCompletted = PercentCompletted);
                    Task.Delay(200).Wait();
                }
                Dispatcher.Invoke(()=> t.TaskName = $"Usunięto {filesCount} tymczasowych plików");
            }));

            var wavFiles = Directory.GetFiles(MediaLocalFileRepositoryFactory.AudioRecording.RootDirectory, $"*.{nameof(FileExtensionEnum.WAV)}");
            var mp3Files = Directory.GetFiles(MediaLocalFileRepositoryFactory.AudioRecording.RootDirectory, $"*.{nameof(FileExtensionEnum.MP3)}");

            TaskList.Add(new TaskVM("Zadanie testowe 1", true, (t) =>
            {              
                // TODO
            }));

            TaskList.Add(new TaskVM("Zadanie testowe 2", false, (t) =>
            {
                for (int p = 0; p < 100; p++)
                {
                    Dispatcher.Invoke(() => t.PercentCompletted = p);
                    Task.Delay(50).Wait();
                }
            }));

            TaskList.Add(new TaskVM("Zadanie błędne", true, (t) =>
            {
                Task.Delay(4000).Wait();
                throw new Exception("dsgtyryrt");
            }));

            TaskList.Add(new TaskVM("Zadanie testowe 3", true, (t) =>
            {
                for (int p = 0; p < 100; p++)
                {
                    Dispatcher.Invoke(() => t.PercentCompletted = p);
                    Task.Delay(50).Wait();
                }
            }));
        }
    }
}
