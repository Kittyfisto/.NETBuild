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
			new BinaryExpression(new StringLiteral("Foo"),
			                     BinaryOperation.Equals,
			                     new StringLiteral("Foo"))
				.Evaluate(_fs, null).Should().Be(true);

			new BinaryExpression(new StringLiteral("Foo"),
								 BinaryOperation.Equals,
								 new StringLiteral("Bar"))
				.Evaluate(_fs, null).Should().Be(false);
		}

		[Test]
		public void TestEvaluate2()
		{
			new BinaryExpression(new PropertyReference("Foo"),
			                     BinaryOperation.Equals,
			                     new StringLiteral("Hello World"))
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
			new BinaryExpression(new StringLiteral("true"), BinaryOperation.And, new StringLiteral("true"))
				.Evaluate(_fs, new BuildEnvironment()).Should().Be(true);
			new BinaryExpression(new StringLiteral("true"), BinaryOperation.And, new StringLiteral("false"))
				.Evaluate(_fs, new BuildEnvironment()).Should().Be(false);
			new BinaryExpression(new StringLiteral("false"), BinaryOperation.And, new StringLiteral("true"))
				.Evaluate(_fs, new BuildEnvironment()).Should().Be(false);
			new BinaryExpression(new StringLiteral("false"), BinaryOperation.And, new StringLiteral("false"))
				.Evaluate(_fs, new BuildEnvironment()).Should().Be(false);
		}

		[Test]
		public void TestEvaluate4()
		{
			new BinaryExpression(new StringLiteral("true"), BinaryOperation.Or, new StringLiteral("true"))
				.Evaluate(_fs, new BuildEnvironment()).Should().Be(true);
			new BinaryExpression(new StringLiteral("true"), BinaryOperation.Or, new StringLiteral("false"))
				.Evaluate(_fs, new BuildEnvironment()).Should().Be(true);
			new BinaryExpression(new StringLiteral("false"), BinaryOperation.Or, new StringLiteral("true"))
				.Evaluate(_fs, new BuildEnvironment()).Should().Be(true);
			new BinaryExpression(new StringLiteral("false"), BinaryOperation.Or, new StringLiteral("false"))
				.Evaluate(_fs, new BuildEnvironment()).Should().Be(false);
		}
	}
}