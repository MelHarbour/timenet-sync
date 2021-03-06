﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TimeNetSync.ViewModel;

namespace TimeNetSync.Model
{
    public class Competitor : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int Bib { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Nation { get; set; }
        public string Code { get; set; }
        public string Team { get; set; }
        public string Club { get; set; }
        public string Info1 { get; set; }
        public string Info2 { get; set; }

        /// <summary>
        /// This is actually called "Class" and "ClassId" in the database, but name changed to avoid a reserved word
        /// </summary>
        public int CategoryId { get; set; }
        public Sex Sex { get; set; }
        public int YearOfBirth { get; set; }
        public string Info3 { get; set; }
        public string Info4 { get; set; }
        public string Info5 { get; set; }
        public string InfoResult { get; set; }

        public Competitor()
        {
            Results.ListChanged += HandleResultsChange;
        }

        private BindingList<MultisportResult> results = new BindingList<MultisportResult>();
        public BindingList<MultisportResult> Results
        {
            get { return results; }
        }

        private void HandleResultsChange(object sender, ListChangedEventArgs e)
        {
            OnPropertyChanged("StartTime");
            OnPropertyChanged("FinishTime");
            OnPropertyChanged("RunTime");
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

        public MultisportResult StartTime
        {
            get
            {
                return Results.FirstOrDefault(x => x.Section == 0);
            }
        }

        public MultisportResult FinishTime
        {
            get
            {
                return Results.FirstOrDefault(x => x.Section == 3); // This is hard coded for WEHoRR
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "RunTime")]
        public TimeSpan RunTime
        {
            get
            {
                if (StartTime != null && FinishTime != null)
                    return FinishTime.TimeOfDay - StartTime.TimeOfDay;
                else
                    return new TimeSpan(0, 0, 0);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "RunTime")]
        public TimeSpan RunTimeToSection(int sectionNumber)
        {
            MultisportResult endResult = Results.FirstOrDefault(x => x.Section == sectionNumber);

            if (StartTime != null && endResult != null)
                return endResult.TimeOfDay - StartTime.TimeOfDay;
            else
                return new TimeSpan(0, 0, 0);
        }

        public MultisportResult SectionTime(int sectionNumber)
        {
            return Results.FirstOrDefault(x => x.Section == sectionNumber);
        }

        public ResultState State
        {
            get
            {
                MultisportResult result = Results.FirstOrDefault(x => x.Section == 3);
                if (result != null)
                    return result.State;
                else
                    return ResultState.Ok;
            }
        }
    }
}
