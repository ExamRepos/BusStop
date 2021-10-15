using System;

namespace BusStop.Domain
{
    internal sealed class TimeTableService
    {
        public TimeTableService(string companyId, DateTime departureTime, DateTime arrivalTime)
        {
            if (departureTime >= arrivalTime)
            {
                throw new ArgumentException("Departure time should be before arrival time.");
            }

            CompanyId = companyId;
            DepartureTime = departureTime;
            ArrivalTime = arrivalTime;
        }

        public string CompanyId { get; private set; }

        public DateTime DepartureTime { get; private set; }

        public DateTime ArrivalTime { get; private set; }

        public TimeSpan Duration
        {
            get
            {
                var result = ArrivalTime - DepartureTime;

                return result;
            }
        }

        public override string ToString()
        {
            return FormattableString.Invariant($"{CompanyId} {DepartureTime:HH:mm}-{ArrivalTime:HH:mm}");
        }
    }
}
