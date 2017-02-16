using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeNetSync.Model
{
    public class Competitor
    {
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
    }
}
