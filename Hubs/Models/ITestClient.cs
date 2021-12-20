using System.Threading.Tasks;

namespace Modulr.Hubs.Models;

public interface ITestClient
{
    Task ReceiveUpdate(string output);
}