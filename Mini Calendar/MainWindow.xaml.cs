using Newtonsoft.Json;
using System;
using System.Data.Common;
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
using System.Windows.Threading;

namespace Mini_Calendar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<CalendarEvent> events = new List<CalendarEvent>();
        private DispatcherTimer statusTimer;

        public MainWindow()
        {
            InitializeComponent();
            InitializeStatusTimer();
            LoadEventsFromSettings();
            PopulateMonthYear();
            ApplyInitialTheme();
            GenerateMonthView(DateTime.Now);

            // Show a status message to indicate the app is ready
            ShowStatusMessage("Ready");
        }

        private void LoadEventsFromSettings()
        {
            var existingEventsJson = Properties.Settings.Default.Events ?? "[]";
            events = JsonConvert.DeserializeObject<List<CalendarEvent>>(existingEventsJson) ?? new List<CalendarEvent>();
        }

        private void InitializeStatusTimer()
        {
            statusTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            statusTimer.Tick += (sender, e) =>
            {
                statusMessage.Content = string.Empty; // Clear the status message
                statusTimer.Stop(); // Stop the timer
            };
        }

        private void ShowStatusMessage(string message)
        {
            statusMessage.Content = message; // Set the status message
            statusTimer.Start(); // Start the timer to clear the message after 2 seconds
        }

        private void ApplyInitialTheme()
        {
            var themeName = Properties.Settings.Default.Theme ?? "LightTheme";
            ApplyTheme(themeName);
            UpdateThemeMenuCheckState(themeName);
        }

        private void UpdateThemeMenuCheckState(string themeName)
        {
            menuLightTheme.IsChecked = themeName == "LightTheme";
            menuDarkTheme.IsChecked = themeName == "DarkTheme";
            menuSystemTheme.IsChecked = themeName == "SystemTheme"; // Adjust if using a dynamic system theme detection
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

            SetupDayOfWeekHeaders();

            int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            int startDayOfWeek = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            int dayOffset = (int)firstDayOfMonth.DayOfWeek - startDayOfWeek;
            dayOffset = dayOffset < 0 ? dayOffset + 7 : dayOffset; // Ensure positive offset

            int totalCells = daysInMonth + dayOffset;
            int rows = totalCells / 7 + (totalCells % 7 == 0 ? 0 : 1);

            for (int i = 0; i < rows; i++)
            {
                calendarGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            for (int i = 0; i < 7; i++)
            {
                calendarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }

            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime currentDay = new DateTime(date.Year, date.Month, day);
                int column = (dayOffset + day - 1) % 7;
                int row = (dayOffset + day - 1) / 7 + 1; // +1 to account for header row

                AddDayToGrid(currentDay, column, row);
            }
        }

        private void SetupDayOfWeekHeaders()
        {
            // Add headers to the grid similar to how days are added in GenerateMonthView, but just for the first row

            // Add the day of week headers
            for (int i = 0; i < 7; i++)
            {
                TextBlock dayOfWeekBlock = new TextBlock
                {
                    Text = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[i],
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                // Apply theme dynamically to TextBlock
                dayOfWeekBlock.SetResourceReference(TextBlock.ForegroundProperty, "PrimaryForeground");

                Grid.SetRow(dayOfWeekBlock, 0);
                Grid.SetColumn(dayOfWeekBlock, i);
                calendarGrid.Children.Add(dayOfWeekBlock);
            }

            // Set up columns for days of the week
            for (int i = 0; i < 7; i++)
            {
                calendarGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // Set up rows for the days of the month
            for (int i = 0; i < 6; i++)
            {
                calendarGrid.RowDefinitions.Add(new RowDefinition());
            }
        }

        private void AddDayToGrid(DateTime day, int column, int row)
        {
            var dayEvents = events.Where(e => e.Date.Date == day.Date).ToList();

            StackPanel stackPanel = new StackPanel();
            TextBlock dayBlock = new TextBlock
            {
                Text = day.Day.ToString(),
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Apply theme dynamically to TextBlock
            dayBlock.SetResourceReference(TextBlock.ForegroundProperty, "PrimaryForeground");

            stackPanel.Children.Add(dayBlock);

            foreach (var eventData in dayEvents)
            {
                TextBlock eventBlock = new TextBlock
                {
                    Text = eventData.Title,
                    Foreground = new SolidColorBrush(Colors.White),
                    Background = new SolidColorBrush(Colors.DarkBlue),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(2)
                };

                // Apply theme dynamically to TextBlock
                eventBlock.SetResourceReference(TextBlock.ForegroundProperty, "PrimaryForeground");

                stackPanel.Children.Add(eventBlock);
            }

            Border border = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(1),
                Child = stackPanel
            };

            // Apply theme dynamically to Border
            border.SetResourceReference(Border.BorderBrushProperty, "PrimaryBorderBrush");
            border.SetResourceReference(Border.BackgroundProperty, "PrimaryBackground");

            Grid.SetRow(border, row);
            Grid.SetColumn(border, column);
            calendarGrid.Children.Add(border);
        }

        private void AddEvent_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddEventDialog();
            if (dialog.ShowDialog() == true)
            {
                var newEvent = dialog.NewEvent;
                SaveEvent(newEvent);
                //MessageBox.Show($"Added: {newEvent.Title} on {newEvent.Date.ToShortDateString()}");
                ShowStatusMessage("Event added successfully.");
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