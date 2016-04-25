using System;
using System.Collections.Generic;

namespace Sttz.Tweener.Core {

	using TypePair = KeyValuePair<Type, Type>;

	// Tween pool interface
	public interface ITweenPool
	{
		// Get a tween instance from the pool
		Tween<TTarget, TValue> GetTween<TTarget, TValue>()
			where TTarget : class;
		// Return a tween instance to the pool
		// The instance will be reset before adding it
		void Return(ITween tween);

		// Get a group instance from the pool
		TweenGroup<TTarget> GetGroup<TTarget>()
			where TTarget : class;
		// Return a group to the pool
		// The instance will be reset before adding it
		void Return(ITweenGroup tweenGroup);
	};

	/// <summary>
	/// Tween pool.
	/// </summary>
	public class TweenPool : ITweenPool
	{
		///////////////////
		// Fields

		// Pooled TweenGroup instances
		protected Dictionary<Type, Queue<ITweenGroup>> _groups
			= new Dictionary<Type, Queue<ITweenGroup>>();
		// Pooled Tween<T> instances by type
		protected Dictionary<TypePair, Queue<ITween>> _tweens
			= new Dictionary<TypePair, Queue<ITween>>();

		///////////////////
		// Pooling

		// Get a tween from the pool, create a new once if necessary
		public Tween<TTarget, TValue> GetTween<TTarget, TValue>()
			where TTarget : class
		{
			var key = new TypePair(typeof(TTarget), typeof(TValue));

			Queue<ITween> queue;
			if (!_tweens.TryGetValue(key, out queue) || queue.Count == 0) {
				return new Tween<TTarget, TValue>();
			}

			return (Tween<TTarget, TValue>)queue.Dequeue();
		}

		// Return a tween
		public void Return(ITween tween)
		{
			var key = new TypePair(tween.TargetType, tween.ValueType);

			Queue<ITween> queue;
			if (!_tweens.TryGetValue(key, out queue)) {
				_tweens[key] = queue = new Queue<ITween>();
			}

			tween.Internal.Reset();

			queue.Enqueue(tween);
		}

		// Get a group from the pool, create a new once if necessary
		public TweenGroup<TTarget> GetGroup<TTarget>()
			where TTarget : class
		{
			Queue<ITweenGroup> queue;
			if (!_groups.TryGetValue(typeof(TTarget), out queue) || queue.Count == 0) {
				return new TweenGroup<TTarget>();
			}

			return (TweenGroup<TTarget>)queue.Dequeue();
		}

		// Return a tween
		public void Return(ITweenGroup tweenGroup)
		{
			var key = tweenGroup.DefaultTargetType;

			Queue<ITweenGroup> queue;
			if (!_groups.TryGetValue(key, out queue)) {
				_groups[key] = queue = new Queue<ITweenGroup>();
			}

			tweenGroup.Internal.Reset();

			queue.Enqueue(tweenGroup);
		}
	}
}
