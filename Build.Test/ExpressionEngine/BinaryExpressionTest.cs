using Build.BuildEngine;
using Build.ExpressionEngine;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Build.Test.ExpressionEngine
{
	[TestFixture]
	public sealed class BinaryExpressionTest
	{
		private IFileSystem _fs;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			_fs = new Mock<IFileSystem>().Object;
		}

		[Test]
		public void TestEvaluate1()
		{
			new BinaryExpression(new Literal("Foo"),
			                     BinaryOperation.Equals,
			                     new Literal("Foo"))
				.Evaluate(_fs, null).Should().Be(true);

			new BinaryExpression(new Literal("Foo"),
								 BinaryOperation.Equals,
								 new Literal("Bar"))
				.Evaluate(_fs, null).Should().Be(false);
		}

		[Test]
		public void TestEvaluate2()
		{
			new BinaryExpression(new Variable("Foo"),
			                     BinaryOperation.Equals,
			                     new Literal("Hello World"))
				.Evaluate(_fs, new BuildEnvironment
					{
						Properties =
							{
								{"Foo", "Hello World"}
							}
					}).Should().Be(true);
		}

		[Test]
		public void TestEvaluate3()
		{
			new BinaryExpression(new Literal("true"), BinaryOperation.And, new Literal("true"))
				.Evaluate(_fs, new BuildEnvironment()).Should().Be(true);
			new BinaryExpression(new Literal("true"), BinaryOperation.And, new Literal("false"))
				.Evaluate(_fs, new BuildEnvironment()).Should().Be(false);
			new BinaryExpression(new Literal("false"), BinaryOperation.And, new Literal("true"))
				.Evaluate(_fs, new BuildEnvironment()).Should().Be(false);
			new BinaryExpression(new Literal("false"), BinaryOperation.And, new Literal("false"))
				.Evaluate(_fs, new BuildEnvironment()).Should().Be(false);
		}

		[Test]
		public void TestEvaluate4()
		{
			new BinaryExpression(new Literal("true"), BinaryOperation.Or, new Literal("true"))
				.Evaluate(_fs, new BuildEnvironment()).Should().Be(true);
			new BinaryExpression(new Literal("true"), BinaryOperation.Or, new Literal("false"))
				.Evaluate(_fs, new BuildEnvironment()).Should().Be(true);
			new BinaryExpression(new Literal("false"), BinaryOperation.Or, new Literal("true"))
				.Evaluate(_fs, new BuildEnvironment()).Should().Be(true);
			new BinaryExpression(new Literal("false"), BinaryOperation.Or, new Literal("false"))
				.Evaluate(_fs, new BuildEnvironment()).Should().Be(false);
		}
	}
}