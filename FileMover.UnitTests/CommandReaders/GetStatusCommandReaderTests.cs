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
    public class GetStatusCommandReaderTests
    {
        private readonly Mock<IFileMoverService> _mockFileMoverService = new Mock<IFileMoverService>();
        private readonly Mock<IWriter> _mockWriter = new Mock<IWriter>();

        [Theory]
        [InlineData("statu")]
        [InlineData("statussss")]
        [InlineData("s")]
        [InlineData("hjkkjklkk")]
        public async Task GetStatusCommandReader_ValidateAndRunAsync_DoesNotCheckStatusWhenCommandDoesntMatch(string command)
        {
            var sut = GetSystemUnderTest();

            await sut.ValidateAndRunAsync(command);

            _mockFileMoverService.Verify(a => a.PrintFileStatusAsync(), Times.Never);
        }

        [Theory]
        [InlineData("status")]
        [InlineData("Status")]
        [InlineData("STATUS")]
        [InlineData("sTaTuS")]
        public async Task GetStatusCommandReader_ValidateAndRunAsync_ChecksStatusWhenCommandMatches(string command)
        {
            var sut = GetSystemUnderTest();

            await sut.ValidateAndRunAsync(command);

            _mockFileMoverService.Verify(a => a.PrintFileStatusAsync(), Times.Once);
        }

        private GetStatusCommandReader GetSystemUnderTest()
        {
            return new GetStatusCommandReader(_mockFileMoverService.Object, _mockWriter.Object);
        }
    }
}
