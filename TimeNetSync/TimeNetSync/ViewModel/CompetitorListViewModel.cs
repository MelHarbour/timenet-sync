using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TimeNetSync.Model;

namespace TimeNetSync.ViewModel
{
    public class CompetitorListViewModel : INotifyPropertyChanged
    {
        private string filePath;
        private string spreadsheetId;
        private string rangeTarget;
        private PropertyObservableCollection<Competitor> competitors = new PropertyObservableCollection<Competitor>();

        public string FilePath
        {
            get { return filePath; }
            set { SetField(ref filePath, value); }
        }

        public string SpreadsheetId
        {
            get { return spreadsheetId; }
            set { SetField(ref spreadsheetId, value); }
        }

        public string RangeTarget
        {
            get { return rangeTarget; }
            set { SetField(ref rangeTarget, value); }
        }

        public PropertyObservableCollection<Competitor> Competitors
        {
            get { return competitors; }
            set { competitors = value; }
        }

        public CompetitorListViewModel()
        {

        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
