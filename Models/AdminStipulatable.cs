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
        public int ID { get; }
        public string Name { get; }
        public string Description { get; }
        public bool Valid { get; private set; }
        public IReadOnlyCollection<TesterFile> IncludedFiles => _includedFiles;
        public IReadOnlyCollection<TesterFile> TesterFiles => _testerFiles;
        public IReadOnlyCollection<string> RequiredFiles => _requiredFiles;
        private readonly List<TesterFile> _testerFiles = new();
        private readonly List<TesterFile> _includedFiles = new();
        private readonly List<string> _requiredFiles = new();

        public AdminStipulatable(Stipulatable sp)
        {
            ID = sp.ID;
            Description = sp.Description;
            Name = sp.Name;
            
            foreach(var included in sp.IncludedFiles)
                _includedFiles.Add(new TesterFile {File = included});
            foreach(var tester in sp.TesterFiles)
                _testerFiles.Add(new TesterFile {File = tester});
            foreach(var required in sp.RequiredFiles)
                _requiredFiles.Add(required);
        }

        public bool Validate(ModulrConfig config)
        {
            var success = true;
            
            var includePath = Path.Join(config.IncludeLocation, "" + ID);
            var sourcePath = Path.Join(config.SourceLocation, "" + ID);

            foreach (var tester in _testerFiles)
            {
                if (!_requiredFiles.Contains(tester.File) && !File.Exists(Path.Join(sourcePath, tester.File)))
                    success = false;
                else
                    tester.Exists = true;
            }
            
            foreach (var include in _includedFiles)
            {
                if (!File.Exists(Path.Join(includePath, include.File)))
                    success = false;
                else
                    include.Exists = true;
            }

            Valid = success;
            return success;
        }
    }
}