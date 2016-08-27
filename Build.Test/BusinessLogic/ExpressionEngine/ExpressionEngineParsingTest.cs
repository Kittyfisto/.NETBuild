using Build.ExpressionEngine;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.BusinessLogic.ExpressionEngine
{
	[TestFixture]
	public sealed class ExpressionEngineParsingTest
	{
		[SetUp]
		public void SetUp()
		{
			_engine = new Build.ExpressionEngine.ExpressionEngine();
		}

		private Build.ExpressionEngine.ExpressionEngine _engine;

		[Test]
		public void TestParse1()
		{
			_engine.Parse("foo").Should().Be(new Literal("foo"));
		}

		[Test]
		public void TestParse10()
		{
			_engine.Parse(" '$(Configuration)' == '' ").Should().Be(new BinaryExpression(
				                                                        new ConcatExpression(new Variable("Configuration")),
				                                                        BinaryOperation.Equals,
				                                                        new ConcatExpression()));
		}

		[Test]
		public void TestParse11()
		{
			_engine.Parse(" '$(Platform)' != '' ").Should().Be(new BinaryExpression(
				                                                   new ConcatExpression(new Variable("Platform")),
				                                                   BinaryOperation.EqualsNot,
				                                                   new ConcatExpression()));
		}

		[Test]
		public void TestParse2()
		{
			_engine.Parse("$(foo)").Should().Be(new Variable("foo"));
		}

		[Test]
		public void TestParse20()
		{
			_engine.Parse(" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ")
			       .Should().Be(new BinaryExpression(
				                    new ConcatExpression(new Variable("Configuration"),
				                                         new Literal("|"),
				                                         new Variable("Platform")
					                    ),
				                    BinaryOperation.Equals,
				                    new ConcatExpression(
					                    new Literal("Debug|AnyCPU"))));
		}

		[Test]
		public void TestParse3()
		{
			_engine.Parse("!False").Should().Be(new UnaryExpression(UnaryOperation.Not, new Literal("False")));
		}

		[Test]
		public void TestParse4()
		{
			_engine.Parse("foo == bar")
			       .Should()
			       .Be(new BinaryExpression(new Literal("foo"), BinaryOperation.Equals, new Literal("bar")));
		}

		[Test]
		public void TestParse5()
		{
			_engine.Parse("$(Configuration) == Debug")
			       .Should()
			       .Be(new BinaryExpression(new Variable("Configuration"), BinaryOperation.Equals, new Literal("Debug")));
		}

		[Test]
		public void TestParse6()
		{
			_engine.Parse("$(Configuration) == Debug|Foo")
			       .Should()
			       .Be(new BinaryExpression(new Variable("Configuration"), BinaryOperation.Equals,
			                                new Literal("Debug|Foo")));
		}

		[Test]
		public void TestParse7()
		{
			_engine.Parse(" 'Debug' ").Should().Be(new ConcatExpression(new Literal("Debug")));
		}

		[Test]
		public void TestParse8()
		{
			_engine.Parse(" 'Debug|AnyCPU' ")
			       .Should().Be(new ConcatExpression(new Literal("Debug|AnyCPU")));
		}

		[Test]
		public void TestParse9()
		{
			_engine.Parse("'$(Configuration)|$(Platform)'").Should().Be(new ConcatExpression(
				                                                            new Variable("Configuration"),
				                                                            new Literal("|"),
				                                                            new Variable("Platform")));
		}
	}
}