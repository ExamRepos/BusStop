using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusStop.Constants;
using BusStop.Domain;
using BusStop.Domain.IO;
using BusStop.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BusStop.Tests.Unit
{
    public class TimeTableWriterTests
    {
        private const string FilePath = "TestFilePath";

        private Mock<IFileWriter> fileWriterMock;

        private TimeTableWriter systemUnderTest;

        [SetUp]
        public void Setup()
        {
            fileWriterMock = new Mock<IFileWriter>();

            systemUnderTest = new TimeTableWriter(fileWriterMock.Object);
        }

        [Test]
        public async Task WriteTimeTableAsync_ValidTimeTable_ShouldWriteDataInCorrectFormatToAFileWriter()
        {
            // Arrange
            const string ExpectedOutputFilePath = FilePath;
            const string ExpectedTimeTableFileContents = @"Posh 10:10 11:00
Posh 10:15 11:10
Posh 12:05 12:30
Posh 17:25 18:01

Grotty 10:10 11:00
Grotty 12:30 13:25
Grotty 12:45 13:25
Grotty 16:30 18:45";

            string actualOutputFilePath = null;
            string actualTimeTableFileContents = null;

            fileWriterMock
                .Setup(x => x.WriteFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((filePath, contents, cancellationToken) =>
                {
                    actualOutputFilePath = filePath;
                    actualTimeTableFileContents = contents;
                });

            var tableServices = new List<TimeTableService>
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

            var timeTable = new TimeTable(tableServices);

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await systemUnderTest.WriteTimeTableAsync(FilePath, timeTable, cancellationTokenSource.Token).ConfigureAwait(false);

            // Assert
            actualOutputFilePath.Should().Be(ExpectedOutputFilePath);
            actualTimeTableFileContents.Should().Be(ExpectedTimeTableFileContents);
        }
    }
}