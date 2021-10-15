using System.Collections.Generic;

namespace BusStop.Domain
{
    internal sealed class TimeTable
    {
        public TimeTable(IEnumerable<TimeTableService> services)
        {
            Services = services;
        }

        public IEnumerable<TimeTableService> Services { get; private set; }
    }
}
