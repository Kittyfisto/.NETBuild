using System;
using System.IO;
using System.Reflection;
using Build.BuildEngine;
using Build.BuildEngine.Tasks.Compilers;
using Build.Parser;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.BuildEngine.Tasks.Compilers
{
	[TestFixture]
	public sealed class EmbeddedResourceTest
		: AbstractCompilerTest
	{
		[Test]
		public void TestBuild()
		{
			var filepath = TestPath.Get(@"TestData\CSharp\EmbeddedResource\EmbeddedResource.csproj");
			var environment = new BuildEnvironment();
			var project = ExpressionEngine.Evaluate(CSharpProjectParser.Instance.Parse(filepath), environment);

			Clean(@"TestData\CSharp\EmbeddedResource\bin\Debug\");

			Console.WriteLine();
			var compiler = new CSharpProjectCompiler(AssemblyResolver, Logger, project, environment);
			new Action(compiler.Run).ShouldNotThrow();

			var filename = TestPath.Get(Path.Combine(@"TestData\CSharp\EmbeddedResource\", compiler.OutputFilePath));
			var assembly = Assembly.LoadFile(filename);
			var resources = assembly.GetManifestResourceNames();
			resources.Should().BeEquivalentTo(new[]
				{
					"EmbeddedResource.HelloWorld.txt",
					"EmbeddedResource.SomeFolder.SomeDataFile.xml",
					"EmbeddedResource.SomeFolder.Some File With Spaces.xml"
				});

			ReadResource(assembly, "EmbeddedResource.HelloWorld.txt").Should().Be("Hello World!");
			ReadResource(assembly, "EmbeddedResource.SomeFolder.SomeDataFile.xml").Should().Be("<?xml version=\"1.0\" encoding=\"utf-8\" ?> ");
			ReadResource(assembly, "EmbeddedResource.SomeFolder.Some File With Spaces.xml").Should().Be("<?xml version=\"1.0\" encoding=\"utf-8\" ?> \r\n<importantdata value=\"42\" />");
		}

		private string ReadResource(Assembly assembly, string resourceName)
		{
			var stream = assembly.GetManifestResourceStream(resourceName);
			stream.Should().NotBeNull();

			var reader = new StreamReader(stream);
			return reader.ReadToEnd();
		}
	}
}