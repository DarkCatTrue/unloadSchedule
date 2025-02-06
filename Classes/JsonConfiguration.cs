namespace unloadSchedule
{
    public class JsonConfiguration
    {
        public string SchedulePath { get; set; }
        public string Ip { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public JsonConfiguration(string schedulePath, string ip, string login, string password)
        {
            SchedulePath = schedulePath;
            Ip = ip;
            Login = login;
            Password = password;
        }
    }
    public class CurrentTask
    {
        public string ScheduleDate { get; set; }
        public string CurrentFile { get; set; }
        public CurrentTask(string scheduleDate, string currentFile)
        {
            ScheduleDate = scheduleDate;
            CurrentFile = currentFile;
        }
    }

}
