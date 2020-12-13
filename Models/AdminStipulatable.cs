using System.Collections.Generic;
using System.IO;
using Modulr.Tester;

namespace Modulr.Models
{
    public class TesterFile
    {
        public string File { get; set; }
        public bool Exists { get; set; }
    }
    
    public class AdminStipulatable
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Valid { get; set; }
        public IReadOnlyCollection<TesterFile> TesterFiles => _testerFiles;
        public IReadOnlyCollection<string> RequiredFiles => _requiredFiles;
        private readonly List<TesterFile> _testerFiles = new List<TesterFile>();
        private readonly List<string> _requiredFiles = new List<string>();

        public AdminStipulatable(Stipulatable sp)
        {
            ID = sp.ID;
            Name = sp.Name;
            foreach(var tester in sp.TesterFiles)
                _testerFiles.Add(new TesterFile {File = tester});
            foreach(var required in sp.RequiredFiles)
                _requiredFiles.Add(required);
        }

        public bool Validate(ModulrConfig config)
        {
            var success = true;

            foreach (var tester in _testerFiles)
            {
                if (!_requiredFiles.Contains(tester.File) && !File.Exists(Path.Join(config.SourceLocation, tester.File)))
                    success = false;
                else
                    tester.Exists = true;
            }

            Valid = success;
            return success;
        }
    }
}