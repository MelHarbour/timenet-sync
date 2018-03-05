using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeNetSync.Services
{
    public interface IResultsOutput
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required by the Google API library")]
        void Update(string target, string targetRange, IList<IList<object>> values);
    }
}
