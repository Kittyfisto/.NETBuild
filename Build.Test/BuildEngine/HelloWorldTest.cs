using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.BuildEngine
{
	[TestFixture]
	public sealed class HelloWorldTest
		: AbstractBuildEngineTest
	{
		protected override string[] ProjectDirectories
		{
			get { return new[] {"HelloWorld"}; }
		}

		protected override string[] ExpectedOutputFiles
		{
			get
			{
				return new[]
					{
						@"HelloWorld\bin\Debug\HelloWorld.exe",
						@"HelloWorld\bin\Debug\HelloWorld.pdb",
						@"HelloWorld\bin\Debug\HelloWorld.exe.config"
					};
			}
		}

		protected override string ProjectFilePath
		{
			get { return @"HelloWorld\HelloWorld.csproj"; }
		}

		protected override void PostBuildChecks()
		{
			string output;
			int exitCode = Run(@"TestData\CSharp\HelloWorld\bin\Debug\HelloWorld.exe", out output);
			exitCode.Should().Be(0);
			output.Should().Be("Hello World!");
		}
	}
}