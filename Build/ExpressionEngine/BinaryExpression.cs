using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;

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
					op = "AND";
					break;

				case BinaryOperation.Or:
					op = "OR";
					break;

				case BinaryOperation.Equals:
					op = "==";
					break;

				case BinaryOperation.EqualsNot:
					op = "!=";
					break;

				case BinaryOperation.GreaterThan:
					op = ">";
					break;

				case BinaryOperation.GreaterOrEquals:
					op = ">=";
					break;

				case BinaryOperation.LessThan:
					op = "<";
					break;

				case BinaryOperation.LessOrEquals:
					op = "<=";
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
		public object Evaluate(IFileSystem fileSystem, BuildEnvironment environment)
		{
			object leftValue = LeftHandSide.Evaluate(fileSystem, environment);
			object rightValue = RightHandSide.Evaluate(fileSystem, environment);

			switch (Operation)
			{
				case BinaryOperation.Equals:
					return Equals(leftValue, rightValue);

				case BinaryOperation.EqualsNot:
					return !Equals(leftValue, rightValue);

				case BinaryOperation.And:
					return Expression.CastToBoolean(LeftHandSide, leftValue) &&
					       Expression.CastToBoolean(RightHandSide, rightValue);

				case BinaryOperation.Or:
					return Expression.CastToBoolean(LeftHandSide, leftValue) ||
					       Expression.CastToBoolean(RightHandSide, rightValue);

				case BinaryOperation.GreaterThan:
					return Expression.CastToNumber(LeftHandSide, leftValue) >
						   Expression.CastToNumber(RightHandSide, rightValue);

				case BinaryOperation.GreaterOrEquals:
					return Expression.CastToNumber(LeftHandSide, leftValue) >=
						   Expression.CastToNumber(RightHandSide, rightValue);

				case BinaryOperation.LessThan:
					return Expression.CastToNumber(LeftHandSide, leftValue) <
						   Expression.CastToNumber(RightHandSide, rightValue);

				case BinaryOperation.LessOrEquals:
					return Expression.CastToNumber(LeftHandSide, leftValue) <=
						   Expression.CastToNumber(RightHandSide, rightValue);

				default:
					throw new NotImplementedException();
			}
		}

		public string ToString(IFileSystem fileSystem, BuildEnvironment environment)
		{
			throw new NotImplementedException();
		}

		public void ToItemList(IFileSystem fileSystem, BuildEnvironment environment, List<ProjectItem> items)
		{
			throw new NotImplementedException();
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