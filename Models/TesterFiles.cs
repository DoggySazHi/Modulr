using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Modulr.Models
{
    // We can't hold dictionaries, so we're trying this out instead...
    public class TesterFiles
    {
        public string AuthToken { get; set; }
        public string CaptchaToken { get; set; }
        public string ConnectionID { get; set; }
        public int TestID { get; set; }
        public List<string> FileNames { get; set; }
        public List<IFormFile> Files { get; set; }

        public bool IsLikelyValid()
        {
            if (FileNames == null || Files == null)
                return false;
            var fileCount = FileNames.Count;
            if (fileCount == 0 || Files.Count != fileCount)
                return false;
            for (var i = 0; i < fileCount; i++)
                FileNames[i] = Path.GetFileName(FileNames[i]);
            return true;
        }

        public bool IsEmpty()
        {
            return FileNames == null || Files == null || FileNames.Count == 0 || Files.Count == 0;
        }
    }
}