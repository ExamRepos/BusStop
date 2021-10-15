using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BusStop.Interfaces;

namespace BusStop.Domain.IO
{
    internal sealed class FileReader : IFileReader
    {
        public Task<string> ReadFileAsync(string filePath, CancellationToken cancellationToken)
        {
            return File.ReadAllTextAsync(filePath, cancellationToken);
        }
    }
}
