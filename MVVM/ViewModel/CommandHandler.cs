using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static MaterialDesignThemes.Wpf.Theme;

namespace unloadSchedule.MVVM.ViewModel
{
    public class CommandHandler
    {
        public ICommand GotoSettingsCommand { get; set; }
        public ICommand GotoMainCommand {  get; set; }

        public CommandHandler()
        {
            GotoSettingsCommand = new RelayCommand(SettingsPage);
            GotoMainCommand = new RelayCommand(MainPage);
        }
        public void SettingsPage()
        {
            Page settings = new settingsPage();
            MainWindow.pageManager.ChangePage(settings);
        }
        public void MainPage()
        {
            Page main = new Main();
            MainWindow.pageManager.ChangePage(main);
        }
    }
}
