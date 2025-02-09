using Newtonsoft.Json;
using System.IO;

namespace unloadSchedule
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
