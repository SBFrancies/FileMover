using FileMover.Data.Entities;
using FileMover.Enums;
using FileMover.Interface;
using FileMover.Services;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FileMover.UnitTests.Services
{
    public class QueueProcessorServiceTests
    {
        private readonly Mock<IFileSystem> _mockFileSystem = new Mock<IFileSystem>();
        private readonly Mock<ISession> _mockSession = new Mock<ISession>();
        private readonly Mock<IFileTransferDataAccess> _mockFileTransferDataAccess = new Mock<IFileTransferDataAccess>();

        [Fact]
        public async Task QueueProcessorService_ProcessQueueAsync_SavesErrorIfDestinationDirectoryDoesNotExist()
        {
            var fileTransfer = new FileTransfer
            {
                SourcePath = "SourcePath\\test.txt",
                DestinationPath = "DoesNotExist\\test.txt",
            };

            var directory = Path.GetDirectoryName(fileTransfer.DestinationPath);
            _mockFileSystem.Setup(a => a.DirectoryExists(directory)).Returns(false);

            var blockingCollection = new BlockingCollection<FileTransfer>(new ConcurrentQueue<FileTransfer>(new[] {fileTransfer}));
            var sut = GetSystemUnderTest();
            new Task(async () => await sut.ProcessQueueAsync(blockingCollection),TaskCreationOptions.LongRunning).Start();

            await Task.Delay(TimeSpan.FromSeconds(5));

            _mockFileSystem.Verify(a => a.DirectoryExists(directory), Times.Once);
            _mockFileSystem.Verify(a => a.DeleteFile(It.IsAny<string>()), Times.Never);
            _mockFileSystem.Verify(a => a.CopyFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
            _mockSession.Verify(a => a.TransferComplete(fileTransfer.SourcePath), Times.Once);
            _mockFileTransferDataAccess.Verify(a => a.UpdateFileTransferAsync(
                It.Is<FileTransfer>(a => a.Status == TransferStatus.Error &&
                a.ErrorMessage == "Destination directory does not exist")), Times.Once);
        }

        [Fact]
        public async Task QueueProcessorService_ProcessQueueAsync_SavesErrorIfSourceFileDoesNotExist()
        {
            var fileTransfer = new FileTransfer
            {
                SourcePath = "SourcePath\\test.txt",
                DestinationPath = "DoesExist\\test.txt",
            };

            var directory = Path.GetDirectoryName(fileTransfer.DestinationPath);
            _mockFileSystem.Setup(a => a.DirectoryExists(directory)).Returns(true);
            _mockFileSystem.Setup(a => a.FileExists(fileTransfer.SourcePath)).Returns(false);

            var blockingCollection = new BlockingCollection<FileTransfer>(new ConcurrentQueue<FileTransfer>(new[] { fileTransfer }));
            var sut = GetSystemUnderTest();
            new Task(async () => await sut.ProcessQueueAsync(blockingCollection), TaskCreationOptions.LongRunning).Start();

            await Task.Delay(TimeSpan.FromSeconds(5));

            _mockFileSystem.Verify(a => a.DirectoryExists(directory), Times.Once);
            _mockFileSystem.Verify(a => a.FileExists(fileTransfer.SourcePath), Times.Once);
            _mockFileSystem.Verify(a => a.DeleteFile(It.IsAny<string>()), Times.Never);
            _mockFileSystem.Verify(a => a.CopyFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
            _mockSession.Verify(a => a.TransferComplete(fileTransfer.SourcePath), Times.Once);
            _mockFileTransferDataAccess.Verify(a => a.UpdateFileTransferAsync(
                It.Is<FileTransfer>(a => a.Status == TransferStatus.Error &&
                a.ErrorMessage == "File does not exist in source directory")), Times.Once);
        }

        [Fact]
        public async Task QueueProcessorService_ProcessQueueAsync_SavesErrorIfSourceIfFileExistsDestination()
        {
            var fileTransfer = new FileTransfer
            {
                SourcePath = "SourcePath\\test.txt",
                DestinationPath = "DoesExist\\test.txt",
            };

            var directory = Path.GetDirectoryName(fileTransfer.DestinationPath);
            _mockFileSystem.Setup(a => a.DirectoryExists(directory)).Returns(true);
            _mockFileSystem.Setup(a => a.FileExists(fileTransfer.SourcePath)).Returns(true);
            _mockFileSystem.Setup(a => a.FileExists(fileTransfer.DestinationPath)).Returns(true);

            var blockingCollection = new BlockingCollection<FileTransfer>(new ConcurrentQueue<FileTransfer>(new[] { fileTransfer }));
            var sut = GetSystemUnderTest();
            new Task(async () => await sut.ProcessQueueAsync(blockingCollection), TaskCreationOptions.LongRunning).Start();

            await Task.Delay(TimeSpan.FromSeconds(5));

            _mockFileSystem.Verify(a => a.DirectoryExists(directory), Times.Once);
            _mockFileSystem.Verify(a => a.FileExists(fileTransfer.SourcePath), Times.Once);
            _mockFileSystem.Verify(a => a.FileExists(fileTransfer.DestinationPath), Times.Once);
            _mockFileSystem.Verify(a => a.DeleteFile(It.IsAny<string>()), Times.Never);
            _mockFileSystem.Verify(a => a.CopyFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
            _mockSession.Verify(a => a.TransferComplete(fileTransfer.SourcePath), Times.Once);
            _mockFileTransferDataAccess.Verify(a => a.UpdateFileTransferAsync(
                It.Is<FileTransfer>(a => a.Status == TransferStatus.Error &&
                a.ErrorMessage == "File already exists in destination directory")), Times.Once);
        }

        [Fact]
        public async Task QueueProcessorService_ProcessQueueAsync_SavesErrorWhenExceptionIsThrown()
        {
            var fileTransfer = new FileTransfer
            {
                SourcePath = "SourcePath\\test.txt",
                DestinationPath = "DoesExist\\test.txt",
            };

            var directory = Path.GetDirectoryName(fileTransfer.DestinationPath);
            _mockFileSystem.Setup(a => a.DirectoryExists(directory)).Returns(true);
            _mockFileSystem.Setup(a => a.FileExists(fileTransfer.SourcePath)).Returns(true);
            _mockFileSystem.Setup(a => a.FileExists(fileTransfer.DestinationPath)).Returns(false);
            _mockFileTransferDataAccess.Setup(a => a.UpdateFileTransferAsync(It.Is<FileTransfer>(a => a.Status == TransferStatus.Copying))).ThrowsAsync(new Exception("Test exception"));

            var blockingCollection = new BlockingCollection<FileTransfer>(new ConcurrentQueue<FileTransfer>(new[] { fileTransfer }));
            var sut = GetSystemUnderTest();
            new Task(async () => await sut.ProcessQueueAsync(blockingCollection), TaskCreationOptions.LongRunning).Start();

            await Task.Delay(TimeSpan.FromSeconds(5));

            _mockFileSystem.Verify(a => a.DirectoryExists(directory), Times.Once);
            _mockFileSystem.Verify(a => a.FileExists(fileTransfer.SourcePath), Times.Once);
            _mockFileSystem.Verify(a => a.FileExists(fileTransfer.DestinationPath), Times.Once);
            _mockFileSystem.Verify(a => a.DeleteFile(It.IsAny<string>()), Times.Never);
            _mockFileSystem.Verify(a => a.CopyFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
            _mockSession.Verify(a => a.TransferComplete(fileTransfer.SourcePath), Times.Once);
            _mockFileTransferDataAccess.Verify(a => a.UpdateFileTransferAsync(
                It.Is<FileTransfer>(a => a.Status == TransferStatus.Error &&
                a.ErrorMessage == "Test exception")), Times.AtLeastOnce);
        }

        [Fact]
        public async Task QueueProcessorService_ProcessQueueAsync_CopiesFileAndUpdatesStatusWhenNoErrors()
        {
            var fileTransfer = new FileTransfer
            {
                SourcePath = "SourcePath\\test.txt",
                DestinationPath = "DoesExist\\test.txt",
            };

            var directory = Path.GetDirectoryName(fileTransfer.DestinationPath);
            _mockFileSystem.Setup(a => a.DirectoryExists(directory)).Returns(true);
            _mockFileSystem.Setup(a => a.FileExists(fileTransfer.SourcePath)).Returns(true);
            _mockFileSystem.Setup(a => a.FileExists(fileTransfer.DestinationPath)).Returns(false);

            var blockingCollection = new BlockingCollection<FileTransfer>(new ConcurrentQueue<FileTransfer>(new[] { fileTransfer }));
            var sut = GetSystemUnderTest();
            new Task(async () => await sut.ProcessQueueAsync(blockingCollection), TaskCreationOptions.LongRunning).Start();

            await Task.Delay(TimeSpan.FromSeconds(5));

            _mockFileSystem.Verify(a => a.DirectoryExists(directory), Times.Once);
            _mockFileSystem.Verify(a => a.FileExists(fileTransfer.SourcePath), Times.Once);
            _mockFileSystem.Verify(a => a.FileExists(fileTransfer.DestinationPath), Times.Once);
            _mockSession.Verify(a => a.TransferComplete(fileTransfer.SourcePath), Times.Once);
            _mockFileSystem.Verify(a => a.CopyFileAsync(fileTransfer.SourcePath, fileTransfer.DestinationPath), Times.Once);
            _mockFileSystem.Verify(a => a.DeleteFile(fileTransfer.SourcePath), Times.Once);
            _mockFileTransferDataAccess.Verify(a => a.UpdateFileTransferAsync(It.Is<FileTransfer>(a =>a.Status != TransferStatus.Awaiting && a.Status != TransferStatus.Error)), Times.Exactly(2));
        }

        private QueueProcessorService GetSystemUnderTest()
        {
            return new QueueProcessorService(_mockFileSystem.Object, _mockSession.Object, _mockFileTransferDataAccess.Object);
        }
    }
}
