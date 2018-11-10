using System;
using System.Collections.Generic;

namespace Sttz.Tweener.Core {

using TypePair = KeyValuePair<Type, Type>;

/// <summary>
/// Tween pool.
/// </summary>
public class TweenPool
{
	// -------- Fields --------

	// Pooled TweenGroup instances
	protected Dictionary<Type, Queue<TweenGroup>> _groups
		= new Dictionary<Type, Queue<TweenGroup>>();
	// Pooled Tween<T> instances by type
	protected Dictionary<TypePair, Queue<Tween>> _tweens
		= new Dictionary<TypePair, Queue<Tween>>();

	// -------- Pooling --------

	// Get a tween from the pool, create a new once if necessary
	public Tween<TTarget, TValue> GetTween<TTarget, TValue>()
		where TTarget : class
	{
		var key = new TypePair(typeof(TTarget), typeof(TValue));

		Queue<Tween> queue;
		if (!_tweens.TryGetValue(key, out queue) || queue.Count == 0) {
			return new Tween<TTarget, TValue>();
		}

		return (Tween<TTarget, TValue>)queue.Dequeue();
	}

	// Return a tween
	public void Return(Tween tween)
	{
		var key = new TypePair(tween.TargetType, tween.ValueType);

		Queue<Tween> queue;
		if (!_tweens.TryGetValue(key, out queue)) {
			_tweens[key] = queue = new Queue<Tween>();
		}

		tween.Reset();

		queue.Enqueue(tween);
	}

	// Get a group from the pool, create a new once if necessary
	public TweenGroup<TTarget> GetGroup<TTarget>()
		where TTarget : class
	{
		Queue<TweenGroup> queue;
		if (!_groups.TryGetValue(typeof(TTarget), out queue) || queue.Count == 0) {
			return new TweenGroup<TTarget>();
		}

		return (TweenGroup<TTarget>)queue.Dequeue();
	}

	// Return a tween
	public void Return(TweenGroup tweenGroup)
	{
		var key = tweenGroup.DefaultTargetType;

		Queue<TweenGroup> queue;
		if (!_groups.TryGetValue(key, out queue)) {
			_groups[key] = queue = new Queue<TweenGroup>();
		}

		tweenGroup.Reset();

		queue.Enqueue(tweenGroup);
	}
}

}
