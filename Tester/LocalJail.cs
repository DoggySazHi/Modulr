namespace Modulr.Tester
{
    public class LocalJail : ModulrJail
    {
        public LocalJail(string sourceFolder, string connectionID = null, params string[] files) : base(sourceFolder, connectionID, files)
        {
        }

        public override string GetAllOutput()
        {
            throw new System.NotImplementedException();
        }

        public override void Wait()
        {
            throw new System.NotImplementedException();
        }

        public override void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}