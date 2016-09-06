using System;
using System.Collections.Generic;
using Build.ExpressionEngine;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.ExpressionEngine
{
	[TestFixture]
	public sealed class TokenOrExpressionExtensionsTest
	{
		[Test]
		public void TestTrim1()
		{
			var tokens = new List<TokenOrExpression>();
			new Action(tokens.Trim).ShouldNotThrow();
			tokens.Should().BeEmpty();
		}

		[Test]
		public void TestTrim2()
		{
			var tokens = new List<TokenOrExpression>
				{
					new TokenOrExpression(new Token(TokenType.Literal))
				};
			new Action(tokens.Trim).ShouldNotThrow();
			tokens.Should().Equal(new object[]
				{
					new TokenOrExpression(new Token(TokenType.Literal))
				});
		}

		[Test]
		public void TestTrim3()
		{
			var tokens = new List<TokenOrExpression>
				{
					new TokenOrExpression(new Token(TokenType.Whitespace))
				};
			new Action(tokens.Trim).ShouldNotThrow();
			tokens.Should().BeEmpty();
		}

		[Test]
		public void TestTrim4()
		{
			var tokens = new List<TokenOrExpression>
				{
					new TokenOrExpression(new Token(TokenType.Whitespace, "	\r\n"))
				};
			new Action(tokens.Trim).ShouldNotThrow();
			tokens.Should().BeEmpty();
		}

		[Test]
		public void TestTrim5()
		{
			var tokens = new List<TokenOrExpression>
				{
					new TokenOrExpression(new Token(TokenType.Whitespace, "	\r\n")),
					new TokenOrExpression(new Token(TokenType.Literal, "Reference"))
				};
			new Action(tokens.Trim).ShouldNotThrow();
			tokens.Should().Equal(new object[]
				{
					new TokenOrExpression(new Token(TokenType.Literal, "Reference"))
				});
		}

		[Test]
		public void TestTrim6()
		{
			var tokens = new List<TokenOrExpression>
				{
					new TokenOrExpression(new Token(TokenType.Literal, "Reference")),
					new TokenOrExpression(new Token(TokenType.Whitespace, "	\r\n"))
				};
			new Action(tokens.Trim).ShouldNotThrow();
			tokens.Should().Equal(new object[]
				{
					new TokenOrExpression(new Token(TokenType.Literal, "Reference"))
				});
		}

		[Test]
		public void TestTrim7()
		{
			var tokens = new List<TokenOrExpression>
				{
					new TokenOrExpression(new Token(TokenType.Whitespace, "  ")),
					new TokenOrExpression(new Token(TokenType.Literal, "Reference")),
					new TokenOrExpression(new Token(TokenType.Whitespace, "	\r\n"))
				};
			new Action(tokens.Trim).ShouldNotThrow();
			tokens.Should().Equal(new object[]
				{
					new TokenOrExpression(new Token(TokenType.Literal, "Reference"))
				});
		}

		[Test]
		public void TestTrim8()
		{
			var tokens = new List<TokenOrExpression>
				{
					new TokenOrExpression(new Token(TokenType.Whitespace, "  ")),
					new TokenOrExpression(new Token(TokenType.Literal, "Reference")),
					new TokenOrExpression(new Token(TokenType.At)),
					new TokenOrExpression(new Token(TokenType.Whitespace, "	\r\n"))
				};
			new Action(tokens.Trim).ShouldNotThrow();
			tokens.Should().Equal(new object[]
				{
					new TokenOrExpression(new Token(TokenType.Literal, "Reference")),
					new TokenOrExpression(new Token(TokenType.At))
				});
		}

		[Test]
		public void TestTrim9()
		{
			var tokens = new List<TokenOrExpression>
				{
					new TokenOrExpression(new Token(TokenType.Whitespace, "  ")),
					new TokenOrExpression(new Token(TokenType.Literal, "Reference")),
					new TokenOrExpression(new Token(TokenType.Whitespace, "  ")),
					new TokenOrExpression(new Token(TokenType.At)),
					new TokenOrExpression(new Token(TokenType.Whitespace, "	\r\n"))
				};
			new Action(tokens.Trim).ShouldNotThrow();
			tokens.Should().Equal(new object[]
				{
					new TokenOrExpression(new Token(TokenType.Literal, "Reference")),
					new TokenOrExpression(new Token(TokenType.Whitespace, "  ")),
					new TokenOrExpression(new Token(TokenType.At))
				});
		}

		[Test]
		public void TestTrim10()
		{
			var tokens = new List<TokenOrExpression>
				{
					new TokenOrExpression(new Token(TokenType.Whitespace, "  ")),
					new TokenOrExpression(new Token(TokenType.Whitespace, "  ")),
					new TokenOrExpression(new Token(TokenType.Whitespace, "  ")),
					new TokenOrExpression(new Token(TokenType.Whitespace, "  ")),
					new TokenOrExpression(new Token(TokenType.Whitespace, "	\r\n")),
					new TokenOrExpression(new Token(TokenType.Whitespace, "  ")),
					new TokenOrExpression(new Token(TokenType.Literal, "Reference")),
					new TokenOrExpression(new Token(TokenType.Whitespace, "  ")),
					new TokenOrExpression(new Token(TokenType.At)),
					new TokenOrExpression(new Token(TokenType.Whitespace, "	\r\n")),
					new TokenOrExpression(new Token(TokenType.Whitespace, "  ")),
					new TokenOrExpression(new Token(TokenType.Whitespace, "  ")),
					new TokenOrExpression(new Token(TokenType.Whitespace, "	\r\n"))
				};
			new Action(tokens.Trim).ShouldNotThrow();
			tokens.Should().Equal(new object[]
				{
					new TokenOrExpression(new Token(TokenType.Literal, "Reference")),
					new TokenOrExpression(new Token(TokenType.Whitespace, "  ")),
					new TokenOrExpression(new Token(TokenType.At))
				});
		}
	}
}