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
			return IsTrue(fileSystem, environment);
		}

		public bool IsTrue(IFileSystem fileSystem, BuildEnvironment environment)
		{
			object lhs = LeftHandSide.Evaluate(fileSystem, environment);
			object rhs = RightHandSide.Evaluate(fileSystem, environment);

			switch (Operation)
			{
				case BinaryOperation.Equals:
					return Equals(lhs, rhs);

				case BinaryOperation.EqualsNot:
					return !Equals(lhs, rhs);

				case BinaryOperation.And:
					if (!Expression.IsTrue(lhs))
						return false;
					if (!Expression.IsTrue(rhs))
						return false;
					return true;

				case BinaryOperation.Or:
					if (Expression.IsTrue(lhs))
						return true;
					if (Expression.IsTrue(rhs))
						return true;
					return false;

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