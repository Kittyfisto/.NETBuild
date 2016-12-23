using Build.BuildEngine;
using Build.ExpressionEngine;
using Build.IO;
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
			new UnaryExpression(UnaryOperation.Not, new StringLiteral("False")).Evaluate(_fs, env).Should().Be(true);
		}

		[Test]
		public void TestEvaluate2()
		{
			var env = new BuildEnvironment();
			new UnaryExpression(UnaryOperation.Not, new BinaryExpression(
				                                        new StringLiteral("foo"), BinaryOperation.Equals, new StringLiteral("foo")))
				.Evaluate(_fs, env).Should().Be(false, "because !(foo == foo) is false");
			new UnaryExpression(UnaryOperation.Not, new BinaryExpression(
														new StringLiteral("foo"), BinaryOperation.Equals, new StringLiteral("bar")))
				.Evaluate(_fs, env).Should().Be(true, "because !(foo == bar) is true");
		}
	}
}