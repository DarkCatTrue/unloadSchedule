namespace unloadSchedule.MVVM.Model
{
    public class CurrentTaskJson
    {
        public string ScheduleDate { get; set; }
        public string CurrentFile { get; set; }
        public CurrentTaskJson(string scheduleDate, string currentFile)
        {
            ScheduleDate = scheduleDate;
            CurrentFile = currentFile;
        }
    }
}
