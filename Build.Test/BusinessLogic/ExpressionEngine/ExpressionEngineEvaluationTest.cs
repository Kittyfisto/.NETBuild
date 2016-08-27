using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;
using Build.Parser;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.BusinessLogic.ExpressionEngine
{
	[TestFixture]
	public sealed class ExpressionEngineTest
	{
		private Build.ExpressionEngine.ExpressionEngine _engine;

		[SetUp]
		public void SetUp()
		{
			_engine = new Build.ExpressionEngine.ExpressionEngine();
		}

		[Test]
		public void TestEvaluate1()
		{
			var condition = new Condition(" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ");
			var environment = new BuildEnvironment
				{
					{"Configuration", "Release"},
					{"Platform", "AnyCPU"}
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
					{"Configuration", "Debug"},
					{"Platform", "AnyCPU"}
				};

			_engine.IsTrue(condition, environment).Should().BeFalse();
		}

		[Test]
		public void TestEvaluate3()
		{
			var condition = new Condition(" '$(Configuration)' == '' ");
			_engine.IsTrue(condition, new BuildEnvironment()).Should().BeTrue();
			_engine.IsTrue(condition, new BuildEnvironment{{"Configuration", "Release"}}).Should().BeFalse();
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
			environment[Properties.Configuration].Should().Be("Debug");
			environment[Properties.Platform].Should().Be("AnyCPU");
		}

		#region Item Evaluation

		[Test]
		[Description("Verifies that evaluating an item ")]
		public void TestEvaluateItem1()
		{
			var item = new ProjectItem("Compile", "Build.exe");
			var environment = new BuildEnvironment {{Properties.MSBuildProjectDirectory, Directory.GetCurrentDirectory()}};
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
		[Description("Verifies that evaluating a project adds resvered/well known properties in the environment")]
		public void TestEaluateProject1()
		{
			var fname = @"..\..\Build.Test.csproj";
			var project = CSharpProjectParser.Instance.Parse(fname);
			var envirnoment = new BuildEnvironment();
			_engine.Evaluate(project, envirnoment);

			envirnoment[Properties.MSBuildProjectDirectory].Should().Be(@"C:\Snapshots\NETBuild\Build.Test");
			envirnoment[Properties.MSBuildProjectDirectoryNoRoot].Should().Be(@"Snapshots\NETBuild\Build.Test");
			envirnoment[Properties.MSBuildProjectExtension].Should().Be(".csproj");
			envirnoment[Properties.MSBuildProjectFile].Should().Be("Build.Test.csproj");
			envirnoment[Properties.MSBuildProjectFullPath].Should().Be(@"C:\Snapshots\NETBuild\Build.Test\Build.Test.csproj");
			envirnoment[Properties.MSBuildProjectName].Should().Be("Build.Test");
		}

		#endregion
	}
}