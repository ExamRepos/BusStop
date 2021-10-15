using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BusStop.Interfaces;

namespace BusStop.Domain.IO
{
    internal sealed class FileWriter : IFileWriter
    {
        public Task WriteFileAsync(string filePath, string contents, CancellationToken cancellationToken)
        {
            return File.WriteAllTextAsync(filePath, contents, cancellationToken);
        }
    }
}
