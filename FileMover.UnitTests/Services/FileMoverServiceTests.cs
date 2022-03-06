using AutoFixture;
using FileMover.Data.Entities;
using FileMover.Enums;
using FileMover.Interface;
using FileMover.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FileMover.UnitTests.Services
{
    public class FileMoverServiceTests
    {
        private readonly Mock<IFileProcessor> _mockFileProcessor = new Mock<IFileProcessor>();
        private readonly Mock<IFileSystem> _mockFileSystem = new Mock<IFileSystem>();
        private readonly Mock<ISession> _mockSession = new Mock<ISession>();
        private readonly Mock<IFileTransferDataAccess> _mockFileTransferDataAccess = new Mock<IFileTransferDataAccess>();
        private readonly Mock<IWriter> _mockWriter = new Mock<IWriter>();
        private readonly Mock<ITaskFactory> _mockTaskFactory = new Mock<ITaskFactory>();
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public async Task FileMoverService_MoveFilesAsync_CannotTransferToSameDirectory()
        {
            var source = "test";
            var destination = "test";

            var sut = GetSystemUnderTest();

            await sut.MoveFilesAsync(source, destination);

            _mockWriter.Verify(a => a.WriteLine("Source directory must be different from destination directory"), Times.Once);
            _mockWriter.Verify(a => a.WriteLine($"Transferring files from {source} to {destination}"), Times.Never);
            _mockFileSystem.Verify(a => a.GetDirectoryFiles(source), Times.Never);
            _mockSession.Verify(a => a.AddToSession(It.IsAny<string>(), It.IsAny<IList<string>>()), Times.Never);
            _mockFileProcessor.Verify(a => a.CopyFilesAsync(It.IsAny<string[]>(), destination, null), Times.Never);
        }

        [Fact]
        public async Task FileMoverService_MoveFiles_NoTransferIfNoFilesInSourceDirectory()
        {
            var source = "test";
            var destination = "test-2";

            _mockFileSystem.Setup(a => a.GetDirectoryFiles(source)).Returns(Array.Empty<string>());

            var sut = GetSystemUnderTest();

            await sut.MoveFilesAsync(source, destination);

            _mockWriter.Verify(a => a.WriteLine("Source directory must be different from destination directory"), Times.Never);
            _mockWriter.Verify(a => a.WriteLine($"Transferring files from {source} to {destination}"), Times.Once);
            _mockFileSystem.Verify(a => a.GetDirectoryFiles(source), Times.Once);
            _mockSession.Verify(a => a.AddToSession(It.IsAny<string>(), It.IsAny<IList<string>>()), Times.Never);
            _mockFileProcessor.Verify(a => a.CopyFilesAsync(It.IsAny<string[]>(), destination, null), Times.Never);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(32)]
        [InlineData(1023)]
        public async Task FileMoverService_MoveFiles_WillTransferFilesInSourceDirectory(int countFilesInSource)
        {
            var source = "test";
            var destination = "test-2";
            var sourceFiles = _fixture.CreateMany<string>(countFilesInSource).ToArray();

            _mockFileSystem.Setup(a => a.GetDirectoryFiles(source)).Returns(sourceFiles);

            var sut = GetSystemUnderTest();

            await sut.MoveFilesAsync(source, destination);

            _mockWriter.Verify(a => a.WriteLine("Source directory must be different from destination directory"), Times.Never);
            _mockWriter.Verify(a => a.WriteLine($"Transferring files from {source} to {destination}"), Times.Once);
            _mockFileSystem.Verify(a => a.GetDirectoryFiles(source), Times.Once);
            _mockSession.Verify(a => a.AddToSession(It.IsAny<string>(), It.IsAny<IList<string>>()), Times.Once);
            _mockFileProcessor.Verify(a => a.CopyFilesAsync(It.IsAny<string[]>(), destination, null), Times.Once);
        }

        [Fact]
        public async Task FileMoverService_PrintFileStatusAsync_WillPrintMessageWhenNoTransfersMade()
        {
            var sessionId = Guid.NewGuid();
            _mockSession.SetupGet(a => a.SessionId).Returns(sessionId);
            _mockFileTransferDataAccess.Setup(a => a.GetSessionFileTransfersAsync(sessionId)).ReturnsAsync(Array.Empty<FileTransfer>());

            var sut = GetSystemUnderTest();

            await sut.PrintFileStatusAsync();

            _mockWriter.Verify(a => a.WriteLine("No transfers to display"), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(32)]
        [InlineData(123)]
        public async Task FileMoverService_PrintFileStatusAsync_WillPrintOutStatusForEachTransfer(int countTransfers)
        {
            var sessionId = Guid.NewGuid();
            _mockSession.SetupGet(a => a.SessionId).Returns(sessionId);
            var transfers = _fixture.CreateMany<FileTransfer>(countTransfers);
            _mockFileTransferDataAccess.Setup(a => a.GetSessionFileTransfersAsync(sessionId)).ReturnsAsync(transfers);
            var sut = GetSystemUnderTest();

            await sut.PrintFileStatusAsync();

            _mockWriter.Verify(a => a.WriteLine("No transfers to display"), Times.Never);

            foreach(var item in transfers)
            {
                _mockWriter.Verify(a => a.WriteLine(item.ToString()), Times.Once);
            }

            _mockWriter.Verify(a => a.WriteLine(It.IsAny<string>()), Times.Exactly(countTransfers));
        }

        [Fact]
        public async Task FileMoverService_LoadPartialDownloadsAsync_NoItemsFoundNoTaskStarted()
        {
            var sessionId = Guid.NewGuid();
            _mockSession.SetupGet(a => a.SessionId).Returns(sessionId);
            _mockFileTransferDataAccess.Setup(a => a.GetIncompleteTransfersAsync(sessionId)).ReturnsAsync(Array.Empty<FileTransfer>());

            var sut = GetSystemUnderTest();

            await sut.LoadPartialDownloadsAsync();

            _mockFileTransferDataAccess.Verify(a => a.GetIncompleteTransfersAsync(sessionId), Times.Once);
            _mockTaskFactory.Verify(a => a.CreateAndStartTask(It.IsAny<Action>(), It.IsAny<TaskCreationOptions>()), Times.Never);
        }

        [Fact]
        public async Task FileMoverService_LoadPartialDownloadsAsync_WillCreatTaskFoEachItemFoundAndDeleteExistngFiles()
        {
            var fileTransfers = new[]
            {
                new FileTransfer
                {
                    Id = Guid.NewGuid(),
                    SourcePath = "source\\test1.txt",
                    Status = TransferStatus.Copying,
                    DestinationPath = "dest\\test1.txt"        
                },
                new FileTransfer
                {
                    Id = Guid.NewGuid(),
                    SourcePath = "source\\test1.txt",
                    Status = TransferStatus.Awaiting,
                    DestinationPath = "dest\\test1.txt"
                },
                new FileTransfer
                {
                    Id = Guid.NewGuid(),
                    SourcePath = "source\\test2.txt",
                    Status = TransferStatus.Copying,
                    DestinationPath = "dest\\test2.txt"
                },
                new FileTransfer
                {
                    Id = Guid.NewGuid(),
                    SourcePath = "source\\test2.txt",
                    Status = TransferStatus.Awaiting,
                    DestinationPath = "dest\\test2.txt"
                },
            };

            _mockFileSystem.Setup(a => a.FileExists("dest\\test1.txt")).Returns(true);
            _mockFileSystem.Setup(a => a.FileExists("dest\\test2.txt")).Returns(false);

            var sessionId = Guid.NewGuid();
            _mockSession.SetupGet(a => a.SessionId).Returns(sessionId);
            _mockFileTransferDataAccess.Setup(a => a.GetIncompleteTransfersAsync(sessionId)).ReturnsAsync(fileTransfers);

            var sut = GetSystemUnderTest();

            await sut.LoadPartialDownloadsAsync();

            _mockFileTransferDataAccess.Verify(a => a.GetIncompleteTransfersAsync(sessionId), Times.Once);
            _mockFileSystem.Verify(a => a.FileExists(It.IsAny<string>()), Times.Exactly(2));
            _mockFileSystem.Verify(a => a.DeleteFile(It.IsAny<string>()), Times.Once);
            _mockTaskFactory.Verify(a => a.CreateAndStartTask(It.IsAny<Action>(), TaskCreationOptions.LongRunning), Times.Exactly(4));
        }

        private FileMoverService GetSystemUnderTest()
        {
            return new FileMoverService(_mockFileProcessor.Object, _mockFileSystem.Object, _mockSession.Object, _mockFileTransferDataAccess.Object, _mockWriter.Object, _mockTaskFactory.Object);
        }
    }
}
