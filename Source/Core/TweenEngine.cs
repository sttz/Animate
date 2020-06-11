//
// Reflection in Animate is disabled by default.
// Instead of uncommenting the line below, add the define to
// the Scripting Define Symbols in Unity's Player Settings.
//#define ANIMATE_REFLECTION
//

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sttz.Tweener.Core {

/// <summary>
/// The tween engine drives the tweens and interfaces with the host environment.
/// </summary>
/// <remarks>
/// <see cref="Animate"/> is the main entry point but contains mostly
/// convenience methods. The heavy lifting is done in the tween engine,
/// which is responsible for managing the groups and calling the update
/// methods.
/// 
/// To configure the engine, it's possible to overwrite the factory method
/// at <see cref="Animate.EngineFactory"/> to use a different configuration
/// or to use a different engine altogether.
/// </remarks>
/// <seealso cref="UnityTweenEngine"/>
public interface ITweenEngine
{
	/// <summary>
	/// The global options instance.
	/// </summary>
	/// <seealso cref="Animate.Options"/>
	TweenOptions Options { get; }
	/// <summary>
	/// The pool used for pooling tweens and groups.
	/// </summary>
	/// <remarks>
	/// This property can be set to `null` to completely disable pooling.
	/// </remarks>
	TweenPool Pool { get; set; }

	/// <summary>
	/// The group containing the tweens started by the
	/// <see cref="Animate.To"/>, <see cref="Animate.From"/>, 
	/// <see cref="Animate.FromTo"/> and <see cref="Animate.By"/> methods.
	/// </summary>
	/// <remarks>
	/// All tweens need to be part of a group. In use cases where no group is
	/// explicitly set, this singles group is used.
	/// </remarks>
	TweenGroup<object> SinglesGroup { get; }
	/// <summary>
	/// Register a new group with the engine.
	/// </summary>
	/// <remarks>
	/// The engine retains the group until all its tweens have completed.
	/// </remarks>
	void RegisterGroup(TweenGroup tweenGroup);

	/// <summary>
	/// Generic tween creation method.
	/// </summary>
	/// <remarks>
	/// This intended for internal use, use the various methods on
	/// <see cref="Animate"/> or <see cref="TweenGroup"/> instead.
	/// </remarks>
	Tween<TTarget, TValue> Create<TTarget, TValue>(
		TweenMethod tweenMethod,
		TTarget target, 
		float duration,
		string property, 
		TValue startValue,
		TValue endValue,
		TValue diffValue,
		TweenOptions parentOptions = null
	) where TTarget : class;

	/// <summary>
	/// Internal method called by <see cref="Tween"/> to load non-static plugins.
	/// </summary>
	void LoadDynamicPlugins<TTarget, TValue>(Tween<TTarget, TValue> tween) where TTarget : class;

	/// <seealso cref="Animate.Has"/>
	bool Has(object target, string property = null);
	/// <seealso cref="Animate.Stop"/>
	void Stop(object target, string property = null);
	/// <seealso cref="Animate.Finish"/>
	void Finish(object target, string property = null);
	/// <seealso cref="Animate.Cancel"/>
	void Cancel(object target, string property = null);

	/// <summary>
	/// Internal method called by <see cref="Tween"/> to do overwriting of tweens.
	/// </summary>
	void Overwrite(Tween tween);
}

/// <summary>
/// Tween engine.
/// </summary>
public class UnityTweenEngine : MonoBehaviour, ITweenEngine
{
	// -------- Properties --------

	public TweenOptions Options {
		get {
			return _options;
		}
	}

	public TweenPool Pool { get; set; }

	public TweenGroup<object> SinglesGroup {
		get {
			if (_singlesGroup == null) {
				_singlesGroup = new TweenGroup<object>();
				_singlesGroup.Use(null, Options, this);
				_singlesGroup.Options.Recycle = TweenRecycle.Tweens;
			}
			return _singlesGroup;
		}
	}

	// -------- Methods --------

	/// <summary>
	/// Create the UnityTweenEngine.
	/// </summary>
	public static UnityTweenEngine Create()
	{
		var go = new GameObject("Animate");
		DontDestroyOnLoad(go);
		return go.AddComponent<UnityTweenEngine>();
	}

	public Tween<TTarget, TValue> Create<TTarget, TValue>(
		TweenMethod tweenMethod,
		TTarget target, 
		float duration,
		string property, 
		TValue startValue,
		TValue endValue,
		TValue diffValue,
		TweenOptions parentOptions = null
	)
		where TTarget : class 
	{
		// Basic sanity checks
		if (target == null) {
			Options.Log(
				TweenLogLevel.Error, 
				"Trying to tween {0} on a null object.".LazyFormat(property)
			);
			return null;
		}
		if (property == null) {
			Options.Log(
				TweenLogLevel.Error, 
				"Property to tween on object {0} is null.".LazyFormat(target)
			);
			return null;
		}

		// Get instance from pool or create new one
		Tween<TTarget, TValue> tween = null;
		if (Pool != null) {
			tween = Pool.GetTween<TTarget, TValue>();
		} else {
			tween = new Tween<TTarget, TValue>();
		}

		// Setup instance
		tween.Use(
			tweenMethod, target, duration, property, 
			startValue, endValue, diffValue, 
			parentOptions
		);
		return tween;
	}

	public void LoadDynamicPlugins<TTarget, TValue>(Tween<TTarget, TValue> tween)
		where TTarget : class
	{
		// Dynamic plugins are ones that use generic plugin implementations.
		// With AOT/IL2CPP, the compiler needs to figure out at compile time,
		// which closed generic types it needs to generate. If we try to use
		// a type for which no implementation was generated, we'll get an error
		// at runtime.
		// 
		// If there are too many indirections, IL2CPP isn't able to figure
		// out from an `Animate.To` call that the corresponding `TweenCodegen*`
		// or `TweenReflection*` implementations needs to be generated.
		// 
		// Therefore, the plugins are loaded here and a direct generic method
		// call is made form `Tween.LoadPlugins`. This way, IL2CPP can figure
		// out which `TweenReflection*` classes it needs to generate.
		// 
		// Though, it's not clear to me what the rules are regarding to what
		// IL2CPP is able to figure out and what not. This call is made
		// through the `ITweenEngine` interface but IL2CPP is somehow able 
		// to figure out it needs to look at `UnityTweenEngine`.

		// The order matters here, plugins loaded later
		// can override plugins loaded earlier.

		#if ANIMATE_REFLECTION
			#if !ENABLE_IL2CPP && !NET_STANDARD_2_0
				// Codegen doesn't work with AOT (IL2CPP) or .Net Standard
				TweenCodegenAccessorPlugin.Load(tween, false);
				TweenCodegenArithmeticPlugin.Load(tween, false);
			#else
				TweenReflectionAccessorPlugin.Load(tween, false);
				TweenReflectionArithmeticPlugin.Load(tween, false);
			#endif
		#endif
	}

	public void RegisterGroup(TweenGroup tweenGroup)
	{
		tweenGroup.RetainCount++;
		_newGroups.Add(tweenGroup);
	}

	public bool Has(object target, string property)
	{
		if (target == null) {
			Options.Log(
				TweenLogLevel.Warning, 
				"Animate.Has() called with null target."
			);
			return false;
		}

		foreach (var tweenGroup in _groups) {
			if (tweenGroup.Has(target, property)) {
				return true;
			}
		}

		return false;
	}

	public void Stop(object target, string property)
	{
		if (target == null) {
			Options.Log(
				TweenLogLevel.Warning, 
				"Animate.Stop() called with null target."
			);
			return;
		}

		foreach (var tweenGroup in _groups) {
			tweenGroup.Stop(target, property);
		}
	}

	public void Finish(object target, string property)
	{
		if (target == null) {
			Options.Log(
				TweenLogLevel.Warning, 
				"Animate.Finish() called with null target."
			);
			return;
		}

		foreach (var tweenGroup in _groups) {
			tweenGroup.Finish(target, property);
		}
	}

	public void Cancel(object target, string property)
	{
		if (target == null) {
			Options.Log(
				TweenLogLevel.Warning, 
				"Animate.Cancel() called with null target."
			);
			return;
		}

		foreach (var tweenGroup in _groups) {
			tweenGroup.Cancel(target, property);
		}
	}

	public void Overwrite(Tween tween)
	{
		foreach (var tweenGroup in _groups) {
			tweenGroup.Overwrite(tween);
		}
	}

	// -------- Internals --------

	TweenOptions _options = new TweenOptions();
	protected List<TweenGroup> _groups = new List<TweenGroup>();
	protected List<TweenGroup> _newGroups = new List<TweenGroup>();
	protected TweenGroup<object> _singlesGroup;

	// MonoBehaviour.Update
	protected void Update()
	{
		ProcessTweens(TweenTiming.Update);
	}

	// MonoBehaviour.FixedUpdate
	protected void FixedUpdate()
	{
		ProcessTweens(TweenTiming.FixedUpdate);
	}

	// MonoBehaviour.FixedUpdate
	protected void LateUpdate()
	{
		ProcessTweens(TweenTiming.LateUpdate);
	}

	// Process tweens
	protected void ProcessTweens(TweenTiming timing)
	{
		// Add newly registered groups
		if (_newGroups.Count > 0) {
			_groups.AddRange(_newGroups);
			_newGroups.Clear();
		}

		// Update groups and remove invalid ones
		for (int i = 0; i < _groups.Count; i++) {
			if (!_groups[i].Update(timing)) {
				// Return group to the pool
				_groups[i].RetainCount--;
				_groups.RemoveAt(i); i--;
			}
		}
	}
}

}
