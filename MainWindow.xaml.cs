using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

using System.Runtime.CompilerServices;
using System.Linq.Expressions;

using CsvHelper;
using CsvHelper.Configuration;
using System.Runtime.InteropServices.WindowsRuntime;

namespace TimeTrack
{
    public partial class MainWindow : Window
    {
        TimeKeeper time_keeper;
        public MainWindow()
        {
            time_keeper = new TimeKeeper();

            InitializeComponent();
            this.DataContext = time_keeper;

            ImportFromCSV("DEBUG.csv");
            //ImportFromCSV(CSVName());

            //DgTimeRecords.ItemsSource = time_records;
            FldStartTime.Focus();
        }

        private void BtnSubmit(object sender, RoutedEventArgs e)
        {
            DateTime? start_time = TimeString.StringToDateTime(FldStartTime.Text);
            DateTime? end_time = TimeString.StringToDateTime(FldEndTime.Text);
            /*
            if (start_time != null && end_time != null)
            {
                time_records.Add(new TimeEntry((DateTime)start_time, (DateTime)end_time, FldCaseNumber.Text, FldNotes.Text));
                ClearAndSetFields();
                DgTimeRecords.SelectedIndex = time_records.Count - 1;
                DgTimeRecords.Focus();
                ExportToCSV(CSVName());
            }*/
        }

        private void BtnInsert(object sender, RoutedEventArgs e)
        {
            
            //int insert_index = DgTimeRecords.SelectedIndex >= 0 ? DgTimeRecords.SelectedIndex + 1 : time_records.Count;
            
            //time_records.Insert(insert_index, new TimeEntry());
            
            //DgTimeRecords.SelectedIndex = insert_index;
            DgTimeRecords.Focus();
             
        }

        private void BtnExport(object sender, RoutedEventArgs e)
        {
            if (DgTimeRecords.SelectedItem != null)
            {
                TimeEntry selected = (TimeEntry)DgTimeRecords.SelectedItem;
                string text = selected.StartTime + " - " + selected.EndTime + "\n" + selected.Notes;
                Clipboard.SetText(text);
                selected.Recorded = true;
            }
        }

        private void BtnMoveUp(object sender, RoutedEventArgs e)
        {
            /*
            if (DgTimeRecords.SelectedIndex > 0)
                time_records.Move(DgTimeRecords.SelectedIndex, DgTimeRecords.SelectedIndex - 1);
            */
            DgTimeRecords.Focus();
        }

        private void BtnMoveDown(object sender, RoutedEventArgs e)
        {/*
            if (DgTimeRecords.SelectedIndex >= 0 && DgTimeRecords.SelectedIndex != time_records.Count - 1)
                time_records.Move(DgTimeRecords.SelectedIndex, DgTimeRecords.SelectedIndex + 1);
            */
            DgTimeRecords.Focus();
        }

        private void ClearAndSetFields()
        {
            FldStartTime.Text = FldEndTime.Text;

            FldEndTime.Clear();
            FldCaseNumber.Clear();
            FldNotes.Clear();

            FldEndTime.Focus();
        }

        private void ClearFields()
        {
            FldStartTime.Clear();
            FldEndTime.Clear();
            FldCaseNumber.Clear();
            FldNotes.Clear();

            FldStartTime.Focus();
        }

        private string CSVName()
        {
            return "TimeTrack_" + DateTime.Today.ToString("yyyy-MM-dd") + ".csv";
        }

        public void ExportToCSV(string file)
        {/*
            try
            {
                using (var writer = new StreamWriter(file))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.RegisterClassMap<TimeEntryCSVMap>();
                    csv.WriteRecords(time_records);
                }
            }
            catch(Exception) { }*/
        }

        public void ImportFromCSV(string file)
        {/*
            try
            {
                if (File.Exists(file))
                {
                    using (var reader = new StreamReader(file))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csv.Configuration.RegisterClassMap<TimeEntryCSVMap>();
                        foreach (var i in csv.GetRecords<TimeEntry>())
                            time_records.Add(i);
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }*/
        }

