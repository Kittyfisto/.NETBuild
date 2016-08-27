using Build.BuildEngine;
using Build.ExpressionEngine;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.ExpressionEngine
{
	[TestFixture]
	public sealed class BinaryExpressionTest
	{
		[Test]
		public void TestEvaluate1()
		{
			new BinaryExpression(new Literal("Foo"),
			                     BinaryOperation.Equals,
			                     new Literal("Foo"))
				.Evaluate(null).Should().Be(true);

			new BinaryExpression(new Literal("Foo"),
								 BinaryOperation.Equals,
								 new Literal("Bar"))
				.Evaluate(null).Should().Be(false);
		}

		[Test]
		public void TestEvaluate2()
		{
			new BinaryExpression(new Variable("Foo"),
								 BinaryOperation.Equals,
								 new Literal("Hello World"))
				.Evaluate(new BuildEnvironment
					{
						{"Foo", "Hello World"}
					}).Should().Be(true);
		}
	}
}