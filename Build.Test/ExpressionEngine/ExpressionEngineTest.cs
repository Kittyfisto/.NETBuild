using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Build.DomainModel.MSBuild;
using Build.IO;
using Build.Parser;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.ExpressionEngine
{
	[TestFixture]
	public sealed class ExpressionEngineTest
	{
		private Build.ExpressionEngine.ExpressionEngine _engine;

		[SetUp]
		public void SetUp()
		{
			_engine = new Build.ExpressionEngine.ExpressionEngine(new FileSystem());
		}

		[Test]
		public void TestEvaluate1()
		{
			var condition = " '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ";
			var environment = new BuildEnvironment
				{
					Properties =
						{
							{"Configuration", "Release"},
							{"Platform", "AnyCPU"}
						}
				};

			for(int i = 0; i < 100; ++i)
			{
				_engine.EvaluateCondition(condition, environment);
			}

			var sw = new Stopwatch();
			sw.Start();

			_engine.EvaluateCondition(condition, environment).Should().BeTrue();

			sw.Stop();
			Console.WriteLine("Parsing&Evalauation: {0}ms", sw.ElapsedMilliseconds);
		}

		[Test]
		public void TestEvaluate2()
		{
			const string condition = " '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ";
			var environment = new BuildEnvironment
				{
					Properties =
						{
							{"Configuration", "Debug"},
							{"Platform", "AnyCPU"}
						}
				};

			_engine.EvaluateCondition(condition, environment).Should().BeFalse();
		}

		[Test]
		public void TestEvaluate3()
		{
			var condition = " '$(Configuration)' == '' ";
			_engine.EvaluateCondition(condition, new BuildEnvironment()).Should().BeTrue();
			_engine.EvaluateCondition(condition, new BuildEnvironment
				{
					Properties =
						{
							{"Configuration", "Release"}
						}
				}).Should().BeFalse();
		}

		[Test]
		public void TestEvaluate4()
		{
			var propertyGroup = new PropertyGroup(new List<Property>
				{
					new Property(Properties.Configuration, "Debug", " '$(Configuration)' == '' "),
					new Property(Properties.Platform, "AnyCPU", " '$(Platform)' == '' ")
				});
			var environment = new BuildEnvironment();
			_engine.Evaluate(propertyGroup, environment);
			environment.Properties[Properties.Configuration].Should().Be("Debug");
			environment.Properties[Properties.Platform].Should().Be("AnyCPU");
		}

		[Test]
		public void TestEvaluate5()
		{
			var itemGroup = new ItemGroup
				{
					new ProjectItem {Include = "$(Foo)"}
				};
			var environment = new BuildEnvironment();
			_engine.Evaluate(itemGroup, environment);
			environment.Items.Should().BeEmpty("Because '$(Foo)' evaluates to an empty string and thus to zero items");
		}

		[Test]
		public void TestEvaluate6()
		{
			var itemGroup = new ItemGroup
				{
					new ProjectItem {Include = "$(Foo)", Type = "Content"}
				};
			var environment = new BuildEnvironment();
			environment.Properties[Properties.MSBuildProjectDirectory] = Directory.GetCurrentDirectory();
			environment.Properties["Foo"] = "a.txt";
			_engine.Evaluate(itemGroup, environment);
			environment.Items.Count().Should().Be(1);
			var item = environment.Items.First();
			item.Type.Should().Be("Content");
			item.Include.Should().Be("a.txt");
			item[Metadatas.Filename].Should().Be("a");
			item[Metadatas.Extension].Should().Be(".txt");
			item[Metadatas.Identity].Should().Be("$(Foo)");
		}

		[Test]
		public void TestEvaluateExpression()
		{
			_engine.EvaluateExpression(null, new BuildEnvironment()).Should().Be(string.Empty);
		}

		[Test]
		public void TestIsTrue1()
		{
			_engine.EvaluateCondition("true == true", new BuildEnvironment()).Should().BeTrue();
			_engine.EvaluateCondition("'true' == 'true'", new BuildEnvironment()).Should().BeTrue();

			_engine.EvaluateCondition("false == true", new BuildEnvironment()).Should().BeFalse();
			_engine.EvaluateCondition("'false' == 'true'", new BuildEnvironment()).Should().BeFalse();
		}

		[Test]
		public void TestIsTrue2()
		{
			var environment = new BuildEnvironment();
			environment.Properties["Foo"] = "true";
			_engine.EvaluateCondition("$(Foo) == true", environment).Should().BeTrue();
			_engine.EvaluateCondition("'$(Foo)' == 'true'", environment).Should().BeTrue();

			environment.Properties["Foo"] = "FOO";
			_engine.EvaluateCondition("$(Foo) == 'true", environment).Should().BeFalse();
			_engine.EvaluateCondition("'$(Foo)' == 'true'", environment).Should().BeFalse();
		}

		[Test]
		public void TestIsTrue3()
		{
			var environment = new BuildEnvironment();
			environment.Properties["Foo"] = "42";
			_engine.EvaluateCondition("$(Foo) == 42", environment).Should().BeTrue();
			_engine.EvaluateCondition("'$(Foo)' == '42'", environment).Should().BeTrue();

			environment.Properties["Foo"] = "9001";
			_engine.EvaluateCondition("$(Foo) == 42", environment).Should().BeFalse();
			_engine.EvaluateCondition("'$(Foo)' == '42'", environment).Should().BeFalse();
		}

		[Test]
		public void TestIsTrue4()
		{
			var environment = new BuildEnvironment();
			environment.Properties["Foo"] = "42";
			_engine.EvaluateCondition("$(Foo) == 42 OR true", environment).Should().BeTrue();

			environment.Properties["Foo"] = "9001";
			_engine.EvaluateCondition("$(Foo) == 42 OR true", environment).Should().BeTrue();

			environment.Properties["Foo"] = "9001";
			_engine.EvaluateCondition("$(Foo) == 42 OR false", environment).Should().BeFalse();
		}

		[Test]
		public void TestIsTrue5()
		{
			var environment = new BuildEnvironment();
			environment.Properties["OutputType"] = "Library";
			_engine.EvaluateCondition("$(OutputType) == 'Exe' OR $(OutputType) == 'Winexe'", environment).Should().BeFalse();

			environment.Properties["OutputType"] = "Exe";
			_engine.EvaluateCondition("$(OutputType) == 'Exe' OR $(OutputType) == 'Winexe'", environment).Should().BeTrue();

			environment.Properties["OutputType"] = "Winexe";
			_engine.EvaluateCondition("$(OutputType) == 'Exe' OR $(OutputType) == 'Winexe'", environment).Should().BeTrue();
		}

		[Test]
		public void TestEvaluateConcatenation1()
		{
			_engine.EvaluateConcatenation("$(Foo)", new BuildEnvironment
				{
					Properties =
						{
							{"Foo", "Bar"}
						}
				})
			       .Should().Be("Bar");
		}

		[Test]
		public void TestEvaluateConcatenation2()
		{
			_engine.EvaluateConcatenation("$(AssemblyName)$(Extension).config",
			                              new BuildEnvironment
				                              {
					                              Properties =
						                              {
							                              {"AssemblyName", "Foo"},
							                              {"Extension", ".exe"}
						                              }
				                              })
			       .Should().Be("Foo.exe.config");
		}

		[Test]
		[Description("Verifies that item lists are evaluated to the item's full path")]
		public void TestEvaluateConcatenation3()
		{
			var environment = new BuildEnvironment();
			var item = new ProjectItem {Type = "Content", Include = "foo.cs"};
			item[Metadatas.FullPath] = @"C:\snapshots\.NETBuild\foo.cs";
			environment.Items.Add(item);
			_engine.EvaluateConcatenation("@(Content)", environment)
			       .Should().Be(@"C:\snapshots\.NETBuild\foo.cs");
		}

		#region Property Evaluation

		[Test]
		public void TestEvaluateProperty1()
		{
			var environment = new BuildEnvironment();
			_engine.Evaluate(new Property
				{
					Name = "Foo",
					Value = "Bar"
				}, environment);
			environment.Properties["Foo"].Should().Be("Bar");
		}

		[Test]
		public void TestEvaluateProperty2()
		{
			var environment = new BuildEnvironment
				{
					Properties =
						{
							{"Extension", ".exe"}
						}
				};
			_engine.Evaluate(new Property
			{
				Name = "Foo",
				Value = "$(Extension)"
			}, environment);
			environment.Properties["Foo"].Should().Be(".exe");
		}

		[Test]
		public void TestEvaluateProperty3()
		{
			var environment = new BuildEnvironment
			{
				Properties =
						{
							{"Filename", "AwesomeApp"},
							{"Extension", ".exe"}
						}
			};
			_engine.Evaluate(new Property
			{
				Name = "Foo",
				Value = "$(Filename)$(Extension)"
			}, environment);
			environment.Properties["Foo"].Should().Be("AwesomeApp.exe");
		}

		#endregion

		#region Conditions

		[Test]
		public void TestEvaluateCondition1()
		{
			_engine.EvaluateCondition("", new BuildEnvironment()).Should().BeTrue(
				"Because MSBuild evaluates an empty condition to true");
		}

		[Test]
		public void TestEvaluateCondition2()
		{
			_engine.EvaluateCondition("true", new BuildEnvironment()).Should().BeTrue();
			_engine.EvaluateCondition("TRUE", new BuildEnvironment()).Should().BeTrue();
			_engine.EvaluateCondition("TrUe", new BuildEnvironment()).Should().BeTrue();
			_engine.EvaluateCondition("tRuE", new BuildEnvironment()).Should().BeTrue();

			_engine.EvaluateCondition("false", new BuildEnvironment()).Should().BeFalse();
			_engine.EvaluateCondition("FALSE", new BuildEnvironment()).Should().BeFalse();
			_engine.EvaluateCondition("FaLsE", new BuildEnvironment()).Should().BeFalse();
			_engine.EvaluateCondition("fAlSe", new BuildEnvironment()).Should().BeFalse();
		}

		[Test]
		public void TestEvaluateCondition3()
		{
			var environment = new BuildEnvironment {Properties = {{"Foo", "true"}}};
			_engine.EvaluateCondition("$(Foo)", environment).Should().BeTrue();
			environment.Properties["Foo"] = "false";
			_engine.EvaluateCondition("$(Foo)", environment).Should().BeFalse();
		}

		[Test]
		public void TestEvaluateCondition4()
		{
			new Action(() => _engine.EvaluateCondition("$(Foo)", new BuildEnvironment()))
				.ShouldThrow<EvaluationException>()
				.WithMessage("Specified condition \"$(Foo)\" evaluates to \"\" instead of a boolean.");
		}

		[Test]
		public void TestEvaluateCondition5()
		{
			_engine.EvaluateCondition("42 > 10", new BuildEnvironment()).Should().BeTrue();
			_engine.EvaluateCondition("42 > 42", new BuildEnvironment()).Should().BeFalse();
			_engine.EvaluateCondition("10 > 42", new BuildEnvironment()).Should().BeFalse();

			_engine.EvaluateCondition("42 >= 10", new BuildEnvironment()).Should().BeTrue();
			_engine.EvaluateCondition("42 >= 42", new BuildEnvironment()).Should().BeTrue();
			_engine.EvaluateCondition("10 >= 42", new BuildEnvironment()).Should().BeFalse();

			_engine.EvaluateCondition("42 < 10", new BuildEnvironment()).Should().BeFalse();
			_engine.EvaluateCondition("42 < 42", new BuildEnvironment()).Should().BeFalse();
			_engine.EvaluateCondition("10 < 42", new BuildEnvironment()).Should().BeTrue();

			_engine.EvaluateCondition("42 <= 10", new BuildEnvironment()).Should().BeFalse();
			_engine.EvaluateCondition("42 <= 42", new BuildEnvironment()).Should().BeTrue();
			_engine.EvaluateCondition("10 <= 42", new BuildEnvironment()).Should().BeTrue();
		}

		[Test]
		public void TestEvaluateCondition6()
		{
			new Action(() => _engine.EvaluateCondition("42 > true", new BuildEnvironment()))
				.ShouldThrow<EvaluationException>()
				.WithMessage("A numeric comparison was attempted on \"true\" that evaluates to \"true\" instead of a number.");
		}

		[Test]
		public void TestEvaluateCondition7()
		{
			new Action(() => _engine.EvaluateCondition("$(Foo) >= 3.14159", new BuildEnvironment()))
				.ShouldThrow<EvaluationException>()
				.WithMessage("A numeric comparison was attempted on \"$(Foo)\" that evaluates to \"\" instead of a number.");
		}

		[Test]
		public void TestEvaluateCondition8()
		{
			var environment = new BuildEnvironment();
			environment.Properties["Foo"] = "2.7";
			_engine.EvaluateCondition("$(Foo) >= 3.14159", environment).Should().BeFalse();
			environment.Properties["Foo"] = "3.2";
			_engine.EvaluateCondition("$(Foo) >= 3.14159", environment).Should().BeTrue();
		}

		#endregion

		#region ItemLists

		[Test]
		public void TestEvaluateItemList1()
		{
			var environment = new BuildEnvironment();
			environment.Properties[Properties.MSBuildProjectDirectory] = Directory.GetCurrentDirectory();
			var items = _engine.EvaluateItemList("data.xml", environment);
			items.Should().NotBeNull();
			items.Length.Should().Be(1);
			var item = items[0];
			item.Should().NotBeNull();
			item.Include.Should().Be("data.xml");
			item[Metadatas.FullPath].Should().Be(Path.Combine(Directory.GetCurrentDirectory(), "data.xml"));
			item.Type.Should().Be("None");
		}

		[Test]
		public void TestEvaluateItemList2()
		{
			var environment = new BuildEnvironment();
			environment.Properties[Properties.MSBuildProjectDirectory] = Directory.GetCurrentDirectory();
			var items = _engine.EvaluateItemList("data.xml;schema.xsd", environment);
			items.Length.Should().Be(2);
			items[0].Include.Should().Be("data.xml");
			items[1].Include.Should().Be("schema.xsd");
		}

		[Test]
		public void TestEvaluateItemList3()
		{
			var environment = new BuildEnvironment();
			environment.Properties[Properties.MSBuildProjectDirectory] = Directory.GetCurrentDirectory();
			environment.Properties["Filename"] = "data.xml";

			var items = _engine.EvaluateItemList("$(Filename)", environment);
			items.Length.Should().Be(1);
			items[0].Include.Should().Be("data.xml");
		}

		[Test]
		public void TestEvaluateItemList4()
		{
			var environment = new BuildEnvironment();
			environment.Properties[Properties.MSBuildProjectDirectory] = Directory.GetCurrentDirectory();
			environment.Properties["DataFilename"] = "data.xml";
			environment.Properties["SchemaFilename"] = "schema.xsd";

			var items = _engine.EvaluateItemList("$(DataFilename);$(SchemaFilename)", environment);
			items.Length.Should().Be(2);
			items[0].Include.Should().Be("data.xml");
			items[1].Include.Should().Be("schema.xsd");
		}

		[Test]
		public void TestEvaluateItemList5()
		{
			var environment = new BuildEnvironment();
			var item1 = new ProjectItem
			{
				Type = "Content",
				Include = "data.xml"
			};
			environment.Items.Add(item1);
			var item2 = new ProjectItem
			{
				Type = "Content",
				Include = "schema.xsd"
			};
			environment.Items.Add(item2);
			var item3 = new ProjectItem
			{
				Type = "Compile",
				Include = "Program.cs"
			};
			environment.Items.Add(item3);

			_engine.EvaluateItemList("@(Content)", environment)
				   .Should()
				   .Equal(new object[] { item1, item2 });
		}

		[Test]
		[Description("Verifies that a new ProjectItem is created and returned for every queried file, even if it doesn't exist!")]
		public void TestEvaluateItemList6()
		{
			var environment = new BuildEnvironment();
			environment.Properties[Properties.MSBuildProjectDirectory] = @"C:\work\doodledoo";
			var items = _engine.EvaluateItemList("someimportantfile.bin", environment);
			items.Should().NotBeNull();
			items.Length.Should().Be(1);
			var item = items[0];
			item.Should().NotBeNull();
			item.Include.Should().Be("someimportantfile.bin");
		}

		[Test]
		public void TestEvaluateItemList7()
		{
			var environment = new BuildEnvironment();
			environment.Properties[Properties.MSBuildProjectDirectory] = @"C:\work\doodledoo";
			environment.Properties[Properties.OutputPath] = @"bin\debug";
			var item = new ProjectItem { Include = @"C:\Code\Test\foo.dll", Type = "ResolvedReferencedProjects" };
			item[Metadatas.Filename] = "foo";
			item[Metadatas.Extension] = ".dll";
			environment.Items.Add(item);
			var items = _engine.EvaluateItemList(@"@(ResolvedReferencedProjects -> '$(OutputPath)\%(Filename)%(Extension)')",
			                                     environment);
			items.Should().NotBeNull();
			items.Length.Should().Be(1);
			var actualItem = items[0];
			actualItem.Include.Should().Be(@"bin\debug\foo.dll");
			actualItem.Type.Should().Be("ResolvedReferencedProjects");
			actualItem[Metadatas.Filename].Should().Be("foo");
			actualItem[Metadatas.Extension].Should().Be(".dll");
			actualItem[Metadatas.Directory].Should().Be(@"work\doodledoo\bin\debug\");
			actualItem[Metadatas.FullPath].Should().Be(@"C:\work\doodledoo\bin\debug\foo.dll");
			//actualItem[Metadatas.Identity].Should().Be(@"@(ResolvedReferencedProjects -> '$(OutputPath)\%(Filename)%(Extension)')");
		}

		[Test]
		public void TestEvaluateItemList8()
		{
			var environment = new BuildEnvironment();
			environment.Properties[Properties.MSBuildProjectDirectory] = @"C:\work\doodledoo";
			var items = _engine.EvaluateItemList("$(Foo);$(Bar)", environment);
			items.Should().BeEmpty();
		}

		[Test]
		public void TestEvaluateItemList9()
		{
			var environment = new BuildEnvironment();
			environment.Properties[Properties.MSBuildProjectDirectory] = @"C:\work\doodledoo";
			environment.Properties["Foo"] = ";;;";
			var items = _engine.EvaluateItemList("$(Foo)", environment);
			items.Should().BeEmpty();
		}

		[Test]
		public void TestEvaluateItemList10()
		{
			var environment = new BuildEnvironment();
			environment.Properties[Properties.MSBuildProjectDirectory] = @"C:\work\doodledoo";
			environment.Properties["Foo"] = "  ;	;\r;	\r\n ";
			var items = _engine.EvaluateItemList("$(Foo)", environment);
			items.Should().BeEmpty();
		}

		#endregion

		#region Item Evaluation

		[Test]
		[Description("Verifies that evaluating an item ")]
		public void TestEvaluateItem1()
		{
			var item = new ProjectItem("Compile", "Build.exe");
			var environment = new BuildEnvironment
				{
					Properties =
						{
							{Properties.MSBuildProjectDirectory, Directory.GetCurrentDirectory()}
						}
				};
			_engine.Evaluate(item, environment);
			environment.Items.Count().Should().Be(1);
			var evaluated = environment.Items.First();
			evaluated.Type.Should().Be("Compile");
			evaluated.Include.Should().Be("Build.exe");
			evaluated.Metadata.Count().Should().Be(10);
			evaluated[Metadatas.Extension].Should().Be(".exe");
			evaluated[Metadatas.Filename].Should().Be("Build");
			evaluated[Metadatas.Identity].Should().Be("Build.exe");
			evaluated[Metadatas.RootDir].Should().Be(System.IO.Path.GetPathRoot(Directory.GetCurrentDirectory()));
			evaluated[Metadatas.RelativeDir].Should().Be(string.Empty);
			evaluated[Metadatas.RecursiveDir].Should().Be(string.Empty);
			evaluated[Metadatas.FullPath].Should().Be(Path.Combine(Directory.GetCurrentDirectory(), "Build.exe"));
		}

		#endregion

		#region Project Evaluation

		[Test]
		public void TestEvaluateProject1()
		{
			var project = new Project
				{
					Filename = @"C:\snapshots\foo.csproj",
					Properties =
						{
							new PropertyGroup
								{
									new Property("Foo", "Bar")
								}
						}
				};
			var environment = new BuildEnvironment();
			_engine.Evaluate(project, environment);
			environment.Properties["Foo"].Should().Be("Bar");
		}

		[Test]
		public void TestEvaluateProject2()
		{
			var item = new ItemGroup
				{
					new ProjectItem
						{
							Type = "None",
							Include = "foo.txt"
						}
				};
			var project = new Project
			{
				Filename = @"C:\work\interestingproject\foo.csproj",
				ItemGroups = { item }
			};
			var environment = new BuildEnvironment();
			_engine.Evaluate(project, environment);
			environment.Items.Count().Should().Be(1);
			var actualItem = environment.Items.First();
			actualItem.Should().NotBeNull();
			actualItem.Include.Should().Be("foo.txt");
			actualItem[Metadatas.FullPath].Should().Be(@"C:\work\interestingproject\foo.txt");
			actualItem[Metadatas.Directory].Should().Be(@"work\interestingproject\");
			actualItem[Metadatas.Extension].Should().Be(".txt");
			actualItem[Metadatas.Filename].Should().Be("foo");
			actualItem[Metadatas.Identity].Should().Be("foo.txt");
			actualItem[Metadatas.RelativeDir].Should().Be("");
			actualItem[Metadatas.RootDir].Should().Be(@"C:\");
		}

		[Test]
		[Description("Verifies that evaluating a project adds resvered/well known properties in the environment")]
		public void TestEvaluateProject3()
		{
			var fname = TestPath.Get(@"Build.Test\Build.Test.csproj");
			var project = new ProjectParser(new FileSystem()).Parse(fname);
			var envirnoment = new BuildEnvironment();
			_engine.Evaluate(project, envirnoment);

		    var fullPath = Path.Normalize(Path.Combine(Directory.GetCurrentDirectory(), fname));
		    var directory = Path.GetDirectory(fullPath);

			envirnoment.Properties[Properties.MSBuildProjectDirectory].Should().Be(directory);
			envirnoment.Properties[Properties.MSBuildProjectDirectoryNoRoot].Should().Be(directory.Remove(0, 3));
			envirnoment.Properties[Properties.MSBuildProjectExtension].Should().Be(".csproj");
			envirnoment.Properties[Properties.MSBuildProjectFile].Should().Be("Build.Test.csproj");
			envirnoment.Properties[Properties.MSBuildProjectFullPath].Should().Be(fullPath);
			envirnoment.Properties[Properties.MSBuildProjectName].Should().Be("Build.Test");
		}

		#endregion
	}
}