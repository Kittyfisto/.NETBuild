using System;
using Build.ExpressionEngine;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.ExpressionEngine
{
	[TestFixture]
	public sealed class ExpressionParserTest
	{
		[SetUp]
		public void SetUp()
		{
			_parser = new ExpressionParser();
		}

		private ExpressionParser _parser;

		[Test]
		public void TestParse1()
		{
			_parser.ParseExpression("foo").Should().Be(new Literal("foo"));
		}

		[Test]
		public void TestParse2()
		{
			_parser.ParseExpression("$(foo)").Should().Be(new Variable("foo"));
		}

		[Test]
		public void TestParse3()
		{
			_parser.ParseExpression("!False").Should().Be(new UnaryExpression(UnaryOperation.Not, new Literal("False")));
		}

		[Test]
		public void TestParse4()
		{
			_parser.ParseExpression("'foo'=='bar'")
			       .Should()
			       .Be(new BinaryExpression(
				           new ConcatExpression(new Literal("foo")),
				           BinaryOperation.Equals,
				           new ConcatExpression(new Literal("bar"))));
		}

		[Test]
		public void TestParse5()
		{
			_parser.ParseExpression("'foo' == 'bar'")
			       .Should()
			       .Be(new BinaryExpression(
				           new ConcatExpression(new Literal("foo")),
				           BinaryOperation.Equals,
				           new ConcatExpression(new Literal("bar"))));
		}

		[Test]
		public void TestParse6()
		{
			_parser.ParseExpression("$(Configuration) == Debug")
			       .Should()
			       .Be(new BinaryExpression(new Variable("Configuration"), BinaryOperation.Equals, new Literal("Debug")));
		}

		[Test]
		public void TestParse7()
		{
			_parser.ParseExpression("$(Configuration) == Debug|Foo")
			       .Should()
			       .Be(new BinaryExpression(new Variable("Configuration"), BinaryOperation.Equals,
			                                new Literal("Debug|Foo")));
		}

		[Test]
		public void TestParse8()
		{
			_parser.ParseExpression(" 'Debug' ").Should().Be(new ConcatExpression(new Literal("Debug")));
		}

		[Test]
		public void TestParse9()
		{
			_parser.ParseExpression(" 'Debug|AnyCPU' ")
			       .Should().Be(new ConcatExpression(new Literal("Debug|AnyCPU")));
		}

		[Test]
		public void TestParse10()
		{
			_parser.ParseExpression(" '$(Configuration)' == '' ").Should().Be(new BinaryExpression(
																		new ConcatExpression(new Variable("Configuration")),
																		BinaryOperation.Equals,
																		new ConcatExpression()));
		}

		[Test]
		public void TestParse11()
		{
			_parser.ParseExpression(" '$(Platform)' != '' ").Should().Be(new BinaryExpression(
																   new ConcatExpression(new Variable("Platform")),
																   BinaryOperation.EqualsNot,
																   new ConcatExpression()));
		}

		[Test]
		public void TestParse12()
		{
			_parser.ParseExpression("'$(Configuration)|$(Platform)'").Should().Be(new ConcatExpression(
				                                                            new Variable("Configuration"),
				                                                            new Literal("|"),
				                                                            new Variable("Platform")));
		}

		[Test]
		public void TestParse13()
		{
			_parser.ParseExpression("'$(Configuration)' == 'Debug' AND '$(Platform)' == 'AnyCPU'").
			        Should().Be(new BinaryExpression(
				                    new BinaryExpression(new ConcatExpression(new Variable("Configuration")), BinaryOperation.Equals, new ConcatExpression(new Literal("Debug"))),
				                    BinaryOperation.And,
				                    new BinaryExpression(new ConcatExpression(new Variable("Platform")), BinaryOperation.Equals, new ConcatExpression(new Literal("AnyCPU")))));
		}

		[Test]
		public void TestParse14()
		{
			_parser.ParseExpression("$(Configuration) == 'Debug' AND $(Platform) == 'AnyCPU'").
					Should().Be(new BinaryExpression(
									new BinaryExpression(new Variable("Configuration"), BinaryOperation.Equals, new ConcatExpression(new Literal("Debug"))),
									BinaryOperation.And,
									new BinaryExpression(new Variable("Platform"), BinaryOperation.Equals, new ConcatExpression(new Literal("AnyCPU")))));
		}

		[Test]
		public void TestParse15()
		{
			_parser.ParseExpression("$(Configuration) == 'Debug' OR $(Platform) == 'AnyCPU'").
					Should().Be(new BinaryExpression(
									new BinaryExpression(new Variable("Configuration"), BinaryOperation.Equals, new ConcatExpression(new Literal("Debug"))),
									BinaryOperation.Or,
									new BinaryExpression(new Variable("Platform"), BinaryOperation.Equals, new ConcatExpression(new Literal("AnyCPU")))));
		}

		[Test]
		public void TestParse16()
		{
			_parser.ParseExpression("'$(Configuration)' == 'Debug' OR '$(Platform)' == 'AnyCPU'").
					Should().Be(new BinaryExpression(
									new BinaryExpression(new ConcatExpression(new Variable("Configuration")), BinaryOperation.Equals, new ConcatExpression(new Literal("Debug"))),
									BinaryOperation.Or,
									new BinaryExpression(new ConcatExpression(new Variable("Platform")), BinaryOperation.Equals, new ConcatExpression(new Literal("AnyCPU")))));
		}

		[Test]
		public void TestParse17()
		{
			_parser.ParseExpression("Exists('App.config')").
			        Should().Be(new FunctionExpression(FunctionOperation.Exists,
			                                           new ConcatExpression(new Literal("App.config"))));
		}

		[Test]
		public void TestParse18()
		{
			_parser.ParseExpression("true != $(Foobar)").
					Should().Be(new BinaryExpression(new Literal("true"), BinaryOperation.EqualsNot, new Variable("Foobar")));
		}

		[Test]
		public void TestParse19()
		{
			_parser.ParseExpression("42 > $(Foobar)").
					Should().Be(new BinaryExpression(new Literal("42"), BinaryOperation.GreaterThan, new Variable("Foobar")));
			_parser.ParseExpression("42 < $(Foobar)").
					Should().Be(new BinaryExpression(new Literal("42"), BinaryOperation.LessThan, new Variable("Foobar")));
			_parser.ParseExpression("42 >= $(Foobar)").
					Should().Be(new BinaryExpression(new Literal("42"), BinaryOperation.GreaterOrEquals, new Variable("Foobar")));
			_parser.ParseExpression("42 <= $(Foobar)").
					Should().Be(new BinaryExpression(new Literal("42"), BinaryOperation.LessOrEquals, new Variable("Foobar")));
		}

		[Test]
		public void TestParse20()
		{
			new Action(() => _parser.ParseExpression("Exists('foo.txt') > 42"))
				.ShouldThrow<ParseException>()
				.WithMessage("Expected literal or variable left of >, but found \"Exists('foo.txt')\"");
		}

		[Test]
		public void TestParse21()
		{
			new Action(() => _parser.ParseExpression("42 > false"))
				.ShouldThrow<ParseException>()
				.WithMessage("Expected literal or variable right of >, but found \"false\"");
		}

		[Test]
		public void TestParse22()
		{
			new Action(() => _parser.ParseExpression("true >= 42"))
				.ShouldThrow<ParseException>()
				.WithMessage("Expected literal or variable left of >=, but found \"true\"");
		}

		[Test]
		public void TestParse23()
		{
			new Action(() => _parser.ParseExpression("42 <= true"))
				.ShouldThrow<ParseException>()
				.WithMessage("Expected literal or variable right of <=, but found \"true\"");
		}

		[Test]
		public void TestParse30()
		{
			_parser.ParseExpression(" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ")
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
		public void TestParse40()
		{
			_parser.ParseExpression("@(Content)").Should().Be(new ItemListExpression("Content"));
		}
	}
}