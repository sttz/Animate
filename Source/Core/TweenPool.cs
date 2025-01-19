using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sttz.Tweener.Core {

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

#if UNITY_2023_1_OR_NEWER
	// Pooled AwaitableCompletionSource instances
	protected Queue<AwaitableCompletionSource> _completionSources;
#endif

	// -------- Pooling --------

	protected struct TypePair : IEquatable<TypePair>
	{
		public Type targetType;
		public Type valueType;

		public TypePair(Type targetType, Type valueType)
		{
			this.targetType = targetType;
			this.valueType = valueType;
		}

		public override bool Equals(object obj)
		{
			if (obj is TypePair other) {
				return Equals(other);
			}
			return false;
		}

		public bool Equals(TypePair other)
		{
			return (
				other.targetType == targetType
				&& other.valueType == valueType
			);
		}

		public override int GetHashCode()
		{
			return (
				targetType.GetHashCode()
				^ valueType.GetHashCode()
			);
		}
	}

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

#if UNITY_2023_1_OR_NEWER
	// Get a completion source from the pool, create a new one if necessary
	public AwaitableCompletionSource GetAwaitableCompletionSource()
	{
		if (_completionSources == null || _completionSources.Count == 0) {
			return new AwaitableCompletionSource();
		}

		return _completionSources.Dequeue();
	}

	// Return a completion source
	public void Return(AwaitableCompletionSource source)
	{
		source.Reset();
		_completionSources ??= new();
		_completionSources.Enqueue(source);
	}
#endif
}

}
