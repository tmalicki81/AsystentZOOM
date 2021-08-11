using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Common.Dialog;
using AsystentZOOM.VM.Enums;
using AsystentZOOM.VM.Interfaces;
using AsystentZOOM.VM.Model;
using AsystentZOOM.VM.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AsystentZOOM.GUI.View
{
    /// <summary>
    /// Interaction logic for TimePiecePanelView.xaml
    /// </summary>
    public partial class TimePiecePanelView : UserControl, IPanelView<TimePieceVM>
    {
        public static TimePiecePanelView Instance;

        public TimePiecePanelView()
        {
            Instance = this;
            InitializeComponent();

            cmbTimerFormat.ItemsSource = new Dictionary<string, string>
            {
                { @"mm\:ss",          "02:25"        },
                { @"hh\:mm\:ss",      "16:02:25"     },
                { @"hh\:mm\:ss\.fff", "16:02:25.468" }
            };

            cmbDirection.ItemsSource = new Dictionary<TimePieceDirectionEnum, string>
            {
                { TimePieceDirectionEnum.Forward, "do przodu" },
                { TimePieceDirectionEnum.Back,    "do tyłu"   }
            };

            cmbAlertMinTime.ItemsSource = new Dictionary<TimeSpan, string>
            {
                { TimeSpan.Zero,            "Brak"               },
                { TimeSpan.FromMinutes(5),  "Przed 5 minutami"   },
                { TimeSpan.FromMinutes(4),  "Przed 4 minutami"   },
                { TimeSpan.FromMinutes(3),  "Przed 3 minutami"   },
                { TimeSpan.FromMinutes(2),  "Przed 2 minutami"   },
                { TimeSpan.FromMinutes(1),  "Przed 1 minutą"     },
                { TimeSpan.FromSeconds(30), "Przed 30 sekundami" },
                { TimeSpan.FromSeconds(10), "Przed 10 sekundami" },
                { TimeSpan.FromSeconds(9),  "Przed 9 sekundami"  },
                { TimeSpan.FromSeconds(8),  "Przed 8 sekundami"  },
                { TimeSpan.FromSeconds(7),  "Przed 7 sekundami"  },
                { TimeSpan.FromSeconds(6),  "Przed 6 sekundami"  },
                { TimeSpan.FromSeconds(5),  "Przed 5 sekundami"  },
                { TimeSpan.FromSeconds(4),  "Przed 4 sekundami"  },
                { TimeSpan.FromSeconds(3),  "Przed 3 sekundami"  },
                { TimeSpan.FromSeconds(2),  "Przed 2 sekundami"  },
                { TimeSpan.FromSeconds(1),  "Gdy czas minie"     }
            };
        }

        public int PanelNr => 3;
        public string PanelName => "Pomiar czasu";
        public TimePieceVM ViewModel => (TimePieceVM)DataContext;
        public void ChangeVisible(bool visible)
        {
        }

        private class IoConsts
        {
            internal static string InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $@"\Asystent ZOOM\Zegar";
            internal const string DefaultExt = "tim";
            internal const string Filter = "Pliki ustawień minutnika lub stopera|*.tim";
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool? result = DialogHelper.ShowOpenFile("Załaduj ustawienia minutnika lub stopera", IoConsts.Filter, false, true, IoConsts.DefaultExt, IoConsts.InitialDirectory, out string[] fileNames);
                string fileName = fileNames?.FirstOrDefault();
                if (result != true || string.IsNullOrEmpty(fileName))
                    return;

                Type t = ViewModel.GetType();
                var xmlSerializer = new CustomXmlSerializer(t);
                using (var f = File.OpenRead(fileName))
                {                    
                    f.Position = 0;
                    using (new SingletonInstanceHelper(t))
                    {
                        var viewModel = (TimePieceVM)xmlSerializer.Deserialize(f);
                        SingletonVMFactory.SetSingletonValues(viewModel);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Directory.Exists(IoConsts.InitialDirectory))
                    Directory.CreateDirectory(IoConsts.InitialDirectory);

                bool? result = DialogHelper.ShowSaveFile(
                    "Zapisz ustawienia minutnika lub stopera", 
                    IoConsts.Filter, true, IoConsts.DefaultExt, IoConsts.InitialDirectory, 
                    out string[] fileNames);
                string fileName = fileNames?.FirstOrDefault();
                if (result != true || string.IsNullOrEmpty(fileName))
                    return;

                using (var f = File.Create(fileName))
                {
                    var xmlSerializer = new CustomXmlSerializer(ViewModel.GetType());
                    xmlSerializer.Serialize(f, ViewModel);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnOpenBackgroundMediaFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool? result = DialogHelper.ShowOpenFile(
                    "Wybierz plik multimedialny jako tło", 
                    "Wszystkie multimedia|*.mp3;*.mp4;*.jpg;*.bcg|Pliki video|*.mp4|Obrazy|*.jpg|Pliki audio|*.mp3|Tło (gradient)|*.bcg", 
                    true, out string[] fileNames);

                string fileName = fileNames?.FirstOrDefault();
                if (result != true) return;

                var backgroubdMediaFileInfo = BaseMediaFileInfo.Factory.Create(null, fileName, null);
                backgroubdMediaFileInfo.CheckFileExist();
                backgroubdMediaFileInfo.FillMetadata();
                ViewModel.BackgroundMediaFile = backgroubdMediaFileInfo;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnClearBackgroundMediaFile_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.BackgroundMediaFile = null;
        }
    }
}
