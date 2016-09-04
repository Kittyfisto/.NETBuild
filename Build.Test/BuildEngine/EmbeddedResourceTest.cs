﻿using System.IO;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.BuildEngine
{
	[TestFixture]
	public sealed class EmbeddedResourceTest
		: AbstractBuildEngineTest
	{
		protected override string[] ProjectDirectories
		{
			get { return new[]{"EmbeddedResource"}; }
		}

		protected override string[] ExpectedOutputFiles
		{
			get
			{
				return new[]
					{
						@"EmbeddedResource\bin\Debug\EmbeddedResource.dll",
						@"EmbeddedResource\bin\Debug\EmbeddedResource.pdb",
					};
			}
		}

		protected override string ProjectFilePath
		{
			get { return @"EmbeddedResource\EmbeddedResource.csproj"; }
		}

		protected override void PostBuildChecks()
		{
			var source = TestPath.Get(@"TestData\CSharp\EmbeddedResource\bin\Debug\EmbeddedResource.dll");
			var dest = Path.Combine(Path.GetDirectory(source), "tmp.dll");
			File.Copy(source, dest, true);

			var assembly = Assembly.LoadFile(dest);
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
	}
}
