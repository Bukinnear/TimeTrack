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
using CsvHelper.Configuration.Attributes;
using System.Runtime.InteropServices;

namespace TimeTrack
{
    public partial class MainWindow : Window
    {
        private TimeKeeper time_keeper;

        public MainWindow()
        {
            time_keeper = new TimeKeeper();

            InitializeComponent();
            this.DataContext = time_keeper;

            ImportFromCSV("DEBUG.csv");
            //ImportFromCSV(CSVName());

            FldStartTime.Focus();
            time_keeper.UpdateSelectedTime(-1);
        }

        private void BtnSubmit(object sender, RoutedEventArgs e)
        {
            if (time_keeper.SubmitEntry())
            {
                time_keeper.ClearFieldsAndSetStartTime();
                DgTimeRecords.SelectedIndex = time_keeper.Entries.Count - 1;
                DgTimeRecords.Focus();
                ExportToCSV(CSVName());
            }
        }

        private void BtnInsert(object sender, RoutedEventArgs e)
        {
            if (time_keeper.InsertEntry(DgTimeRecords.SelectedIndex - 1, new TimeEntry()))
            {
                DgTimeRecords.SelectedIndex = DgTimeRecords.SelectedIndex + 1;
                DgTimeRecords.Focus();
                ExportToCSV(CSVName());
            }
        }

