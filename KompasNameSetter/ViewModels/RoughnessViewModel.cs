using Kompas6API5;
using Kompas6Constants;
using KompasAPI7;
using KompasNameSetter.Commands;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    class RoughnessViewModel : BaseViewModel
    {
        private List<string> files;
        private Brush folderButtonColor;
        private string roughnessValue;
        private string infoText;
        private _Application kompas;
        private double i;
        private double _currentProgress;
        private string progressText;
        private Brush fileButtonColor;
        private string roughTypeSelected;
        private bool addSign;
        private readonly BackgroundWorker worker;

        public ICommand OpenFolder { get; set; }
        public ICommand Change { get; set; }
        public ICommand OpenFiles { get; set; }

        public double CurrentProgress { get => _currentProgress; set { _currentProgress = value; OnPropertyChanged(); } }
        public string ProgressText { get => progressText; set { progressText = value; OnPropertyChanged(); } }
        public string RoughnessValue { get => roughnessValue; set { roughnessValue = value; OnPropertyChanged(); } }
        public Brush FolderButtonColor { get => folderButtonColor; set { folderButtonColor = value; OnPropertyChanged(); } }
        public string InfoText { get => infoText; set { infoText = value; OnPropertyChanged(); } }
        public string RoughTypeSelected { get => roughTypeSelected; set { roughTypeSelected = value; OnPropertyChanged(); } }

        public Brush FileButtonColor { get => fileButtonColor; set { fileButtonColor = value; OnPropertyChanged(); } }
        public bool AddSign { get => addSign; set { addSign = value; OnPropertyChanged(); } }

        public ObservableCollection<string> Types { get; set; }

        public RoughnessViewModel()
        {
            OpenFolder = new Command(OpenFolderExecute);
            Change = new Command(ChangeExecute);
            OpenFiles = new Command(OpenFilesExecute);

            Types = new ObservableCollection<string>()
            {
                "Без указания типа обработки",
                "С удалением слоя материала",
                "Без удаления слоя материала"
            };

            worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };

            worker.ProgressChanged += OnProgressChanged;
            worker.DoWork += DoWork;
            worker.RunWorkerCompleted += RunWorkerCompleted;

            RoughTypeSelected = Types[0];
        }

        private void OpenFilesExecute(object obj)
        {
            files = new List<string>();
            var ofd = new OpenFileDialog()
            {
                Multiselect = true,
                Title = "Выберите файлы",
                Filter = "Чертежи(*.cdw)|*.cdw",
            };
            if (ofd.ShowDialog() == true)
            {
                files = ofd.FileNames.ToList();
                FileButtonColor = Brushes.LightGreen;
                FolderButtonColor = Brushes.LightGray;
            }
            InfoText = $"Выбрано {files.Count} файлов";
        }

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CurrentProgress = 0;
            ProgressText = "";
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            i = 0;
            kompas = (_Application)Activator.CreateInstance(Type.GetTypeFromProgID("KOMPAS.Application.7"));

            foreach (var path in files)
            {
                i += 1;

                (sender as BackgroundWorker).ReportProgress((int)((i / files.Count) * 100), null);

                kompas.Documents.Open(path);

                IKompasDocument kompasDocument = kompas.ActiveDocument;
                IKompasDocument2D kompasDocument2D = (IKompasDocument2D)kompasDocument;

                IDrawingDocument drawingDocument = (IDrawingDocument)kompasDocument;

                switch (RoughTypeSelected)
                {
                    case "Без указания типа обработки":
                        drawingDocument.SpecRough.SignType = ksRoughSignEnum.ksNoProcessingType;
                        break;
                    case "С удалением слоя материала":
                        drawingDocument.SpecRough.SignType = ksRoughSignEnum.ksDeleteMaterial;
                        break;
                    case "Без удаления слоя материала":
                        drawingDocument.SpecRough.SignType = ksRoughSignEnum.ksWithoutDeleteMaterial;
                        break;
                }


                drawingDocument.SpecRough.AddSign = AddSign;
                drawingDocument.SpecRough.Text = RoughnessValue == null || RoughnessValue == "" ? RoughnessValue : "Ra " + RoughnessValue;
                drawingDocument.SpecRough.Update();

                kompasDocument2D.Save();
                kompasDocument2D.Close(DocumentCloseOptions.kdDoNotSaveChanges);
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
                    files = Directory.GetFiles(fbd.SelectedPath, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".cdw")).ToList();
                    FolderButtonColor = Brushes.LightGreen;
                    FileButtonColor = Brushes.LightGray;
                }
            }

            InfoText = $"Выбрано {files.Count} файлов";
        }
    }
}
