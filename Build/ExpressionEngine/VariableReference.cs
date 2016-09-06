using System;
using System.Collections.Generic;
using Build.DomainModel.MSBuild;

namespace Build.ExpressionEngine
{
	public sealed class VariableReference
		: IExpression
	{
		public readonly string Name;

		public VariableReference(string name)
		{
			Name = name;
		}

		public override string ToString()
		{
			return string.Format("$({0})", Name);
		}

		public object Evaluate(IFileSystem fileSystem, BuildEnvironment environment)
		{
			return ToString(fileSystem, environment);
		}

		public string ToString(IFileSystem fileSystem, BuildEnvironment environment)
		{
			return environment.Properties[Name];
		}

		public string ToString(IFileSystem fileSystem, BuildEnvironment environment, ProjectItem item)
		{
			return ToString(fileSystem, environment);
		}

		public void ToItemList(IFileSystem fileSystem, BuildEnvironment environment, List<ProjectItem> items)
		{
			var fileNames = ToString(fileSystem, environment)
				.Split(new[] {Tokenizer.ItemListSeparator}, StringSplitOptions.RemoveEmptyEntries);

			foreach (var fileName in fileNames)
			{
				var cleaned = fileName.Trim();
				if (!string.IsNullOrEmpty(cleaned))
				{
					var item = fileSystem.CreateProjectItem("None", cleaned, null, environment);
					items.Add(item);
				}
			}
		}

		private bool Equals(VariableReference other)
		{
			return string.Equals(Name, other.Name);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is VariableReference && Equals((VariableReference) obj);
		}

		public override int GetHashCode()
		{
			return (Name != null ? Name.GetHashCode() : 0);
		}
	}
}