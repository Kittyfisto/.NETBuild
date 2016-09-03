using System;
using System.Collections.Generic;
using System.ComponentModel;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;

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

		public object Evaluate(IFileSystem fileSystem, BuildEnvironment environment)
		{
			var value = Parameter.ToString(fileSystem, environment);
			switch (Operation)
			{
				case FunctionOperation.Exists:
					return Exists(fileSystem, environment, value);

				case FunctionOperation.HasTrailingSlash:
					var path = value;
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

		private bool Exists(IFileSystem fileSystem, BuildEnvironment environment, string value)
		{
			var fileName = value;
			if (fileName == null)
				return false;

			if (!Path.IsPathRooted(fileName))
			{
				var root = environment.Properties[Properties.MSBuildProjectDirectory];
				fileName = Path.Combine(root, fileName);
			}

			return fileSystem.Exists(fileName);
		}

		public string ToString(IFileSystem fileSystem, BuildEnvironment environment)
		{
			throw new NotImplementedException();
		}

		public void ToItemList(IFileSystem fileSystem, BuildEnvironment environment, List<ProjectItem> items)
		{
			throw new NotImplementedException();
		}
	}
}