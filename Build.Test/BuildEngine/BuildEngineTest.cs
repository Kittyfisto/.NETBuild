using System.IO;
using System.Reflection;
using System.Xml;
using Build.IO;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.BuildEngine
{
	[TestFixture]
	public sealed class BuildEngineTest
	{
		[SetUp]
		public void Setup()
		{
			_filesystem = new InMemoryFileSystem();
		}

		private InMemoryFileSystem _filesystem;
		
		private string AddFile(string embeddedFileName)
		{
			var assembly = Assembly.GetCallingAssembly();
			var resourceName = string.Format("Build.Test.Resources.{0}", embeddedFileName);
			
			using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
			using (var stream = _filesystem.OpenWrite(embeddedFileName))
			{
				resourceStream.CopyTo(stream);
			}

			return embeddedFileName;
		}

		private void CreateProject(string[] files)
		{
			var projectName = "Project.csproj";

			CreateProject(projectName, files);
		}

		private void CreateProject(string projectFileName, params string[] compiles)
		{
			using (var stream = _filesystem.OpenWrite(projectFileName))
			using (var streamWriter = new StreamWriter(stream))
			using (var writer = new XmlTextWriter(streamWriter))
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

		[Test]
		public void TestErrors(
			[Values(
			"CS0019", "CS0029", "CS0034", "CS0039", "CS0050", "CS0051", "CS0052", "CS0071",
			"CS0103", "CS0106", "CS0115", "CS0116", "CS0120", "CS0122", "CS0133", "CS0151", "CS0163", "CS0165", "CS0173", "CS0178", "CS0188",
			"CS0201", "CS0229", "CS0233", "CS0234", "CS0246", "CS0260", "CS0266", "CS0269", "CS0270",
			"CS0304", "CS0310", "CS0311",
			"CS0413", "CS0417", /*"CS0433",*/ "CS0445", "CS0446",
			"CS0504", "CS0507", /*"CS0518",*/ "CS0523", "CS0545", "CS0552", "CS0563", /*CS0570,*/ "CS0571", "CS0579", "CS0592",
			"CS7013")] string errorCode)
		{
			CreateProject(new[] {AddFile(string.Format("{0}.cs", errorCode))});

			var arguments = new Arguments();
			using (var engine = new Build.BuildEngine.BuildEngine(arguments, _filesystem))
			{
				engine.Execute();

				var message = string.Format("error {0}", errorCode);
				engine.Log.Errors.Should().Contain(x => x.Contains(message));
			}

		}
	}
}