using System.Threading;
using System.Threading.Tasks;

namespace BusStop.Interfaces
{
    internal interface IFileWriter
    {
        Task WriteFileAsync(string filePath, string contents, CancellationToken cancellationToken);
    }
}
