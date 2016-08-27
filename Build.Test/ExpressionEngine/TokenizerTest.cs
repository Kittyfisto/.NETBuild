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
			TestTokenizeWhiteLeadingWhitespace("(", new Token(TokenType.OpenBracket));
			TestTokenizeWhiteLeadingWhitespace(")", new Token(TokenType.CloseBracket));
			TestTokenizeWhiteLeadingWhitespace("'", new Token(TokenType.Quotation));
			TestTokenizeWhiteLeadingWhitespace("!", new Token(TokenType.Not));
			TestTokenizeWhiteLeadingWhitespace("==", new Token(TokenType.Equals));
			TestTokenizeWhiteLeadingWhitespace("!=", new Token(TokenType.NotEquals));
			TestTokenizeWhiteLeadingWhitespace("<", new Token(TokenType.LessThan));
			TestTokenizeWhiteLeadingWhitespace("<=", new Token(TokenType.LessOrEquals));
			TestTokenizeWhiteLeadingWhitespace(">", new Token(TokenType.GreaterThan));
			TestTokenizeWhiteLeadingWhitespace(">=", new Token(TokenType.GreaterOrEquals));
			TestTokenizeWhiteLeadingWhitespace("And", new Token(TokenType.And));
			TestTokenizeWhiteLeadingWhitespace("Or", new Token(TokenType.Or));
			TestTokenizeWhiteLeadingWhitespace("$(Foo)", new Token(TokenType.Variable, "Foo"));
			TestTokenizeWhiteLeadingWhitespace("SomeValue", new Token(TokenType.Literal, "SomeValue"));
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
					          new Token(TokenType.Quotation),
					          new Token(TokenType.Literal, "Debug"),
					          new Token(TokenType.Quotation)
				          });
		}

		[Test]
		public void TestTokenize4()
		{
			_tokenizer.Tokenize("$(Configuration) == ''")
			          .Should().Equal(new object[]
				          {
					          new Token(TokenType.Variable, "Configuration"),
					          new Token(TokenType.Equals),
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

		private void TestTokenizeWhiteLeadingWhitespace(string token, Token expectedToken)
		{
			var expectedTokens = new object[] {expectedToken};
			_tokenizer.Tokenize(token).Should().Equal(expectedTokens);
			_tokenizer.Tokenize(" "+token).Should().Equal(expectedTokens);
			_tokenizer.Tokenize("  "+token).Should().Equal(expectedTokens);
			_tokenizer.Tokenize("	"+token).Should().Equal(expectedTokens);
			_tokenizer.Tokenize("		"+token).Should().Equal(expectedTokens);
			_tokenizer.Tokenize("		"+token).Should().Equal(expectedTokens);
		}
	}
}