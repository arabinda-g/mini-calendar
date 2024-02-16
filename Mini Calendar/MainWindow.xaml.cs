using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mini_Calendar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<CalendarEvent> events = new List<CalendarEvent>();

        public MainWindow()
        {
            InitializeComponent();
            PopulateCalendar(DateTime.Now);
        }

        private void PopulateCalendar(DateTime date)
        {
            var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            var days = new List<CalendarDayViewModel>();

            for (int day = 1; day <= daysInMonth; day++)
            {
                var dayDate = new DateTime(date.Year, date.Month, day);
                days.Add(new CalendarDayViewModel { Date = dayDate });
            }

            calendarDisplay.ItemsSource = days;
        }

        private void AddEvent_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddEventDialog();
            if (dialog.ShowDialog() == true)
            {
                var newEvent = dialog.NewEvent;
                MessageBox.Show($"Added: {newEvent.Title} on {newEvent.Date.ToShortDateString()}");
            }
        }


        private void ExportToIcal_Click(object sender, RoutedEventArgs e)
        {
        }

        private void LightTheme_Click(object sender, RoutedEventArgs e)
        {
        }

        private void DarkTheme_Click(object sender, RoutedEventArgs e)
        {
        }

        private void SystemTheme_Click(object sender, RoutedEventArgs e)
        {
        }
    }

    public class CalendarEvent
    {
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
    }

    public class CalendarDayViewModel
    {
        public DateTime Date { get; set; }
        public List<CalendarEvent> Events { get; set; } = new List<CalendarEvent>();
    }


}