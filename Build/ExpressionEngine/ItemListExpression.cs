using System;
using System.Collections.Generic;
using System.Linq;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;

namespace Build.ExpressionEngine
{
	public sealed class ItemListExpression
		: IExpression
	{
		private readonly IExpression[] _arguments;

		public ItemListExpression(IEnumerable<IExpression> items)
		{
			if (items == null)
				throw new ArgumentNullException("items");

			_arguments = items.ToArray();
		}

		public ItemListExpression(params IExpression[] arguments)
		{
			if (arguments == null)
				throw new ArgumentNullException("arguments");

			_arguments = arguments;
		}

		public IEnumerable<IExpression> Arguments
		{
			get { return _arguments; }
		}

		public override string ToString()
		{
			return string.Join(";", (IEnumerable<object>)_arguments);
		}

		private bool Equals(ItemListExpression other)
		{
			if (_arguments.Length != other._arguments.Length)
				return false;

			for (int i = 0; i < _arguments.Length; ++i)
			{
				if (!Equals(_arguments[i], other._arguments[i]))
					return false;
			}

			return true;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is ItemListExpression && Equals((ItemListExpression) obj);
		}

		public override int GetHashCode()
		{
			return 5;
		}

		public object Evaluate(IFileSystem fileSystem, BuildEnvironment environment)
		{
			throw new NotImplementedException();
		}

		public bool IsTrue(IFileSystem fileSystem, BuildEnvironment environment)
		{
			throw new NotImplementedException();
		}

		public string ToString(IFileSystem fileSystem, BuildEnvironment environment)
		{
			throw new NotImplementedException();
		}

		public void ToItemList(IFileSystem fileSystem, BuildEnvironment environment, List<ProjectItem> items)
		{
			foreach (var item in _arguments)
			{
				item.ToItemList(fileSystem, environment, items);
			}
		}
	}
}