using System;
using System.Collections.Generic;
using System.Text;
using Build.DomainModel.MSBuild;

namespace Build.ExpressionEngine
{
	public sealed class ItemListProjection
		: IExpression
	{
		private readonly IExpression[] _projectedFilename;
		private readonly string _itemListName;

		public ItemListProjection(string itemListName,
		                          params IExpression[] projectedFilename)
		{
			if (itemListName == null)
				throw new ArgumentNullException("itemListName");
			if (projectedFilename == null)
				throw new ArgumentNullException("projectedFilename");

			_itemListName = itemListName;
			_projectedFilename = projectedFilename;
		}

		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.AppendFormat("@({0} -> '", _itemListName);
			foreach (var expression in _projectedFilename)
			{
				builder.Append(expression);
			}
			builder.Append("')");

			return builder.ToString();
		}

		public object Evaluate(IFileSystem fileSystem, BuildEnvironment environment)
		{
			throw new NotImplementedException();
		}

		public string ToString(IFileSystem fileSystem, BuildEnvironment environment)
		{
			throw new NotImplementedException();
		}

		public string ToString(IFileSystem fileSystem, BuildEnvironment environment, ProjectItem item)
		{
			var builder = new StringBuilder();
			foreach (var expression in _projectedFilename)
			{
				var value = expression.ToString(fileSystem, environment, item);
				builder.Append(value);
			}
			return builder.ToString();
		}

		private string Identity
		{
			get
			{
				var builder = new StringBuilder();
				foreach (var expression in _projectedFilename)
				{
					var value = expression.ToString();
					builder.Append(value);
				}
				return builder.ToString();
			}
		}

		public void ToItemList(IFileSystem fileSystem, BuildEnvironment environment, List<ProjectItem> items)
		{
			var identity = Identity;
			var originalItems = environment.Items.GetItemsOfType(_itemListName);
			items.Capacity += originalItems.Count;

// ReSharper disable LoopCanBeConvertedToQuery
			foreach (var originalItem in originalItems)
// ReSharper restore LoopCanBeConvertedToQuery
			{
				var include = ToString(fileSystem, environment, originalItem);
				var item = fileSystem.CreateProjectItem(originalItem.Type,
				                                        include,
				                                        identity,
				                                        environment);
				items.Add(item);
			}
		}

		private bool Equals(ItemListProjection other)
		{
			if (!Equals(_itemListName, other._itemListName))
				return false;

			if (_projectedFilename.Length != other._projectedFilename.Length)
				return false;

// ReSharper disable LoopCanBeConvertedToQuery
			for (int i = 0; i < _projectedFilename.Length; ++i )
// ReSharper restore LoopCanBeConvertedToQuery
			{
				if (!Equals(_projectedFilename[i], other._projectedFilename[i]))
					return false;
			}

			return true;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is ItemListProjection && Equals((ItemListProjection) obj);
		}

		public override int GetHashCode()
		{
			return 42;
		}
	}
}