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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WorkingDaysCalculatorWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }



        private void CalculateButton_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                DateTime startDate = StartDatePicker.SelectedDate ?? DateTime.Now;
                DateTime endDate = EndDatePicker.SelectedDate ?? DateTime.Now;
                bool excludeSundays = ExcludeSundaysComboBox.SelectedIndex == 0;
                int excludeSaturdays = ExcludeSaturdaysComboBox.SelectedIndex + 1;

                int officialHolidays = int.Parse(OfficialHolidaysTextBox.Text);
                int medicalLeaves = int.Parse(MedicalLeavesTextBox.Text);
                int annualLeaves = int.Parse(AnnualLeavesTextBox.Text);
                int casualLeaves = int.Parse(CasualLeavesTextBox.Text);
                int paternityLeaves = int.Parse(PaternityLeavesTextBox.Text);
                int otherLeaves = int.Parse(OtherLeavesTextBox.Text);

                // Calculate the total number of days
                int totalDays = (endDate - startDate).Days + 1;

                // Initialize counters
                int sundayCount = 0;
                int saturdayCount = 0;
                int totalWeeks = 0;
                int workingDays = totalDays;

                // Track alternate Saturdays
                bool isAlternateSaturday = true;

                // List of non-working days
                List<DateTime> nonWorkingDays = new List<DateTime>();

                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    if (date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        sundayCount++;
                        if (excludeSundays)
                        {
                            workingDays--;
                            nonWorkingDays.Add(date);
                        }
                    }
                    else if (date.DayOfWeek == DayOfWeek.Saturday)
                    {
                        saturdayCount++;
                        if (excludeSaturdays == 3)
                        {
                            if (isAlternateSaturday)
                            {
                                workingDays--;
                                nonWorkingDays.Add(date);
                            }
                            isAlternateSaturday = !isAlternateSaturday;
                        }
                        else if (excludeSaturdays == 1)
                        {
                            workingDays--;
                            nonWorkingDays.Add(date);
                        }
                    }
                }

                totalWeeks = (totalDays + 6) / 7;

                workingDays -= (officialHolidays + medicalLeaves + annualLeaves + casualLeaves + paternityLeaves + otherLeaves);

                double months = ((endDate.Year - startDate.Year) * 12.0 + endDate.Month - startDate.Month + 1);

                // Display the stats
                ResultTextBlock.Text = $"\nStats:\n" +
                                       $"Total days: {totalDays}\n" +
                                       $"Total Sundays: {sundayCount}\n" +
                                       $"Total Saturdays: {saturdayCount}\n" +
                                       $"Total weeks: {totalWeeks}\n" +
                                       $"Average days per month: {totalDays / months:F2}\n" +
                                       $"Average weeks per month: {totalWeeks / months:F2}\n" +
                                       $"Working Days: {workingDays}\n" +
                                       $"Average working days per month: {workingDays / months:F2}";

                // Generate the calendar view
                GenerateCalendarView(startDate, endDate, nonWorkingDays);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: Your input is not in the correct format. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset the date pickers
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;

            // Reset the dropdowns
            ExcludeSundaysComboBox.SelectedIndex = 0; // Assuming index 0 is the default value
            ExcludeSaturdaysComboBox.SelectedIndex = 0;

            // Clear all textboxes
            OfficialHolidaysTextBox.Clear();
            MedicalLeavesTextBox.Clear();
            AnnualLeavesTextBox.Clear();
            CasualLeavesTextBox.Clear();
            PaternityLeavesTextBox.Clear();
            OtherLeavesTextBox.Clear();

            // Clear any statistics or result labels
            ResultTextBlock.Text = "";
        }

        private void GenerateCalendarView(DateTime startDate, DateTime endDate, List<DateTime> nonWorkingDays)
        {
            CalendarPanel.Children.Clear();

            DateTime currentDate = startDate;
            while (currentDate <= endDate)
            {
                Grid monthGrid = new Grid
                {
                    Margin = new Thickness(10),
                    Width = 200, // Fixed width for compact layout
                    Height = 220 // Adjust height as needed
                };

                monthGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) }); // Month name row
                for (int i = 0; i < 6; i++) // Up to 6 weeks in a month
                {
                    monthGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
                }
                for (int i = 0; i < 7; i++) // 7 days in a week
                {
                    monthGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }

                // Month label
                TextBlock monthLabel = new TextBlock
                {
                    Text = currentDate.ToString("MMMM yyyy"),
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(monthLabel, 0);
                Grid.SetColumnSpan(monthLabel, 7);
                monthGrid.Children.Add(monthLabel);

                // Days of the week headers
                string[] daysOfWeek = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
                for (int i = 0; i < daysOfWeek.Length; i++)
                {
                    TextBlock dayHeader = new TextBlock
                    {
                        Text = daysOfWeek[i],
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontWeight = FontWeights.Bold,
                        FontSize = 12
                    };
                    Grid.SetRow(dayHeader, 1);
                    Grid.SetColumn(dayHeader, i);
                    monthGrid.Children.Add(dayHeader);
                }

                // Fill in the days
                int daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
                DateTime firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
                int startDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
                int row = 2;

                for (int day = 1; day <= daysInMonth; day++)
                {
                    TextBlock dayBlock = new TextBlock
                    {
                        Text = day.ToString(),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Width = 25,
                        Height = 25,
                        Margin = new Thickness(2),
                        Background = Brushes.Transparent,
                    };

                    DateTime date = new DateTime(currentDate.Year, currentDate.Month, day);
                    if (nonWorkingDays.Contains(date))
                    {
                        dayBlock.Background = Brushes.LightGray;
                    }

                    Grid.SetRow(dayBlock, row);
                    Grid.SetColumn(dayBlock, startDayOfWeek);
                    monthGrid.Children.Add(dayBlock);

                    startDayOfWeek++;
                    if (startDayOfWeek > 6) // Move to the next row (next week)
                    {
                        startDayOfWeek = 0;
                        row++;
                    }
                }

                // Add the month grid to the CalendarPanel
                CalendarPanel.Children.Add(monthGrid);

                // Move to the next month
                currentDate = currentDate.AddMonths(1);
            }
        }


    }
}
