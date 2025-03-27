using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unloadSchedule.MVVM.Model
{
    public class OneDayUnload
    {
        public string CurrentFile { get; set; }
        public double CurrentProgress { get; set; }
        public OneDayUnload(string _CurrentFile, double _CurrentProgress)
        {
            CurrentFile = _CurrentFile;
            CurrentProgress = _CurrentProgress;
        }
    }
}
