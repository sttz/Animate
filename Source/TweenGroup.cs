using System;
using System.Collections;
using System.Collections.Generic;
using Sttz.Tweener.Core;
using UnityEngine;

namespace Sttz.Tweener {

/// <summary>
/// Callback that allows to configure an individual tween's options in a group.
/// </summary>
/// <param name="tween">Tween object to configure</param>
public delegate void TweenConfigurator<TTarget, TValue>(Tween<TTarget, TValue> tween)
	where TTarget : class;

/// <summary>
/// Group of tweens that share common options.
/// </summary>
/// <remarks>
/// A group can provide defaults for all tween options as well as
/// the tween target and allows to control just that group of tweens.
/// 
/// All the events of the group will be cleared once it's being recycled,
/// you normally don't have to unregister event handlers you register.
/// 
/// Every tween needs to be part of a group and every group needs
/// to be registered with the engine. When using the methods on
/// <see cref="Animate"/>, this is automatically taken care of.
/// </remarks>
public abstract class TweenGroup : TweenOptionsContainer
{
	// -------- Properties --------

	/// <summary>
	/// Default target object of the group (can be overridden by tweens).
	/// </summary>
	public abstract object DefaultTarget { get; }

	/// <summary>
	/// Type of the <see cref="DefaultTarget"/>.
	/// </summary>
	/// <remarks>
	/// This is the type of the generic parameter used when creating
	/// the group, not the actual type of the default target object.
	/// </remarks>
	public abstract Type DefaultTargetType { get; }

	// -------- Lifecycle --------

	/// <summary>
	/// Reset the group so that it can be used again.
	/// </summary>
	public virtual void Reset()
	{
		Options.Reset();

		_inUse = false;
		_engine = null;

		if (_newTweens != null) _newTweens.Clear();
		if (_updateTweens != null) _updateTweens.Clear();
		if (_fixedUpdateTweens != null) _fixedUpdateTweens.Clear();
		if (_lateUpdateTweens != null) _lateUpdateTweens.Clear();
	}

	protected override void ReturnToPool()
	{
		if (_engine.Pool != null
				&& Options.Recycle != TweenRecycle.None
				&& (Options.Recycle & TweenRecycle.Groups) > 0) {
			_engine.Pool.Return(this);
		}
	}

	// -------- Manage Tweens --------

	/// <summary>
	/// Trigger the validation of all the tweens in the group 
	/// and optionally force them to be rendered.
	/// </summary>
	/// <remarks>
	/// Usually tweens are validated during initialization in the frame
	/// after they were created. You can call this method to validate
	/// all tweens in the group right after creating them. This will make
	/// the stack trace of validation errors more useful as it will point
	/// to where the group was validated instead of to the where it was 
	/// initialized.
	/// 
	/// Tweens will first render (set their target property) when
	/// they are started, which is usually during the next frame after they
	/// were created or after their start delay. Especially when doing a From
	/// or FromTo tween, you might want the initial value to be set 
	/// immediately to avoid visual glitches. Validate the tween and 
	/// set the <paramref name="forceRender"/> parameter to true in this case.
	/// </remarks>
	/// <param name='forceRender'>
	/// Forces all tweens in the group to render (set its target property 
	/// to the initial value) after they have been validated.
	/// </param>
	public bool Validate(bool forceRender = false)
	{
		bool valid = true;

		AllTweens((tween) => {
			valid &= tween.Validate(forceRender);
			return true;
		});

		return valid;
	}

	/// <summary>
	/// Check if the group contains specific tweens.
	/// </summary>
	/// <param name='target'>
	/// Target object to look for.
	/// </param>
	/// <param name='property'>
	/// Target property to look for (set to null to look for any property
	/// on the target object).
	/// </param>
	public bool Has(object target, string property = null)
	{
		bool found = false;

		AllTweens((tween) => {
			if ((property == null || tween.Property == property)
					&& (target == null || tween.Target == target)) {
				found = true;
				return false;
			}
			return true;
		});

		return found;
	}

	/// <summary>
	/// Check if the group contains any tweens (waiting or tweening).
	/// </summary>
	public bool Has()
	{
		return ((_newTweens == null ? 0 : _newTweens.Count)
			+ (_updateTweens == null ? 0 : _updateTweens.Count)
			+ (_fixedUpdateTweens == null ? 0 : _fixedUpdateTweens.Count)
			+ (_lateUpdateTweens == null ? 0 : _lateUpdateTweens.Count)
			> 0);
	}

