using System;
using System.Collections.Generic;
using Sttz.Tweener.Core.Reflection;
using Sttz.Tweener.Core.Static;
using Sttz.Tweener.Plugins;
using UnityEngine;

#if !ENABLE_IL2CPP && !NET_STANDARD_2_0
using Sttz.Tweener.Core.Codegen;
#endif

namespace Sttz.Tweener.Core {

	/// <summary>
	/// Interface for tween engines.
	/// After a group has been registered, the engine
	/// should call every groups Update method every frame.
	/// </summary>
	public interface ITweenEngine
	{
		// Default options.
		ITweenOptions Options { get; }
		// Wether to enable reflection / codegen
		bool EnableReflection { get; set; }
		// Tween pool to use in the engine
		TweenPool Pool { get; set; }

		// Default group used for sinlge tweens
		TweenGroup<object> SinglesGroup { get; }
		// Register a new group with the engine
		void RegisterGroup(ITweenGroup tweenGroup);

		// Generic tween creation method
		Tween<TTarget, TValue> Create<TTarget, TValue>(
			TweenMethod tweenMethod,
			TTarget target, 
			float duration,
			string property, 
			TValue startValue,
			TValue endValue,
			TValue diffValue,
			ITweenOptions parentOptions = null
		) where TTarget : class;
		
		// Manage default plugins
		void AddDefaultPlugin(PluginLoader loader);
		void RemoveDefaultPlugin(PluginLoader loader);

		// Load the default plugins for a new tween
		void LoadStaticPlugins(ITween tween);
		void LoadDynamicPlugins<TTarget, TValue>(Tween<TTarget, TValue> tween) where TTarget : class;

		// Check if any tween exists on the engine
		bool Has(object target, string property = null);
		// Stop tweens on the engine (leave at current value)
		void Stop(object target, string property = null);
		// Finish tweens on the engine (jump to final value)
		void Finish(object target, string property = null);
		// Cancel tweens on the engine (reset to initial value)
		void Cancel(object target, string property = null);

		// Let tween overwrite others based on its settings
		void Overwrite(ITween tween);
	}

	/// <summary>
	/// Tween engine.
	/// </summary>
	public class UnityTweenEngine : MonoBehaviour, ITweenEngine
	{
		///////////////////
		// Options

		/// <summary>
		/// Global defaults. Global options get overriden by templates',
		/// groups' and tweens' options (in that order).
		/// </summary>
		public ITweenOptions Options {
			get {
				return _options;
			}
		}
		protected ITweenOptions _options = new TweenTemplate();

		/// <summary>
		/// Wether to use reflection when tweening.
		/// </summary>
		/// <remarks>
		/// On JIT platforms (not using IL2CPP) not using .Net Standard,
		/// code generation will be used. On all other platforms, plain
		/// reflection instead.
		/// 
		/// When reflection is disabled, only static plugins will
		/// be used.
		/// 
		/// Static plugins will always be preferred, reflection is only
		/// used as a fallback.
		/// </remarks>
		public bool EnableReflection { get; set; }

		///////////////////
		// Pooling

		/// <summary>
		/// Global pool manager. Set to null to disable pooling globally.
		/// You can also disable pooling by setting the AfterUse option 
		/// in any scope, which can be re-enabled at a lower scope.
		/// </summary>
		public TweenPool Pool { get; set; }

		///////////////////
		// Fields

		/// <summary>
		/// Tween groups
		/// </summary>
		protected List<ITweenGroup> _groups = new List<ITweenGroup>();
		/// <summary>
		/// Group used for single tweens, e.g. Animate.To()
		/// </summary>
		protected TweenGroup<object> _singlesGroup;

		/// <summary>
		/// Additional loaders that are called for each tween.
		/// </summary>
		List<PluginLoader> pluginLoaders = new List<PluginLoader>();

		///////////////////
		// Methods

		/// <summary>
		/// Create the UnityTweenEngine.
		/// </summary>
		public static UnityTweenEngine Create()
		{
			var go = new GameObject("Animate");
			DontDestroyOnLoad(go);
			return go.AddComponent<UnityTweenEngine>();
		}

