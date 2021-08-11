using AsystentZOOM.VM.Common;
using AsystentZOOM.VM.Common.Dialog;
using AsystentZOOM.VM.Enums;
using AsystentZOOM.VM.Interfaces;
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
    /// Interaction logic for BackgroundPanelView.xaml
    /// </summary>
    public partial class BackgroundPanelView : UserControl, IPanelView<BackgroundVM>
    {
        public BackgroundPanelView()
        {
            InitializeComponent();

            cmbGradientDirection.ItemsSource = new Dictionary<GradientDirectionEnum, string>
            {
                { GradientDirectionEnum.None, "" },
                { GradientDirectionEnum.CenterToEdge, "Od środka" },
                { GradientDirectionEnum.LeftToRight, "Od lewej do prawej" },
                { GradientDirectionEnum.TopToDown, "Od góry do dołu" }
            };
        }

        public int PanelNr => 2;
        public string PanelName => "Tło";
        public void ChangeVisible(bool visible)
        {
        }

        public BackgroundVM ViewModel 
            => (BackgroundVM)DataContext;

        private class IoConsts
        {
            internal static string InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Asystent ZOOM\Tło";
            internal const string DefaultExt = "bcg";
            internal const string Filter = "Pliki ustawień tła|*.bcg";
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            
            try
            {
                bool? result = DialogHelper.ShowOpenFile("Załaduj ustawienia tła", IoConsts.Filter, false, out string[] fileNames);
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
                        var viewModel = (BackgroundVM)xmlSerializer.Deserialize(f);
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

                bool? result = DialogHelper.ShowSaveFile("Zapisz ustawienia tła", IoConsts.Filter, true, IoConsts.DefaultExt, IoConsts.InitialDirectory, out string[] fileNames);
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
    }
}
