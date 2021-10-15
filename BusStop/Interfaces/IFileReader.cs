using System.Threading;
using System.Threading.Tasks;

namespace BusStop.Interfaces
{
    internal interface IFileReader
    {
        Task<string> ReadFileAsync(string filePath, CancellationToken cancellationToken);
    }
}
