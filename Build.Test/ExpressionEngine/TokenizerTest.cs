using Build.ExpressionEngine;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.ExpressionEngine
{
	[TestFixture]
	public sealed class TokenizerTest
	{
		private Tokenizer _tokenizer;

		[SetUp]
		public void Setup()
		{
			_tokenizer = new Tokenizer();
		}

		[Test]
		public void TestTokenize1()
		{
			_tokenizer.Tokenize("(").Should().Equal(new Token(TokenType.OpenBracket));
			_tokenizer.Tokenize(")").Should().Equal(new Token(TokenType.CloseBracket));
			_tokenizer.Tokenize("'").Should().Equal(new Token(TokenType.Quotation));
			_tokenizer.Tokenize("!").Should().Equal(new Token(TokenType.Not));
			_tokenizer.Tokenize("==").Should().Equal(new Token(TokenType.Equals));
			_tokenizer.Tokenize("!=").Should().Equal(new Token(TokenType.NotEquals));
			_tokenizer.Tokenize("<").Should().Equal(new Token(TokenType.LessThan));
			_tokenizer.Tokenize("<=").Should().Equal(new Token(TokenType.LessOrEquals));
			_tokenizer.Tokenize(">").Should().Equal(new Token(TokenType.GreaterThan));
			_tokenizer.Tokenize(">=").Should().Equal(new Token(TokenType.GreaterOrEquals));
			_tokenizer.Tokenize("AND").Should().Equal(new Token(TokenType.And));
			_tokenizer.Tokenize("OR").Should().Equal(new Token(TokenType.Or));
			_tokenizer.Tokenize("$").Should().Equal(new Token(TokenType.Dollar));
			_tokenizer.Tokenize("@").Should().Equal(new Token(TokenType.At));
			_tokenizer.Tokenize("%").Should().Equal(new Token(TokenType.Percent));
			_tokenizer.Tokenize(";").Should().Equal(new Token(TokenType.ItemListSeparator));
			_tokenizer.Tokenize("->").Should().Equal(new Token(TokenType.Arrow));
			_tokenizer.Tokenize("SomeValue").Should().Equal(new Token(TokenType.Literal, "SomeValue"));
		}

		[Test]
		public void TestTokenize2()
		{
			var literals = new[]
				{
					"|",
					"FOO|DEBUG"
				};
			foreach (var literal in literals)
			{
				_tokenizer.Tokenize(literal).Should().Equal(new object[] { new Token(TokenType.Literal, literal) });
			}
		}

		[Test]
		public void TestTokenize3()
		{
			_tokenizer.Tokenize(" 'Debug' ")
			          .Should().Equal(new object[]
				          {
							  new Token(TokenType.Whitespace, " "),
					          new Token(TokenType.Quotation),
					          new Token(TokenType.Literal, "Debug"),
					          new Token(TokenType.Quotation),
							  new Token(TokenType.Whitespace, " ")
				          });
		}

		[Test]
		public void TestTokenize4()
		{
			_tokenizer.Tokenize("$(Configuration) == ''")
			          .Should().Equal(new object[]
				          {
					          new Token(TokenType.Dollar),
							  new Token(TokenType.OpenBracket),
							  new Token(TokenType.Literal, "Configuration"),
							  new Token(TokenType.CloseBracket),
							  new Token(TokenType.Whitespace, " "),
					          new Token(TokenType.Equals),
							  new Token(TokenType.Whitespace, " "),
					          new Token(TokenType.Quotation),
					          new Token(TokenType.Quotation)
				          });
		}

		[Test]
		public void TestTokenize5()
		{
			_tokenizer.Tokenize("!False").Should().Equal(new object[]
				{
					new Token(TokenType.Not),
					new Token(TokenType.Literal, "False")
				});
		}

		[Test]
		public void TestTokenize6()
		{
			_tokenizer.Tokenize("Exists('App.config')").Should().Equal(new object[]
				{
					new Token(TokenType.Literal, "Exists"),
					new Token(TokenType.OpenBracket),
					new Token(TokenType.Quotation),
					new Token(TokenType.Literal, "App.config"),
					new Token(TokenType.Quotation),
					new Token(TokenType.CloseBracket)
				});
		}

		[Test]
		public void TestTokenize7()
		{
			_tokenizer.Tokenize("").Should().BeEmpty();
		}

		[Test]
		public void TestTokenize8()
		{
			_tokenizer.Tokenize("'Debug'").Should().Equal(new object[]
				{
					new Token(TokenType.Quotation),
					new Token(TokenType.Literal, "Debug"),
					new Token(TokenType.Quotation)
				});
		}

		[Test]
		[Description("Verifies that whitespace is not left out")]
		public void TestTokenize9()
		{
			_tokenizer.Tokenize(" Debug ").Should().Equal(new object[]
				{
					new Token(TokenType.Whitespace, " "),
					new Token(TokenType.Literal, "Debug"),
					new Token(TokenType.Whitespace, " ")
				});
			_tokenizer.Tokenize("Some File Name").Should().Equal(new object[]
				{
					new Token(TokenType.Literal, "Some"),
					new Token(TokenType.Whitespace, " "),
					new Token(TokenType.Literal, "File"),
					new Token(TokenType.Whitespace, " "),
					new Token(TokenType.Literal, "Name")
				});
		}

		[Test]
		[Description("Verifies that whitespace is grouped together")]
		public void TestTokenize10()
		{
			_tokenizer.Tokenize("	 \n").Should().Equal(new object[]
				{
					new Token(TokenType.Whitespace, "	 \n")
				});
		}

		[Test]
		public void TestTokenize11()
		{
			_tokenizer.Tokenize(null).Should().BeEmpty();
			_tokenizer.Tokenize(string.Empty).Should().BeEmpty();
		}

		[Test]
		public void TestTokenize12()
		{
			_tokenizer.Tokenize("@(References)").Should().Equal(new object[]
				{
					new Token(TokenType.At),
					new Token(TokenType.OpenBracket),
					new Token(TokenType.Literal, "References"),
					new Token(TokenType.CloseBracket)
				});
		}

		[Test]
		public void TestTokenize13()
		{
			_tokenizer.Tokenize("->").Should().Equal(new object[]
				{
					new Token(TokenType.Arrow),
				});
		}

		[Test]
		public void TestTokenize14()
		{
			_tokenizer.Tokenize("@(Compile -> '%(Filename)')").Should().Equal(new object[]
				{
					new Token(TokenType.At),
					new Token(TokenType.OpenBracket),
					new Token(TokenType.Literal, "Compile"),
					new Token(TokenType.Whitespace, " "),
					new Token(TokenType.Arrow),
					new Token(TokenType.Whitespace, " "),
					new Token(TokenType.Quotation),
					new Token(TokenType.Percent),
					new Token(TokenType.OpenBracket),
					new Token(TokenType.Literal, "Filename"),
					new Token(TokenType.CloseBracket),
					new Token(TokenType.Quotation),
					new Token(TokenType.CloseBracket)
				});
		}

		[Test]
		public void TestGroup1()
		{
			_tokenizer.GroupWhiteSpaceAndLiteral(new[] { new Token(TokenType.Literal, "f"), new Token(TokenType.OpenBracket)
				}).Should().Equal(new[] { new Token(TokenType.Literal, "f"), new Token(TokenType.OpenBracket) });
		}

		[Test]
		public void TestGroup2()
		{
			_tokenizer.GroupWhiteSpaceAndLiteral(new[] { new Token(TokenType.Whitespace, "	"), new Token(TokenType.OpenBracket)
				}).Should().Equal(new[] { new Token(TokenType.Whitespace, "	"), new Token(TokenType.OpenBracket) });
		}

		[Test]
		public void TestGroup3()
		{
			_tokenizer.GroupWhiteSpaceAndLiteral(
				new[] { new Token(TokenType.Whitespace, "	"), new Token(TokenType.Literal, "Foobar")
				}).Should().Equal(new[] { new Token(TokenType.Literal, "	Foobar") });
		}

		[Test]
		public void TestGroup4()
		{
			_tokenizer.GroupWhiteSpaceAndLiteral(
				new[] { new Token(TokenType.Whitespace, "	"), new Token(TokenType.Whitespace, "\r\n")
				}).Should().Equal(new[] { new Token(TokenType.Whitespace, "	\r\n") });
		}

		[Test]
		public void TestGroup5()
		{
			_tokenizer.GroupWhiteSpaceAndLiteral(
				new[]
				{ 
					new Token(TokenType.Whitespace, "	"),
					new Token(TokenType.Literal, "Foobar"),
					new Token(TokenType.Whitespace, "\r\n")
				}).Should().Equal(new[] { new Token(TokenType.Literal, "	Foobar\r\n") });
		}

		[Test]
		public void TestGroup6()
		{
			_tokenizer.GroupWhiteSpaceAndLiteral(
				new[]
					{
						new Token(TokenType.Whitespace, "	"),
						new Token(TokenType.Literal, "Foobar"),
						new Token(TokenType.Whitespace, "\r\n"),
						new Token(TokenType.Dollar, "Stuff"),
						new Token(TokenType.Literal, "More stuff"),
						new Token(TokenType.Whitespace, "	")
					}).Should().Equal(new[]
						{
							new Token(TokenType.Literal, "	Foobar\r\n"),
							new Token(TokenType.Dollar, "Stuff"),
							new Token(TokenType.Literal, "More stuff	")
						});
		}
	}
}