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
    public partial class AddEventDialog : Window
    {
        public CalendarEvent NewEvent { get; private set; }

        public AddEventDialog()
        {
            InitializeComponent();
        }

        private void AddEvent_Click(object sender, RoutedEventArgs e)
        {
            // Create a new event with the dialog's input data
            NewEvent = new CalendarEvent
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
