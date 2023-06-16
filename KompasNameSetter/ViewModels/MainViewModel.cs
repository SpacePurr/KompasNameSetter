using Kompas6API5;
using Kompas6Constants3D;
using KompasNameSetter.Commands;
using KompasNameSetter.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace KompasNameSetter.ViewModels
{
    class MainViewModel : BaseViewModel
    {
        public WorkFrame SelectedItem { get => selectedItem; set { selectedItem = value; OnPropertyChanged(); } }
        private string currentFrame;
        private WorkFrame selectedItem;

        public ObservableCollection<WorkFrame> Frames { get; set; }

        public string CurrentFrame { get => currentFrame; set { currentFrame = value; OnPropertyChanged(); } }

        public MainViewModel()
        {
            Frames = new ObservableCollection<WorkFrame>()
            {
                new WorkFrame("Обозначение", "\\KompasNameSetter;component/Views/MarkerView.xaml"),
                new WorkFrame("Шероховатость", "\\KompasNameSetter;component/Views/RoughnessView.xaml")
            };
        }


        public void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            CurrentFrame = SelectedItem.Path;

        }


    }
}
