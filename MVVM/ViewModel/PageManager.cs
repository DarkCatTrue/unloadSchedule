using System.Windows.Controls;

namespace unloadSchedule
{
    public class PageManager
    {
        public Frame mainFrame;
        public PageManager(Frame frame)
        {
            mainFrame = frame;
        }
        public void ChangePage(Page page)
        {
            mainFrame.Navigate(page);
        }
    }

}
