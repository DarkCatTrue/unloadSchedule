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

        private void scheduledUnload_Checked(object sender, RoutedEventArgs e)
        {
            if (scheduledUnload.IsChecked == true)
            {
                timeReserv.IsEnabled = true;
                datePicker.IsEnabled = true;
                unloadList.Visibility = Visibility.Visible;
                FirstList.Visibility = Visibility.Visible;
                SecondList.Visibility = Visibility.Visible;
                ThirdList.Visibility = Visibility.Visible;
            }
        }

        private void scheduledUnload_Unchecked(object sender, RoutedEventArgs e)
        {
            if (scheduledUnload.IsChecked == false)
            {
                timeReserv.IsEnabled = false;
                datePicker.IsEnabled = false;
                timeReserv.Text = string.Empty;
                datePicker.Text = string.Empty;
                unloadList.Visibility = Visibility.Collapsed;
                FirstList.Visibility = Visibility.Collapsed;
                SecondList.Visibility = Visibility.Collapsed;
                ThirdList.Visibility = Visibility.Collapsed;
            }
        }
    }
}
