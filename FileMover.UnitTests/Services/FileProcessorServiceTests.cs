using AutoFixture;
using FileMover.Data.Entities;
using FileMover.Interface;
using FileMover.Services;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FileMover.UnitTests.Services
{
    public class FileProcessorServiceTests
    {
        private readonly Mock<IQueueProcessorFactory<FileTransfer>> _mockQueueProcessorFactory  = new Mock<IQueueProcessorFactory<FileTransfer>>();    
        private readonly Mock<IFileTransferDataAccess> _mockFileTransferDataAccess= new Mock<IFileTransferDataAccess>();
        private readonly Mock<IIdGenerator> _mockIdGenerator= new Mock<IIdGenerator>();
        private readonly Mock<IDateTimeProvider> _mockClock = new Mock<IDateTimeProvider>();
        private readonly Mock<ISession> _mockSession = new Mock<ISession>();
        private readonly Mock<ITaskFactory> _mockTaskFactory = new Mock<ITaskFactory>();
        private readonly Fixture _fixture = new Fixture();

        [Theory]
        [InlineData(1)]
        [InlineData(23)]
        [InlineData(5)]
        public async Task FileProcessorService_CopyFilesAsync_SavesAndStartsProcessingEachFile(int fileCount)
        {
            var files = _fixture.CreateMany<string>(fileCount).ToArray();
            var destination = "dest";

            var sut = GetSystemUnderTest();

            await sut.CopyFilesAsync(files, destination, null);

            _mockIdGenerator.Verify(a => a.GenerateId(), Times.Exactly(fileCount));
            _mockClock.Verify(a => a.UtcNow, Times.Exactly(fileCount));
            _mockFileTransferDataAccess.Verify(a => a.SaveFileTransferAsync(It.IsAny<FileTransfer>()), Times.Exactly(fileCount));

            var extensions = files.Select(a => Path.GetExtension(a)).Distinct();

            _mockTaskFactory.Verify(a => a.CreateAndStartTask(It.IsAny<Action>(), TaskCreationOptions.LongRunning), Times.Exactly(extensions.Count()));
        }

        [Fact]
        public async Task FileProcessorService_CopyFilesAsync_WhenIdIsPopulatedNoNewEntityIsSaved()
        {
            var files = new[] { "source/test.txt" };
            var destination = "dest";

            var sut = GetSystemUnderTest();

            await sut.CopyFilesAsync(files, destination, Guid.NewGuid());

            _mockIdGenerator.Verify(a => a.GenerateId(), Times.Never);
            _mockClock.Verify(a => a.UtcNow, Times.Once);
            _mockFileTransferDataAccess.Verify(a => a.SaveFileTransferAsync(It.IsAny<FileTransfer>()), Times.Never);

            _mockTaskFactory.Verify(a => a.CreateAndStartTask(It.IsAny<Action>(), TaskCreationOptions.LongRunning), Times.Once);
        }


        private FileProcessorService GetSystemUnderTest()
        {
            return new FileProcessorService(_mockQueueProcessorFactory.Object, _mockFileTransferDataAccess.Object, _mockIdGenerator.Object, _mockClock.Object, _mockSession.Object, _mockTaskFactory.Object);
        }
    }
}
