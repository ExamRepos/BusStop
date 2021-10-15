using System;
using BusStop.Domain;

namespace BusStop.Tests
{
    internal static class Utils
    {
        public static TimeTableService GetService(string companyId, (int Hours, int Minutes) departureTime, (int Hours, int Minutes) arrivalTime)
        {
            return new TimeTableService(companyId, GetTime(departureTime.Hours, departureTime.Minutes), GetTime(arrivalTime.Hours, arrivalTime.Minutes));
        }

        public static DateTime GetTime(int hours, int minutes)
        {
            return new DateTime(1, 1, 1, hours, minutes, 0);
        }
    }
}
