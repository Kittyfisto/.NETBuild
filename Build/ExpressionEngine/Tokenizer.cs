﻿using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace Build.ExpressionEngine
{
	public sealed class Tokenizer
	{
		private static readonly Dictionary<TokenType, string> SpecialTokens;
		public const string ItemListSeparator = ";";

		static Tokenizer()
		{
			SpecialTokens = new Dictionary<TokenType, string>
				{
					{TokenType.OpenBracket, "("},
					{TokenType.CloseBracket, ")"},
					{TokenType.LessOrEquals, "<="},
					{TokenType.GreaterOrEquals, ">="},
					{TokenType.GreaterThan, ">"},
					{TokenType.LessThan, "<"},
					{TokenType.Equals, "=="},
					{TokenType.NotEquals, "!="},
					{TokenType.Not, "!"},
					{TokenType.And, "AND"},
					{TokenType.Or, "OR"},
					{TokenType.Quotation, "'"},
					{TokenType.Dollar, "$"},
					{TokenType.At, "@"},
					{TokenType.Percent, "%"},
					{TokenType.Arrow, "->"},
					{TokenType.ItemListSeparator, ItemListSeparator}
				};
		}

		public static string ToString(TokenType type)
		{
			string value;
			SpecialTokens.TryGetValue(type, out value);
			return value;
		}

		[Pure]
		public List<Token> Tokenize(string expression)
		{
			var tokens = new List<Token>();
			if (expression != null)
			{
				for (int i = 0; i < expression.Length; )
				{
					Token token;
					if (!Match(ref i, expression, out token))
						throw new ParseException(string.Format("Unable to parse: {0}", expression.Substring(i)));

					tokens.Add(token);
				}
			}
			return tokens;
		}

		private bool Match(ref int startIndex, string expression, out Token token)
		{
			int length;
			if (StartsWithWhitespace(expression, startIndex, out length))
			{
				token = new Token(TokenType.Whitespace, expression.Substring(startIndex, length));
				startIndex += length;
				return true;
			}

			foreach (var pair in SpecialTokens)
			{
				if (StartsWith(expression, startIndex, pair.Value))
				{
					startIndex += pair.Value.Length;
					token = new Token(pair.Key);
					return true;
				}
			}

			// => Literal, but we need to determine the extent.
			int i;
			for (i = startIndex; i < expression.Length; ++i)
			{
				if (char.IsWhiteSpace(expression[i]))
					break;

				foreach (var pair in SpecialTokens)
				{
					if (StartsWith(expression, i, pair.Value))
						goto eol;
				}
			}
			eol:

			var value = expression.Substring(startIndex, i - startIndex);
			startIndex += value.Length;
			token = new Token(TokenType.Literal, value);
			return true;
		}

		private static bool StartsWithWhitespace(string expression, int startIndex, out int length)
		{
			length = -1;
			if (expression.Length == 0)
			{
				return false;
			}

			int i;
			for (i = startIndex; i < expression.Length; ++i)
			{
				if (!char.IsWhiteSpace(expression[i]))
					break;
			}

			length = i - startIndex;
			if (length > 0)
				return true;

			return false;
		}

		[Pure]
		private static bool StartsWith(string expression, int startIndex, string token)
		{
			if (expression.Length - startIndex < token.Length)
				return false;

// ReSharper disable LoopCanBeConvertedToQuery
			for (int i = 0; i < token.Length; ++i)
// ReSharper restore LoopCanBeConvertedToQuery
			{
				if (expression[startIndex + i] != token[i])
					return false;
			}

			return true;
		}

		/// <summary>
		/// Returns a new list of tokens where all adjacent literal and whitespace
		/// tokens have been joined to a single Literal token, containing whitespace.
		/// </summary>
		/// <param name="tokens"></param>
		/// <returns></returns>
		public List<Token> GroupWhiteSpaceAndLiteral(IEnumerable<Token> tokens)
		{
			var ret = new List<Token>();
			var builder = new StringBuilder();
			var type = TokenType.Whitespace;
			foreach (var token in tokens)
			{
				if (token.Type == TokenType.Literal ||
				    token.Type == TokenType.Whitespace ||
					token.Type == TokenType.ItemListSeparator)
				{
					if (type != TokenType.Literal && token.Type == TokenType.Literal)
						type = TokenType.Literal;

					builder.Append(
						type == TokenType.ItemListSeparator
							? ToString(type)
							: token.Value);
				}
				else
				{
					if (builder.Length > 0)
					{
						ret.Add(new Token(type, builder.ToString()));
						type = TokenType.Whitespace;
						builder.Clear();
					}

					ret.Add(token);
				}
			}

			if (builder.Length > 0)
			{
				ret.Add(new Token(type, builder.ToString()));
				builder.Clear();
			}

			return ret;
		}
	}
}