using System.Collections.Generic;
using System.IO;

namespace Modulr.Models
{
    public class UpdateTesterFiles
    {
        public string AuthToken { get; set; }
        public int TestID { get; set; }
        public string TestName { get; set; }
        public List<string> Testers { get; set; }
        public List<string> Extra { get; set; }
        public List<string> Required { get; set; }

        public bool IsLikelyValid()
        {
            if (TestName == null || Testers == null || Required == null || Extra == null)
                return false;
            Testers.ForEach(o => Path.GetFileName(o));
            Required.ForEach(o => Path.GetFileName(o));
            Extra.ForEach(o => Path.GetFileName(o));
            return true;
        }
    }
}