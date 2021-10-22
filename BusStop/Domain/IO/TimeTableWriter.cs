using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusStop.Interfaces;

namespace BusStop.Domain.IO
{
    internal sealed class TimeTableWriter : ITimeTableWriter
    {
        private readonly IFileWriter fileWriter;

        public TimeTableWriter(IFileWriter fileWriter)
        {
            this.fileWriter = fileWriter;
        }

        public async Task WriteTimeTableAsync(string filePath, TimeTable timeTable, CancellationToken cancellationToken)
        {
            string formattedTimeTable = GetFormattedTimeTable(timeTable);

            await fileWriter.WriteFileAsync(filePath, formattedTimeTable, cancellationToken).ConfigureAwait(false);
        }

        private static string GetFormattedTimeTable(TimeTable timeTable)
        {
            var groupedServiceRecordsForCompany = timeTable.Services
                .GroupBy(x => x.CompanyId)
                .OrderByDescending(x => x.Key)
                .Select(GetServiceRecordsForCompany);

            var result = string.Join(Environment.NewLine + Environment.NewLine, groupedServiceRecordsForCompany);

            return result;
        }

        private static string GetServiceRecordsForCompany(IGrouping<string, TimeTableService> group)
        {
            var services = group
                .OrderBy(x => x.DepartureTime)
                .Select(GetService);

            string result = string.Join(Environment.NewLine, services);

            return result;
        }

        private static string GetService(TimeTableService service)
        {
            string result = FormattableString.Invariant($"{service.CompanyId} {service.DepartureTime:HH:mm} {service.ArrivalTime:HH:mm}");

            return result;
        }
    }
}
