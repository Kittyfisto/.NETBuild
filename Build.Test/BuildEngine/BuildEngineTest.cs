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
				var references = new[] {"System", "System.Core", "System.Xml.Linq"};
				foreach (var reference in references)
				{
					writer.WriteStartElement("Reference");
					writer.WriteAttributeString("Include", reference);
					writer.WriteEndElement();
				}
				writer.WriteEndElement(); //< ItemGroup


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
			"CS0616", "CS0650", "CS0686",
			"CS0702", "CS0703", /*CS0731,*/
			"CS0826", "CS0834", /*"CS0840",*/ "CS0843", /*"CS0845",*/
			"CS1001", "CS1009", "CS1018", "CS1019", "CS1026", "CS1029", "CS1061",
			"CS1112",
			"CS1501", "CS1503", "CS1513", "CS1514", "CS1519", "CS1540", /*CS1546, CS1548, CS1564, CS1567,*/ "CS1579",
			"CS1612", "CS1614", /*CS1644,*/ "CS1656", "CS1674",
			/*"CS1703", "CS1704", "CS1705"*/ "CS1708", "CS1716", "CS1721", /*"CS1726",*/ "CS1729",
			"CS1919", "CS1921", /*CS1926,*/ "CS1936",
			"CS7013")] string errorCode)
		{
			const string projectName = "TestProject.csproj";

			CreateProject(projectName, AddFile(string.Format("{0}.cs", errorCode)));

			var arguments = new Arguments();
			using (var engine = new Build.BuildEngine.BuildEngine(arguments, _filesystem))
			{
				engine.Execute();

				var message = string.Format("error {0}", errorCode);
				engine.Log.Errors.Should().Contain(x => x.Contains(message));

				const string intermediateOutputPath = @"obj\debug\TestProject";
				VerifyNoOutputFiles(intermediateOutputPath);

				const string outputPath = @"bin\debug\TestProject";
				VerifyNoOutputFiles(outputPath);
			}
		}

		private void VerifyNoOutputFiles(string outputPath)
		{
			var root = Path.GetRootDir(_filesystem.CurrentDirectory);
			var assembly = Path.Combine(root, outputPath + ".dll");
			var pdb = Path.Combine(root, outputPath + ".pdb");
			const string reason = "because the build shouldn't create output files when it failed";

			_filesystem.Exists(assembly).Should().BeFalse(reason);
			_filesystem.Exists(pdb).Should().BeFalse(reason);
		}
	}
}