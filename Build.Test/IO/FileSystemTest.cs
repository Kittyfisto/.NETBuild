using System.IO;
using Build.IO;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.IO
{
	[TestFixture]
	public sealed class FileSystemTest
		: AbstractFilesystemTest
	{
		private IFileSystem _fileSystem;

		[SetUp]
		public void Setup()
		{
			_fileSystem = new FileSystem();
		}

		[Test]
		public void TestCurrentDirectory()
		{
			_fileSystem.CurrentDirectory.Should().Be(Directory.GetCurrentDirectory());
		}

		protected override IFileSystem Filesystem
		{
			get { return _fileSystem; }
		}
	}
}