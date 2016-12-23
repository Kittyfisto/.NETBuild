using System.IO;
using System.Reflection;
using System.Xml;
using Build.BuildEngine;
using Build.IO;
using NUnit.Framework;

namespace Build.Test.BuildEngine
{
	[TestFixture]
	public sealed class BuildEngineTest
	{
		private InMemoryFileSystem _filesystem;
		private BuildLog _log;

		[SetUp]
		public void Setup()
		{
			_filesystem = new InMemoryFileSystem();
		}

		[Test]
		public void TestErrorCS0015()
		{
			CreateProject(new[] {AddFile("CS0015.cs")});
			Build();
		}

		private void Build()
		{
			var arguments = new Arguments();
			using (var engine = new Build.BuildEngine.BuildEngine(arguments, _filesystem))
			{
				engine.Execute();

				_log = engine.Log;
			}
		}

		private string AddFile(string embeddedFileName)
		{
			var assembly = Assembly.GetCallingAssembly();
			var resourceName = string.Format("Build.Test.Resources.{0}", embeddedFileName);

			var test = assembly.GetManifestResourceNames();
			using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
			using (var stream = _filesystem.OpenWrite(embeddedFileName))
			{
				resourceStream.CopyTo(stream);
			}

			return embeddedFileName;
		}

		private void CreateProject(string[] files)
		{
			var testName = TestContext.CurrentContext.Test.Name;
			var projectName = string.Format("{0}.csproj", testName);

			CreateProject(projectName, files);
		}

		private void CreateProject(string projectFileName, params string[] compiles)
		{
			var settings = new XmlWriterSettings
			{
				NewLineHandling = NewLineHandling.Entitize,
				NewLineChars = "\r\n",
			};
			using (var stream = _filesystem.OpenWrite(projectFileName))
			using (StreamWriter streamWriter = new StreamWriter(stream))
			using (XmlTextWriter writer = new XmlTextWriter(streamWriter))
			{
				writer.Formatting = Formatting.Indented;
				writer.Indentation = 4;

				writer.WriteStartDocument();

				var projectName = Path.GetFilenameWithoutExtension(projectFileName);
				writer.WriteStartElement("Project");
				writer.WriteAttributeString("ToolsVersion", "4.0");
				writer.WriteAttributeString("DefaultTargets", "Build");

				writer.WriteStartElement("PropertyGroup");

				writer.WriteStartElement("Configuration");
				writer.WriteAttributeString("Condition", " '$(Configuration)' == '' ");
				writer.WriteString("Debug");
				writer.WriteEndElement(); //< Configuration

				writer.WriteStartElement("Platform");
				writer.WriteAttributeString("Condition", " '$(Platform)' == '' ");
				writer.WriteString("AnyCPU");
				writer.WriteEndElement(); //< Platform

				writer.WriteElementString("OutputType", "Library");
				writer.WriteElementString("AppDesignerFolder", "Properties");
				writer.WriteElementString("RootNamespace", projectName);
				writer.WriteElementString("AssemblyName", projectName);
				writer.WriteElementString("TargetFrameworkVersion", "v4.5");
				writer.WriteElementString("FileAlignment", "512");

				writer.WriteEndElement(); //< PropertyGroup


				writer.WriteStartElement("PropertyGroup");
				writer.WriteAttributeString("Condition", " '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ");
				writer.WriteElementString("DebugSymbols", "true");
				writer.WriteElementString("DebugType", "full");
				writer.WriteElementString("Optimize", "false");
				writer.WriteElementString("OutputPath", @"bin\Debug\");
				writer.WriteElementString("DefineConstants", "DEBUG;TRACE");
				writer.WriteElementString("ErrorReport", "prompt");
				writer.WriteElementString("WarningLevel", "4");
				writer.WriteEndElement(); //< PropertyGroup


				writer.WriteStartElement("PropertyGroup");
				writer.WriteAttributeString("Condition", " '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ");
				writer.WriteElementString("DebugType", "pdbonly");
				writer.WriteElementString("Optimize", "true");
				writer.WriteElementString("OutputPath", @"bin\Release\");
				writer.WriteElementString("DefineConstants", "TRACE");
				writer.WriteElementString("ErrorReport", "prompt");
				writer.WriteElementString("WarningLevel", "4");
				writer.WriteEndElement(); //< PropertyGroup


				writer.WriteStartElement("ItemGroup");
				foreach (var compile in compiles)
				{
					writer.WriteStartElement("Compile");
					writer.WriteAttributeString("Include", compile);
					writer.WriteEndElement();
				}
				writer.WriteEndElement(); //< ItemGroup

				writer.WriteStartElement("Import");
				writer.WriteAttributeString("Project", @"$(MSBuildToolsPath)\Microsoft.CSharp.targets");

				writer.WriteEndElement(); //< Project

				writer.WriteEndDocument();
			}
		}
	}
}