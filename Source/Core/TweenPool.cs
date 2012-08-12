using System;
using System.Collections.Generic;

namespace Sttz.Tweener.Core {

	// Tween pool interface
	public interface ITweenPool
	{
		// Get a tween instance from the pool
		Tween<TValue> GetTween<TValue>();
		// Return a tween instance to the pool
		// The instance will be reset before adding it
		void Return(ITween tween);

		// Get a group instance from the pool
		TweenGroup GetGroup();
		// Return a group to the pool
		// The instance will be reset before adding it
		void Return(TweenGroup tweenGroup);
	};

	/// <summary>
	/// Tween pool.
	/// </summary>
	public class TweenPool : ITweenPool
	{
		///////////////////
		// Fields

		// Pooled TweenGroup instances
		protected Queue<TweenGroup> _groups
			= new Queue<TweenGroup>();
		// Pooled Tween<T> instances by type
		protected Dictionary<Type, Queue<ITween>> _tweens
			= new Dictionary<Type, Queue<ITween>>();

		///////////////////
		// Pooling

		// Get a tween from the pool, create a new once if necessary
		public Tween<TValue> GetTween<TValue>()
		{
			if (!_tweens.ContainsKey(typeof(TValue)) 
					|| _tweens[typeof(TValue)].Count == 0) {
				return new Tween<TValue>();
			} else {
				return _tweens[typeof(TValue)].Dequeue() as Tween<TValue>;
			}
		}

		// Return a tween
		public void Return(ITween tween)
		{
			if (!_tweens.ContainsKey(tween.ValueType)) {
				_tweens[tween.ValueType] = new Queue<ITween>();
			}

			tween.Internal.Reset();

			_tweens[tween.ValueType].Enqueue(tween);
		}

		// Get a group from the pool, create a new once if necessary
		public TweenGroup GetGroup()
		{
			if (_groups.Count == 0) {
				return new TweenGroup();
			} else {
				return _groups.Dequeue();;
			}
		}

		// Return a tween
		public void Return(TweenGroup tweenGroup)
		{
			tweenGroup.Reset();
			_groups.Enqueue(tweenGroup);
		}
	}
}
