using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeNetSync.Model
{
    public class MultisportResult
    {
        public int Id { get; set; }
        public int Bib { get; set; }
        public int Section { get; set; }
        public TimeSpan TimeOfDay { get; set; }
        public ResultState State { get; set; }
        public bool Confirmed { get; set; }
    }
}
