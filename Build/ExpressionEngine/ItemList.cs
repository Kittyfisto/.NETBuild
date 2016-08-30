using System;
using System.Collections.Generic;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;

namespace Build.ExpressionEngine
{
	public sealed class ItemList
		: IExpression
	{
		private readonly string _itemListName;

		public ItemList(string itemListName)
		{
			if (itemListName == null)
				throw new ArgumentNullException("itemListName");

			_itemListName = itemListName;
		}

		public string ItemListName
		{
			get { return _itemListName; }
		}

		public override string ToString()
		{
			return string.Format("@({0})", _itemListName);
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

		public List<ProjectItem> ToItemList(IFileSystem fileSystem, BuildEnvironment environment)
		{
			throw new NotImplementedException();
		}

		private bool Equals(ItemList other)
		{
			return string.Equals(_itemListName, other._itemListName);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is ItemList && Equals((ItemList) obj);
		}

		public override int GetHashCode()
		{
			return _itemListName.GetHashCode();
		}
	}
}