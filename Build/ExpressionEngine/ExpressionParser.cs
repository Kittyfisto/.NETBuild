using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Build.ExpressionEngine
{
	public sealed class ExpressionParser
	{
		private readonly Tokenizer _tokenizer;

		public ExpressionParser()
		{
			_tokenizer = new Tokenizer();
		}

		[Pure]
		public IExpression ParseCondition(string expression)
		{
			try
			{
				List<Token> tokens = _tokenizer.Tokenize(expression);
				IExpression expr = Parse(tokens);
				IExpression optimized = ExpressionOptimizer.Run(expr);
				return optimized;
			}
			catch (Exception e)
			{
				throw new ParseException(string.Format("Unable to parse \"{0}\": {1}", expression, e.Message), e);
			}
		}

		[Pure]
		public IExpression ParseItemList(string expression)
		{
			try
			{
				List<Token> tokens = _tokenizer.Tokenize(expression);
				var stack = new List<TokenOrExpression>();
				foreach (Token token in tokens)
				{
					stack.Add(token);
				}

				bool successfullyParsed;
				do
				{
					successfullyParsed = false;
					if (TryParseItemListContent(stack))
						successfullyParsed = true;

					if (TryParseItemList(stack))
						successfullyParsed = true;
				} while (stack.Count > 1 && successfullyParsed);

				if (stack.Count == 0) //< Empty input was given
					return new ItemListExpression();

				if (stack.Count > 1 || stack[0].Expression == null)
					throw new ParseException("Expected exactly one token on the stack");

				IExpression itemList = stack[0].Expression;
				IExpression optimized = ExpressionOptimizer.Run(itemList);
				return optimized;
			}
			catch (Exception e)
			{
				throw new ParseException(string.Format("Unable to parse \"{0}\": {1}", expression, e.Message), e);
			}
		}

		public IExpression ParseConcatenation(string expression)
		{
			try
			{
				if (string.IsNullOrEmpty(expression))
					return StringLiteral.Empty;

				// This is much simpler since we don't interpret general expressions
				// in string concatenation: We only replace property values..
				List<Token> tokens = _tokenizer.Tokenize(expression);
				var stack = new List<TokenOrExpression>(tokens.Count);
				foreach (Token token in tokens)
					stack.Add(token);

				bool successfullyParsed;
				do
				{
					successfullyParsed = false;
					if (TryParseLiteral(stack))
						successfullyParsed = true;
					if (TryParseSpecialCharsAsLiteral(stack, consumeItemListSeparator: true))
						successfullyParsed = true;
					if (TryParseItemListReference(stack))
						successfullyParsed = true;
					if (TryParseVariableReference(stack))
						successfullyParsed = true;
					if (TryParseItemListReference(stack))
						successfullyParsed = true;
					if (TryParseConcatenation(stack, consumeItemListSeparator: true, includeMetadataReference: true))
						successfullyParsed = true;
				} while (stack.Count > 1 && successfullyParsed);

				if (stack.Count != 1)
					throw new ParseException();
				if (stack[0].Expression == null)
					throw new ParseException();

				IExpression expr = stack[0].Expression;
				IExpression optimized = ExpressionOptimizer.Run(expr);
				return optimized;
			}
			catch (Exception e)
			{
				throw new ParseException(string.Format("Unable to parse \"{0}\": {1}", expression, e.Message), e);
			}
		}

		public IExpression ParseExpression(string expression)
		{
			try
			{
				List<Token> tokens = _tokenizer.Tokenize(expression);
				IExpression expr = Parse(tokens);
				IExpression optimized = ExpressionOptimizer.Run(expr);
				return optimized;
			}
			catch (Exception e)
			{
				throw new ParseException(string.Format("Unable to parse \"{0}\": {1}", expression, e.Message), e);
			}
		}

		#region Optimization

		#endregion

		#region Actual Parsing

		private bool IsFunctionName(Token token, out FunctionOperation operation)
		{
			switch (token.Value)
			{
				case "Exists":
					operation = FunctionOperation.Exists;
					return true;

				case "HasTrailingSlash":
					operation = FunctionOperation.HasTrailingSlash;
					return true;
			}

			operation = (FunctionOperation) (-1);
			return false;
		}

		private static bool IsBinaryOperator(TokenType type, out BinaryOperation operation)
		{
			switch (type)
			{
				case TokenType.Equals:
					operation = BinaryOperation.Equals;
					return true;

				case TokenType.NotEquals:
					operation = BinaryOperation.EqualsNot;
					return true;
				case TokenType.LessThan:
					operation = BinaryOperation.LessThan;
					return true;

				case TokenType.LessOrEquals:
					operation = BinaryOperation.LessOrEquals;
					return true;

				case TokenType.GreaterThan:
					operation = BinaryOperation.GreaterThan;
					return true;

				case TokenType.GreaterOrEquals:
					operation = BinaryOperation.GreaterOrEquals;
					return true;

				case TokenType.And:
					operation = BinaryOperation.And;
					return true;

				case TokenType.Or:
					operation = BinaryOperation.Or;
					return true;

				default:
					operation = (BinaryOperation) (-1);
					return false;
			}
		}

		private static bool IsOperator(TokenType type)
		{
			BinaryOperation unused1;
			if (IsBinaryOperator(type, out unused1))
				return true;

			UnaryOperation unused2;
			if (IsUnaryOperator(type, out unused2))
				return true;

			return false;
		}

		private static bool IsUnaryOperator(TokenType type, out UnaryOperation operation)
		{
			switch (type)
			{
				case TokenType.Not:
					operation = UnaryOperation.Not;
					return true;

				default:
					operation = (UnaryOperation) (-1);
					return false;
			}
		}

		private static int Precedence(TokenType type)
		{
			switch (type)
			{
				case TokenType.Not:
					return 5;

				case TokenType.LessThan:
				case TokenType.LessOrEquals:
				case TokenType.GreaterThan:
				case TokenType.GreaterOrEquals:
					return 4;

				case TokenType.Equals:
				case TokenType.NotEquals:
					return 3;

				case TokenType.And:
					return 2;

				case TokenType.Or:
					return 1;

				default:
					return 0;
			}
		}

		private IExpression Parse(IEnumerable<Token> tokens)
		{
			var stack = new List<TokenOrExpression>();
			int highestPrecedence = 0;
			IEnumerator<Token> iterator = tokens.GetEnumerator();
			while (iterator.MoveNext())
			{
				Token token = iterator.Current;
				if (token.Type == TokenType.Whitespace)
					continue;

				if (IsOperator(token.Type))
				{
					int precedence = Precedence(token.Type);
					if (precedence < highestPrecedence)
					{
						TryParseLeftToRight(stack);
					}

					stack.Add(token);
					highestPrecedence = precedence;
				}
				else if (token.Type == TokenType.Quotation)
				{
					// Consume everything until the quote closes again...
					var content = new List<TokenOrExpression>();
					var arguments = new List<IExpression>();
					while (iterator.MoveNext() &&
					       iterator.Current.Type != TokenType.Quotation)
					{
						content.Add(iterator.Current);
						if (TryParseLeftToRight(content))
						{
							arguments.Add(content[0].Expression);
							content.Clear();
						}
					}

					if (content.Count != 0)
						throw new ParseException();

					stack.Add(new TokenOrExpression(new ConcatExpression(arguments)));
				}
				else
				{
					stack.Add(token);
				}
			}

			if (stack.Count == 0)
				return null;

			if (!TryParseLeftToRight(stack))
				throw new ParseException();

			TokenOrExpression tok = stack[0];
			return tok.Expression;
		}

		private IExpression Parse(TokenOrExpression tokenOrExpression)
		{
			if (tokenOrExpression.Expression != null)
				return tokenOrExpression.Expression;

			return Parse(tokenOrExpression.Token);
		}

		/// <summary>
		///     Parses an expression from the given stack in left-to-right order.
		///     Operator precedences are ignored.
		/// </summary>
		/// <param name="tokens"></param>
		private bool TryParseLeftToRight(List<TokenOrExpression> tokens)
		{
			int beginTokens;
			do
			{
				beginTokens = tokens.Count;
				if (!TryParseOne(tokens))
					return false;

				if (tokens.Count > beginTokens)
					throw new Exception("Parser encountered an error: There shouldn't be more tokens than we started with!");
			} while (tokens.Count < beginTokens);

			if (tokens.Count == 1 &&
			    tokens[0].Expression != null)
				return true;

			return false;
		}

		private bool TryParseOne(List<TokenOrExpression> tokens)
		{
			if (TryParseVariableReference(tokens))
				return true;

			if (TryParseItemListReference(tokens))
				return true;

			if (TryParseFunctionCall(tokens))
				return true;

			if (TryParseBinaryOperation(tokens))
				return true;

			if (TryParseUnaryOperation(tokens))
				return true;

			if (tokens.Count >= 1)
			{
				if (tokens[0].Expression != null)
					return true;

				if (tokens[0].Token.Type == TokenType.Literal)
				{
					tokens[0] = new TokenOrExpression(new StringLiteral(tokens[0].Token.Value));
					return true;
				}
			}

			return false;
		}

		private bool TryParseConcatenation(List<TokenOrExpression> tokens,
		                                   bool consumeItemListSeparator,
		                                   bool includeMetadataReference)
		{
			Func<TokenOrExpression, bool> isLiteralOrVariable = (pair) =>
				{
					if (pair.Expression is StringLiteral)
						return true;
					if (pair.Expression is ItemListReference)
						return true;
					if (pair.Expression is VariableReference)
						return true;
					if (pair.Expression is ConcatExpression)
						return true;
					if (pair.Expression is MetadataReference)
						return includeMetadataReference;

					switch (pair.Token.Type)
					{
						case TokenType.Equals:
						case TokenType.NotEquals:
						case TokenType.And:
						case TokenType.Or:
						case TokenType.GreaterThan:
						case TokenType.GreaterOrEquals:
						case TokenType.LessThan:
						case TokenType.LessOrEquals:
						case TokenType.Literal:
						case TokenType.Not:
						case TokenType.OpenBracket:
						case TokenType.CloseBracket:
						case TokenType.Quotation:
						case TokenType.Whitespace:
							return true;

						case TokenType.Percent:
							return includeMetadataReference;

						case TokenType.ItemListSeparator:
							return consumeItemListSeparator;
					}

					return false;
				};

			if (tokens.Count >= 2 &&
			    isLiteralOrVariable(tokens[0]) &&
			    (consumeItemListSeparator || tokens[1].Token.Type != TokenType.ItemListSeparator))
			{
				List<TokenOrExpression> lhs = tokens.Cut(0, 1);
				if (!TryParseOneConcatenationContent(lhs, consumeItemListSeparator, includeMetadataReference))
					throw new ParseException("Internal error");

				IExpression leftHandSide = lhs[0].Expression;

				if (!TryParseOneConcatenationContent(tokens, consumeItemListSeparator, includeMetadataReference))
					throw new ParseException("Internal error");

				IExpression rightHandSide = tokens[0].Expression;
				tokens[0] = new TokenOrExpression(
					new ConcatExpression(leftHandSide, rightHandSide));
				return true;
			}

			return false;
		}

		private bool TryParseOneConcatenationContent(List<TokenOrExpression> tokens,
		                                             bool consumeItemListSeparator,
		                                             bool includeMetadataReference)
		{
			if (tokens.Count >= 1 && tokens[0].Expression != null)
			{
				IExpression expression = tokens[0].Expression;
				if (expression is ConcatExpression)
					return true;
				if (expression is StringLiteral)
					return true;
				if (expression is ItemListReference)
					return true;
				if (expression is VariableReference)
					return true;
				if (expression is MetadataReference)
					return includeMetadataReference;

				return false;
			}

			if (TryParseLiteral(tokens))
				return true;

			if (TryParseVariableReference(tokens))
				return true;

			if (TryParseItemListReference(tokens))
				return true;

			if (TryParseMetadataReference(tokens))
				return true;

			if (TryParseOperatorsAsLiteral(tokens))
				return true;

			if (TryParseSpecialCharsAsLiteral(tokens, consumeItemListSeparator: consumeItemListSeparator))
				return true;

			return false;
		}

		private bool TryParseSpecialCharsAsLiteral(List<TokenOrExpression> tokens,
		                                           bool consumeItemListSeparator)
		{
			if (tokens.Count >= 1)
			{
				TokenType type = tokens[0].Token.Type;
				switch (type)
				{
					case TokenType.Not:
					case TokenType.OpenBracket:
					case TokenType.CloseBracket:
					case TokenType.Quotation:
					case TokenType.Arrow:
						tokens[0] = new TokenOrExpression(new StringLiteral(Tokenizer.ToString(type)));
						return true;

					case TokenType.ItemListSeparator:
						if (consumeItemListSeparator)
						{
							tokens[0] = new TokenOrExpression(
								new StringLiteral(Tokenizer.ToString(type))
								);
							return true;
						}
						break;

					case TokenType.Whitespace:
						tokens[0] = new TokenOrExpression(new StringLiteral(tokens[0].Token.Value));
						return true;
				}
			}

			return false;
		}

		private bool TryParseLiteral(List<TokenOrExpression> tokens)
		{
			if (tokens.Count >= 1 &&
			    (tokens[0].Token.Type == TokenType.Literal))
			{
				string value = tokens[0].Token.Value;
				tokens[0] = new TokenOrExpression(
					new StringLiteral(value));
				return true;
			}

			return false;
		}

		private bool TryParseOperatorsAsLiteral(List<TokenOrExpression> tokens)
		{
			UnaryOperation unaryOperation;
			if (tokens.Count >= 1 &&
			    IsUnaryOperator(tokens[0].Token.Type, out unaryOperation))
			{
				tokens[0] = new TokenOrExpression(new StringLiteral(
					                                  Tokenizer.ToString(tokens[0].Token.Type)
					                                  ));
				return true;
			}

			BinaryOperation binaryOperation;
			if (tokens.Count >= 1 &&
			    IsBinaryOperator(tokens[0].Token.Type, out binaryOperation))
			{
				tokens[0] = new TokenOrExpression(new StringLiteral(
					                                  Tokenizer.ToString(tokens[0].Token.Type)
					                                  ));
				return true;
			}

			return false;
		}

		private bool TryParseMetadataReference(List<TokenOrExpression> tokens)
		{
			if (Matches(tokens, TokenType.Percent, TokenType.OpenBracket, TokenType.Literal, TokenType.CloseBracket))
			{
				TokenOrExpression name = tokens[2];
				tokens.RemoveRange(0, 4);
				tokens.Insert(0, new TokenOrExpression(new MetadataReference(name.Token.Value)));
				return true;
			}

			return false;
		}

		private bool TryParseVariableReference(List<TokenOrExpression> tokens)
		{
			if (Matches(tokens, TokenType.Dollar, TokenType.OpenBracket, TokenType.Literal, TokenType.CloseBracket))
			{
				TokenOrExpression name = tokens[2];
				tokens.RemoveRange(0, 4);
				tokens.Insert(0, new TokenOrExpression(new VariableReference(name.Token.Value)));
				return true;
			}

			return false;
		}

		private bool TryParseItemListProjection(List<TokenOrExpression> tokens)
		{
			int index;
			if (Matches(tokens, TokenType.At, TokenType.OpenBracket, TokenType.Literal, TokenType.Whitespace, TokenType.Arrow, TokenType.Whitespace) &&
			    TryFindClosingBracket(tokens, out index))
			{
				TokenOrExpression name = tokens[2];
				List<TokenOrExpression> content = tokens.Splice(6, index - 6);
				content.Trim(TokenType.Whitespace);
				content.Trim(TokenType.Quotation);
				tokens.RemoveRange(0, index + 1);

				bool parsed;
				do
				{
					parsed = false;
					if (TryParseOneConcatenationContent(content, consumeItemListSeparator: false, includeMetadataReference: true))
						parsed = true;
					if (TryParseConcatenation(content, consumeItemListSeparator: false, includeMetadataReference: true))
						parsed = true;
				} while (parsed && content.Count > 1);

				if (content.Count != 1 || content[0].Expression == null)
					throw new ParseException();

				IExpression projectionRule = content[0].Expression;
				tokens.Insert(0, new TokenOrExpression(new ItemListProjection(
					                                       name.Token.Value,
					                                       projectionRule
					                                       )));
				return true;
			}

			return false;
		}

		private static bool TryFindClosingBracket(List<TokenOrExpression> tokens, out int index)
		{
			int depth = 0;
			for (int i = 0; i < tokens.Count; ++i)
			{
				Token token = tokens[i].Token;
				switch (token.Type)
				{
					case TokenType.OpenBracket:
						++depth;
						break;

					case TokenType.CloseBracket:
						if (--depth == 0)
						{
							index = i;
							return true;
						}
						break;
				}
			}

			index = -1;
			return false;
		}

		private bool TryParseItemListReference(List<TokenOrExpression> tokens)
		{
			if (Matches(tokens, TokenType.At, TokenType.OpenBracket, TokenType.Literal, TokenType.CloseBracket))
			{
				TokenOrExpression name = tokens[2];
				tokens.RemoveRange(0, 4);
				tokens.Insert(0, new TokenOrExpression(new ItemListReference(name.Token.Value)));
				return true;
			}

			return false;
		}

		private bool TryParseItemListContent(List<TokenOrExpression> tokens)
		{
			if (TryParseConcatenation(tokens, consumeItemListSeparator: false, includeMetadataReference: false))
				return true;
			if (TryParseOperatorsAsLiteral(tokens))
				return true;
			if (TryParseVariableReference(tokens))
				return true;
			if (TryParseLiteral(tokens))
				return true;
			if (TryParseItemListProjection(tokens))
				return true;
			if (TryParseItemListReference(tokens))
				return true;
			return false;
		}

		private bool TryParseItemList(List<TokenOrExpression> tokens)
		{
			if (tokens.Count >= 3 &&
			    tokens[0].Token.Type != TokenType.ItemListSeparator &&
			    tokens[1].Token.Type == TokenType.ItemListSeparator)
			{
				IExpression leftHandSide = Parse(tokens[0]);
				tokens.RemoveRange(0, 2);

				List<TokenOrExpression> rhs;
				int index;
				if (!TryFindFirst(tokens, TokenType.ItemListSeparator, out index))
				{
					// No matter, we shall consume everything
					rhs = tokens.Cut(0, tokens.Count);
				}
				else
				{
					rhs = tokens.Cut(0, index);
				}

				while (TryParseItemListContent(rhs))
				{
				}

				if (rhs.Count != 1)
					throw new ParseException();

				IExpression rightHandSide = rhs[0].Expression;
				tokens.Insert(0, new TokenOrExpression(new ItemListExpression(leftHandSide, rightHandSide)));
				return true;
			}
			return false;
		}

		private bool TryParseFunctionCall(List<TokenOrExpression> tokens)
		{
			FunctionOperation operation;
			int endIndex;
			if (tokens.Count >= 3 && IsFunctionName(tokens[0].Token, out operation) &&
			    tokens[1].Token.Type == TokenType.OpenBracket &&
			    TryFindFirst(tokens, TokenType.CloseBracket, out endIndex))
			{
				// Function call fn(...)
				// We want to retrieve the arguments from the token-stack
				// parse them and trash the fn(...) part
				List<TokenOrExpression> arguments = tokens.Splice(2, endIndex - 2);
				tokens.RemoveRange(0, endIndex + 1);

				if (!TryParseOne(arguments))
					throw new ParseException();
				if (arguments.Count != 1)
					throw new ParseException();

				// Currently all function calls have one parameter
				IExpression parameter = arguments[0].Expression;
				var expression = new FunctionExpression(operation, parameter);
				tokens.Insert(0, new TokenOrExpression(expression));
				return true;
			}

			return false;
		}

		private bool TryParseBinaryOperation(List<TokenOrExpression> tokens)
		{
			BinaryOperation operation;
			if (tokens.Count >= 3 && IsBinaryOperator(tokens[1].Token.Type, out operation))
			{
				// Binary expression
				TokenOrExpression lhs = tokens[0];
				TokenType op = tokens[1].Token.Type;
				tokens.RemoveRange(0, 2);
				TryParseLeftToRight(tokens);
				if (tokens.Count != 1)
					throw new ParseException();

				IExpression leftHandSide = Parse(lhs);
				IExpression rightHandSide = tokens[0].Expression;

				var expression = new BinaryExpression(leftHandSide, operation, rightHandSide);
				tokens.RemoveAt(0);
				tokens.Insert(0, new TokenOrExpression(expression));
				return true;
			}

			return false;
		}

		private bool TryParseUnaryOperation(List<TokenOrExpression> tokens)
		{
			UnaryOperation unaryOperation;
			if (tokens.Count >= 2 && IsUnaryOperator(tokens[0].Token.Type, out unaryOperation))
			{
				tokens.RemoveAt(0);
				if (!TryParseLeftToRight(tokens))
					throw new ParseException();
				if (tokens.Count != 1)
					throw new ParseException();

				IExpression rightHandSide = tokens[0].Expression;
				var expression = new UnaryExpression(unaryOperation, rightHandSide);
				tokens.RemoveAt(0);
				tokens.Insert(0, new TokenOrExpression(expression));
				return true;
			}

			return false;
		}

		private bool TryFindFirst(List<TokenOrExpression> tokens, TokenType type, out int index)
		{
			for (int i = 0; i < tokens.Count; ++i)
			{
				if (tokens[i].Token.Type == type)
				{
					index = i;
					return true;
				}
			}

			index = -1;
			return false;
		}

		private IExpression Parse(Token token)
		{
			switch (token.Type)
			{
				case TokenType.Literal:
					return new StringLiteral(token.Value);

				default:
					throw new ParseException(string.Format("Expected token or literal but found: {0}", token));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tokens"></param>
		/// <param name="types"></param>
		/// <returns></returns>
		private static bool Matches(List<TokenOrExpression> tokens,
									params TokenType[] types)
		{
			if (tokens.Count < types.Length)
				return false;

			// ReSharper disable LoopCanBeConvertedToQuery
			for (int i = 0; i < types.Length; ++i)
			// ReSharper restore LoopCanBeConvertedToQuery
			{
				TokenType type = types[i];
				Token token = tokens[i].Token;
				if (token.Type != type)
					return false;
			}

			return true;
		}

		#endregion
	}
}