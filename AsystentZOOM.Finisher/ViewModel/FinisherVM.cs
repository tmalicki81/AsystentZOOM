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

        public async void ExecuteAllTask()
        {
            foreach (var task in TaskList)
            {
                await task.TaskExecute();//.TaskExecuteCommand.Execute();
            }
        }

        public FinisherVM()
        {
            TaskList.Add(new TaskVM("Usuwanie tymczasowych plików", true, (t) =>
            {
                List<string> filetToDelete = new();
                filetToDelete.AddRange(Directory.GetFiles(MediaLocalFileRepositoryFactory.Meetings.RootDirectory, $"*.{nameof(FileExtensionEnum.TMP_MEETING)}"));
                filetToDelete.AddRange(Directory.GetFiles(MediaLocalFileRepositoryFactory.TimePiece.RootDirectory, $"*.{nameof(FileExtensionEnum.TMP_TIM)}"));
                filetToDelete.ForEach(f => File.Delete(f));
            }));

            TaskList.Add(new TaskVM("Zadanie testowe 1", true, (t) =>
            {
                for (int p = 0; p < 100; p++)
                {
                    Dispatcher.Invoke(() => t.PercentComplette = p);
                    Task.Delay(50).Wait();
                }
            }));

            TaskList.Add(new TaskVM("Zadanie testowe 2", false, (t) =>
            {
                for (int p = 0; p < 100; p++)
                {
                    Dispatcher.Invoke(() => t.PercentComplette = p);
                    Task.Delay(50).Wait();
                }
            }));

            //TaskList.Add(new TaskVM("Zadanie błędne", true, (t) =>
            //{
            //    Task.Delay(4000).Wait();
            //    throw new Exception("dsgtyryrt");
            //}));

            TaskList.Add(new TaskVM("Zadanie testowe 3", true, (t) =>
            {
                for (int p = 0; p < 100; p++)
                {
                    Dispatcher.Invoke(() => t.PercentComplette = p);
                    Task.Delay(50).Wait();
                }
            }));
        }
    }
}
