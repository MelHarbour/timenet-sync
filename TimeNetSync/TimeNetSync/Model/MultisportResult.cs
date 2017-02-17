using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TimeNetSync.Model
{
    public class MultisportResult : INotifyPropertyChanged
    {
        private int id;
        private int bib;
        private int section;
        private TimeSpan timeOfDay;
        private ResultState state;
        private bool confirmed;

        public int Id
        {
            get { return id; }
            set { SetField(ref id, value); }
        }

        public int Bib
        {
            get { return bib; }
            set { SetField(ref bib, value); }
        }
        
        public int Section
        {
            get { return section; }
            set { SetField(ref section, value); }
        }

        public TimeSpan TimeOfDay
        {
            get { return timeOfDay; }
            set { SetField(ref timeOfDay, value); }
        }

        public ResultState State
        {
            get { return state; }
            set { SetField(ref state, value); }
        }

        public bool Confirmed
        {
            get { return confirmed; }
            set { SetField(ref confirmed, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
    }
}
