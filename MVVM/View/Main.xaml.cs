using System.Windows;
using System.Windows.Controls;

namespace unloadSchedule
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        public Main()
        {
            InitializeComponent();
            DataContext = new ViewModel();
        }
    }
}