        private void BtnExport(object sender, RoutedEventArgs e)
        {
            if (DgTimeRecords.SelectedItem != null)
            {
                TimeEntry selected = (TimeEntry)DgTimeRecords.SelectedItem;
                string text = ((DateTime)selected.StartTime).ToShortTimeString() + " - " + 
                    ((DateTime)selected.EndTime).ToShortTimeString() + "\n" + selected.Notes;
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

        private void BtnUpdate(object sender, RoutedEventArgs e)
        {
            time_keeper.UpdateTimeTotals();
            time_keeper.UpdateSelectedTime(DgTimeRecords.SelectedIndex);
            ExportToCSV(CSVName());
        }

        private string CSVName()
        {
            return "TimeTrack_" + time_keeper.Today.ToString("yyyy-MM-dd") + ".csv";
        }

        public void ExportToCSV(string file)
        {
            try
            {
                using (var writer = new StreamWriter(file))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.RegisterClassMap<TimeEntryCSVMap>();
                    csv.WriteRecords(time_keeper.Entries);
                }
            }
            catch(Exception) { }
        }

        public void ImportFromCSV(string file)
        {
            try
            {
                if (File.Exists(file))
                {
                    using (var reader = new StreamReader(file))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csv.Configuration.RegisterClassMap<TimeEntryCSVMap>();
                        foreach (var i in csv.GetRecords<TimeEntry>())
                            time_keeper.AddEntry(i);
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

        private void DgTimeRecords_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            ExportToCSV(CSVName());
        }

        private void DgTimeRecords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgTimeRecords.SelectedItem != null)
                time_keeper.UpdateSelectedTime(DgTimeRecords.SelectedIndex);
        }

        private void ChkLunch_Checked(object sender, RoutedEventArgs e)
        {
            ChkOther.IsChecked = false;

            time_keeper.CaseNumberField = null;
            FldCaseNumber.IsEnabled = false;
            FldCaseNumber.Background = Brushes.LightGray;

            time_keeper.NotesField = "Lunch";
            FldNotes.IsEnabled = false;
            FldNotes.Background = Brushes.LightGray;
        }
        private void ChkLunch_Unchecked(object sender, RoutedEventArgs e)
        {
            time_keeper.CaseNumberField = null;
            FldCaseNumber.IsEnabled = true;
            FldCaseNumber.Background = Brushes.White;

            time_keeper.NotesField = null;
            FldNotes.IsEnabled = true;
            FldNotes.Background = Brushes.White;
        }
        private void ChkOther_Checked(object sender, RoutedEventArgs e)
        {
            ChkLunch.IsChecked = false;

            time_keeper.CaseNumberField = "Other";
            FldCaseNumber.IsEnabled = false;
            FldCaseNumber.Background = Brushes.LightGray;
        }
        private void ChkOther_Unchecked(object sender, RoutedEventArgs e)
        {
            time_keeper.CaseNumberField = null;
            FldCaseNumber.IsEnabled = true;
            FldCaseNumber.Background = Brushes.White;
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

    public class TimeKeeper : INotifyPropertyChanged
    {
        public TimeKeeper()
        {
            time_records = new ObservableCollection<TimeEntry>();
            today = DateTime.Today;
        }

        private DateTime today;
        private ObservableCollection<TimeEntry> time_records;
        private string start_time;
        private string end_time;
        private string case_no;
        private string notes;

        private double hours_total;
        private double gaps_total;
        private string selected_hours;
        private string selected_mins;

        // Accessor functions

        public DateTime Today { get => today; }

        public ObservableCollection<TimeEntry> Entries
        {
            get => time_records; 
            set { time_records = value; OnPropertyChanged(); }
        }

        public string StartTimeField
        {
            get => start_time;
            set { start_time = value; OnPropertyChanged(); }
        }

        public string EndTimeField
        {
            get => end_time; 
            set { end_time = value; OnPropertyChanged(); }
        }

        public DateTime? StartTimeFieldAsTime() => TimeStringConverter.StringToDateTime(start_time); 

        public DateTime? EndTimeFieldAsTime() => TimeStringConverter.StringToDateTime(end_time); 

        public string CaseNumberField
        {
            get => case_no; 
            set { case_no = value; OnPropertyChanged(); }
        }

        public string NotesField
        {
            get => notes; 
            set { notes = value; OnPropertyChanged(); }
        }

        public double HoursTotal 
        { 
            get => hours_total; 
            set { hours_total = value; OnPropertyChanged(); } 
        }

        public double GapsTotal
        {
            get => gaps_total;
            set { gaps_total = value; OnPropertyChanged(); }
        }

        public string SelectedHours
        {
            get => selected_hours;
            set { selected_hours = value; OnPropertyChanged(); }
        }

        public string SelectedMins
        {
            get => selected_mins;
            set { selected_mins = value; OnPropertyChanged(); }
        }


        // Functions

        public void AddEntry(DateTime start_time, DateTime end_time, string case_number = "", string notes = "")
        {
            time_records.Add(new TimeEntry(start_time, end_time, case_number, notes));
            UpdateTimeTotals();
        }

        public void AddEntry(TimeEntry entry)
        {
            time_records.Add(entry);
            UpdateTimeTotals();
        }

        public bool InsertEntry(int index, DateTime start_time, DateTime end_time, string case_number = "", string notes = "")
        {
            if (index <= time_records.Count)
            {
                time_records.Insert(index, new TimeEntry(start_time, end_time, case_number, notes));
                UpdateTimeTotals();
                return true;
            }
            else
                return false;
        }

        public bool InsertEntry(int index, TimeEntry entry)
        {
            if (index <= time_records.Count)
            {
                time_records.Insert(index, entry);
                UpdateTimeTotals();
                return true;
            }
            else
                return false;
        }

        public bool SubmitEntry()
        {
            DateTime? start_time = StartTimeFieldAsTime();
            DateTime? end_time = EndTimeFieldAsTime();

            if (start_time != null && end_time != null)
            {
                AddEntry((DateTime)start_time, (DateTime)end_time, case_no, notes);
                return true;
            }
            else
                return false;
        }

        public void ClearFieldsAndSetStartTime()
        {
            StartTimeField = ((DateTime)EndTimeFieldAsTime()).ToShortTimeString();

            EndTimeField = "";
            CaseNumberField = "";
            NotesField = "";
        }

        public void ClearFields()
        {
            StartTimeField = "";
            EndTimeField = "";
            CaseNumberField = "";
            NotesField = "";
        }

        public void UpdateTimeTotals()
        {
            TimeSpan time = new TimeSpan();
            TimeSpan gap = new TimeSpan();

            foreach (var i in Entries)
            {
                if (i.StartTime != null && i.EndTime != null)
                {
                    if (i.CaseNumber != null && i.CaseNumber != "")
                        time += (TimeSpan)(i.EndTime - i.StartTime);
                    else
                    {
                        if (i.Notes != null && i.Notes.ToLower().Trim() == "lunch")
                            continue;
                        else
                            gap += (TimeSpan)(i.EndTime - i.StartTime);
                    }
                }
            }

            HoursTotal = Math.Round(time.TotalHours, 2, MidpointRounding.AwayFromZero);
            GapsTotal = gap.TotalMinutes;
        }

        public void UpdateSelectedTime(int index)
        {
            if (index >= 0 && index < time_records.Count)
            {
                var time_span = (time_records[index].EndTime - time_records[index].StartTime);
                if (time_span != null)
                {
                    SelectedHours = (((TimeSpan)time_span).Hours).ToString();
                    SelectedMins = (((TimeSpan)time_span).Minutes).ToString();
                }                
            }
            else
            {
                SelectedHours = "-";
                SelectedMins = "-";
            }    
        }

        // Inheritance functions

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public static class TimeStringConverter
    {
        static DateTime today = DateTime.Today;
        static DateTime work_hours_start = DateTime.ParseExact("07:00AM", "hh:mmtt", CultureInfo.InvariantCulture);
        static DateTime work_hours_end = DateTime.ParseExact("07:00PM", "hh:mmtt", CultureInfo.InvariantCulture);

        public static bool IsValidTimeFormat(string value)
        {
            if (value == null)
                return false;

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

    public class TimeEntryUIConverter : IValueConverter
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
            return TimeStringConverter.StringToDateTime((string)value);
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