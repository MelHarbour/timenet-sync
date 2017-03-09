using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeNetSync.ViewModel
{
    public sealed class PropertyObservableCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        public PropertyObservableCollection()
        {
            CollectionChanged += FullObservableCollectionCollectionChanged;
        }

        public PropertyObservableCollection(IEnumerable<T> pItems): this()
        {
            foreach (var item in pItems)
            {
                this.Add(item);
            }
        }

        private void FullObservableCollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (object item in e.NewItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged += ItemPropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (object item in e.OldItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged -= ItemPropertyChanged;
                }
            }
        }

        public event PropertyChangedEventHandler CollectionItemChanged;

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.CollectionItemChanged?.Invoke(sender, e);
        }
    }
}
