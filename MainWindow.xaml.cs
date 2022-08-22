using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TimeTrack
{
    public partial class MainWindow : Window
    {
        private TimeKeeper time_keeper;

        public MainWindow()
        {
            InitializeComponent();
            time_keeper = DataContext as TimeKeeper;

            if (Database.Exists())
                LoadEntriesForDate(DateTime.Today);
            else
            {
                switch (PromptForNewDatabase())
                {
                    case MessageBoxResult.OK:
                        Database.CreateDatabase();
                        break;
                    case MessageBoxResult.Cancel:
                        System.Windows.Application.Current.Shutdown();
                        break;
                }
            }
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            FldStartTime.Focus();
            time_keeper.UpdateSelectedTime();
            time_keeper.SetStartTimeField();
            time_keeper.UpdateTimeTotals();
        }
        
        private MessageBoxResult PromptForNewDatabase()
        {
            string messageBoxText = "The entries database could not be found in this directory.\nWould you like to create a new one?";
            string caption = "TimeTrack - Error";

            return MessageBox.Show(messageBoxText, caption, MessageBoxButton.OKCancel, MessageBoxImage.Error, MessageBoxResult.OK);
        }

        private void LoadEntriesForDate(DateTime date)
        {
            time_keeper.Entries = Database.Retrieve(date);
            time_keeper.CurrentIdCount = Database.CurrentIdCount(date);
            time_keeper.Date = date;
        }

        private void Submit()
        {
            if (time_keeper.SubmitEntry())
            {
                time_keeper.ClearFieldsAndSetStartTime();
                ChkLunch.IsChecked = false;
                DgTimeRecords.SelectedIndex = time_keeper.Entries.Count - 1;
                DgTimeRecords.ScrollIntoView(time_keeper.Entries.Last());
                FldEndTime.Focus();
                Database.Update(time_keeper.Entries);
            }
        }

        private void BtnSubmit(object sender, RoutedEventArgs e)
        {
            Submit();
        }

        private void BtnInsert(object sender, RoutedEventArgs e)
        {
            if (time_keeper.InsertBlankEntry(DgTimeRecords.SelectedIndex))
            {
                DgTimeRecords.SelectedIndex = DgTimeRecords.SelectedIndex - 1;
                //DgTimeRecords.ScrollIntoView(); TODO: scroll to inserted entry
                DgTimeRecords.Focus();
                Database.Update(time_keeper.Entries);
            }
        }

        private void BtnExport(object sender, RoutedEventArgs e)
        {
            if (DgTimeRecords.SelectedItem == null)
                return;

            TimeEntry selected = (TimeEntry)DgTimeRecords.SelectedItem;
            string text = selected.StartTimeAsString() + " - " + selected.EndTimeAsString() + "\n" + selected.Notes;

            bool retry;
            int retry_count = 0;
            int max_retries = 4;

            do
            {
                retry = false;
                try
                {
                    Clipboard.SetData(DataFormats.UnicodeText, text);
                }
                catch
                {
                    try
                    {
                        var clipboard_contents = Clipboard.GetText(TextDataFormat.UnicodeText);
                        if (!(clipboard_contents == text))
                        {
                            retry = true;
                            retry_count++;
                        }
                    }
                    catch
                    {
                        retry = true;
                        retry_count++;
                    }
                }
            } while (retry && retry_count < max_retries);

            if (!retry)
            {
                selected.Recorded = true;
                Database.Update(time_keeper.Entries);
            }
        }

        private void BtnExportAll(object sender, RoutedEventArgs e)
        {
            if (DgTimeRecords.Items.IsEmpty)
                return;

            string path = "C:\\temp\\_time_export.csv";
            if (File.Exists(path))
                File.Delete(path);

            string[] output = new string[DgTimeRecords.Items.Count];
            var all_records = DgTimeRecords.Items;

            for (int i = 0; i < all_records.Count; i++)
            {
                TimeEntry entry = all_records[i] as TimeEntry;

                if (entry.Recorded)
                    continue;
                if (string.IsNullOrEmpty(entry.CaseNumber.Trim()))
                    continue;

                string case_number = entry.CaseNumber.Trim();
                string hours = entry.Hours().ToString();
                string minutes = entry.Minutes().ToString();
                string time_period = entry.StartTimeAsString() + " - " + entry.EndTimeAsString();

                output[i] = case_number + "," + hours + "," + minutes + "," + time_period + "," + entry.Notes;
            }
            File.WriteAllLines(path, output);

            foreach (var i in DgTimeRecords.Items)
            {
                var entry = i as TimeEntry;
                if (string.IsNullOrEmpty(entry.CaseNumber.Trim()))
                    continue;
                entry.Recorded = true;
            }

            Database.Update(time_keeper.Entries);

        }

        private void BtnToggleAllRecorded(object sender, RoutedEventArgs e)
        {
            bool new_status = true;

            foreach (var i in time_keeper.Entries)
            {
                if (i.Recorded == true)
                {
                    new_status = false;
                    break;
                }
            }

            foreach (var i in time_keeper.Entries)
            {
                if (new_status && string.IsNullOrEmpty(i.CaseNumber.Trim()))
                    continue;
                i.Recorded = new_status;
            }
            Database.Update(time_keeper.Entries);
        }

        private void BtnLoadDate(object sender, RoutedEventArgs e)
        {
            var date = time_keeper.Date;
            time_keeper.CurrentDate = date.Date.ToShortDateString();
            LoadEntriesForDate(date);
        }

        private void ChkLunch_Checked(object sender, RoutedEventArgs e)
        {
            time_keeper.CaseNumberField = null;
            FldCaseNumber.IsEnabled = false;
            FldCaseNumber.Background = Brushes.LightGray;

            time_keeper.NotesField = "Lunch";
            FldNotes.IsEnabled = false;
            FldNotes.Background = Brushes.LightGray;

            if (time_keeper.EndTimeField == null || time_keeper.EndTimeField == "")
            {
                var EndLunch = DateTime.Today + TimeStringConverter.StringToTimeSpan(time_keeper.StartTimeField);
                if (EndLunch != null)
                {
                    EndLunch = ((DateTime)EndLunch).AddHours(1);
                    time_keeper.EndTimeField = ((DateTime)EndLunch).ToShortTimeString();
                    
                }
            }
        }

        private void ChkLunch_Unchecked(object sender, RoutedEventArgs e)
        {
            time_keeper.CaseNumberField = null;
            FldCaseNumber.IsEnabled = true;
            FldCaseNumber.Background = Brushes.White;

            time_keeper.EndTimeField = null;
            time_keeper.NotesField = null;
            FldNotes.IsEnabled = true;
            FldNotes.Background = Brushes.White;
        }

        private void DgTimeRecords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DgTimeRecords.SelectedItem != null)
                time_keeper.UpdateSelectedTime();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnSub.Focus();
                Submit();
            }
        }
    }

    public class TimeKeeper : INotifyPropertyChanged
    {
        public TimeKeeper()
        {
            time_records = new ObservableCollection<TimeEntry>();
            date = DateTime.Today.Date;
            current_date = date.Date.ToShortDateString();
            current_id_count = 0;
        }

        // Accessor functions

        public DateTime Date
        {
            get => date;
            set => date = value;
        }
        public int CurrentIdCount
        {
            set => current_id_count = value;
        }
        public ObservableCollection<TimeEntry> Entries
        {
            get => time_records;
            //set { time_records = value; OnPropertyChanged(); }
            set { time_records = value; OnPropertyChanged(); AddChangedHandlerToAllEntries(); }
        }
        public string CurrentDate
        {
            get => current_date;
            set { current_date = value; OnPropertyChanged(); }
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
        public TimeSpan? StartTimeFieldAsTime() => TimeStringConverter.StringToTimeSpan(start_time);
        public TimeSpan? EndTimeFieldAsTime() => TimeStringConverter.StringToTimeSpan(end_time);
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
        public TimeEntry SelectedItem { get => selected_item; set { selected_item = value; OnPropertyChanged(); } }

        // Functions

        public void AddEntry(DateTime date, int id, TimeSpan start_time, TimeSpan end_time, string case_number = "", string notes = "")
        {
            var entry = new TimeEntry(date, id, start_time, end_time, case_number, notes);
            entry.TimeEntryChanged += OnTimeEntryChanged;
            time_records.Add(entry);
            UpdateTimeTotals();
        }

        public bool InsertBlankEntry(int index)
        {
            if (time_records.Count == 0 || index > time_records.Count)
                return false;

            time_records.Insert(index, new TimeEntry(date, ++current_id_count));
            UpdateTimeTotals();
            return true;
        }

        public bool SubmitEntry()
        {
            TimeSpan? start_time = StartTimeFieldAsTime();
            TimeSpan? end_time = EndTimeFieldAsTime();

            if (start_time == null || end_time == null)
                return false;

            AddEntry(date, ++current_id_count, (TimeSpan)start_time, (TimeSpan)end_time, case_no, notes);
            return true;
        }

        public void RemoveCurrentlySelectedEntry()
        {
            Database.Delete(SelectedItem.Date, SelectedItem.ID);
            Entries.Remove(SelectedItem);
            OnTimeEntryChanged(true);
            SelectLastEntry();
        }

        public void SelectLastEntry()
        {
            if (Entries.Count > 0)
                SelectedItem = Entries.Last();
            else
                UpdateSelectedTime();
        }

        public void ClearFieldsAndSetStartTime()
        {
            SetStartTimeField();

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

        public void UpdateSelectedTime()
        {
            bool blank_value = false;

            if (SelectedItem != null)
            {
                var time_span = (SelectedItem.EndTime - SelectedItem.StartTime);
                if (time_span != null)
                {
                    SelectedHours = ((TimeSpan)time_span).Hours.ToString();
                    SelectedMins = ((TimeSpan)time_span).Minutes.ToString();
                }
                else
                    blank_value = true;
            }
            else
                blank_value = true;

            if (blank_value)
            {
                SelectedHours = "-";
                SelectedMins = "-";
            }
        }

        public void SetStartTimeField()
        {
            if (time_records.Count > 0)
                StartTimeField = time_records.Last<TimeEntry>().EndTimeAsString();
            else
                StartTimeField = null;
        }

        public void AddChangedHandlerToAllEntries()
        {
            foreach (var entry in time_records)
            {
                ((TimeEntry)entry).TimeEntryChanged += OnTimeEntryChanged;
            }
        }

        public void OnTimeEntryChanged(bool time_changed)
        {
            if (time_changed)
            {
                UpdateTimeTotals();
                UpdateSelectedTime();
                SetStartTimeField();
            }
            Database.Update(time_records);
        }

        // Inheritance items

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Event commands

        private ICommand remove_command;
        public ICommand RemoveCommand
        {
            get
            {
                if (remove_command == null)
                    remove_command = new RelayCommand(p => RemoveCurrentlySelectedEntry());
                return remove_command;
            }
        }

        private ICommand submit_command;
        public ICommand SubmitCommand
        {
            get => submit_command;
            set => submit_command = value;
        }

        // Private vars

        private DateTime date;
        private String current_date;
        private int current_id_count;
        private ObservableCollection<TimeEntry> time_records;
        private string start_time;
        private string end_time;
        private string case_no;
        private string notes;

        private double hours_total;
        private double gaps_total;
        private string selected_hours;
        private string selected_mins;
        private TimeEntry selected_item;
    }

    class Database
    {
        // Public variables
        public const string date_format = "yyyy-MM-dd";

        //Public functions
        public static bool Exists()
        {
            return File.Exists(databasePath);
        }
        public static void CreateDatabase()
        {
            Connect();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table'";
            cmd.Prepare();
            var result = cmd.ExecuteScalar();

            if ((string)result == "time_entries")
                return;

            cmd.CommandText = @"CREATE TABLE time_entries(date TEXT, id INTEGER, start_time TEXT, end_time TEXT, case_number TEXT, notes TEXT, recorded INTEGER, CONSTRAINT pk PRIMARY KEY(date, id));";
            cmd.ExecuteNonQuery();
            Close();
        }
        public static int CurrentIdCount(DateTime date)
        {
            Connect();
            cmd.CommandText = "SELECT MAX(id) FROM time_entries WHERE date = @date;";
            cmd.Parameters.AddWithValue("@date", DateToString(date));
            
            cmd.Prepare();
            var query = cmd.ExecuteReader();
            if (!query.Read() || query.IsDBNull(0))
            {
                Close();
                return 0;
            }

            Close();
            return query.GetInt32(0);
        }
        public static ObservableCollection<TimeEntry> Retrieve(DateTime date)
        {
            var return_val = new ObservableCollection<TimeEntry>();
            
            Connect();            
            cmd.CommandText = "SELECT * FROM time_entries WHERE date = @date ORDER BY start_time ASC, end_time ASC, id ASC";
            cmd.Parameters.AddWithValue("@date", DateToString(date));

            cmd.Prepare();
            var query = cmd.ExecuteReader();

            while (query.Read())
            {
                var out_date = StringToDate(query.GetString(0));
                var id = query.GetInt32(1);
                int recorded = query.GetInt32(6);

                TimeSpan? start_time = null;
                TimeSpan? end_time = null;
                string case_no = "";
                string notes = "";
                        

                if (!query.IsDBNull(2))
                    start_time = StringToTimeSpan(query.GetString(2));
                if (!query.IsDBNull(3))
                    end_time = StringToTimeSpan(query.GetString(3));
                if (!query.IsDBNull(4))
                    case_no = query.GetString(4);
                if (!query.IsDBNull(5))
                    notes = query.GetString(5);

                return_val.Add(new TimeEntry(out_date, id, start_time, end_time, case_no, notes, Convert.ToBoolean(recorded)));
            }
            Close();

            return return_val;
        }
        public static void Update(ObservableCollection<TimeEntry> entries)
        {
            if (entries.Count < 1)
                return;

            Connect();
            for (int i = 0; i < entries.Count; i++)
            {
                try
                {
                    cmd.CommandText = "INSERT OR REPLACE INTO time_entries(date, id, start_time, end_time, case_number, notes, recorded) " +
                        "VALUES(@date, @id, @start_time, @end_time, @case_number, @notes, @recorded)";
                    cmd.Parameters.AddWithValue("@date", DateToString(entries[i].Date));
                    cmd.Parameters.AddWithValue("@id", entries[i].ID);
                    cmd.Parameters.AddWithValue("@start_time", entries[i].StartTime);
                    cmd.Parameters.AddWithValue("@end_time", entries[i].EndTime);
                    cmd.Parameters.AddWithValue("@case_number", entries[i].CaseNumber);
                    cmd.Parameters.AddWithValue("@notes", entries[i].Notes);
                    cmd.Parameters.AddWithValue("@recorded", entries[i].Recorded);

                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Console.WriteLine("FAILED INSERT:" + e.Message);
                    throw e;
                }
            }
            Close();
        }
        public static void Delete(DateTime date, int id)
        {
            Connect();
            cmd.CommandText = "DELETE FROM time_entries WHERE date = @date";
            cmd.Parameters.AddWithValue("@date", DateToString(date));
            cmd.Prepare();
            cmd.ExecuteNonQuery();
            Close();
        }

        // Private functions
        private static void Connect()
        {
            connection = new SQLiteConnection(databaseURI);
            connection.Open();
            cmd = connection.CreateCommand();
        }
        private static void Close()
        {
            cmd.Dispose();
            connection.Dispose();
        }
        private static string DateToString(DateTime date)
        {
            return date.ToString(date_format);
        }
        private static DateTime StringToDate(string str)
        {
            return DateTime.ParseExact(str, date_format, DateTimeFormatInfo.InvariantInfo);
        }
        private static TimeSpan StringToTimeSpan(string str)
        {
            return TimeSpan.ParseExact(str, "c", CultureInfo.InvariantCulture, TimeSpanStyles.None);
        }

        // Private variables
        private const string databasePath = "timetrack.db";
        //private static string databasePath = @"URI=file:C:\\temp\\test\\test.db";
        private const string databaseURI = @"URI=file:" + databasePath;
        private static SQLiteConnection connection;
        private static SQLiteCommand cmd;
    }

    public static class TimeStringConverter
    {
        static DateTime work_hours_start = DateTime.ParseExact("07:00AM", "hh:mmtt", CultureInfo.InvariantCulture);
        static DateTime work_hours_end = DateTime.ParseExact("07:00PM", "hh:mmtt", CultureInfo.InvariantCulture);

        public static bool IsValidTimeFormat(string value)
        {
            if (value == null)
                return false;

            /* Regex Explantion:
             * ^                : starting at the beginning of the string
             * \d{1,2}          : between 1, and 2 digit characters
             * [;:.]?           : ";", ":", or "." - optional
             * ()               : encapsulated logic. The same as you are used to.
             * (\d{2})?         : exactly 2 digit characters - optional
             * (\s?)+           : any number of whitespaces - optional
             * (...)?           : everything contained in the brackets is optional
             * (?i: ...)        : contained commands will be case-insensitive
             * ...[AP]M)?       : either A, or P characters, followed by M. All optional
             * $                : the end of the string
             */
            string valid_time_format = @"^\d{1,2}[;:.]?(\d{2})?((\s?)+(?i:[AP]M)?)?$";

            /* Regex Explantion:
             * ^                : Start of the string
             * \d{1,2}          : 2 digits
             * [;:.]?           : either ";", ":" or "." - optional
             * (\d{2})?         : 2 digits - optional
             * (?i: ...)?       : contained commands will be case-insensitive. All optional
             * (\s?)+           : any number of whitespaces - optional
             * (?!AM)           : fail if "AM" is present
             * (PM)?            : PM - optional
             * $                : end of the string
             */
            string valid_24hour_format = @"^\d{2}[;:.]?(\d{2})?(?i:(\s?)+(?!AM)(PM)?)?$";

            if (Regex.IsMatch(value, valid_time_format))
            {
                if (Is24HourFormat(value))
                    return Regex.IsMatch(value, valid_24hour_format);

                return true;
            }

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
            value = value.Replace(".", ":");
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
            if (value.TimeOfDay > work_hours_start.TimeOfDay && value.TimeOfDay < work_hours_end.TimeOfDay)
                return value;

            switch (value.ToString("tt"))
            {
                case "AM":
                    return value.AddHours(12);
                case "PM":
                    return value.AddHours(-12);
                default:
                    return value;
            }
        }
        public static TimeSpan? StringToTimeSpan(string value)
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
                if (!time_period_present && !format_24_hour)
                    return_val = ClampToWorkHours(return_val);

                return return_val.TimeOfDay;
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public RelayCommand(Action<object> execute)
        {
            this.execute = execute;
            this.canExecute = null;
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.CanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }

    public class HotkeyDelegateCommand : ICommand
    {
        // Specify the keys and mouse actions that invoke the command. 
        public Key HotKey { get; set; }

        Action<object> _executeDelegate;

        public HotkeyDelegateCommand(Action<object> executeDelegate)
        {
            _executeDelegate = executeDelegate;
        }

        public void Execute(object parameter)
        {
            _executeDelegate(parameter);
        }

        public bool CanExecute(object parameter) { return true; }
#pragma warning disable CS0067 // boilerplate code
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067 // boilerplate code
    }
}