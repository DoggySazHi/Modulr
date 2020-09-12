using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Modulr.Models
{
    public class Stipulatable
    {
        public int ID;
        public string Name;
        public IReadOnlyCollection<string> TesterFiles => _testerFiles;
        public IReadOnlyCollection<string> RequiredFiles => _requiredFiles;
        private readonly List<string> _testerFiles = new List<string>();
        private readonly List<string> _requiredFiles = new List<string>();

        [JsonConstructor]
        public Stipulatable(int id, string name, IEnumerable<string> testerFiles, IEnumerable<string> requiredFiles)
        {
            ID = id;
            Name = name;
            foreach(var file in testerFiles)
                _testerFiles.Add(Path.GetFileName(file));
            foreach(var file in requiredFiles)
                _requiredFiles.Add(Path.GetFileName(file));
        }
    }
}