namespace Modulr.Models
{
    public class Test
    {
        public string FunctionName { get; set; }
        public string FunctionInput { get; set; }
        public string FunctionActualOutput { get; set; }

        public override string ToString()
        {
            return $"{FunctionName} - {FunctionInput} -> {FunctionActualOutput}";
        }
    }
}