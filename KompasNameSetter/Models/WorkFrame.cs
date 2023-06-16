using KompasNameSetter.Commands;
using KompasNameSetter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace KompasNameSetter.Models
{
    class WorkFrame : BaseViewModel
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public WorkFrame(string name, string path)
        {
            Name = name;
            Path = path;
        }
       
    }
}