	/// <summary>
	/// Stop all tweens in the group (leaving them at their current value),
	/// optionally limiting the tweens to a target object and property.
	/// </summary>
	/// <param name='target'>
	/// Only stop tweens on the target object (set to null to stop all
	/// tweens on all objects).
	/// </param>
	/// <param name='property'>
	/// Only stop tweens with the target property (set to null to stop all
	/// tweens on the target object).
	/// </param>
	public void Stop(object target = null, string property = null)
	{
		AllTweens((tween) => {
			if ((property == null || tween.Property == property)
					&& (target == null || tween.Target == target)) {
				tween.Stop();
			}
			return true;
		});
	}

	/// <summary>
	/// Finish all tweens in the group (setting them to their end value),
	/// optionally limiting the tweens to a target object and property.
	/// </summary>
	/// <param name='target'>
	/// Only finish tweens on the target object (set to null to stop all
	/// tweens on all objects).
	/// </param>
	/// <param name='property'>
	/// Only finish tweens with the target property (set to null to stop all
	/// tweens on the target object).
	/// </param>
	public void Finish(object target = null, string property = null)
	{
		AllTweens((tween) => {
			if ((property == null || tween.Property == property)
					&& (target == null || tween.Target == target)) {
				tween.Finish();
			}
			return true;
		});
	}

	/// <summary>
	/// Cancel all tweens in the group (setting them to their start value),
	/// optionally limiting the tweens to a target object and property.
	/// </summary>
	/// <param name='target'>
	/// Only cancel tweens on the target object (set to null to stop all
	/// tweens on all objects).
	/// </param>
	/// <param name='property'>
	/// Only cancel tweens with the target property (set to null to stop all
	/// tweens on the target object).
	/// </param>
	public void Cancel(object target = null, string property = null)
	{
		AllTweens((tween) => {
			if ((property == null || tween.Property == property)
					&& (target == null || tween.Target == target)) {
				tween.Cancel();
			}
			return true;
		});
	}

	/// <summary>
	/// Create a coroutine that will wait until all tweens in the
	/// group have completed (the group becomes empty).
	/// </summary>
	/// <remarks>
	/// Adding new tweens to the group, while not all of its
	/// tweens have completed, will prolong the time until the
	/// coroutine returns.
	/// </remarks>
	/// <returns>
	/// A coroutine you can yield in one of your own to wait until
	/// all tweens in the group have completed.
	/// </returns>
	public Coroutine WaitForEndOfGroup()
	{
		if (_engine == null) {
			Options.Log(TweenLogLevel.Error, 
				"Group needs to be added to the engine "
				+ "before WaitForEndOfGroup() can be used.");
			return null;
		}
		return (_engine as MonoBehaviour).StartCoroutine(WaitForEndOfGroupCoroutine());
	}

	// Coroutine implementation for WaitForEndOfTween
	protected IEnumerator WaitForEndOfGroupCoroutine()
	{
		while (Has()) {
			yield return null;
		}
	}

	// -------- Fields --------

	protected List<Tween> _newTweens;
	protected List<Tween> _updateTweens;
	protected List<Tween> _fixedUpdateTweens;
	protected List<Tween> _lateUpdateTweens;

	protected bool _inUse;
	protected ITweenEngine _engine;

	// -------- Internals --------

	protected void AddInternal(Tween tween)
	{
		// Retain the tween until it's done
		tween.RetainCount++;

		// Add ourself to the scope chain
		tween.Options.ParentOptions = Options;
		// Pass engine to tween
		tween.TweenEngine = _engine;

		// Tween gets added to main group first and then
		// re-grouped in the update.
		if (_newTweens == null) _newTweens = new List<Tween>();
		_newTweens.Add(tween);

		// (Re-)register group with engine
		if (_newTweens.Count == 1) {
			_engine.RegisterGroup(this);
		}
	}

	// Regroup tween to its proper group
	protected void Regroup(Tween tween)
	{
		if ((tween.Options.TweenTiming & TweenTiming.Update) > 0) {
			if (_updateTweens == null) _updateTweens = new List<Tween>();
			_updateTweens.Add(tween);

		} else if ((tween.Options.TweenTiming & TweenTiming.LateUpdate) > 0)  {
			if (_lateUpdateTweens == null) _lateUpdateTweens = new List<Tween>();
			_lateUpdateTweens.Add(tween);

		} else {
			if (_fixedUpdateTweens == null) _fixedUpdateTweens = new List<Tween>();
			_fixedUpdateTweens.Add(tween);
		}
	}

