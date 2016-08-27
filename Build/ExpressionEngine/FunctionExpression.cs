﻿using System;
using System.ComponentModel;
using System.IO;
using Build.BuildEngine;

namespace Build.ExpressionEngine
{
	public sealed class FunctionExpression
		: IExpression
	{
		public readonly FunctionOperation Operation;
		public readonly IExpression Parameter;

		public FunctionExpression(FunctionOperation operation, IExpression parameter)
		{
			if (parameter == null)
				throw new ArgumentNullException("parameter");

			Operation = operation;
			Parameter = parameter;
		}

		public override string ToString()
		{
			switch (Operation)
			{
				case FunctionOperation.HasTrailingSlash:
					return string.Format("HasTrailingSlash({0})", Parameter);

				case FunctionOperation.Exists:
					return string.Format("Exists({0})", Parameter);

				default:
					return string.Empty;
			}
		}

		private bool Equals(FunctionExpression other)
		{
			return Operation == other.Operation && Parameter.Equals(other.Parameter);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is FunctionExpression && Equals((FunctionExpression) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((int) Operation*397) ^ Parameter.GetHashCode();
			}
		}

		public object Evaluate(BuildEnvironment environment)
		{
			var value = Parameter.Evaluate(environment);
			switch (Operation)
			{
				case FunctionOperation.Exists:
					var fileName = value as string;
					if (fileName == null)
						return false;

					return File.Exists(fileName);

				case FunctionOperation.HasTrailingSlash:
					var path = value as string;
					if (path == null)
						return false;

					if (path.EndsWith("\\"))
						return true;

					if (path.EndsWith("/"))
						return true;

					return false;

				default:
					throw new InvalidEnumArgumentException("Operation", (int)Operation, typeof(FunctionOperation));
			}
		}
	}
}