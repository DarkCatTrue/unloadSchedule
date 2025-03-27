namespace unloadSchedule.MVVM.Model
{
    public class AllUnload
    {
        public string CurrentFile { get; set; }
        public double CurrentProgress {  get; set; } 
        public AllUnload(string currentFile, double currentProgress)
        {
            CurrentFile = currentFile;
            CurrentProgress = currentProgress;
        }
    }
}
