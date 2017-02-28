using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeNetSync.Services
{
    public interface IResultsOutput
    {
        void Update(string target, string targetRange, IList<IList<object>> values);
    }
}
