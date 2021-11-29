using AsystentZOOM.VM.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        }
    }
}