        private void DgTimeRecords_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            ExportToCSV(CSVName());
        }
    }

    public class TimeEntry : INotifyPropertyChanged
    {
        public TimeEntry()
        {
            start_time = null;
            end_time = null;
            case_number = "";
            notes = "";
            ID = ID_index += 1;
        }
        public TimeEntry(DateTime in_start, DateTime in_end)
        {
            start_time = in_start;
            end_time = in_end;
            case_number = "";
            notes = "";
            ID = ID_index += 1;
        }

        public TimeEntry(DateTime in_start, DateTime in_end, string in_case, string in_notes)
        {
            start_time = in_start;
            end_time = in_end;
            case_number = in_case;
            notes = in_notes;
            ID = ID_index += 1;
        }
        
        private static int ID_index;

        private int id;
        private DateTime? start_time;
        private DateTime? end_time;
        private string case_number;
        private string notes;
        private bool recorded;

        public int ID
        {
            get { return id; }
            set { id = value; OnPropertyChanged(); }
        }
        public DateTime? StartTime
        {
            get { return start_time; }
            set { start_time = value; OnPropertyChanged(); }
        }
        public DateTime? EndTime
        {
            get { return end_time; }
            set { end_time = value; OnPropertyChanged(); }
        }
        public string CaseNumber
        {
            get { return case_number; }
            set { case_number = value; OnPropertyChanged(); }
        }
        public string Notes 
        {
            get { return notes; }
            set { notes = value; OnPropertyChanged(); }
        }
        public bool Recorded
        {
            get { return recorded; }
            set { recorded = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class TimeConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;
            else
                return ((DateTime)value).ToShortTimeString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return TimeString.StringToDateTime((string)value);
        }
    }

    public class TimeKeeper : INotifyPropertyChanged
    {
        public TimeKeeper()
        {
            time_records = new ObservableCollection<TimeEntry>();
        }

        private ObservableCollection<TimeEntry> time_records;
        public ObservableCollection<TimeEntry> TimeRecords
        {
            get { return time_records; }
            set { time_records = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;



        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public static class TimeString
    {
        static DateTime today = DateTime.Today;
        static DateTime work_hours_start = DateTime.ParseExact("07:00AM", "hh:mmtt", CultureInfo.InvariantCulture);
        static DateTime work_hours_end = DateTime.ParseExact("07:00PM", "hh:mmtt", CultureInfo.InvariantCulture);

        public static bool IsValidTimeFormat(string value)
        {
            /* Regex Explantion:
             * ^                : starting at the beginning of the string
             * \d{1,2}          : between 1, and 2 digit characters
             * [;:]?            : either ; or : - optional
             * ()               : encapsulated logic. The same as you are used to.
             * (\d{2})?         : exactly 2 digit characters - option
             * (\s?)+           : any number of whitespaces - optional
             * (...)?           : everything contained in the brackets is optional
             * (?i: ...         : contained commands will be case-insensitive
             * ...[AP]M)?      : either A, or P characters, followed by M. All optional
             * $                : the end of the string
             */
            string valid_time_format = @"^\d{1,2}[;:]?(\d{2})?((\s?)+(?i:[AP]M)?)?$";

            /* Regex:
             * Start of the string
             * 2 digits
             * either ; or : - optional
             * 2 digits - optional
             * case insensitive
             * any number of whitespaces
             * fail if "AM" is present
             * PM - optional
             * end of the string
             */
            string valid_24hour_format = @"^\d{2}[;:]?(\d{2})?(?i:(\s?)+(?!AM)(PM)?)?$";

            if (Regex.IsMatch(value, valid_time_format))
            {
                if (Is24HourFormat(value))
                {
                    return Regex.IsMatch(value, valid_24hour_format);
                }
                else
                    return true;
            }
            else
                return false;
        }
        public static bool Is24HourFormat(string value)
        {
            /* Regex: 
             * at the start of the string 
             * 2 digits
             * a non-digit character, or the end of the string (not captured)
             * OR
             * 2 more digits
             */
            if (value.Length >= 2 && Regex.IsMatch(value, @"^\d{2}((?:\D|$)|\d{2})"))
            {
                int hour = Convert.ToInt32(value.Substring(0, 2));
                return hour >= 13 && hour <= 23;
            }
            else
                return false;
        }
        public static bool TimePeriodPresent(string value)
        {
            return Regex.IsMatch(value, @"(?i)[AP]M$");
        }
        private static bool ContainsMinutes(string value)
        {
            // Regex: looks for 1 or 2 digits, a colon, then 2 digits .
            return Regex.IsMatch(value, @"^\d{1,2}:\d{2}");
        }
        private static string CleanTimeString(string value, bool remove_period = false)
        {
            value = value.Trim();
            value = value.Replace(";", ":");
            value = value.Replace(" ", "");

            bool period_present = TimePeriodPresent(value);

            if (period_present && remove_period)
                value = value.Remove(value.Length - 2, 2);

            if (!value.Contains(":"))
            {
                // Regex: if there are only 3, or 1 digits
                // followed by either a non digit, or the end of the string
                if (Regex.IsMatch(value, @"^(\d{3}|\d)(?:\D|$)"))
                    value = value.Insert(1, ":");
                else
                    value = value.Insert(2, ":");
            }

            // if there is only 1 hour digit, add a 0 to the front.
            if (Regex.IsMatch(value, @"^\d:"))
                value = "0" + value;

            return value;
        }
        private static DateTime ClampToWorkHours(DateTime value)
        {
            if (value.TimeOfDay < work_hours_start.TimeOfDay || value.TimeOfDay > work_hours_end.TimeOfDay)
            {
                switch (value.ToString("tt"))
                {
                    case "AM":
                        value = value.AddHours(12);
                        break;
                    case "PM":
                        value = value.AddHours(-12);
                        break;
                }
            }

            return value;
        }
        public static DateTime? StringToDateTime(string value)
        {
            if (!IsValidTimeFormat(value))
                return null;

            DateTime return_val;
            string time_format;
            bool time_period_present = false;
            bool format_24_hour = Is24HourFormat(value);

            value = CleanTimeString(value, format_24_hour);

            if (format_24_hour)
                time_format = "HH:";
            else
            {
                int hour_digit_count = value.Length;

                if (time_period_present = TimePeriodPresent(value))
                    hour_digit_count -= 2;

                if (hour_digit_count > 3)
                    time_format = "hh:";
                else
                    time_format = "hh:";
            }

            if (ContainsMinutes(value))
                time_format += "mm";

            if (!format_24_hour && TimePeriodPresent(value))
                time_format += "tt";

            if (!DateTime.TryParseExact(value, time_format, CultureInfo.InvariantCulture, DateTimeStyles.None, out return_val))
                return null;
            else
            {
                return_val = today.Date + return_val.TimeOfDay;

                if (!time_period_present && !format_24_hour)
                    return_val = ClampToWorkHours(return_val);

                return return_val;
            }
        }
    }

    public class TimeEntryCSVMap : ClassMap<TimeEntry>
    {
        public TimeEntryCSVMap()
        {
            Map(m => m.StartTime).Name("StartTime");
            Map(m => m.EndTime).Name("EndTime");
            Map(m => m.CaseNumber).Name("CaseNumber");
            Map(m => m.Recorded).Name("Recorded");
            Map(m => m.Notes).Name("Notes");            
        }
    }
}


/*REFERENCES
 * 
 * Data Binding
 * https://www.wpf-tutorial.com/data-binding/introduction/
 * 
 * Implementing OnPropertyChanged
 * https://docs.microsoft.com/en-us/dotnet/desktop/wpf/data/how-to-implement-property-change-notification?view=netframeworkdesktop-4.8
 * 
 * CSVHelper
 * https://joshclose.github.io/CsvHelper/getting-started/ 
 * 
 * DataGrid Post-Editing Event
 * https://stackoverflow.com/questions/3938040/wpf-datagrid-row-editing-ended-event/49948858
 * 
 * Embedding DLL/referencess
 * https://stackoverflow.com/questions/189549/embedding-dlls-in-a-compiled-executable
 * 
 * Handling/Validating Text Input
 * https://stackoverflow.com/questions/1268552/how-do-i-get-a-textbox-to-only-accept-numeric-input-in-wpf
 */