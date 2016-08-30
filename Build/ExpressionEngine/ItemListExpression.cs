using System;
using System.Collections.Generic;
using System.Text;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;

namespace Build.ExpressionEngine
{
	public sealed class ItemListExpression
		: IExpression
	{
		private readonly string _itemListName;

		public ItemListExpression(string itemListName)
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
			var items = ToItemList(fileSystem, environment);
			var builder = new StringBuilder();
			for(int i = 0; i < items.Count; ++i)
			{
				builder.Append(items[i].Include);
				if (i < items.Count - 1)
					builder.Append(';');
			}
			return builder.ToString();
		}

		public List<ProjectItem> ToItemList(IFileSystem fileSystem, BuildEnvironment environment)
		{
			var items = environment.Items.GetItemsOfType(_itemListName);
			return items;
		}

		private bool Equals(ItemListExpression other)
		{
			return string.Equals(_itemListName, other._itemListName);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is ItemListExpression && Equals((ItemListExpression) obj);
		}

		public override int GetHashCode()
		{
			return _itemListName.GetHashCode();
		}
	}
}