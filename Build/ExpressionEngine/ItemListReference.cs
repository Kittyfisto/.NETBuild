using System;
using System.Collections.Generic;
using System.Text;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;

namespace Build.ExpressionEngine
{
	public sealed class ItemListReference
		: IExpression
	{
		private readonly string _itemListName;

		public ItemListReference(string itemListName)
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
			return ToString(fileSystem, environment);
		}

		public string ToString(IFileSystem fileSystem, BuildEnvironment environment)
		{
			var items = new List<ProjectItem>();
			ToItemList(fileSystem, environment, items);
			var builder = new StringBuilder();
			for (int i = 0; i < items.Count; ++i)
			{
				var item = items[i];
				var fullpath = item[Metadatas.FullPath];
				builder.Append(fullpath);
				if (i < items.Count - 1)
					builder.Append(Tokenizer.ItemListSeparator);
			}
			return builder.ToString();
		}

		public void ToItemList(IFileSystem fileSystem, BuildEnvironment environment, List<ProjectItem> items)
		{
			items.AddRange(environment.Items.GetItemsOfType(_itemListName));
		}

		private bool Equals(ItemListReference other)
		{
			return string.Equals(_itemListName, other._itemListName);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is ItemListReference && Equals((ItemListReference) obj);
		}

		public override int GetHashCode()
		{
			return _itemListName.GetHashCode();
		}
	}
}