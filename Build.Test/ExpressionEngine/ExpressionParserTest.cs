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

		#region Expression

		[Test]
		public void TestParse1()
		{
			_parser.ParseExpression("foo").Should().Be(new StringLiteral("foo"));
		}

		[Test]
		public void TestParse2()
		{
			_parser.ParseExpression("$(foo)").Should().Be(new VariableReference("foo"));
		}

		[Test]
		public void TestParse3()
		{
			_parser.ParseExpression("!False").Should().Be(new UnaryExpression(UnaryOperation.Not, new StringLiteral("False")));
		}

		[Test]
		public void TestParse4()
		{
			_parser.ParseExpression("'foo'=='bar'")
			       .Should()
			       .Be(new BinaryExpression(
				           new StringLiteral("foo"),
				           BinaryOperation.Equals,
				           new StringLiteral("bar")));
		}

		[Test]
		public void TestParse5()
		{
			_parser.ParseExpression("'foo' == 'bar'")
			       .Should()
			       .Be(new BinaryExpression(
				           new StringLiteral("foo"),
				           BinaryOperation.Equals,
				           new StringLiteral("bar")));
		}

		[Test]
		public void TestParse6()
		{
			_parser.ParseExpression("$(Configuration) == Debug")
			       .Should()
			       .Be(new BinaryExpression(new VariableReference("Configuration"), BinaryOperation.Equals, new StringLiteral("Debug")));
		}

		[Test]
		public void TestParse7()
		{
			_parser.ParseExpression("$(Configuration) == Debug|Foo")
			       .Should()
			       .Be(new BinaryExpression(new VariableReference("Configuration"), BinaryOperation.Equals,
			                                new StringLiteral("Debug|Foo")));
		}

		[Test]
		public void TestParse8()
		{
			_parser.ParseExpression(" 'Debug' ").Should().Be(new StringLiteral("Debug"));
		}

		[Test]
		public void TestParse9()
		{
			_parser.ParseExpression(" 'Debug|AnyCPU' ")
			       .Should().Be(new StringLiteral("Debug|AnyCPU"));
		}

		[Test]
		public void TestParse10()
		{
			_parser.ParseExpression(" '$(Configuration)' == '' ").Should().Be(new BinaryExpression(
																		new VariableReference("Configuration"),
																		BinaryOperation.Equals,
																		new StringLiteral("")));
		}

		[Test]
		public void TestParse11()
		{
			_parser.ParseExpression(" '$(Platform)' != '' ").Should().Be(new BinaryExpression(
																   new VariableReference("Platform"),
																   BinaryOperation.EqualsNot,
																   new StringLiteral("")));
		}

		[Test]
		public void TestParse12()
		{
			_parser.ParseExpression("'$(Configuration)|$(Platform)'").Should().Be(new ConcatExpression(
				                                                            new VariableReference("Configuration"),
				                                                            new StringLiteral("|"),
				                                                            new VariableReference("Platform")));
		}

		[Test]
		public void TestParse13()
		{
			_parser.ParseExpression("'$(Configuration)' == 'Debug' AND '$(Platform)' == 'AnyCPU'").
			        Should().Be(new BinaryExpression(
				                    new BinaryExpression(new VariableReference("Configuration"), BinaryOperation.Equals, new StringLiteral("Debug")),
				                    BinaryOperation.And,
				                    new BinaryExpression(new VariableReference("Platform"), BinaryOperation.Equals, new StringLiteral("AnyCPU"))));
		}

		[Test]
		public void TestParse14()
		{
			_parser.ParseExpression("$(Configuration) == 'Debug' AND $(Platform) == 'AnyCPU'").
					Should().Be(new BinaryExpression(
									new BinaryExpression(new VariableReference("Configuration"), BinaryOperation.Equals, new StringLiteral("Debug")),
									BinaryOperation.And,
									new BinaryExpression(new VariableReference("Platform"), BinaryOperation.Equals, new StringLiteral("AnyCPU"))));
		}

		[Test]
		public void TestParse15()
		{
			_parser.ParseExpression("$(Configuration) == 'Debug' OR $(Platform) == 'AnyCPU'").
					Should().Be(new BinaryExpression(
									new BinaryExpression(new VariableReference("Configuration"), BinaryOperation.Equals, new StringLiteral("Debug")),
									BinaryOperation.Or,
									new BinaryExpression(new VariableReference("Platform"), BinaryOperation.Equals, new StringLiteral("AnyCPU"))));
		}

		[Test]
		public void TestParse16()
		{
			_parser.ParseExpression("'$(Configuration)' == 'Debug' OR '$(Platform)' == 'AnyCPU'").
					Should().Be(new BinaryExpression(
									new BinaryExpression(new VariableReference("Configuration"), BinaryOperation.Equals, new StringLiteral("Debug")),
									BinaryOperation.Or,
									new BinaryExpression(new VariableReference("Platform"), BinaryOperation.Equals, new StringLiteral("AnyCPU"))));
		}

		[Test]
		public void TestParse17()
		{
			_parser.ParseExpression("Exists('App.config')").
			        Should().Be(new FunctionExpression(FunctionOperation.Exists,
			                                           new StringLiteral("App.config")));
		}

		[Test]
		public void TestParse18()
		{
			_parser.ParseExpression("true != $(Foobar)").
					Should().Be(new BinaryExpression(new StringLiteral("true"), BinaryOperation.EqualsNot, new VariableReference("Foobar")));
		}

		[Test]
		public void TestParse19()
		{
			_parser.ParseExpression("42 > $(Foobar)").
					Should().Be(new BinaryExpression(new StringLiteral("42"), BinaryOperation.GreaterThan, new VariableReference("Foobar")));
			_parser.ParseExpression("42 < $(Foobar)").
					Should().Be(new BinaryExpression(new StringLiteral("42"), BinaryOperation.LessThan, new VariableReference("Foobar")));
			_parser.ParseExpression("42 >= $(Foobar)").
					Should().Be(new BinaryExpression(new StringLiteral("42"), BinaryOperation.GreaterOrEquals, new VariableReference("Foobar")));
			_parser.ParseExpression("42 <= $(Foobar)").
					Should().Be(new BinaryExpression(new StringLiteral("42"), BinaryOperation.LessOrEquals, new VariableReference("Foobar")));
		}

		[Test]
		public void TestParse20()
		{
			_parser.ParseExpression("Exists('foo.txt') > 42").Should().Be(
				new BinaryExpression(new FunctionExpression(FunctionOperation.Exists, new StringLiteral("foo.txt")),
				                     BinaryOperation.GreaterThan,
				                     new StringLiteral("42"))
				);
		}

		[Test]
		public void TestParse21()
		{
			_parser.ParseExpression("42 > false").Should().Be(
				new BinaryExpression(new StringLiteral("42"),
				                     BinaryOperation.GreaterThan,
				                     new StringLiteral("false"))
				);
		}

		[Test]
		public void TestParse22()
		{
			_parser.ParseExpression("true >= 42").Should().Be(
				new BinaryExpression(new StringLiteral("true"),
				                     BinaryOperation.GreaterOrEquals,
				                     new StringLiteral("42"))
				);
		}

		[Test]
		public void TestParse23()
		{
			_parser.ParseExpression("42 <= true").Should().Be(
				new BinaryExpression(new StringLiteral("42"),
				                     BinaryOperation.LessOrEquals,
				                     new StringLiteral("true"))
				);
		}

		[Test]
		public void TestParse24()
		{
			_parser.ParseConcatenation("Hello World!").Should().Be(
				new StringLiteral("Hello World!"));
		}

		[Test]
		public void TestParse30()
		{
			_parser.ParseExpression(" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ")
				   .Should().Be(new BinaryExpression(
									new ConcatExpression(new VariableReference("Configuration"),
														 new StringLiteral("|"),
														 new VariableReference("Platform")
										),
									BinaryOperation.Equals,
										new StringLiteral("Debug|AnyCPU")));
		}

		[Test]
		public void TestParse40()
		{
			_parser.ParseExpression("@(Content)").Should().Be(new ItemListReference("Content"));
		}

		#endregion

		#region Concatenation

		[Test]
		public void TestParseConcatenation1()
		{
			_parser.ParseConcatenation("Debug").Should().Be(new StringLiteral("Debug"));
		}

		[Test]
		public void TestParseConcatenation2()
		{
			_parser.ParseConcatenation(" Debug ").Should().Be(new StringLiteral(" Debug "));
		}

		[Test]
		public void TestParseConcatenation3()
		{
			_parser.ParseConcatenation(" 'Debug' ").Should().Be(new StringLiteral(" 'Debug' "));
		}

		[Test]
		public void TestParseConcatenation4()
		{
			_parser.ParseConcatenation("$(Foo)").Should().Be(new VariableReference("Foo"));
		}

		[Test]
		public void TestParseConcatenation5()
		{
			_parser.ParseConcatenation(" $(Foo) ").Should().Be(
				new ConcatExpression(
					new StringLiteral(" "),
					new VariableReference("Foo"),
					new StringLiteral(" ")));
		}

		[Test]
		public void TestParseConcatenation6()
		{
			_parser.ParseConcatenation(" '$(Foo)' ").Should().Be(
				new ConcatExpression(
					new StringLiteral(" '"),
					new VariableReference("Foo"),
					new StringLiteral("' ")));
		}

		[Test]
		public void TestParseConcatenation7()
		{
			_parser.ParseConcatenation("$(Foo) $(Bar)").Should().Be(
				new ConcatExpression(
					new VariableReference("Foo"),
					new StringLiteral(" "),
					new VariableReference("Bar")));
		}

		[Test]
		public void TestParseConcatenation8()
		{
			_parser.ParseConcatenation("$(Foo) @(Content)").Should().Be(
				new ConcatExpression(
					new VariableReference("Foo"),
					new StringLiteral(" "),
					new ItemListReference("Content")));
		}

		[Test]
		public void TestParseConcatenation9()
		{
			_parser.ParseConcatenation("Program.cs;Foo.cs").Should().Be(
				new StringLiteral("Program.cs;Foo.cs"));
		}

		[Test]
		public void TestParseConcatenation10()
		{
			_parser.ParseConcatenation("$(Foo);$(Bar).cs").Should().Be(
				new ConcatExpression(
					new VariableReference("Foo"),
					new StringLiteral(";"),
					new VariableReference("Bar"),
					new StringLiteral(".cs")
					));
		}

		[Test]
		public void TestParseConcatenation11()
		{
			_parser.ParseConcatenation("@(Content)").Should().Be(
				new ItemListReference("Content")
				);
		}

		[Test]
		public void TestParseConcatenation12()
		{
			_parser.ParseConcatenation(null).Should().Be(StringLiteral.Empty);
		}

		#endregion

		#region Condition

		[Test]
		public void TestParseCondition1()
		{
			_parser.ParseCondition("").Should().BeNull();
		}

		[Test]
		public void TestParseCondition2()
		{
			_parser.ParseCondition("$(Foo)").Should().Be(new VariableReference("Foo"));
		}

		[Test]
		public void TestParseCondition3()
		{
			_parser.ParseCondition("$(Foo) > 42").Should().Be(
				new BinaryExpression(new VariableReference("Foo"),
				                     BinaryOperation.GreaterThan,
				                     new StringLiteral("42"))
				);
		}

		#endregion

		#region ItemLists

		[Test]
		public void TestParseItemList1()
		{
			_parser.ParseItemList("Foo").Should().Be(new StringLiteral("Foo"));
			_parser.ParseItemList("Foo.exe").Should().Be(new StringLiteral("Foo.exe"));
		}

		[Test]
		public void TestParseItemList2()
		{
			_parser.ParseItemList("Foo;Bar").Should().Be(new ItemListExpression(
				new StringLiteral("Foo"), new StringLiteral("Bar")
				));
		}

		[Test]
		public void TestParseItemList3()
		{
			_parser.ParseItemList("Foo;Bar;data.xml").Should().Be(new ItemListExpression(
				new StringLiteral("Foo"), new StringLiteral("Bar"), new StringLiteral("data.xml")
				));
		}

		[Test]
		public void TestParseItemList4()
		{
			_parser.ParseItemList("$(OutputPath)")
			       .Should().Be(new VariableReference("OutputPath"));
		}

		[Test]
		public void TestParseItemList5()
		{
			_parser.ParseItemList("$(OutputPath)$(Extension)")
			       .Should().Be(
				       new ConcatExpression(new VariableReference("OutputPath"),
				                            new VariableReference("Extension")));
		}

		[Test]
		public void TestParseItemList6()
		{
			_parser.ParseItemList("$(File1);$(File2)")
			       .Should().Be(
				       new ItemListExpression(
					       new VariableReference("File1"),
					       new VariableReference("File2")
					       ));
		}

		[Test]
		public void TestParseItemList7()
		{
			_parser.ParseItemList("$(File1);data.xml")
			       .Should().Be(
				       new ItemListExpression(
					       new VariableReference("File1"),
					       new StringLiteral("data.xml")
					       ));
		}

		[Test]
		public void TestParseItemList8()
		{
			_parser.ParseItemList("$(OutputPath)\\$(OutputAssemblyName).config")
			       .Should().Be(new ConcatExpression(
				                    new VariableReference("OutputPath"),
				                    new StringLiteral("\\"),
				                    new VariableReference("OutputAssemblyName"),
				                    new StringLiteral(".config")
				                    )
				);
		}

		[Test]
		public void TestParseItemList9()
		{
			_parser.ParseItemList("@(Compile)")
			       .Should().Be(
				       new ItemListReference("Compile"));
		}

		[Test]
		public void TestParseItemList10()
		{
			_parser.ParseItemList("@(Compile);Foo.xsd")
			       .Should().Be(
				       new ItemListExpression(
					       new ItemListReference("Compile"),
					       new StringLiteral("Foo.xsd")
					       ));
		}

		[Test]
		public void TestParseItemList11()
		{
			_parser.ParseItemList("")
			       .Should().Be(new ItemListExpression());
		}

		[Test]
		public void TestParseItemList12()
		{
			_parser.ParseItemList("data.xaml;$(Foo);foo'bar")
				   .Should().Be(
				   new ItemListExpression(
					   new StringLiteral("data.xaml"),
					   new VariableReference("Foo"),
					   new StringLiteral("foo'bar")
					   ));
		}

		[Test]
		public void TestParseItemList13()
		{
			_parser.ParseItemList("data.xaml;$(Foo);foo'bar;program.cs")
				   .Should().Be(
				   new ItemListExpression(
					   new StringLiteral("data.xaml"),
					   new VariableReference("Foo"),
					   new StringLiteral("foo'bar"),
					   new StringLiteral("program.cs")
					   ));
		}

		[Test]
		public void TestParseItemList14()
		{
			_parser.ParseItemList("data.xaml;$(Foo);foo'bar;@(Content);program.cs")
				   .Should().Be(
				   new ItemListExpression(
					   new StringLiteral("data.xaml"),
					   new VariableReference("Foo"),
					   new StringLiteral("foo'bar"),
					   new ItemListReference("Content"),
					   new StringLiteral("program.cs")
					   ));
		}

		#endregion
	}
}