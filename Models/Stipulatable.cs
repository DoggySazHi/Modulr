using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Modulr.Models
{
    public class Stipulatable
    {
        public int ID { get; }
        public string Name { get; }
        public string Description { get; }
        public IEnumerable<string> TesterFiles => _testerFiles;
        public IEnumerable<string> RequiredFiles => _requiredFiles;
        public IEnumerable<string> IncludedFiles => _includedFiles;
        private readonly List<string> _testerFiles = new();
        private readonly List<string> _requiredFiles = new();
        private readonly List<string> _includedFiles = new();

        [JsonConstructor]
        public Stipulatable(int id, string name, string description, IEnumerable<string> includedFiles, IEnumerable<string> testerFiles, IEnumerable<string> requiredFiles)
        {
            ID = id;
            Name = name;
            Description = description;
            foreach(var file in testerFiles)
                _testerFiles.Add(Path.GetFileName(file));
            foreach(var file in requiredFiles)
                _requiredFiles.Add(Path.GetFileName(file));
            foreach(var file in includedFiles)
                _includedFiles.Add(Path.GetFileName(file));
        }
    }
}