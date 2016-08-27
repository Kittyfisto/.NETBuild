using System;
using System.Diagnostics.Contracts;
using Build.BuildEngine;

namespace Build.ExpressionEngine
{
	public sealed class BinaryExpression
		: IExpression
	{
		public readonly IExpression LeftHandSide;
		public readonly BinaryOperation Operation;
		public readonly IExpression RightHandSide;

		public BinaryExpression(IExpression leftHandSide, BinaryOperation operation, IExpression rightHandSide)
		{
			if (leftHandSide == null)
				throw new ArgumentNullException("leftHandSide");
			if (rightHandSide == null)
				throw new ArgumentNullException("rightHandSide");

			LeftHandSide = leftHandSide;
			Operation = operation;
			RightHandSide = rightHandSide;
		}

		public override string ToString()
		{
			string op;
			switch (Operation)
			{
				case BinaryOperation.And:
					op = "And";
					break;

				case BinaryOperation.Or:
					op = "Or";
					break;

				case BinaryOperation.Equals:
					op = "==";
					break;

				case BinaryOperation.EqualsNot:
					op = "!=";
					break;

				default:
					op = null;
					break;
			}

			return string.Format("{0} {1} {2}",
			                     LeftHandSide,
			                     op,
			                     RightHandSide);
		}

		[Pure]
		public object Evaluate(BuildEnvironment environment)
		{
			object lhs = LeftHandSide.Evaluate(environment);
			object rhs = RightHandSide.Evaluate(environment);

			switch (Operation)
			{
				case BinaryOperation.Equals:
					return Equals(lhs, rhs);

				case BinaryOperation.EqualsNot:
					return !Equals(lhs, rhs);

				default:
					throw new NotImplementedException();
			}
		}

		private bool Equals(BinaryExpression other)
		{
			return LeftHandSide.Equals(other.LeftHandSide) && Operation == other.Operation &&
			       RightHandSide.Equals(other.RightHandSide);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is BinaryExpression && Equals((BinaryExpression) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = LeftHandSide.GetHashCode();
				hashCode = (hashCode*397) ^ (int) Operation;
				hashCode = (hashCode*397) ^ RightHandSide.GetHashCode();
				return hashCode;
			}
		}
	}
}