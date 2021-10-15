using System.Threading;
using System.Threading.Tasks;
using BusStop.Domain;

namespace BusStop.Interfaces
{
    internal interface ITimeTableWriter
    {
        Task WriteTimeTableAsync(string filePath, TimeTable timeTable, CancellationToken cancellationToken);
    }
}
