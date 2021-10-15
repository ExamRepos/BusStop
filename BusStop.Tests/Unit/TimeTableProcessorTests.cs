using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusStop.Constants;
using BusStop.Domain;
using BusStop.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BusStop.Tests.Unit
{
    public class TimeTableProcessorTests
    {
        private const string FilePath = "TestFilePath";

        private Mock<ITimeTableReader> timeTableReaderMock;
        private Mock<ITimeTableWriter> timeTableWriterMock;

        private TimeTableProcessor systemUnderTest;

        [SetUp]
        public void Setup()
        {
            timeTableReaderMock = new Mock<ITimeTableReader>();
            timeTableWriterMock = new Mock<ITimeTableWriter>();

            systemUnderTest = new TimeTableProcessor(timeTableReaderMock.Object, timeTableWriterMock.Object);
        }

        [Test]
        public async Task ProcessTimeTableAsync_ValidTimeTable_ShouldReturnFilteredAndOrderedTimeTable()
        {
            var inputTableServices = new List<TimeTableService>
            {
                Utils.GetService(BusCompany.Posh, (10, 15), (11, 10)),
                Utils.GetService(BusCompany.Posh, (10, 10), (11, 00)),
                Utils.GetService(BusCompany.Grotty, (10, 10), (11, 00)),
                Utils.GetService(BusCompany.Grotty, (16, 30), (18, 45)),
                Utils.GetService(BusCompany.Posh, (12, 05), (12, 30)),
                Utils.GetService(BusCompany.Grotty, (12, 30), (13, 25)),
                Utils.GetService(BusCompany.Grotty, (12, 45), (13, 25)),
                Utils.GetService(BusCompany.Posh, (17, 25), (18, 01)),
            };

            var expectedTableServices = new List<TimeTableService>
            {
                Utils.GetService(BusCompany.Posh, (10, 10), (11, 00)),
                Utils.GetService(BusCompany.Posh, (10, 15), (11, 10)),
                Utils.GetService(BusCompany.Posh, (12, 05), (12, 30)),
                Utils.GetService(BusCompany.Grotty, (12, 45), (13, 25)),
                Utils.GetService(BusCompany.Posh, (17, 25), (18, 01)),
            };

            await ProcessAndCompareTimeTableServicesAsync(inputTableServices, expectedTableServices);
        }

        [Test]
        public async Task ProcessTimeTableAsync_MultipleServicesWithSameArrivalTime_ShouldReturnTheOneWithLatestDepartureDate()
        {
            var inputTableServices = new List<TimeTableService>
            {
                Utils.GetService(BusCompany.Posh, (10, 10), (11, 00)),
                Utils.GetService(BusCompany.Posh, (10, 20), (11, 00)),
                Utils.GetService(BusCompany.Posh, (10, 15), (11, 00)),
            };

            var expectedTableServices = new List<TimeTableService>
            {
                Utils.GetService(BusCompany.Posh, (10, 20), (11, 00)),
            };

            await ProcessAndCompareTimeTableServicesAsync(inputTableServices, expectedTableServices);
        }

        [Test]
        public async Task ProcessTimeTableAsync_MultipleServicesWithSameDepartureTime_ShouldReturnTheOneWithEarliestArrivalDate()
        {
            var inputTableServices = new List<TimeTableService>
            {
                Utils.GetService(BusCompany.Posh, (10, 10), (10, 55)),
                Utils.GetService(BusCompany.Posh, (10, 10), (10, 50)),
                Utils.GetService(BusCompany.Posh, (10, 10), (11, 00)),
            };

            var expectedTableServices = new List<TimeTableService>
            {
                Utils.GetService(BusCompany.Posh, (10, 10), (10, 50)),
            };

            await ProcessAndCompareTimeTableServicesAsync(inputTableServices, expectedTableServices);
        }

        [Test]
        public async Task ProcessTimeTableAsync_MultipleServicesWithLaterDepartureTimeAndEarlierArrivalTime_ShouldReturnTheShortestService()
        {
            var inputTableServices = new List<TimeTableService>
            {
                Utils.GetService(BusCompany.Posh, (10, 10), (10, 55)),
                Utils.GetService(BusCompany.Posh, (10, 20), (10, 50)),
                Utils.GetService(BusCompany.Posh, (10, 15), (11, 00)),
            };

            var expectedTableServices = new List<TimeTableService>
            {
                Utils.GetService(BusCompany.Posh, (10, 20), (10, 50)),
            };

            await ProcessAndCompareTimeTableServicesAsync(inputTableServices, expectedTableServices);
        }

        [Test]
        public async Task ProcessTimeTableAsync_ServicesLongerThanOneHour_ShouldBeFilteredOut()
        {
            var inputTableServices = new List<TimeTableService>
            {
                Utils.GetService(BusCompany.Posh, (10, 10), (11, 15)),
            };

            var expectedTableServices = new List<TimeTableService>();

            await ProcessAndCompareTimeTableServicesAsync(inputTableServices, expectedTableServices);
        }

        [Test]
        public async Task ProcessTimeTableAsync_MultipleServicesWithSameDepartureAndArrivalTime_ShouldFilterOutGrotty()
        {
            var inputTableServices = new List<TimeTableService>
            {
                Utils.GetService(BusCompany.Posh, (10, 10), (10, 50)),
                Utils.GetService(BusCompany.Grotty, (10, 10), (10, 50)),
            };

            var expectedTableServices = new List<TimeTableService>
            {
                Utils.GetService(BusCompany.Posh, (10, 10), (10, 50)),
            };

            await ProcessAndCompareTimeTableServicesAsync(inputTableServices, expectedTableServices);
        }

        [Test]
        public async Task ProcessTimeTableAsync_UnorderedServices_ShouldBeOrderedByDepartureTime()
        {
            var inputTableServices = new List<TimeTableService>
            {
                Utils.GetService(BusCompany.Posh, (12, 10), (12, 50)),
                Utils.GetService(BusCompany.Grotty, (10, 10), (10, 50)),
                Utils.GetService(BusCompany.Posh, (11, 10), (11, 50)),
            };

            var expectedTableServices = new List<TimeTableService>
            {
                Utils.GetService(BusCompany.Grotty, (10, 10), (10, 50)),
                Utils.GetService(BusCompany.Posh, (11, 10), (11, 50)),
                Utils.GetService(BusCompany.Posh, (12, 10), (12, 50)),
            };

            await ProcessAndCompareTimeTableServicesAsync(inputTableServices, expectedTableServices);
        }

        private async Task ProcessAndCompareTimeTableServicesAsync(List<TimeTableService> sourceTimeTableServices, List<TimeTableService> expectedTimeTableServices)
        {
            // Arrange
            const string ExpectedOutputFilePath = "output.txt";

            string actualOutputFilePath = null;
            TimeTable actualTimeTable = null;

            var timeTable = new TimeTable(sourceTimeTableServices);

            var expectedTimeTable = new TimeTable(expectedTimeTableServices);

            timeTableReaderMock
                .Setup(x => x.ReadTimeTableAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => timeTable);

            timeTableWriterMock
                .Setup(x => x.WriteTimeTableAsync(It.IsAny<string>(), It.IsAny<TimeTable>(), It.IsAny<CancellationToken>()))
                .Callback<string, TimeTable, CancellationToken>((filePath, resultTimeTable, cancellationToken) =>
                {
                    actualOutputFilePath = filePath;
                    actualTimeTable = resultTimeTable;
                });

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await systemUnderTest.ProcessTimeTableAsync(FilePath, cancellationTokenSource.Token).ConfigureAwait(false);

            // Assert
            actualOutputFilePath.Should().Be(ExpectedOutputFilePath);
            actualTimeTable.Should().BeEquivalentTo(expectedTimeTable);
        }
    }
}