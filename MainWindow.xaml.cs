using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
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

using System.Runtime.CompilerServices;
using System.Linq.Expressions;

using CsvHelper;
using CsvHelper.Configuration;

namespace TimeTrack
{
    public partial class MainWindow : Window
    {
        ObservableCollection<TimeEntry> time_records = new ObservableCollection<TimeEntry>();

        public MainWindow()
        {
            InitializeComponent();
            
            //ImportFromCSV("DEBUG.csv");
            ImportFromCSV(CSVName());

            DgTimeRecords.ItemsSource = time_records;
            FldStartTime.Focus();
        }

        private void BtnSubmit(object sender, RoutedEventArgs e)
        {            
            time_records.Add(new TimeEntry(FldStartTime.Text, FldEndTime.Text, FldCaseNumber.Text, FldNotes.Text));
            ClearFields();
            DgTimeRecords.SelectedIndex = time_records.Count - 1;
            DgTimeRecords.Focus();
            ExportToCSV(CSVName());
        }

        private void BtnInsert(object sender, RoutedEventArgs e)
        {
            int insert_index = DgTimeRecords.SelectedIndex >= 0 ? DgTimeRecords.SelectedIndex + 1 : time_records.Count;
            
            time_records.Insert(insert_index, new TimeEntry());
            
            DgTimeRecords.SelectedIndex = insert_index;
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

            if (DgTimeRecords.SelectedIndex > 0)
                time_records.Move(DgTimeRecords.SelectedIndex, DgTimeRecords.SelectedIndex - 1);
            
            DgTimeRecords.Focus();
        }

        private void BtnMoveDown(object sender, RoutedEventArgs e)
        {
            if (DgTimeRecords.SelectedIndex >= 0 && DgTimeRecords.SelectedIndex != time_records.Count - 1)
                time_records.Move(DgTimeRecords.SelectedIndex, DgTimeRecords.SelectedIndex + 1);
            
            DgTimeRecords.Focus();
        }

        private void ClearFields()
        {
            FldStartTime.Text = FldEndTime.Text;

            FldEndTime.Clear();
            FldCaseNumber.Clear();
            FldNotes.Clear();

            FldEndTime.Focus();
        }

        private string CSVName()
        {
            return "TimeTrack_" + DateTime.Today.ToString("yyyy-MM-dd") + ".csv";
        }

        public void ExportToCSV(string file)
        {
            try
            {
                using (var writer = new StreamWriter(file))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.RegisterClassMap<TimeEntryCSVMap>();
                    csv.WriteRecords(time_records);
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
                            time_records.Add(i);
                    }
                }
            }
            catch (Exception) { }
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
            start_time = "";
            end_time = "";
            case_number = "";
            notes = "";
            ID = ID_index += 1;
        }

        public TimeEntry(string in_start, string in_end, string in_case, string in_notes)
        {
            start_time = in_start;
            end_time = in_end;
            case_number = in_case;
            notes = in_notes;
            ID = ID_index += 1;
        }
        
        private static int ID_index;

        private int id;
        private string start_time;
        private string end_time;
        private string case_number;
        private string notes;
        private bool recorded;

        public int ID
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged();                
            }
        }
        public string StartTime 
        {
            get { return start_time; }
            set 
            { 
                start_time = value;
                OnPropertyChanged();
            }
        }
        public string EndTime 
        {
            get { return end_time; }
            set
            {
                end_time = value;
                OnPropertyChanged();
            }
        }
        public string CaseNumber 
        {
            get { return case_number; }
            set
            {
                case_number = value;
                OnPropertyChanged();
            }
        }
        public string Notes 
        {
            get { return notes; }
            set
            {
                notes = value;
                OnPropertyChanged();
            }
        }
        public bool Recorded
        {
            get { return recorded; }
            set
            {
                recorded = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class StringToTime : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            
            return value;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
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
 */