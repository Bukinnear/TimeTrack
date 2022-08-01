using CsvHelper.Configuration;
using System;
using System.ComponentModel;

using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace TimeTrack
{
    public delegate void TimeEntryChangedEventHandler(bool time_changed);

    public class TimeEntry : INotifyPropertyChanged
    {
        public TimeEntry(DateTime date, int id)
        {
            this.date = date;
            this.id = id;
            start_time = null;
            end_time = null;
            case_number = "";
            notes = "";
        }
        public TimeEntry(DateTime date, int id, DateTime start_time, DateTime end_time, string case_number, string notes)
        {
            this.date = date;
            this.id = id;
            this.start_time = start_time;
            this.end_time = end_time;
            this.case_number = case_number;
            this.notes = notes;
        }
        
        public DateTime Date
        {
            get { return date; }
            set { date = value; OnPropertyChanged(); }
        }
        public int ID
        {
            get { return id; }
            set { id = value; OnPropertyChanged(); }
        }
        public DateTime? StartTime
        {
            get { return start_time; }
            set 
            { 
                start_time = value; 
                OnPropertyChanged();
                OnTimeEntryChanged(true);
            }
        }
        public DateTime? EndTime
        {
            get { return end_time; }
            set 
            { 
                end_time = value; 
                OnPropertyChanged();
                OnTimeEntryChanged(true);
            }
        }
        public string CaseNumber
        {
            get { return case_number; }
            set 
            {
                var old_val = case_number;
                case_number = value; 
                OnPropertyChanged();

                // if the previous value was empty/null and is no longer, or visav-versa
                if (((old_val == "" || old_val == null) && (case_number != "" || case_number != null)) ||
                    ((old_val != "" || old_val != null) && (case_number == "" || case_number == null)))
                    OnTimeEntryChanged(true);
                else
                    OnTimeEntryChanged(false);
            }
        }
        public string Notes
        {
            get { return notes; }
            set { notes = value; OnPropertyChanged(); OnTimeEntryChanged(false); }
        }
        public bool Recorded
        {
            get { return recorded; }
            set { recorded = value; OnPropertyChanged(); OnTimeEntryChanged(false); }
        }

        public string StartTimeAsShortString()
        {
            if (start_time == null)
                return "";
            return ((DateTime)start_time).ToShortTimeString();
        }
        public string EndTimeAsShortString()
        {
            if (end_time == null)
                return "";
            return ((DateTime)end_time).ToShortTimeString();
            
        }
        public int Hours()
        {
            if (start_time == null || end_time == null)
                return 0;
            return (((DateTime)end_time) - ((DateTime)start_time)).Hours;
        }
        public int Minutes()
        {
            if (start_time == null || end_time == null)
                return 0;
            return (((DateTime)end_time) - ((DateTime)start_time)).Minutes;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event TimeEntryChangedEventHandler TimeEntryChanged;
        protected void OnTimeEntryChanged(bool time_changed)
        {
            TimeEntryChanged?.Invoke(time_changed);
        }

        private DateTime date;
        private int id;
        private DateTime? start_time;
        private DateTime? end_time;
        private string case_number;
        private string notes;
        private bool recorded;
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