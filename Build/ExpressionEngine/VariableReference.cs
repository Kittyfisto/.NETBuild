using System;
using System.Collections.Generic;
using Build.BuildEngine;
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

		public bool IsTrue(IFileSystem fileSystem, BuildEnvironment environment)
		{
			var value = ToString(fileSystem, environment);
			return string.Equals(value, "true");
		}

		public string ToString(IFileSystem fileSystem, BuildEnvironment environment)
		{
			return environment.Properties[Name];
		}

		public void ToItemList(IFileSystem fileSystem, BuildEnvironment environment, List<ProjectItem> items)
		{
			var fileNames = ToString(fileSystem, environment)
				.Split(new[] {Tokenizer.ItemListSeparator}, StringSplitOptions.RemoveEmptyEntries);
			items.Capacity += fileNames.Length;
			foreach (var fileName in fileNames)
			{
				var item = environment.GetOrCreate(fileSystem, fileName);
				items.Add(item);
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