	// Let a tween overwrite others in this group
	internal void Overwrite(Tween tween)
	{
		int i;
		Tween other;

		if (_newTweens != null) {
			for (i = 0; i < _newTweens.Count; i++) {
				other = _newTweens[i];
				if (tween.Target == other.Target 
						&& tween.Property == other.Property) {
					other.Overwrite(tween);
				}
			}
		}

		if (_updateTweens != null) {
			for (i = 0; i < _updateTweens.Count; i++) {
				other = _updateTweens[i];
				if (tween.Target == other.Target 
						&& tween.Property == other.Property) {
					other.Overwrite(tween);
				}
			}
		}

		if (_fixedUpdateTweens != null) {
			for (i = 0; i < _fixedUpdateTweens.Count; i++) {
				other = _fixedUpdateTweens[i];
				if (tween.Target == other.Target 
						&& tween.Property == other.Property) {
					other.Overwrite(tween);
				}
			}
		}

		if (_lateUpdateTweens != null) {
			for (i = 0; i < _lateUpdateTweens.Count; i++) {
				other = _lateUpdateTweens[i];
				if (tween.Target == other.Target 
						&& tween.Property == other.Property) {
					other.Overwrite(tween);
				}
			}
		}
	}

	// Run an action for all tweens in the group
	protected void AllTweens(Func<Tween, bool> action)
	{
		if (_newTweens != null && _newTweens.Count > 0) {
			foreach (var tween in _newTweens) {
				if (!action(tween)) return;
			}
		}

		if (_updateTweens != null && _updateTweens.Count > 0) {
			foreach (var tween in _updateTweens) {
				if (!action(tween)) return;
			}
		}

		if (_fixedUpdateTweens != null && _fixedUpdateTweens.Count > 0) {
			foreach (var tween in _fixedUpdateTweens) {
				if (!action(tween)) return;
			}
		}

		if (_lateUpdateTweens != null && _lateUpdateTweens.Count > 0) {
			foreach (var tween in _lateUpdateTweens) {
				if (!action(tween)) return;
			}
		}
	}

	// Update tween group
	internal bool Update(TweenTiming timing)
	{
		// Sort in new tweens
		if (_newTweens != null && _newTweens.Count > 0) {
			// Send first update in reverse order to let
			// tweens created later overwrite those created earlier
			for (int i = _newTweens.Count - 1; i >= 0; i--) {
				if (_newTweens[i].Update()) {
					Regroup(_newTweens[i]);
				}
			}
			_newTweens.Clear();
		}

		// Select tween list
		var tweens = _updateTweens;
		if (timing == TweenTiming.FixedUpdate) {
			tweens = _fixedUpdateTweens;
		} else if (timing == TweenTiming.LateUpdate) {
			tweens = _lateUpdateTweens;
		}

		if (tweens != null && tweens.Count > 0) {
			// Update tweens
			var count = tweens.Count;
			for (int i = 0; i < count; i++) {
				// Update tween
				if (!tweens[i].Update()) {
					// Release & remove from list
					tweens[i].RetainCount--;
					tweens.RemoveAt(i); i--; count--;
				}
			}
		}

		// We turn invalid if there are no tweens left
		return Has();
	}
}

/// <summary>
/// Concrete generic implementation of <see cref="TweenGroup"/>.
/// </summary>
/// <remarks>
/// You don't normally need to interact with this subtype. Refer to <see cref="TweenGroup"/>
/// or the helper methods on <see cref="Animate"/> instead.
/// </remarks>
/// <typeparam name="TTarget">Type of the target object (needs to be a reference type)</typeparam>
public class TweenGroup<TTarget> : TweenGroup where TTarget : class
{
	// -------- Properties --------

	// The default target of the group
	public override object DefaultTarget {
		get {
			return _defaultTarget;
		}
	}

	// The type of the default target
	public override Type DefaultTargetType {
		get {
			return typeof(TTarget);
		}
	}

	// -------- Lifecycle --------

	// Initialize a new or pooled instance
	public void Use(TTarget target, TweenOptions parentOptions, ITweenEngine engine)
	{
		if (_inUse) {
			throw new Exception("TweenGroup instance already in use.");
		}
		_inUse = true;

		_defaultTarget = target;
		_engine = engine;
		Options.ParentOptions = parentOptions;
	}

	public override void Reset()
	{
		base.Reset();

		_defaultTarget = null;
	}

	// -------- Manage Tweens --------

	// Add a custom tween
	public TweenGroup<TTarget> Add(Tween tween)
	{
		AddInternal(tween);

		// Set target if tween doesn't have its own
		if (tween.Target == null) {
			tween.Target = _defaultTarget;
		}

		return this;
	}

	// -------- To --------

	// Add a To tween to the group
	public TweenGroup<TTarget> To<TValue>(
		string property, TValue toValue, TweenConfigurator<TTarget, TValue> configurator = null
	) {
		var tween = _engine.Create(
			TweenMethod.To, _defaultTarget, float.NaN, property, 
			default(TValue), toValue, default(TValue)
		);
		if (configurator != null) configurator(tween);
		return Add(tween);
	}

