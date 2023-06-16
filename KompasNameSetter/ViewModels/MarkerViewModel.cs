using Kompas6API5;
using Kompas6Constants3D;
using KompasAPI7;
using KompasNameSetter.Commands;
using KompasNameSetter.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace KompasNameSetter.ViewModels
{
    class MarkerViewModel : BaseViewModel
    {
        private KompasMaster master;
        private List<string> files;
        private Brush folderButtonColor;
        private string infoText;
        private double _currentProgress;
        private KompasObject kompas;
        private _Application kompas7;
        private ksDocument3D doc3D;
        private double i;
        private string progressText;
        private string oldMarkerText;
        private string newMarkerText;
        private Brush fileButtonColor;
        private readonly BackgroundWorker worker;

        public string OldMarkerText { get => oldMarkerText; set { oldMarkerText = value; OnPropertyChanged(); } }
        public string NewMarkerText { get => newMarkerText; set { newMarkerText = value; OnPropertyChanged(); } }
        public string InfoText { get => infoText; set { infoText = value; OnPropertyChanged(); } }
        public string ProgressText { get => progressText; set { progressText = value; OnPropertyChanged(); } }
        public Brush FolderButtonColor { get => folderButtonColor; set { folderButtonColor = value; OnPropertyChanged(); } }
        public double CurrentProgress { get => _currentProgress; set { _currentProgress = value; OnPropertyChanged(); } }
        public Brush FileButtonColor { get => fileButtonColor; set { fileButtonColor = value; OnPropertyChanged(); } }
        public ICommand OpenFolder { get; set; }
        public ICommand OpenFiles { get; set; }
        public ICommand Change { get; set; }
        public ICommand OnSource { get; set; }

        public MarkerViewModel()
        {
            master = new KompasMaster();

            OpenFolder = new Command(OpenFolderExecute);
            OpenFiles = new Command(OpenFilesExecute);
            Change = new Command(ChangeExecute);
            OnSource = new Command(OnSourceExecute);

            worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            worker.ProgressChanged += OnProgressChanged;
            worker.RunWorkerCompleted += RunWorkerCompleted;
        }

        private void OnSourceExecute(object obj)
        {
            worker.DoWork += OnSourceDoWork;
            ProgressText = "Не выключайте программу";
            worker.RunWorkerAsync();
        }

        private void OnSourceDoWork(object sender, DoWorkEventArgs e)
        {
            if (files.Count == 0)
                return;

            List<string> a3dFiles = files.Where(f => f.EndsWith(".a3d")).ToList();

            i = 0;
            kompas = (KompasObject)Activator.CreateInstance(Type.GetTypeFromProgID("KOMPAS.Application.5"));
            kompas7 = (_Application)Activator.CreateInstance(Type.GetTypeFromProgID("KOMPAS.Application.7"));
            doc3D = (ksDocument3D)kompas.Document3D();

            foreach (var path in a3dFiles)
            {
                i += 1;

                (sender as BackgroundWorker).ReportProgress((int)((i / files.Count) * 100), null);

                doc3D.Open(path, false);

                var kompasDocument = kompas7.ActiveDocument;
                var kompasDocument3D = (IKompasDocument3D)kompasDocument;

                var iTopPart = kompasDocument3D.TopPart;
                IPropertyMng propertyManager = kompas7 as IPropertyMng;
                var iPropertyMarkup = propertyManager.GetProperty(kompasDocument, 4.0);
                var iPropertySect = propertyManager.GetProperty(kompasDocument, 20.0);

                for (int i = 0; i < kompasDocument3D.TopPart.Parts.Count; i++)
                {
                    IPart7 iPart7 = iTopPart.PartsEx[0][i];

                    IPropertyKeeper propertyKeeper = (IPropertyKeeper)iPart7;
                    propertyKeeper.GetPropertyValue(iPropertySect, out object value, true, out bool fromSource);
                    propertyKeeper.SetPropertyValue(iPropertySect, null, true);
                    propertyKeeper.GetPropertyValue(iPropertySect, out value, true, out fromSource);

                    propertyKeeper.SetPropertyValue(iPropertyMarkup, null, true);
                    iPart7.Update();
                }

                doc3D.Save();
                doc3D.close();
            }

            kompas.Quit();
            kompas = null;
            MessageBox.Show($"Операция завершена. Переименовано {i} файлов");
        }

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CurrentProgress = 0;
            ProgressText = "";
            worker.DoWork -= DoWork;
            worker.DoWork -= OnSourceDoWork;
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            i = 0;
            kompas = (KompasObject)Activator.CreateInstance(Type.GetTypeFromProgID("KOMPAS.Application.5"));

            doc3D = (ksDocument3D)kompas.Document3D();

            foreach (var path in files)
            {
                i += 1;

                (sender as BackgroundWorker).ReportProgress((int)((i / files.Count) * 100), null);

                doc3D.Open(path, false);

                ksPart part = doc3D.GetPart((int)Part_Type.pTop_Part);


                if (OldMarkerText == null || OldMarkerText == "")
                    part.marking = NewMarkerText;
                else
                    part.marking = part.marking.Replace(OldMarkerText, NewMarkerText);

                part.Update();

                doc3D.Save();
                doc3D.close();
            }

            kompas.Quit();
            kompas = null;
            MessageBox.Show($"Операция завершена. Переименовано {i} файлов");
        }

        private void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            CurrentProgress = e.ProgressPercentage;
        }

        private void ChangeExecute(object obj)
        {
            worker.DoWork += DoWork;
            ProgressText = "Не выключайте программу";
            worker.RunWorkerAsync();
        }

        private void OpenFolderExecute(object obj)
        {
            files = new List<string>();
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {

                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    files = Directory.GetFiles(fbd.SelectedPath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".a3d") || s.EndsWith(".m3d")).ToList();
                    FolderButtonColor = Brushes.LightGreen;
                }
            }

            InfoText = $"Выбрано {files.Count} файлов";
        }

        private void OpenFilesExecute(object obj)
        {
            files = new List<string>();
            var ofd = new OpenFileDialog()
            {
                Multiselect = true,
                Title = "Выберите файлы",
                Filter = "3D(*.m3d, *.a3d)|*.m3d; *.a3d",
            };
            if (ofd.ShowDialog() == true)
            {
                files = ofd.FileNames.ToList();
                FileButtonColor = Brushes.LightGreen;
                FolderButtonColor = Brushes.LightGray;
            }
            InfoText = $"Выбрано {files.Count} файлов";
        }
    }
}
