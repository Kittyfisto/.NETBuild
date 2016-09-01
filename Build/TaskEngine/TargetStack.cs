using System;
using System.Collections.Generic;
using Build.DomainModel.MSBuild;

namespace Build.TaskEngine
{
	public sealed class TargetStack
	{
		private readonly Stack<Target> _order;
		private readonly HashSet<Target> _targets;

		public TargetStack()
		{
			_targets = new HashSet<Target>();
			_order = new Stack<Target>();
		}

		public int Count
		{
			get { return _order.Count; }
		}

		public void Push(Target target)
		{
			if (!_targets.Add(target))
				throw new ArgumentException();

			_order.Push(target);
		}

		public bool TryPush(Target target)
		{
			if (_targets.Add(target))
			{
				_order.Push(target);
				return true;
			}

			return false;
		}

		public Target Pop()
		{
			Target target = _order.Pop();
			_targets.Remove(target);
			return target;
		}

		public Target Peek()
		{
			return _order.Peek();
		}
	}
}