		// Generic Tween creation, using of the Tween.* static methods
		// is strongly encouraged.
		public Tween<TTarget, TValue> Create<TTarget, TValue>(
			TweenMethod tweenMethod,
			TTarget target, 
			float duration,
			string property, 
			TValue startValue,
			TValue endValue,
			TValue diffValue,
			ITweenOptions parentOptions = null
		)
			where TTarget : class 
		{
			// Basic sanity checks
			if (target == null) {
				Options.Internal.Log(
					TweenLogLevel.Error, 
					"Trying to tween {0} on a null object.", property
				);
				return null;
			}
			if (property == null) {
				Options.Internal.Log(
					TweenLogLevel.Error, 
					"Property to tween on object {0} is null.", target
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

		/// <summary>
		/// Add a plugin loader that will be called for every tween.
		/// </summary>
		public void AddDefaultPlugin(PluginLoader loader)
		{
			pluginLoaders.Add(loader);
		}

		/// <summary>
		/// Remove a plugin loader.
		/// </summary>
		public void RemoveDefaultPlugin(PluginLoader loader)
		{
			pluginLoaders.Remove(loader);
		}

		/// <summary>
		/// Load default static tween plugins.
		/// </summary>
		/// <remarks>
		/// The static plugin loading method is not generic. This means plugins
		/// loaded here must pre-create the necessary ITweenPlugin instances
		/// beforehand or only support a predetermined set of types.
		/// 
		/// This method is available on AOT/IL2CPP platforms.
		/// </remarks>
		public void LoadStaticPlugins(ITween tween)
		{
			// The order matters here, plugins loaded later
			// can override plugins loaded earlier.

			// Unity integration for static plugins
			TweenStaticUnityPlugin.Load();

			// Default Plugins
			TweenStaticAccessorPlugin.Load(tween);
			TweenStaticArithmeticPlugin.Load(tween);

			// Load additional plugins
			foreach (var loader in pluginLoaders) {
				loader(tween, true);
			}
		}

		/// <summary>
		/// Load default dynamic tween plugins.
		/// </summary>
		/// <remarks>
		/// This method is not called on AOT/IL2CPP platforms.
		/// 
		/// Due to the indirection in Animate's design, AOT compilers can't figure
		/// out from a Animate.To call that the need to generate a LoadDynamicPlugins
		/// concrete implementation with the matching types. This means that if this
		/// method was referencing a generic type, that type will likely not be
		/// compiled by the AOT compiler and a ExecutionEngineException will be 
		/// thrown at runtime.
		/// 
		/// It might work if the type is referenced elsewhere but that is unlikely,
		/// would defeat the decoupled plugin design or require a clunky API.
		/// Therefore, this method is simply not called on IL2CPP platforms.
		/// </remarks>
		public void LoadDynamicPlugins<TTarget, TValue>(Tween<TTarget, TValue> tween)
			where TTarget : class
		{
			// The order matters here, plugins loaded later
			// can override plugins loaded earlier.

			if (EnableReflection) {
				#if !ENABLE_IL2CPP && !NET_STANDARD_2_0
					// Codegen doesn't work with AOT (IL2CPP) or .Net Standard
					TweenCodegenAccessorPlugin.Load(tween);
					TweenCodegenArithmeticPlugin.Load(tween);
				#else
					TweenReflectionAccessorPlugin.Load(tween);
					TweenReflectionArithmeticPlugin.Load(tween);
				#endif
			}
		}

		// Add a tween to default group
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

		// Register a new tween group
		public void RegisterGroup(ITweenGroup tweenGroup)
		{
			tweenGroup.Options.RetainCount++;
			_groups.Add(tweenGroup);
		}

		// Check if a tween exists
		bool ITweenEngine.Has(object target, string property)
		{
			if (target == null) {
				Options.Internal.Log(
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

		// Stop all tweens, leaving the target at the current value
		void ITweenEngine.Stop(object target, string property)
		{
			if (target == null) {
				Options.Internal.Log(
					TweenLogLevel.Warning, 
					"Animate.Stop() called with null target."
				);
				return;
			}

			foreach (var tweenGroup in _groups) {
				tweenGroup.Stop(target, property);
			}
		}

		// Finish all tweens, jumping to the final value
		void ITweenEngine.Finish(object target, string property)
		{
			if (target == null) {
				Options.Internal.Log(
					TweenLogLevel.Warning, 
					"Animate.Finish() called with null target."
				);
				return;
			}

			foreach (var tweenGroup in _groups) {
				tweenGroup.Finish(target, property);
			}
		}

		// Cancel all tweens, jumping back to the initial value
		void ITweenEngine.Cancel(object target, string property)
		{
			if (target == null) {
				Options.Internal.Log(
					TweenLogLevel.Warning, 
					"Animate.Cancel() called with null target."
				);
				return;
			}

			foreach (var tweenGroup in _groups) {
				tweenGroup.Cancel(target, property);
			}
		}

		// Let a tween overwrite others
		public void Overwrite(ITween tween)
		{
			foreach (var tweenGroup in _groups) {
				tweenGroup.Internal.Overwrite(tween);
			}
		}

		///////////////////
		// Engine

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
			// Update groups and remove invalid ones
			for (int i = 0; i < _groups.Count; i++) {
				if (!_groups[i].Internal.Update(timing)) {
					// Return group to the pool
					_groups[i].Options.RetainCount--;
					_groups.RemoveAt(i); i--;
				}
			}
		}
	}
}

