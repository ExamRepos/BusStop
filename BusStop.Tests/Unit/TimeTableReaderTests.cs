using System;
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
    public class TimeTableReaderTests
    {
        private const string FilePath = "TestFilePath";

        private Mock<IFileReader> fileReaderMock;

        private TimeTableReader systemUnderTest;

        [SetUp]
        public void Setup()
        {
            fileReaderMock = new Mock<IFileReader>();

            systemUnderTest = new TimeTableReader(fileReaderMock.Object);
        }

        [Test]
        public async Task ReadTimeTableAsync_ValidFileContentsWithMultipleServices_ShouldReturnTimeTable()
        {
            // Arrange
            const string ValidTimeTableFileContents = @"Posh 10:15 11:10
Posh 10:10 11:00
Grotty 10:10 11:00
Grotty 16:30 18:45
Posh 12:05 12:30
Grotty 12:30 13:25
Grotty 12:45 13:25
Posh 17:25 18:01";

            fileReaderMock
                .Setup(x => x.ReadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidTimeTableFileContents);

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

            var expectedResult = new TimeTable(tableServices);

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var result = await systemUnderTest.ReadTimeTableAsync(FilePath, cancellationTokenSource.Token).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public async Task ReadTimeTableAsync_ValidFileContentsWithSingleService_ShouldReturnTimeTable()
        {
            // Arrange
            const string ValidTimeTableFileContents = "Posh 10:15 11:10";

            fileReaderMock
                .Setup(x => x.ReadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ValidTimeTableFileContents);

            var tableServices = new List<TimeTableService>
            {
                Utils.GetService(BusCompany.Posh, (10, 15), (11, 10)),
            };

            var expectedResult = new TimeTable(tableServices);

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var result = await systemUnderTest.ReadTimeTableAsync(FilePath, cancellationTokenSource.Token).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        [TestCase("Posh 10:15 11:10 Extra")]
        [TestCase("Extra Posh 10:15 11:10")]
        [TestCase("Posh 10:15")]
        public async Task ReadTimeTableAsync_TooManyItemsInAServiceRecord_ShouldThrowException(string serviceRecord)
        {
            // Arrange
            fileReaderMock
                .Setup(x => x.ReadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceRecord);

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            Func<Task> act = async () => await systemUnderTest.ReadTimeTableAsync(FilePath, cancellationTokenSource.Token).ConfigureAwait(false);

            // Assert
            await act.Should().ThrowAsync<FormatException>().WithMessage(FormattableString.Invariant($"Line on row 1 is not in correct format. Actual value: \"{serviceRecord}\".")).ConfigureAwait(false);
        }

        [Test]
        [TestCase("PoshZ 10:15 11:10", "PoshZ")]
        [TestCase("POSH 10:15 11:10", "POSH")]
        [TestCase("posh 10:15 11:10", "posh")]
        public async Task ReadTimeTableAsync_InvalidCompanyId_ShouldThrowException(string serviceRecord, string companyId)
        {
            // Arrange
            fileReaderMock
                .Setup(x => x.ReadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceRecord);

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            Func<Task> act = async () => await systemUnderTest.ReadTimeTableAsync(FilePath, cancellationTokenSource.Token).ConfigureAwait(false);

            // Assert
            await act.Should().ThrowAsync<FormatException>().WithMessage(FormattableString.Invariant($"The companyId at line 1 is using an unknown format. Actual value: \"{companyId}\".")).ConfigureAwait(false);
        }

        [Test]
        [TestCase("Posh 5:15 11:10", "5:15")]
        [TestCase("Posh 95:15 11:10", "95:15")]
        [TestCase("Posh AA:15 11:10", "AA:15")]
        [TestCase("Posh 10:65 11:10", "10:65")]
        public async Task ReadTimeTableAsync_InvalidDepartureDate_ShouldThrowException(string serviceRecord, string expectedTime)
        {
            // Arrange
            fileReaderMock
                .Setup(x => x.ReadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceRecord);

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            Func<Task> act = async () => await systemUnderTest.ReadTimeTableAsync(FilePath, cancellationTokenSource.Token).ConfigureAwait(false);

            // Assert
            await act.Should().ThrowAsync<FormatException>().WithMessage(FormattableString.Invariant($"The departureTime at line 1 is using an unknown format. Actual value: \"{expectedTime}\".")).ConfigureAwait(false);
        }

        [Test]
        [TestCase("Posh 07:10 5:15", "5:15")]
        [TestCase("Posh 07:10 95:15", "95:15")]
        [TestCase("Posh 07:10 AA:15", "AA:15")]
        [TestCase("Posh 07:10 10:65", "10:65")]
        public async Task ReadTimeTableAsync_InvalidActualDate_ShouldThrowException(string serviceRecord, string expectedTime)
        {
            // Arrange
            fileReaderMock
                .Setup(x => x.ReadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceRecord);

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            Func<Task> act = async () => await systemUnderTest.ReadTimeTableAsync(FilePath, cancellationTokenSource.Token).ConfigureAwait(false);

            // Assert
            await act.Should().ThrowAsync<FormatException>().WithMessage(FormattableString.Invariant($"The arrivalTime at line 1 is using an unknown format. Actual value: \"{expectedTime}\".")).ConfigureAwait(false);
        }

        [Test]
        [TestCase("Posh 07:10  05:15")]
        [TestCase("Posh  07:10  05:15")]
        [TestCase("Posh 07:10     05:15")]
        [TestCase("Posh 07:10,05:15")]
        [TestCase("Posh-07:10 05:15")]
        [TestCase("Posh-07:10-05:15")]
        public async Task ReadTimeTableAsync_InvalidSeparators_ShouldThrowException(string serviceRecord)
        {
            // Arrange
            fileReaderMock
                .Setup(x => x.ReadFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(serviceRecord);

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            Func<Task> act = async () => await systemUnderTest.ReadTimeTableAsync(FilePath, cancellationTokenSource.Token).ConfigureAwait(false);

            // Assert
            await act.Should().ThrowAsync<FormatException>().WithMessage(FormattableString.Invariant($"Line on row 1 is not in correct format. Actual value: \"{serviceRecord}\".")).ConfigureAwait(false);
        }
    }
}