using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
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
            PopulateMonthYear();
            GenerateMonthView(DateTime.Now);

            // Load events from settings
            var existingEventsJson = Properties.Settings.Default.Events ?? "[]";
            events = JsonConvert.DeserializeObject<List<CalendarEvent>>(existingEventsJson) ?? new List<CalendarEvent>();

            // Load the selected theme from settings
            var themeName = Properties.Settings.Default.Theme ?? "LightTheme";
            ApplyTheme(themeName);

            // Checkboxes for theme selection
            var theme = Properties.Settings.Default.Theme;
            if (theme == "LightTheme")
            {
                menuLightTheme.IsChecked = true;
            }
            else if (theme == "DarkTheme")
            {
                menuDarkTheme.IsChecked = true;
            }
            else
            {
                menuSystemTheme.IsChecked = true;
            }
        }

        private void PopulateMonthYear()
        {
            monthComboBox.ItemsSource = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.Take(12);
            monthComboBox.SelectedIndex = DateTime.Now.Month - 1;

            var currentYear = DateTime.Now.Year;
            for (int year = currentYear - 10; year <= currentYear + 10; year++)
            {
                yearComboBox.Items.Add(year);
            }
            yearComboBox.SelectedItem = currentYear;
        }

        private void MonthYearChanged(object sender, SelectionChangedEventArgs e)
        {
            if (monthComboBox.SelectedIndex < 0 || yearComboBox.SelectedItem == null) return;

            var year = (int)yearComboBox.SelectedItem;
            var month = monthComboBox.SelectedIndex + 1;
            var date = new DateTime(year, month, 1);
            GenerateMonthView(date);
        }

        private void GenerateMonthView(DateTime date)
        {
            calendarGrid.Children.Clear();
            calendarGrid.RowDefinitions.Clear();
            calendarGrid.ColumnDefinitions.Clear();

            // Add an extra row for the header
            calendarGrid.RowDefinitions.Add(new RowDefinition());

            // Set up columns for days of the week
            for (int i = 0; i < 7; i++)
            {
                calendarGrid.ColumnDefinitions.Add(new ColumnDefinition());
                var dayName = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[i];
                var header = new TextBlock
                {
                    Text = dayName,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.Bold,
                };

                // Apply theme dynamically to TextBlock
                header.SetResourceReference(TextBlock.ForegroundProperty, "PrimaryForeground");

                Grid.SetRow(header, 0);
                Grid.SetColumn(header, i);
                calendarGrid.Children.Add(header);
            }

            // Set up columns for days of the week
            for (int i = 0; i < 7; i++)
            {
                calendarGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var startDayOfWeek = (int)firstDayOfMonth.DayOfWeek;

            // Adjust start day for cultures where week starts with Sunday
            int adjustedStartDay = startDayOfWeek == 0 ? 6 : startDayOfWeek - 1;

            // Calculate the number of rows needed
            int rows = (adjustedStartDay + daysInMonth) / 7;
            rows += (adjustedStartDay + daysInMonth) % 7 > 0 ? 1 : 0;

            // Set up rows
            for (int i = 0; i < rows; i++)
            {
                calendarGrid.RowDefinitions.Add(new RowDefinition());
            }

            // Populate the grid with day numbers and names
            for (int day = 1, col = adjustedStartDay, row = 1; day <= daysInMonth; day++, col++) // Start from row 1 to skip header
            {
                if (col > 6)
                {
                    col = 0;
                    row++;
                }

                // Correctly calculate the day of the week name based on the column index
                var dayOfWeekName = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[col % 7];
                var dayBlock = new TextBlock
                {
                    Text = $"{dayOfWeekName}\n{day}",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                };

                // Apply theme dynamically to TextBlock
                dayBlock.SetResourceReference(TextBlock.ForegroundProperty, "PrimaryForeground");

                var border = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    Child = dayBlock
                };

                // Apply theme dynamically to Border
                border.SetResourceReference(Border.BorderBrushProperty, "PrimaryBorderBrush");
                border.SetResourceReference(Border.BackgroundProperty, "PrimaryBackground");

                Grid.SetRow(border, row);
                Grid.SetColumn(border, col);
                calendarGrid.Children.Add(border);
            }



            // When setting up each day cell in the grid:
            foreach (var day in Enumerable.Range(1, daysInMonth))
            {
                var currentDay = new DateTime(date.Year, date.Month, day);
                var dayEvents = events.Where(e => e.Date.Date == currentDay.Date).ToList();

                var stackPanel = new StackPanel();
                // Add day label to stackPanel as before
                foreach (var eventData in dayEvents)
                {
                    var eventBlock = new TextBlock
                    {
                        Text = eventData.Title,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White,
                        Background = Brushes.DarkBlue,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(2)
                    };
                    stackPanel.Children.Add(eventBlock);
                }

                // Add stackPanel to the calendar grid as before
                Grid.SetRow(stackPanel, currentDay.Day + adjustedStartDay);
                Grid.SetColumn(stackPanel, (int)currentDay.DayOfWeek);
                calendarGrid.Children.Add(stackPanel);
            }

        }

        private void AddEvent_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddEventDialog();
            if (dialog.ShowDialog() == true)
            {
                var newEvent = dialog.NewEvent;
                SaveEvent(newEvent);
                //MessageBox.Show($"Added: {newEvent.Title} on {newEvent.Date.ToShortDateString()}");
            }
        }

        private void SaveEvent(CalendarEvent calendarEvent)
        {
            // Add the new event
            events.Add(calendarEvent);

            // Save the updated events list back to settings
            var updatedEventsJson = JsonConvert.SerializeObject(events);
            Properties.Settings.Default.Events = updatedEventsJson;
            Properties.Settings.Default.Save();

            // Refresh the calendar to display the new event
            GenerateMonthView(DateTime.Now);
        }

        private List<CalendarEvent> GetAllEvents()
        {
            var eventsJson = Properties.Settings.Default.Events ?? "[]";
            return JsonConvert.DeserializeObject<List<CalendarEvent>>(eventsJson) ?? new List<CalendarEvent>();
        }

        private void ExportToIcal_Click(object sender, RoutedEventArgs e)
        {
            var events = GetAllEvents();
            StringBuilder icalBuilder = new StringBuilder();

            // Begin Calendar
            icalBuilder.AppendLine("BEGIN:VCALENDAR");
            icalBuilder.AppendLine("VERSION:2.0");
            icalBuilder.AppendLine("PRODID:-//Your Company//Your Product//EN");

            foreach (var calendarEvent in events)
            {
                // Begin Event
                icalBuilder.AppendLine("BEGIN:VEVENT");

                // Unique identifier for the event
                icalBuilder.AppendLine($"UID:{calendarEvent.Id}@yourdomain.com");

                // Event start and end times in UTC
                icalBuilder.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");
                icalBuilder.AppendLine($"DTSTART:{calendarEvent.Date.ToUniversalTime():yyyyMMddTHHmmssZ}");
                // Assuming the event duration is 1 hour for simplicity
                icalBuilder.AppendLine($"DTEND:{calendarEvent.Date.AddHours(1).ToUniversalTime():yyyyMMddTHHmmssZ}");

                // Event summary
                icalBuilder.AppendLine($"SUMMARY:{calendarEvent.Title}");

                // Event description
                icalBuilder.AppendLine($"DESCRIPTION:{calendarEvent.Description}");

                // End Event
                icalBuilder.AppendLine("END:VEVENT");
            }

            // End Calendar
            icalBuilder.AppendLine("END:VCALENDAR");

            // Save the iCal data to a file
            SaveIcalToFile(icalBuilder.ToString());
        }

        private void SaveIcalToFile(string icalData)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "iCalendar files (*.ics)|*.ics|All files (*.*)|*.*",
                DefaultExt = ".ics",
                FileName = "events"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, icalData);
                MessageBox.Show("Events exported successfully.", "Export to iCal", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LightTheme_Click(object sender, RoutedEventArgs e)
        {
            ApplyTheme("LightTheme");
            menuLightTheme.IsChecked = true;

            // Uncheck other theme options
            menuDarkTheme.IsChecked = false;
            menuSystemTheme.IsChecked = false;
        }

        private void DarkTheme_Click(object sender, RoutedEventArgs e)
        {
            ApplyTheme("DarkTheme");
            menuDarkTheme.IsChecked = true;
            menuSystemTheme.IsChecked = false;
            menuLightTheme.IsChecked = false;
        }

        private void SystemTheme_Click(object sender, RoutedEventArgs e)
        {
            // Determine if the system is using a light or dark theme
            var isDarkTheme = SystemParameters.HighContrast || // Fallback check for high contrast mode
                                                               // Check for Windows theme setting (Registry, System Theme APIs, etc.)
                              false; // Placeholder for actual system theme check

            ApplyTheme(isDarkTheme ? "DarkTheme" : "LightTheme");
            menuSystemTheme.IsChecked = true;
            menuLightTheme.IsChecked = false;
            menuDarkTheme.IsChecked = false;
        }

        private void ApplyTheme(string themeName)
        {
            // Clear existing resources
            Application.Current.Resources.MergedDictionaries.Clear();

            // Load and apply the new theme
            var themeUri = $"Themes/{themeName}.xaml"; // Assuming themes are in a Themes folder
            var themeResourceDictionary = new ResourceDictionary { Source = new Uri(themeUri, UriKind.Relative) };
            Application.Current.Resources.MergedDictionaries.Add(themeResourceDictionary);

            // Save the selected theme to settings
            Properties.Settings.Default.Theme = themeName;
            Properties.Settings.Default.Save();
        }
    }

    public class CalendarEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
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