using Build.BuildEngine;
using Build.ExpressionEngine;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.BusinessLogic.ExpressionEngine
{
	[TestFixture]
	public sealed class UnaryExpressionTest
	{
		[Test]
		public void TestEvaluate1()
		{
			var env = new BuildEnvironment();
			new UnaryExpression(UnaryOperation.Not, new Literal("False")).Evaluate(env).Should().Be(true);
			new UnaryExpression(UnaryOperation.Not, new Literal(null)).Evaluate(env).Should().Be(true);
			new UnaryExpression(UnaryOperation.Not, new Literal("SomeValue")).Evaluate(env).Should().Be(false);
		}

		[Test]
		public void TestEvaluate2()
		{
			var env = new BuildEnvironment();
			new UnaryExpression(UnaryOperation.Not, new BinaryExpression(
				                                        new Literal("foo"), BinaryOperation.Equals, new Literal("foo")))
				.Evaluate(env).Should().Be(false, "because !(foo == foo) is false");
			new UnaryExpression(UnaryOperation.Not, new BinaryExpression(
														new Literal("foo"), BinaryOperation.Equals, new Literal("bar")))
				.Evaluate(env).Should().Be(true, "because !(foo == bar) is true");
		}
	}
}