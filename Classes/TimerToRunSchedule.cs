using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace unloadSchedule.Classes
{
    public class TimerToRunSchedule
    {
        public async Task Timer (Func<Task> ReturnFilesSchedule, Func<Task> StartUpload, DateTime targetTime)
        {
            TimeSpan delay = targetTime - DateTime.Now;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay);
                await ReturnFilesSchedule();
                await StartUpload();
            }
        }
    }
}
