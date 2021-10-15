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

        private string GetFormattedTimeTable(TimeTable timeTable)
        {
            var orderedRecords = timeTable.Services
                .OrderByDescending(x => x.CompanyId)
                .ThenBy(x => x.DepartureTime);

            var groupedServiceRecordsForCompany = orderedRecords
                .GroupBy(x => x.CompanyId)
                .Select(GetServiceRecordsForCompany);

            if (!groupedServiceRecordsForCompany.Any())
            {
                return string.Empty;
            }

            var result = groupedServiceRecordsForCompany.Aggregate((x1, x2) => x1 + Environment.NewLine + Environment.NewLine + x2);

            return result;
        }

        private string GetServiceRecordsForCompany(IGrouping<string, TimeTableService> group)
        {
            string result = group
                .Select(GetServiceLine)
                .Aggregate((x1, x2) => x1 + Environment.NewLine + x2);

            return result;
        }

        private string GetServiceLine(TimeTableService service)
        {
            string result = FormattableString.Invariant($"{service.CompanyId} {service.DepartureTime:HH:mm} {service.ArrivalTime:HH:mm}");

            return result;
        }
    }
}
