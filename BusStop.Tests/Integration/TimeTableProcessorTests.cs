using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BusStop.Domain;
using BusStop.Domain.IO;
using BusStop.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace BusStop.Tests.Integration
{
    public class TimeTableProcessorTests
    {
        private Mock<IFileWriter> fileWriterMock;

        private TimeTableProcessor systemUnderTest;

        [SetUp]
        public void Setup()
        {
            fileWriterMock = new Mock<IFileWriter>();

            var fileReader = new FileReader();
            var timeTableReader = new TimeTableReader(fileReader);
            var timeTableWriter = new TimeTableWriter(fileWriterMock.Object);

            systemUnderTest = new TimeTableProcessor(timeTableReader, timeTableWriter);
        }

        [Test]
        [TestCase("Test1", "Result1")]
        [TestCase("Test2", "Result2")]
        [TestCase("Test3", "Result3")]
        [TestCase("Test4", "Result4")]
        [TestCase("Test5", "Result5")]
        public async Task ProcessTimeTableAsync_ValidTimeTable_ResultShouldMatchExpectation(string inputFileName, string outputFileName)
        {
            // Arrange
            string inputFilePath = Path.Combine("Data", FormattableString.Invariant($"{inputFileName}.txt"));
            string expectedResultFilePath = Path.Combine("Data", FormattableString.Invariant($"{outputFileName}.txt"));

            string expectedResult = await File.ReadAllTextAsync(expectedResultFilePath).ConfigureAwait(false);

            string actualResult = null;

            fileWriterMock
                .Setup(x => x.WriteFileAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((filePath, contents, cancellationToken) =>
                {
                    actualResult = contents;
                });

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await systemUnderTest.ProcessTimeTableAsync(inputFilePath, cancellationTokenSource.Token).ConfigureAwait(false);

            // Assert
            actualResult.Should().Be(expectedResult);
        }
    }
}