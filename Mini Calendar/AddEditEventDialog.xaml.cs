using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Mini_Calendar
{
    /// <summary>
    /// Interaction logic for AddEventDialog.xaml
    /// </summary>
    public partial class AddEditEventDialog : Window
    {
        public CalendarEvent eventData { get; private set; } = new CalendarEvent();

        public AddEditEventDialog(CalendarEvent? existingEvent = null)
        {
            InitializeComponent();

            // If an existing event was passed in, populate the dialog's fields with the event's data
            if (existingEvent != null)
            {
                titleTextBox.Text = existingEvent.Title;
                datePicker.SelectedDate = existingEvent.Date;
                descriptionTextBox.Text = existingEvent.Description;
                btnSaveEvent.Content = "Save";
                Title = "Edit Event";
            }
        }

        private void SaveEvent_Click(object sender, RoutedEventArgs e)
        {
            // Create a new event with the dialog's input data
            eventData = new CalendarEvent
            {
                Title = titleTextBox.Text,
                Date = datePicker.SelectedDate ?? DateTime.Now,
                Description = descriptionTextBox.Text
            };

            // Close the dialog
            this.DialogResult = true;
        }
    }
}
