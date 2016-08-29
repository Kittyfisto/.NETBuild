using Build.BuildEngine;
using Build.ExpressionEngine;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Build.Test.ExpressionEngine
{
	[TestFixture]
	public sealed class UnaryExpressionTest
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
			var env = new BuildEnvironment();
			new UnaryExpression(UnaryOperation.Not, new Literal("False")).Evaluate(_fs, env).Should().Be(true);
			new UnaryExpression(UnaryOperation.Not, new Literal(null)).Evaluate(_fs, env).Should().Be(true);
			new UnaryExpression(UnaryOperation.Not, new Literal("SomeValue")).Evaluate(_fs, env).Should().Be(false);
		}

		[Test]
		public void TestEvaluate2()
		{
			var env = new BuildEnvironment();
			new UnaryExpression(UnaryOperation.Not, new BinaryExpression(
				                                        new Literal("foo"), BinaryOperation.Equals, new Literal("foo")))
				.Evaluate(_fs, env).Should().Be(false, "because !(foo == foo) is false");
			new UnaryExpression(UnaryOperation.Not, new BinaryExpression(
														new Literal("foo"), BinaryOperation.Equals, new Literal("bar")))
				.Evaluate(_fs, env).Should().Be(true, "because !(foo == bar) is true");
		}
	}
}