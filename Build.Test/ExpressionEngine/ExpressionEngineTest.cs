﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;
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
			var condition = new Condition(" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ");
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
				_engine.IsTrue(condition, environment);
			}

			var sw = new Stopwatch();
			sw.Start();

			_engine.IsTrue(condition, environment).Should().BeTrue();

			sw.Stop();
			Console.WriteLine("Parsing&Evalauation: {0}ms", sw.ElapsedMilliseconds);
		}

		[Test]
		public void TestEvaluate2()
		{
			var condition = new Condition(" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ");
			var environment = new BuildEnvironment
				{
					Properties =
						{
							{"Configuration", "Debug"},
							{"Platform", "AnyCPU"}
						}
				};

			_engine.IsTrue(condition, environment).Should().BeFalse();
		}

		[Test]
		public void TestEvaluate3()
		{
			var condition = new Condition(" '$(Configuration)' == '' ");
			_engine.IsTrue(condition, new BuildEnvironment()).Should().BeTrue();
			_engine.IsTrue(condition, new BuildEnvironment
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
					new Property(Properties.Configuration, "Debug", new Condition(" '$(Configuration)' == '' ")),
					new Property(Properties.Platform, "AnyCPU", new Condition(" '$(Platform)' == '' "))
				});
			var environment = new BuildEnvironment();
			_engine.Evaluate(propertyGroup, environment);
			environment.Properties[Properties.Configuration].Should().Be("Debug");
			environment.Properties[Properties.Platform].Should().Be("AnyCPU");
		}

		[Test]
		public void TestIsTrue1()
		{
			_engine.IsTrue(new Condition("true == true"), new BuildEnvironment()).Should().BeTrue();
			_engine.IsTrue(new Condition("'true' == 'true'"), new BuildEnvironment()).Should().BeTrue();

			_engine.IsTrue(new Condition("false == true"), new BuildEnvironment()).Should().BeFalse();
			_engine.IsTrue(new Condition("'false' == 'true'"), new BuildEnvironment()).Should().BeFalse();
		}

		[Test]
		public void TestIsTrue2()
		{
			var environment = new BuildEnvironment();
			environment.Properties["Foo"] = "true";
			_engine.IsTrue(new Condition("$(Foo) == true"), environment).Should().BeTrue();
			_engine.IsTrue(new Condition("'$(Foo)' == 'true'"), environment).Should().BeTrue();

			environment.Properties["Foo"] = "FOO";
			_engine.IsTrue(new Condition("$(Foo) == 'true"), environment).Should().BeFalse();
			_engine.IsTrue(new Condition("'$(Foo)' == 'true'"), environment).Should().BeFalse();
		}

		[Test]
		public void TestIsTrue3()
		{
			var environment = new BuildEnvironment();
			environment.Properties["Foo"] = "42";
			_engine.IsTrue(new Condition("$(Foo) == 42"), environment).Should().BeTrue();
			_engine.IsTrue(new Condition("'$(Foo)' == '42'"), environment).Should().BeTrue();

			environment.Properties["Foo"] = "9001";
			_engine.IsTrue(new Condition("$(Foo) == 42"), environment).Should().BeFalse();
			_engine.IsTrue(new Condition("'$(Foo)' == '42'"), environment).Should().BeFalse();
		}

		[Test]
		public void TestIsTrue4()
		{
			var environment = new BuildEnvironment();
			environment.Properties["Foo"] = "42";
			_engine.IsTrue(new Condition("$(Foo) == 42 OR true"), environment).Should().BeTrue();

			environment.Properties["Foo"] = "9001";
			_engine.IsTrue(new Condition("$(Foo) == 42 OR true"), environment).Should().BeTrue();

			environment.Properties["Foo"] = "9001";
			_engine.IsTrue(new Condition("$(Foo) == 42 OR false"), environment).Should().BeFalse();
		}

		[Test]
		public void TestIsTrue5()
		{
			var environment = new BuildEnvironment();
			environment.Properties["OutputType"] = "Library";
			_engine.IsTrue(new Condition("$(OutputType) == 'Exe' OR $(OutputType) == 'Winexe'"), environment).Should().BeFalse();

			environment.Properties["OutputType"] = "Exe";
			_engine.IsTrue(new Condition("$(OutputType) == 'Exe' OR $(OutputType) == 'Winexe'"), environment).Should().BeTrue();

			environment.Properties["OutputType"] = "Winexe";
			_engine.IsTrue(new Condition("$(OutputType) == 'Exe' OR $(OutputType) == 'Winexe'"), environment).Should().BeTrue();
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

		#region ItemLists

		[Test]
		public void TestEvaluateItemList1()
		{
			var environment = new BuildEnvironment();
			var item = new ProjectItem
				{
					Type = "Content",
					Include = "data.xml"
				};
			environment.Items.Add(item);

			_engine.EvaluateItemList("data.xml", environment)
			       .Should()
			       .Equal(new object[] { item });
		}

		[Test]
		public void TestEvaluateItemList2()
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

			_engine.EvaluateItemList("data.xml;schema.xsd", environment)
				   .Should()
				   .Equal(new object[] { item1, item2 });
		}

		[Test]
		public void TestEvaluateItemList3()
		{
			var environment = new BuildEnvironment();
			var item = new ProjectItem
			{
				Type = "Content",
				Include = "data.xml"
			};
			environment.Items.Add(item);
			environment.Properties["Filename"] = "data.xml";

			_engine.EvaluateItemList("$(Filename)", environment)
				   .Should()
				   .Equal(new object[] { item });
		}

		[Test]
		public void TestEvaluateItemList4()
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
			environment.Properties["DataFilename"] = "data.xml";
			environment.Properties["SchemaFilename"] = "schema.xsd";

			_engine.EvaluateItemList("$(DataFilename);$(SchemaFilename)", environment)
				   .Should()
				   .Equal(new object[] { item1, item2 });
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
			var items = _engine.Evaluate(item, environment);
			items.Should().NotBeNull();
			items.Count().Should().Be(1);
			var evaluated = items.First();
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
		public void TestEaluateProject3()
		{
			const string fname = @"..\..\Build.Test.csproj";
			var project = CSharpProjectParser.Instance.Parse(fname);
			var envirnoment = new BuildEnvironment();
			_engine.Evaluate(project, envirnoment);

			envirnoment.Properties[Properties.MSBuildProjectDirectory].Should().Be(@"C:\Snapshots\.NETBuild\Build.Test");
			envirnoment.Properties[Properties.MSBuildProjectDirectoryNoRoot].Should().Be(@"Snapshots\.NETBuild\Build.Test");
			envirnoment.Properties[Properties.MSBuildProjectExtension].Should().Be(".csproj");
			envirnoment.Properties[Properties.MSBuildProjectFile].Should().Be("Build.Test.csproj");
			envirnoment.Properties[Properties.MSBuildProjectFullPath].Should().Be(@"C:\Snapshots\.NETBuild\Build.Test\Build.Test.csproj");
			envirnoment.Properties[Properties.MSBuildProjectName].Should().Be("Build.Test");
		}

		#endregion
	}
}