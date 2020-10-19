using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Modulr.Models
{
    public class SourceTesterFiles
    {
        public string AuthToken { get; set; }

        public string TestName { get; set; }
        public List<IFormFile> Testers { get; set; }
        public List<IFormFile> Extra { get; set; }
        public List<string> Required { get; set; }

        public bool IsLikelyValid()
        {
            if (TestName == null || Testers == null || Required == null || Extra == null)
                return false;
            Required.ForEach(o => Path.GetFileName(o));
            
            return true;
        }
    }
}