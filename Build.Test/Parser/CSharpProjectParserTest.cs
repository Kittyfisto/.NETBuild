using System;
using System.Collections.Generic;
using Build.DomainModel.MSBuild;
using Build.ExpressionEngine;
using Build.Parser;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.Parser
{
	[TestFixture]
	public sealed class CSharpProjectParserTest
	{
		private CSharpProjectParser _parser;

		[SetUp]
		public void SetUp()
		{
			_parser = CSharpProjectParser.Instance;
		}

		[Test]
		public void TestParse1()
		{
			var path = TestPath.Get(@"Parser/Build.Test.csproj");
			var project = _parser.Parse(path);
			project.Should().NotBeNull();
			project.Filename.Should().Be(path);
			project.Properties.Should().NotBeNull();
			project.Properties.Count.Should().Be(3);

			var group = project.Properties[0];
			group.Should().NotBeNull();
			group.Condition.Should().BeNull();
			group.Count.Should().Be(9);
			group[0].Should().Be(new Property(Properties.Configuration, "Debug", new Condition(" '$(Configuration)' == '' ")));
			group[1].Should().Be(new Property(Properties.Platform, "AnyCPU", new Condition(" '$(Platform)' == '' ")));
			group[2].Should().Be(new Property(Properties.ProjectGuid, "{34B3464E-CBE5-4410-9052-3AB2E720BACF}"));
			group[3].Should().Be(new Property(Properties.OutputType, "Library"));
			group[4].Should().Be(new Property(Properties.AppDesignerFolder, "Properties"));
			group[5].Should().Be(new Property(Properties.RootNamespace, "Build.Test"));
			group[6].Should().Be(new Property(Properties.AssemblyName, "Build.Test"));
			group[7].Should().Be(new Property(Properties.TargetFrameworkVersion, "v4.5"));
			group[8].Should().Be(new Property(Properties.FileAlignment, "512"));

			group = project.Properties[1];
			group.Should().NotBeNull();
			group.Condition.Should().Be(new Condition(" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "));
			group.Count.Should().Be(7);
			group[0].Should().Be(new Property(Properties.DebugSymbols, "true"));
			group[1].Should().Be(new Property(Properties.DebugType, "full"));
			group[2].Should().Be(new Property(Properties.Optimize, "false"));
			group[3].Should().Be(new Property(Properties.OutputPath, @"bin\Debug\"));
			group[4].Should().Be(new Property(Properties.DefineConstants, "DEBUG;TRACE"));
			group[5].Should().Be(new Property(Properties.ErrorReport, "prompt"));
			group[6].Should().Be(new Property(Properties.WarningLevel, "4"));

			group = project.Properties[2];
			group.Should().NotBeNull();
			group.Condition.Should().Be(new Condition(" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' "));
			group.Count.Should().Be(6);
			group[0].Should().Be(new Property(Properties.DebugType, "pdbonly"));
			group[1].Should().Be(new Property(Properties.Optimize, "true"));
			group[2].Should().Be(new Property(Properties.OutputPath, @"bin\Release\"));
			group[3].Should().Be(new Property(Properties.DefineConstants, "TRACE"));
			group[4].Should().Be(new Property(Properties.ErrorReport, "prompt"));
			group[5].Should().Be(new Property(Properties.WarningLevel, "4"));

			var itemGroups = project.ItemGroups;
			itemGroups.Should().NotBeNull();
			itemGroups.Count.Should().Be(4);

			var items = itemGroups[0];
			items.Should().NotBeNull();
			items.Condition.Should().BeNull();
			items.Count.Should().Be(10);
			items[0].Should()
			    .Be(new ProjectItem(Items.Reference, "FluentAssertions",
			                        metadata:
				                        new List<Metadata>
					                        {
						                        new Metadata(Metadatas.HintPath,
						                                     @"..\packages\FluentAssertions.4.0.0\lib\net45\FluentAssertions.dll")
					                        }));
			items[1].Should().Be(new ProjectItem(Items.Reference, "FluentAssertions.Core",
				metadata:new List<Metadata>
					{
						new Metadata(Metadatas.HintPath, @"..\packages\FluentAssertions.4.0.0\lib\net45\FluentAssertions.Core.dll")
					}));
			items[2].Should().Be(new ProjectItem(Items.Reference, "nunit.framework",
				 metadata: new List<Metadata>
					 {
						 new Metadata(Metadatas.HintPath, @"..\packages\NUnit.2.6.4\lib\nunit.framework.dll")
					 }));
			items[3].Should().Be(new ProjectItem(Items.Reference, "System"));
			items[4].Should().Be(new ProjectItem(Items.Reference, "System.Core"));
			items[5].Should().Be(new ProjectItem(Items.Reference, "System.Xml.Linq"));
			items[6].Should().Be(new ProjectItem(Items.Reference, "System.Data.DataSetExtensions"));
			items[7].Should().Be(new ProjectItem(Items.Reference, "Microsoft.CSharp"));
			items[8].Should().Be(new ProjectItem(Items.Reference, "System.Data"));
			items[9].Should().Be(new ProjectItem(Items.Reference, "System.Xml"));

			items = itemGroups[1];
			items.Should().NotBeNull();
			items.Condition.Should().BeNull();
			items.Count.Should().Be(8);
			items[0].Should().Be(new ProjectItem(Items.Compile, @"BusinessLogic\ExpressionEngine\BinaryExpressionTest.cs"));
			items[1].Should().Be(new ProjectItem(Items.Compile, @"BusinessLogic\ExpressionEngine\ExpressionEngineParsingTest.cs"));
			items[2].Should().Be(new ProjectItem(Items.Compile, @"BusinessLogic\ExpressionEngine\ExpressionEngineEvaluationTest.cs"));
			items[3].Should().Be(new ProjectItem(Items.Compile, @"BusinessLogic\ExpressionEngine\TokenizerTest.cs"));
			items[4].Should().Be(new ProjectItem(Items.Compile, @"BusinessLogic\ExpressionEngine\UnaryExpressionTest.cs"));
			items[5].Should().Be(new ProjectItem(Items.Compile, @"BusinessLogic\PathTest.cs"));
			items[6].Should().Be(new ProjectItem(Items.Compile, @"Class1.cs"));
			items[7].Should().Be(new ProjectItem(Items.Compile, @"Properties\AssemblyInfo.cs"));

			items = itemGroups[2];
			items.Should().NotBeNull();
			items.Condition.Should().BeNull();
			items.Count.Should().Be(2);
			items[0].Should().Be(new ProjectItem(Items.Folder, @"BusinessLogic\BuildEngine\"));
			items[1].Should().Be(new ProjectItem(Items.Folder, @"BusinessLogic\Parser\"));

			items = itemGroups[3];
			items.Should().NotBeNull();
			items.Condition.Should().BeNull();
			items.Count.Should().Be(1);
			items[0].Should().Be(new ProjectItem(Items.ProjectReference, @"..\Build\Build.csproj",
			                                     metadata: new List<Metadata>
				                                     {
					                                     new Metadata("Project", "{63F3A0B5-909C-43C5-8B79-CEA032DFE4F1}"),
					                                     new Metadata("Name", "Build")
				                                     }));
		}

		[Test]
		public void TestParse2()
		{
			var path = TestPath.Get(@"Parser/MissingTargetsAttribute.csproj");
			new Action(() => _parser.Parse(path))
				.ShouldThrow<ParseException>()
				.WithMessage("error  : The required attribute \"Name\" is empty or missing from the element <Target>.");
		}
	}
}