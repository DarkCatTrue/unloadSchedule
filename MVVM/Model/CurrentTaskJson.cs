using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unloadSchedule.MVVM.Model
{
    public class CurrentTaskJson
    {
        string filepath = @"Jsons\currentTask.json";
        public string ScheduleDate { get; set; }
        public string CurrentFile { get; set; }
        public CurrentTaskJson(string scheduleDate, string currentFile)
        {
            ScheduleDate = scheduleDate;
            CurrentFile = currentFile;
        }
    }
}