	// Add a To tween to the group
	public TweenGroup<TTarget> To<TValue>(
		float duration, string property, TValue toValue, TweenConfigurator<TTarget, TValue> configurator = null
	) {
		var tween = _engine.Create(
			TweenMethod.To, _defaultTarget, duration, property, 
			default(TValue), toValue, default(TValue)
		);
		if (configurator != null) configurator(tween);
		return Add(tween);
	}

	// Add a To tween to the group
	public TweenGroup<TTarget> To<TValue>(
		TTarget target, float duration, string property, TValue toValue, TweenConfigurator<TTarget, TValue> configurator = null
	) {
		var tween = _engine.Create(
			TweenMethod.To, target, duration, property, 
			default(TValue), toValue, default(TValue)
		);
		if (configurator != null) configurator(tween);
		return Add(tween);
	}

	// -------- From --------

	// Add a From tween to the group
	public TweenGroup<TTarget> From<TValue>(
		string property, TValue fromValue, TweenConfigurator<TTarget, TValue> configurator = null
	) {
		var tween = _engine.Create(
			TweenMethod.From, _defaultTarget, float.NaN, property, 
			fromValue, default(TValue), default(TValue)
		);
		if (configurator != null) configurator(tween);
		return Add(tween);
	}

	// Add a From tween to the group
	public TweenGroup<TTarget> From<TValue>(
		float duration, string property, TValue fromValue, TweenConfigurator<TTarget, TValue> configurator = null
	) {
		var tween = _engine.Create(
			TweenMethod.From, _defaultTarget, duration, property, 
			fromValue, default(TValue), default(TValue)
		);
		if (configurator != null) configurator(tween);
		return Add(tween);
	}

	// Add a From tween to the group
	public TweenGroup<TTarget> From<TValue>(
		TTarget target, float duration, string property, TValue fromValue, TweenConfigurator<TTarget, TValue> configurator = null
	) {
		var tween = _engine.Create(
			TweenMethod.From, target, duration, property, 
			fromValue, default(TValue), default(TValue)
		);
		if (configurator != null) configurator(tween);
		return Add(tween);
	}

	// -------- FromTo --------

	// Add a FromTo tween to the group
	public TweenGroup<TTarget> FromTo<TValue>(
		string property, TValue fromValue, TValue toValue, TweenConfigurator<TTarget, TValue> configurator = null
	) {
		var tween = _engine.Create(
			TweenMethod.FromTo, _defaultTarget, float.NaN, property, 
			fromValue, toValue, default(TValue)
		);
		if (configurator != null) configurator(tween);
		return Add(tween);
	}

	// Add a FromTo tween to the group
	public TweenGroup<TTarget> FromTo<TValue>(
		float duration, string property, TValue fromValue, TValue toValue, TweenConfigurator<TTarget, TValue> configurator = null
	) {
		var tween = _engine.Create(
			TweenMethod.FromTo, _defaultTarget, duration, property, 
			fromValue, toValue, default(TValue)
		);
		if (configurator != null) configurator(tween);
		return Add(tween);
	}

	// Add a FromTo tween to the group
	public TweenGroup<TTarget> FromTo<TValue>(
		TTarget target, float duration, string property, TValue fromValue, TValue toValue, TweenConfigurator<TTarget, TValue> configurator = null
	) {
		var tween = _engine.Create(
			TweenMethod.FromTo, target, duration, property, 
			fromValue, toValue, default(TValue)
		);
		if (configurator != null) configurator(tween);
		return Add(tween);
	}

	// -------- By --------

	// Add a By tween to the group
	public TweenGroup<TTarget> By<TValue>(
		string property, TValue byValue, TweenConfigurator<TTarget, TValue> configurator = null
	) {
		var tween = _engine.Create(
			TweenMethod.By, _defaultTarget, float.NaN, property, 
			default(TValue), default(TValue), byValue
		);
		if (configurator != null) configurator(tween);
		return Add(tween);
	}

	// Add a By tween to the group
	public TweenGroup<TTarget> By<TValue>(
		float duration, string property, TValue byValue, TweenConfigurator<TTarget, TValue> configurator = null
	) {
		var tween = _engine.Create(
			TweenMethod.By, _defaultTarget, duration, property, 
			default(TValue), default(TValue), byValue
		);
		if (configurator != null) configurator(tween);
		return Add(tween);
	}

	// Add a By tween to the group
	public TweenGroup<TTarget> By<TValue>(
		TTarget target, float duration, string property, TValue byValue, TweenConfigurator<TTarget, TValue> configurator = null
	) {
		var tween = _engine.Create(
			TweenMethod.By, target, duration, property, 
			default(TValue), default(TValue), byValue
		);
		if (configurator != null) configurator(tween);
		return Add(tween);
	}

	// -------- Fields --------

	protected TTarget _defaultTarget;
}

}
