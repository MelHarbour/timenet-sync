using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeNetSync.Services
{
    public class GoogleSheetResultsOutput : IResultsOutput
    {
        public void Update(string target, string targetRange, IList<IList<object>> values)
        {
            throw new NotImplementedException();
        }
    }
}
