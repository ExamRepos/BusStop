using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusStop.Constants;
using BusStop.Interfaces;

namespace BusStop.Domain.IO
{
    /// <summary>
    /// Used for reading time tables from sources containing data in the following format:
    /// <br>Posh 10:15 11:10</br>
    /// <br>Posh 10:10 11:00</br>
    /// <br>Grotty 10:10 11:00</br>
    /// <br>Grotty 16:30 18:45</br>
    /// </summary>
    internal sealed class TimeTableReader : ITimeTableReader
    {
        private const string TimeTableServicePartDelimiter = " ";
        private const string TimeFormat = "HH':'mm";

        private static readonly string[] allowedBusCompaniesIds = new string[] { BusCompany.Grotty, BusCompany.Posh };

        private readonly IFileReader fileReader;

        public TimeTableReader(IFileReader fileReader)
        {
            this.fileReader = fileReader;
        }

        public async Task<TimeTable> ReadTimeTableAsync(string filePath, CancellationToken cancellationToken)
        {
            string fileContents = await fileReader.ReadFileAsync(filePath, cancellationToken).ConfigureAwait(false);

            var result = ParseTimeTableFile(fileContents);

            return result;
        }

        private static TimeTable ParseTimeTableFile(string contents)
        {
            var timeTableServices = contents
                .Split(Environment.NewLine)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(ParseServiceRecord)
                .ToList();

            var result = new TimeTable(timeTableServices);

            return result;
        }

        private static TimeTableService ParseServiceRecord(string line, int index)
        {
            var serviceRecordParts = line.Split(TimeTableServicePartDelimiter);

            if (serviceRecordParts.Length != 3)
            {
                throw new FormatException(FormattableString.Invariant($"Line on row {index + 1} is not in correct format. Actual value: \"{line}\"."));
            }

            string companyId = serviceRecordParts[0];
            string departureTimeString = serviceRecordParts[1];
            string arrivalTimeString = serviceRecordParts[2];

            ValidateCompanyId(companyId, index);
            
            DateTime departureTime = ParseTime(departureTimeString, nameof(departureTime), index);
            DateTime arrivalTime = ParseTime(arrivalTimeString, nameof(arrivalTime), index);

            var result = new TimeTableService(companyId, departureTime, arrivalTime);

            return result;
        }

        private static void ValidateCompanyId(string companyId, int lineIndex)
        {
            if (!allowedBusCompaniesIds.Any(x => x.Equals(companyId, StringComparison.InvariantCulture)))
            {
                throw new FormatException(FormattableString.Invariant($"The companyId at line {lineIndex + 1} is using an unknown format. Actual value: \"{companyId}\"."));
            }
        }

        private static DateTime ParseTime(string time, string timeType, int lineIndex)
        {
            if (!DateTime.TryParseExact(time, TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.NoCurrentDateDefault, out DateTime result))
            {
                throw new FormatException(FormattableString.Invariant($"The {timeType} at line {lineIndex + 1} is using an unknown format. Actual value: \"{time}\"."));
            }

            return result;
        }
    }
}
