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
        private PropertyObservableCollection<Competitor> competitors = new PropertyObservableCollection<Competitor>();

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
