using System.IO;
using Build.IO;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.IO
{
	[TestFixture]
	public sealed class InMemoryFileSystemTest
		: AbstractFilesystemTest
	{
		private InMemoryFileSystem _fileSystem;

		[SetUp]
		public void Setup()
		{
			_fileSystem = new InMemoryFileSystem();
		}

		[Test]
		public void TestCtor()
		{
			_fileSystem.CurrentDirectory.Should().Be("A:\\");
		}

		[Test]
		public new void TestCreateDirectory1()
		{
			_fileSystem.ExistsDirectory("foo").Should().BeFalse();
			_fileSystem.CreateDirectory("foo");
			_fileSystem.ExistsDirectory("foo").Should().BeTrue();
		}

		[Test]
		public void TestCurrentDirectory1()
		{
			_fileSystem.CreateDirectory("blub");
			_fileSystem.CurrentDirectory = "a:\\blub";
			_fileSystem.WriteAllText("bar.txt", "hello world!");
			_fileSystem.Exists("bar.txt").Should().BeTrue();
			_fileSystem.Exists("a:\\blub\\bar.txt").Should().BeTrue();
		}

		[Test]
		public void TestExists1()
		{
			_fileSystem.Exists("foo.txt").Should().BeFalse();
			_fileSystem.Exists("a:\\foo.txt").Should().BeFalse();
		}

		[Test]
		public void TestWriteAllText1()
		{
			_fileSystem.WriteAllText("blub.cs", "hello world!");
			_fileSystem.Exists("blub.cs").Should().BeTrue();
			_fileSystem.ReadAllText("blub.cs").Should().Be("hello world!");
		}

		[Test]
		public void TestWriteAllText2()
		{
			_fileSystem.WriteAllText("blub.cs", "hello world!");
			_fileSystem.WriteAllText("blub.cs", "bar");
			_fileSystem.ReadAllText("blub.cs").Should().Be("bar");
		}

		[Test]
		public void TestReadAllText1()
		{
			_fileSystem.WriteAllText("blub.cs", "hello world!");
			_fileSystem.ReadAllText("blub.cs").Should().Be("hello world!");
			_fileSystem.ReadAllText("blub.cs").Should().Be("hello world!");
		}

		[Test]
		public void TestReadAllText2()
		{
			_fileSystem.WriteAllText("a", "foo");
			_fileSystem.WriteAllText("b", "bar");
			_fileSystem.ReadAllText("a").Should().Be("foo");
			_fileSystem.ReadAllText("b").Should().Be("bar");
		}

		[Test]
		public void TestReadAllBytes1()
		{
			_fileSystem.WriteAllBytes("blub.cs", new byte[] {1, 2, 3, 4});
			_fileSystem.ReadAllBytes("blub.cs").Should().Equal(1, 2, 3, 4);
			_fileSystem.ReadAllBytes("blub.cs").Should().Equal(1, 2, 3, 4);
		}

		[Test]
		public void TestReadAllBytes2()
		{
			_fileSystem.WriteAllBytes("a", new byte[] {1});
			_fileSystem.WriteAllBytes("b", new byte[] {42});
			_fileSystem.ReadAllBytes("a").Should().Equal(1);
			_fileSystem.ReadAllBytes("b").Should().Equal(42);
		}

		[Test]
		public new void TestOpenWrite1()
		{
			using (var stream = _fileSystem.OpenWrite("a"))
			{
				stream.Write(new byte[] {1, 2, 3, 4}, 0, 4);
				stream.Write(new byte[] {5, 6, 7, 8}, 0, 4);
			}

			_fileSystem.ReadAllBytes("a").Should().Equal(1, 2, 3, 4, 5, 6, 7, 8);
		}

		[Test]
		public void TestEnumerateFiles1()
		{
			_fileSystem.EnumerateFiles("a").Should().BeEmpty();
			_fileSystem.EnumerateFiles("a", "*").Should().BeEmpty();
			_fileSystem.EnumerateFiles("a", "*", SearchOption.AllDirectories).Should().BeEmpty();
		}

		[Test]
		public void TestEnumerateFiles2()
		{
			_fileSystem.WriteAllText("a.txt", "foo");

			_fileSystem.EnumerateFiles(_fileSystem.CurrentDirectory).Should().Equal("A:\\a.txt");
			_fileSystem.EnumerateFiles(_fileSystem.CurrentDirectory, "*").Should().Equal("A:\\a.txt");
			_fileSystem.EnumerateFiles(_fileSystem.CurrentDirectory, "*.*").Should().Equal("A:\\a.txt");
		}

		[Test]
		public void TestEnumerateFiles3()
		{
			_fileSystem.WriteAllText("b", "foo");
			_fileSystem.WriteAllText("b.txt", "bar");

			_fileSystem.EnumerateFiles(_fileSystem.CurrentDirectory).Should().BeEquivalentTo("A:\\b", "A:\\b.txt");
			_fileSystem.EnumerateFiles(_fileSystem.CurrentDirectory, "*").Should().BeEquivalentTo("A:\\b", "A:\\b.txt");
			_fileSystem.EnumerateFiles(_fileSystem.CurrentDirectory, "*.*").Should().BeEquivalentTo("A:\\b.txt");
			_fileSystem.EnumerateFiles(_fileSystem.CurrentDirectory, "*.cs").Should().BeEmpty();
		}

		protected override IFileSystem Filesystem
		{
			get { return _fileSystem; }
		}
	}
}