using CsvHelper.Configuration;
using System;
using System.ComponentModel;

using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace TimeTrack
{
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