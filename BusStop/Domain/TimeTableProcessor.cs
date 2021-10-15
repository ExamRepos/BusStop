using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusStop.Constants;
using BusStop.Interfaces;

namespace BusStop.Domain
{
    internal sealed class TimeTableProcessor
    {
        private const string OutputFilePath = "output.txt";
        private const int ServiceTimeLimitInMinutes = 60;

        private readonly ITimeTableReader timeTableReader;
        private readonly ITimeTableWriter timeTableWriter;

        public TimeTableProcessor(ITimeTableReader timeTableReader, ITimeTableWriter timeTableWriter)
        {
            this.timeTableReader = timeTableReader;
            this.timeTableWriter = timeTableWriter;
        }

        public async Task ProcessTimeTableAsync(string filePath, CancellationToken cancellationToken)
        {
            var sourceTimeTable = await timeTableReader.ReadTimeTableAsync(filePath, cancellationToken).ConfigureAwait(false);

            var resultTimeTable = FilterTimeTableServices(sourceTimeTable);

            await timeTableWriter.WriteTimeTableAsync(OutputFilePath, resultTimeTable, cancellationToken);
        }

        private static TimeTable FilterTimeTableServices(TimeTable sourceTimeTable)
        {
            var resultTimeTableServices = sourceTimeTable.Services
                .Where(x => x.Duration.TotalMinutes <= ServiceTimeLimitInMinutes)
                .Where(x => IsServiceEfficient(x, sourceTimeTable.Services))
                .Where(x => !BetterServiceAvailableForTheSameTimeFrame(x, sourceTimeTable.Services))
                .OrderBy(x => x.DepartureTime);

            var result = new TimeTable(resultTimeTableServices);

            return result;
        }

        private static bool BetterServiceAvailableForTheSameTimeFrame(TimeTableService currentService, IEnumerable<TimeTableService> allServices)
        {
            bool result = allServices.Any(x =>
                x.DepartureTime == currentService.DepartureTime &&
                x.ArrivalTime == currentService.ArrivalTime &&
                x.CompanyId == BusCompany.Posh &&
                currentService.CompanyId == BusCompany.Grotty);

            return result;
        }

        private static bool IsServiceEfficient(TimeTableService currentService, IEnumerable<TimeTableService> allServices)
        {
            bool result = (!OthersStartAtTheSameTimeAndReachEarlier(currentService, allServices)) &&
                          (!OthersStartLaterAndReachAtTheSameTime(currentService, allServices)) &&
                          (!OthersStartLaterAndReachEarlier(currentService, allServices));

            return result;
        }

        private static bool OthersStartAtTheSameTimeAndReachEarlier(TimeTableService currentService, IEnumerable<TimeTableService> allServices)
        {
            bool result = allServices.Any(x => x.DepartureTime == currentService.DepartureTime && x.ArrivalTime < currentService.ArrivalTime);

            return result;
        }

        private static bool OthersStartLaterAndReachAtTheSameTime(TimeTableService currentService, IEnumerable<TimeTableService> allServices)
        {
            bool result = allServices.Any(x => x.DepartureTime > currentService.DepartureTime && x.ArrivalTime == currentService.ArrivalTime);

            return result;
        }

        private static bool OthersStartLaterAndReachEarlier(TimeTableService currentService, IEnumerable<TimeTableService> allServices)
        {
            bool result = allServices.Any(x => x.DepartureTime > currentService.DepartureTime && x.ArrivalTime < currentService.ArrivalTime);

            return result;
        }
    }
}
