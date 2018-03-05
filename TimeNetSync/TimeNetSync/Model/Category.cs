using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeNetSync.Model
{
    public class Category
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pos")]
        public int Pos { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Settings { get; set; }
    }
}
