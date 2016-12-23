using System;
using System.IO;
using Build.IO;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.IO
{
	[TestFixture]
	public abstract class AbstractFilesystemTest
	{
		protected abstract IFileSystem Filesystem { get; }

		[Test]
		public void TestGetInfo1()
		{
			var info = Filesystem.GetFileInfo(@"P:\\foo.txt");
			info.Exists.Should().BeFalse();
			info.Length.Should().Be(0);
			info.CreatedTime.Should().Be(DateTime.MinValue);
			info.ModifiedTime.Should().Be(DateTime.MinValue);
			info.AccessTime.Should().Be(DateTime.MinValue);
		}

		[Test]
		[Description("Verifies that CreateDirectory throws when the drive doesn't exist")]
		public void TestCreateDirectory1()
		{
			new Action(() => Filesystem.CreateDirectory(@"P:\foo")).ShouldThrow<DirectoryNotFoundException>();
		}

		[Test]
		public void TestCreateDirectory2()
		{
			Filesystem.CreateDirectory("somedirectory");
			Filesystem.ExistsDirectory("somedirectory").Should().BeTrue();
		}

		[Test]
		[Description("Verifies that OpenWrite throws when the drive doesn't exist")]
		public void TestOpenWrite1()
		{
			new Action(() => Filesystem.OpenWrite(@"P:\foo.txt"))
				.ShouldThrow<DirectoryNotFoundException>()
				.WithMessage(@"Could not find a part of the path 'P:\foo.txt'.");
		}

		[Test]
		[Description("Verifies that OpenWrite throws when a directory in the path doesn't exist")]
		public void TestOpenWrite2()
		{
			new Action(() => Filesystem.OpenWrite("foo\\bar.txt"))
				.ShouldThrow<DirectoryNotFoundException>();
		}

		[Test]
		[Description("Verifies that OpenRead throws when the drive doesn't exist")]
		public void TestOpenRead1()
		{
			new Action(() => Filesystem.OpenRead(@"P:\foo.txt"))
				.ShouldThrow<DirectoryNotFoundException>()
				.WithMessage(@"Could not find a part of the path 'P:\foo.txt'.");
		}

		[Test]
		[Description("Verifies that OpenRead throws when a directory in the path doesn't exist")]
		public void TestOpenRead2()
		{
			new Action(() => Filesystem.OpenWrite("foo\\bar.txt"))
				.ShouldThrow<DirectoryNotFoundException>();
		}
	}
}