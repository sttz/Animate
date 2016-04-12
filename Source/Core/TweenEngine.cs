using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sttz.Tweener.Core {

	/// <summary>
	/// Interface for tween engines.
	/// After a group has been registered, the engine
	/// should call every groups Update method every frame.
	/// </summary>
	public interface ITweenEngine
	{
		// Default group used for sinlge tweens
		TweenGroup SinglesGroup { get; }
		// Register a new group with the engine
		void RegisterGroup(TweenGroup tweenGroup);

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
		public static ITweenOptions Options {
			get {
				return _options;
			}
		}
		protected static ITweenOptions _options = new TweenTemplate();

		///////////////////
		// Pooling

		/// <summary>
		/// Global pool manager. Set to null to disable pooling globally.
		/// You can also disable pooling by setting the AfterUse option 
		/// in any scope, which can be re-enabled at a lower scope.
		/// </summary>
		public static ITweenPool Pool { get; set; }

		///////////////////
		// Singleton

		// Singleton
		private static ITweenEngine _instance;
		protected static ITweenEngine Instance {
			get {
				if (System.Object.ReferenceEquals(_instance, null)) {
					var go = new GameObject("Animate");
					DontDestroyOnLoad(go);
					_instance = go.AddComponent<Animate>();
				}
				return _instance;
			}
		}

		///////////////////
		// Fields

		// Tween groups
		protected List<TweenGroup> _groups = new List<TweenGroup>();
		// Group used for single tweens, e.g. Animate.To()
		protected TweenGroup _singlesGroup;

		///////////////////
		// Methods

		// Add a tween to default group
		public TweenGroup SinglesGroup {
			get {
				if (_singlesGroup == null) {
					_singlesGroup = new TweenGroup();
					_singlesGroup.Use(null, Options, this);
					_singlesGroup.Options.Recycle = TweenRecycle.Tweens;
				}
				return _singlesGroup;
			}
		}

		// Register a new tween group
		public void RegisterGroup(TweenGroup tweenGroup)
		{
			tweenGroup.Retain();
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
				tweenGroup.Overwrite(tween);
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
				if (!_groups[i].Update(timing)) {
					// Return group to the pool
					_groups[i].Release();
					_groups.RemoveAt(i); i--;
				}
			}
		}
	}
}

