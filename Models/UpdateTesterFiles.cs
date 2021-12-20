using System.Collections.Generic;
using System.IO;

namespace Modulr.Models;

public class UpdateTesterFiles
{
    public string AuthToken { get; set; }
    public int TestID { get; set; }
    public string TestName { get; set; }
    public string TestDescription { get; set; }
    public List<string> Included { get; set; }
    public List<string> Testers { get; set; }
    public List<string> Required { get; set; }

    public bool IsLikelyValid()
    {
        if (TestName == null || Included == null || Testers == null || Required == null)
            return false;
        TestDescription ??= "";
        Included.ForEach(o => Path.GetFileName(o));
        Testers.ForEach(o => Path.GetFileName(o));
        Required.ForEach(o => Path.GetFileName(o));
        return true;
    }
}