using System.Threading;
using System.Threading.Tasks;
using BusStop.Domain;

namespace BusStop.Interfaces
{
    internal interface ITimeTableReader
    {
        Task<TimeTable> ReadTimeTableAsync(string filePath, CancellationToken cancellationToken);
    }
}
