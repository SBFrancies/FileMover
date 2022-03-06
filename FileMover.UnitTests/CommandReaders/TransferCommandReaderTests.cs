using FileMover.CommandReaders;
using FileMover.Interface;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FileMover.UnitTests.CommandReaders
{
    public class TransferCommandReaderTests
    {
        private readonly Mock<IFileMoverService> _mockFileMoverService = new Mock<IFileMoverService>();
        private readonly Mock<IWriter> _mockWriter = new Mock<IWriter>();

        [Theory]
        [InlineData("transfer C:\\Test1 C:\\Test2")]
        [InlineData("t -s C:\\Test1 -d C:\\Test2")]
        [InlineData("move -s C:\\Test1 -d C:\\Test2")]
        [InlineData("hjkkjkll,.,ldckmfdkfkk")]
        public async Task TransferCommandReader_ValidateAndRunAsync_DoesNotTransferWhenCommandDoesntMatch(string command)
        {
            var sut = GetSystemUnderTest();

            await sut.ValidateAndRunAsync(command);

            _mockFileMoverService.Verify(a => a.MoveFilesAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData("transfer -s C:\\Test1 -d C:\\Test2", "C:\\Test1", "C:\\Test2")]
        [InlineData("TRANSFER -s C:\\Test1 -d C:\\Test2", "C:\\Test1", "C:\\Test2")]
        [InlineData("TrANsfER -s C:\\Test 1 -d C:\\Test 2", "C:\\Test 1", "C:\\Test 2")]
        [InlineData("transfer -s C:\\Code\\FileMover\\FileMover.IntegrationTests\\bin\\Debug\\net6.0 -d D:\\Test\\testing 1", "C:\\Code\\FileMover\\FileMover.IntegrationTests\\bin\\Debug\\net6.0", "D:\\Test\\testing 1")]
        public async Task TransferCommandReader_ValidateAndRunAsync_TransfersWhenCommandMatches(string command, string source, string destination)
        {
            var sut = GetSystemUnderTest();

            await sut.ValidateAndRunAsync(command);

            _mockFileMoverService.Verify(a => a.MoveFilesAsync(source, destination), Times.Once);
        }

        private TransferCommandReader GetSystemUnderTest()
        {
            return new TransferCommandReader(_mockFileMoverService.Object, _mockWriter.Object);
        }
    }
}
