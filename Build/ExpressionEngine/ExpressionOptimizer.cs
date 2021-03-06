﻿using System.Collections.Generic;
using System.Linq;

namespace Build.ExpressionEngine
{
	public static class ExpressionOptimizer
	{
		/// <summary>
		///     Optimizes the given expression tree as far as possible.
		/// </summary>
		/// <remarks>
		///     The returned expression is either the given expression, if no
		///     further optimization could be performed, or a semantically identical expression
		///     that contains less nodes (and is usually flatter) than the input expression.
		/// </remarks>
		/// <remarks>
		///     Currently, recursive concatenations and item lists are flattened as far as possible.
		/// </remarks>
		/// <param name="expression"></param>
		/// <returns></returns>
		public static IExpression Run(IExpression expression)
		{
			bool optimizedThisRound;
			do
			{
				optimizedThisRound = false;
				IExpression optimized;
				var itemList = expression as ItemListExpression;
				if (itemList != null && OptimizeItemList(itemList, out optimized))
				{
					expression = optimized;
					optimizedThisRound = true;
				}
				else
				{
					var concatenation = expression as ConcatExpression;
					if (concatenation != null && OptimizeConcatenation(concatenation, out optimized))
					{
						expression = optimized;
						optimizedThisRound = true;
					}
					else
					{
						var binary = expression as BinaryExpression;
						if (binary != null && OptimizeBinaryOperation(binary, out optimized))
						{
							expression = optimized;
							optimizedThisRound = true;
						}
						else
						{
							var fn = expression as FunctionExpression;
							if (fn != null && OptimizeFunctionCall(fn, out optimized))
							{
								expression = optimized;
								optimizedThisRound = true;
							}
						}
					}
				}
			} while (optimizedThisRound);

			return expression;
		}

		private static bool OptimizeFunctionCall(FunctionExpression expression, out IExpression optimized)
		{
			optimized = null;
			var argument = Run(expression.Parameter);
			if (ReferenceEquals(argument, expression.Parameter))
				return false;

			optimized = new FunctionExpression(expression.Operation, argument);
			return true;
		}

		private static bool OptimizeConcatenation(ConcatExpression expression, out IExpression optimized)
		{
			optimized = null;

			// In the first step we optimize each argument individually so that we optimize
			// the entire tree.
			// This includes flattening child ConcatExpressions.
			var arguments = new List<IExpression>();
			bool optimizedAnything = false;
			foreach (IExpression item in expression.Arguments)
			{
				IExpression optimizedItem = Run(item);
				if (!ReferenceEquals(optimizedItem, item))
					optimizedAnything = true;

				var childConcatenation = optimizedItem as ConcatExpression;
				if (childConcatenation != null)
				{
					arguments.AddRange(childConcatenation.Arguments);
					optimizedAnything = true;
				}
				else
				{
					arguments.Add(optimizedItem);
				}
			}

			// After this step we may end up with two adjacent StringLiteral tokens.
			// These should be joined as well.
			for (int i = 0; i < arguments.Count - 1; )
			{
				var lhs = arguments[i] as StringLiteral;
				var rhs = arguments[i + 1] as StringLiteral;

				if (lhs != null && rhs != null)
				{
					arguments[i] = new StringLiteral(lhs.Value + rhs.Value);
					arguments.RemoveAt(i + 1);
					optimizedAnything = true;
				}
				else
				{
					++i;
				}
			}

			// It's possible that we ended up with a completely empty concatenation
			// in which case an empty string literal yields an identical result.
			if (arguments.Count == 0)
			{
				optimized = StringLiteral.Empty;
				return true;
			}

			// It's also possible we optimize the entire expression down to one child
			// in which case we don't need the concatenation expression anymore.
			if (arguments.Count == 1)
			{
				optimized = arguments[0];
				return true;
			}

			// If none of these steps achieved anything, then we can return early
			if (!optimizedAnything)
				return false;

			optimized = new ConcatExpression(arguments);
			return true;
		}

		private static bool OptimizeItemList(ItemListExpression expression, out IExpression optimized)
		{
			optimized = null;

			var arguments = new List<IExpression>();
			bool optimizedAnything = false;
			foreach (IExpression argument in expression.Arguments)
			{
				IExpression optimizedItem = Run(argument);
				if (!ReferenceEquals(optimizedItem, argument))
					optimizedAnything = true;

				var childList = optimizedItem as ItemListExpression;
				if (childList != null)
				{
					arguments.AddRange(childList.Arguments);
					optimizedAnything = true;
				}
				else
				{
					arguments.Add(optimizedItem);
				}
			}

			if (arguments.Count == 1)
			{
				optimized = arguments[0];
				return true;
			}

			if (!optimizedAnything)
				return false;

			optimized = new ItemListExpression(arguments);
			return true;
		}

		private static bool OptimizeBinaryOperation(BinaryExpression expression, out IExpression optimized)
		{
			optimized = null;

			var leftHandSide = Run(expression.LeftHandSide);
			var rightHandSide = Run(expression.RightHandSide);

			if (ReferenceEquals(leftHandSide, expression.LeftHandSide) &&
			    ReferenceEquals(rightHandSide, expression.RightHandSide))
				return false;

			optimized = new BinaryExpression(leftHandSide, expression.Operation, rightHandSide);
			return true;
		}

	}
}