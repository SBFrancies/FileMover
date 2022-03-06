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
    public class ExitCommandReaderTests
    {
        private readonly Mock<IApplicationProcesses> _mockApplicationProcesses = new Mock<IApplicationProcesses>();

        [Theory]
        [InlineData("close")]
        [InlineData("EXIT")]
        [InlineData("q")]
        [InlineData("hjklkk")]
        public async Task ExitCommandReader_ValidateAndRunAsync_DoesNotCloseApplicationWhenCommandDoesntMatch(string command)
        {
            var sut = GetSystemUnderTest();

            await sut.ValidateAndRunAsync(command);

            _mockApplicationProcesses.Verify(a => a.ExitApplication(It.IsAny<int>()), Times.Never);
        }

        [Theory]
        [InlineData("quit")]
        [InlineData("QUIT")]
        [InlineData("quIT")]
        [InlineData("qUiT")]
        public async Task ExitCommandReader_ValidateAndRunAsync_ExitsApplicationWhenCommandMatches(string command)
        {
            var sut = GetSystemUnderTest();

            await sut.ValidateAndRunAsync(command);

            _mockApplicationProcesses.Verify(a => a.ExitApplication(0), Times.Once);
        }

        public ExitCommandReader GetSystemUnderTest()
        {
            return new ExitCommandReader(_mockApplicationProcesses.Object);
        }
    }
}
