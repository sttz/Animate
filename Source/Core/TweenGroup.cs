using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sttz.Tweener.Core {

	/// <summary>
	/// I tween group internal.
	/// </summary>
	public interface ITweenGroupInternal : ITweenOptionsInternal
	{
		// Reset group, call Use() before using the group again
		void Reset();
		// Let a tween overwrite others in this group
		void Overwrite(ITween tween);
		// Update all tweens in the group
		bool Update(TweenTiming timing);
	}

	/// <summary>
	/// Group of tweens, sharing options
	/// </summary>
	public class TweenGroup : TweenOptionsFluid<ITweenGroup>, ITweenGroup, ITweenGroupInternal
	{
		///////////////////
		// Fields

		// Tweens in this group
		protected List<ITween> _newTweens;
		protected List<ITween> _updateTweens;
		protected List<ITween> _fixedUpdateTweens;
		protected List<ITween> _lateUpdateTweens;

		// Instance currently in use?
		protected bool _inUse;
		// Default target that used for tweens in this group
		protected object _defaultTarget;
		// Tweening engine
		protected ITweenEngine _engine;

		///////////////////
		// Constructor

		// Initialize a new or pooled intance
		public void Use(object target, ITweenOptions parentOptions, ITweenEngine engine)
		{
			if (_inUse) {
				throw new Exception("TweenGroup instance already in use.");
			}
			_inUse = true;

			_defaultTarget = target;
			_parent = parentOptions;
			_engine = engine;
		}

		// Reset group after use
		public override void Reset()
		{
			base.Reset();

			_inUse = false;
			_defaultTarget = null;
			_engine = null;

			if (_newTweens != null) _newTweens.Clear();
			if (_updateTweens != null) _updateTweens.Clear();
			if (_fixedUpdateTweens != null) _fixedUpdateTweens.Clear();
			if (_lateUpdateTweens != null) _lateUpdateTweens.Clear();
		}

		// Accessor to intenral methods
		public ITweenGroupInternal Internal {
			get {
				return this;
			}
		}

		///////////////////
		// Add Tween

		// Add a custom tween
		public ITweenGroup Add(ITween tween)
		{
			// Add ourself to the scope chain
			tween.Options.Internal.ParentOptions = this;
			// Pass engine to tween
			tween.Internal.TweenEngine = _engine;

			// Set target if tween doesn't have its own
			if (tween.Target == null) {
				tween.Internal.Target = _defaultTarget;
			}

			// Tween gets added to main group first and then
			// re-grouped in the update.
			if (_newTweens == null) _newTweens = new List<ITween>();
			_newTweens.Add(tween);

			// (Re-)register group with engine
			if (_newTweens.Count == 1) {
				_engine.RegisterGroup(this);
			}

			return this;
		}

		// Regroup tween to its proper group
		protected void Regroup(ITween tween)
		{
			if ((tween.Options.TweenTiming & TweenTiming.Update) > 0) {
				if (_updateTweens == null) _updateTweens = new List<ITween>();
				_updateTweens.Add(tween);

			} else if ((tween.Options.TweenTiming & TweenTiming.LateUpdate) > 0)  {
				if (_lateUpdateTweens == null) _lateUpdateTweens = new List<ITween>();
				_lateUpdateTweens.Add(tween);

			} else {
				if (_fixedUpdateTweens == null) _fixedUpdateTweens = new List<ITween>();
				_fixedUpdateTweens.Add(tween);
			}
		}

		///////////////////
		// To

		// Add a To tween to the group
		public ITweenGroup To<T>(
			string property, T toValue, 
			params TweenPluginInfo[] plugins
		) {
			return Add(Tween.To(_defaultTarget, float.NaN, property, toValue, plugins));
		}

		// Add a To tween to the group
		public ITweenGroup To<T>(
			float duration, string property, T toValue, 
			params TweenPluginInfo[] plugins
		) {
			return Add(Tween.To(_defaultTarget, duration, property, toValue, plugins));
		}

		// Add a To tween to the group
		public ITweenGroup To<T>(
			object target, float duration, string property, T toValue, 
			params TweenPluginInfo[] plugins
		) {
			return Add(Tween.To(target, duration, property, toValue, plugins));
		}

		///////////////////
		// From

		// Add a From tween to the group
		public ITweenGroup From<T>(
			string property, T fromValue, 
			params TweenPluginInfo[] plugins
		) {
			return Add(Tween.From(_defaultTarget, float.NaN, property, fromValue, plugins));
		}

		// Add a From tween to the group
		public ITweenGroup From<T>(
			float duration, string property, T fromValue, 
			params TweenPluginInfo[] plugins
		) {
			return Add(Tween.From(_defaultTarget, duration, property, fromValue, plugins));
		}

		// Add a From tween to the group
		public ITweenGroup From<T>(
			object target, float duration, string property, T fromValue, 
			params TweenPluginInfo[] plugins
		) {
			return Add(Tween.From(target, duration, property, fromValue, plugins));
		}

		///////////////////
		// FromTo

		// Add a FromTo tween to the group
		public ITweenGroup FromTo<T>(
			string property, T fromValue, T toValue, 
			params TweenPluginInfo[] plugins
		) {
			return Add(Tween.FromTo(_defaultTarget, float.NaN, property, fromValue, toValue, plugins));
		}

		// Add a FromTo tween to the group
		public ITweenGroup FromTo<T>(
			float duration, string property, T fromValue, T toValue, 
			params TweenPluginInfo[] plugins
		) {
			return Add(Tween.FromTo(_defaultTarget, duration, property, fromValue, toValue, plugins));
		}

		// Add a FromTo tween to the group
		public ITweenGroup FromTo<T>(
			object target, float duration, string property, T fromValue, T toValue, 
			params TweenPluginInfo[] plugins
		) {
			return Add(Tween.FromTo(target, duration, property, fromValue, toValue, plugins));
		}

		///////////////////
		// By

		// Add a By tween to the group
		public ITweenGroup By<T>(
			string property, T byValue, 
			params TweenPluginInfo[] plugins
		) {
			return Add(Tween.By(_defaultTarget, float.NaN, property, byValue, plugins));
		}

		// Add a By tween to the group
		public ITweenGroup By<T>(
			float duration, string property, T byValue, 
			params TweenPluginInfo[] plugins
		) {
			return Add(Tween.By(_defaultTarget, duration, property, byValue, plugins));
		}

		// Add a By tween to the group
		public ITweenGroup By<T>(
			object target, float duration, string property, T byValue, 
			params TweenPluginInfo[] plugins
		) {
			return Add(Tween.By(target, duration, property, byValue, plugins));
		}

		///////////////////
		// Manage Tweens

		// Run an action for all tweens in the group
		protected void AllTweens(Func<ITween, bool> action)
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

		// Validate all tweens in the group, optionally forcing a render of the tween
		public bool Validate(bool forceRender = false)
		{
			bool valid = true;

			AllTweens((tween) => {
				valid &= tween.Validate(forceRender);
				return true;
			});

			return valid;
		}

		// Check if a tween exists
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

		// Check if the group contains any active tweens
		public bool Has()
		{
			return ((_newTweens == null ? 0 : _newTweens.Count)
				+ (_updateTweens == null ? 0 : _updateTweens.Count)
				+ (_fixedUpdateTweens == null ? 0 : _fixedUpdateTweens.Count)
				+ (_lateUpdateTweens == null ? 0 : _lateUpdateTweens.Count)
				> 0);
		}

		// Stop tweens, leaving the target at the current value
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

		// Finish tweens, jumping to the final value
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

		// Cancel tweens, jumping back to the initial value
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

		// Let a tween overwrite others in this group
		public void Overwrite(ITween tween)
		{
			int i;
			ITween other;

			if (_newTweens != null) {
				for (i = 0; i < _newTweens.Count; i++) {
					other = _newTweens[i];
					if (tween.Target == other.Target 
							&& tween.Property == other.Property) {
						other.Internal.Overwrite(tween);
					}
				}
			}

			if (_updateTweens != null) {
				for (i = 0; i < _updateTweens.Count; i++) {
					other = _updateTweens[i];
					if (tween.Target == other.Target 
							&& tween.Property == other.Property) {
						other.Internal.Overwrite(tween);
					}
				}
			}

			if (_fixedUpdateTweens != null) {
				for (i = 0; i < _fixedUpdateTweens.Count; i++) {
					other = _fixedUpdateTweens[i];
					if (tween.Target == other.Target 
							&& tween.Property == other.Property) {
						other.Internal.Overwrite(tween);
					}
				}
			}

			if (_lateUpdateTweens != null) {
				for (i = 0; i < _lateUpdateTweens.Count; i++) {
					other = _lateUpdateTweens[i];
					if (tween.Target == other.Target 
							&& tween.Property == other.Property) {
						other.Internal.Overwrite(tween);
					}
				}
			}
		}

		// Return a coroutine that waits for the group to complete
		public Coroutine WaitForEndOfGroup()
		{
			if (_engine == null) {
				Log(TweenLogLevel.Error, 
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

		///////////////////
		// Engine

		// Upate tween group
		public bool Update(TweenTiming timing)
		{
			// Sort in new tweens
			if (_newTweens != null && _newTweens.Count > 0) {
				// Send first update in reverse order to let
				// tweens created later overwrite those created earlier
				for (int i = _newTweens.Count - 1; i >= 0; i--) {
					if (_newTweens[i].Internal.Update()) {
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
					if (!tweens[i].Internal.Update()) {
						// Return to pool
						if (Animate.Pool != null 
								&& tweens[i].Options.Recycle != TweenRecycle.None
								&& (tweens[i].Options.Recycle & TweenRecycle.Tweens) > 0) {
							Animate.Pool.Return(tweens[i]);
						}
						// Remove from list
						tweens.RemoveAt(i); i--; count--;
					}
				}
			}

			// We turn invalid if there are no tweens left
			return Has();
		}
	}

}