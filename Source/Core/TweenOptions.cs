using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Sttz.Tweener.Core {

	// Internal options methods
	public interface ITweenOptionsInternal
	{
		// Parent options
		ITweenOptions ParentOptions { get; set; }
		// Log a message
		void Log(TweenLogLevel level, string message, params object[] args);
	}

	/// <summary>
	/// Tween Options Template.
	/// </summary>
	public class TweenTemplate : TweenOptionsFluid<ITweenTemplate>, ITweenTemplate
	{
		// Constructor
		public TweenTemplate(ITweenOptions parent = null)
		{
			ParentOptions = parent;
		}
	}

	/// <summary>
	/// Bare tween options without fluid interface.
	/// </summary>
	public abstract class TweenOptions : ITweenOptions, ITweenOptionsInternal
	{
		///////////////////
		// Shared plugin instances

		// Shared automatic plugin instances
		protected static Dictionary<Type, object> _sharedPlugins
			= new Dictionary<Type, object>();

		///////////////////
		// Fields

		// Parent options
		protected ITweenOptions _parent;
		// Recylce after use
		protected TweenRecycle _recycle;
		// Tween won't be recycled when retain count > 0
		protected uint _retainCount;

		// Duration of the tween
		protected float _duration = float.NaN;
		// Easing method to use
		protected EasingMethod _easing;
		// Tween during physics update
		protected TweenTiming _tweenTiming;
		// Start delay (in seconds)
		protected float _startDelay = float.NaN;

		// Overwrite setting
		protected TweenOverwrite _overwrite;

		// Log level of current scope
		protected TweenLogLevel _logLevel;

		// 
		public ITweenOptions Options {
			get {
				return this;
			}
		}

		ITweenOptionsInternal ITweenOptions.Internal { 
			get {
				return this;
			}
		}

		///////////////////
		// Reset

		// Reset options
		public virtual void Reset()
		{
			_parent = null;
			_recycle = TweenRecycle.Undefined;
			_retainCount = 0;
			_duration = float.NaN;
			_easing = null;
			_tweenTiming = TweenTiming.Undefined;
			_startDelay = float.NaN;
			_overwrite = TweenOverwrite.Undefined;
			_logLevel = TweenLogLevel.Undefined;

			ResetEvents();
		}

		// Remove all event listeners from all events
		protected void ResetEvents()
		{
			InitializeEventImpl = null;
			StartEventImpl = null;
			UpdateEventImpl = null;
			CompleteEventImpl = null;
			ErrorEventImpl = null;
		}

		///////////////////
		// Properties

		// Duration of the tween
		float ITweenOptions.Duration
		{
			get {
				if (!float.IsNaN(_duration)) {
					return _duration;
				} else if (_parent != null) {
					return _parent.Duration;
				} else {
					return float.NaN;
				}
			}
			set {
				_duration = value;
			}
		}

		// Easing method to use
		EasingMethod ITweenOptions.Easing
		{
			get {
				if (_easing != null) {
					return _easing;
				} else if (_parent != null) {
					return _parent.Easing;
				} else {
					return null;
				}
			}
			set {
				_easing = value;
			}
		}

		// Tween during physics update
		TweenTiming ITweenOptions.TweenTiming
		{
			get {
				if (_tweenTiming != TweenTiming.Undefined) {
					return _tweenTiming;
				} else if (_parent != null) {
					return _parent.TweenTiming;
				} else {
					return TweenTiming.Undefined;
				}
			}
			set {
				_tweenTiming = value;
			}
		}

		// Start delay (in seconds)
		float ITweenOptions.StartDelay
		{
			get {
				if (!float.IsNaN(_startDelay)) {
					return _startDelay;
				} else if (_parent != null) {
					return _parent.StartDelay;
				} else {
					return float.NaN;
				}
			}
			set {
				_startDelay = value;
			}
		}

		// Overwrite setting
		TweenOverwrite ITweenOptions.OverwriteSettings {
			get {
				if (_overwrite != TweenOverwrite.Undefined) {
					return _overwrite;
				} else if (_parent != null) {
					return _parent.OverwriteSettings;
				} else {
					return TweenOverwrite.Undefined;
				}
			}
			set {
				if (value != TweenOverwrite.None 
						&& value != TweenOverwrite.Undefined) {
					// Check exclusive timing options
					var options = (int)(
						value &
						(TweenOverwrite.OnInitialize
						| TweenOverwrite.OnStart)
					);

					if ((options & (options - 1)) != 0) {
						Log(
							TweenLogLevel.Warning, 
							"TweenOverwrite must only contain one of OnInitialize or OnStart."
						);
						return;
					}

					// Check exclusive method options
					options = (int)(
						value &
						(TweenOverwrite.Stop
						| TweenOverwrite.Finish
						| TweenOverwrite.Cancel)
					);

					if ((options & (options - 1)) != 0) {
						Log(
							TweenLogLevel.Warning, 
							"TweenOverwrite must only contain one of Stop, Finish or Cancel."
						);
						return;
					}

					// Check exclusive overlapping options
					options = (int)(
						value &
						(TweenOverwrite.All
						| TweenOverwrite.Overlapping)
					);

					if ((options & (options - 1)) != 0) {
						Log(
							TweenLogLevel.Warning, 
							"TweenOverwrite must only contain one of All, Overlapping."
						);
						return;
					}
				}

				_overwrite = value;
			}
		}

		// Parent tween options
		public ITweenOptions ParentOptions
		{
			get {
				return _parent;
			}
			set {
				_parent = value;
			}
		}

		// Recycling of tweens and groups
		TweenRecycle ITweenOptions.Recycle {
			get {
				if (_recycle != TweenRecycle.Undefined) {
					return _recycle;
				} else if (_parent != null) {
					return _parent.Recycle;
				} else {
					return TweenRecycle.Undefined;
				}
			}
			set {
				_recycle = value;
			}
		}

		// Retain count
		uint ITweenOptions.RetainCount {
			get {
				return _retainCount;
			}
			set {
				if (_retainCount == value)
					return;

				_retainCount = value;

				if (_retainCount == 0) {
					ReturnToPool();
				}
			}
		}

		// Return the target to the pool
		protected virtual void ReturnToPool()
		{
			throw new NotImplementedException();
		}

		///////////////////
		// Events

		// Called when the tween initializes
		private event EventHandler<TweenEventArgs> InitializeEventImpl;
		event EventHandler<TweenEventArgs> ITweenOptions.InitializeEvent {
			add { InitializeEventImpl += value; }
			remove { InitializeEventImpl -= value; }
		}

		// Trigger Initialize event
		protected void TriggerInitialize(ITween tween, TweenEventArgs args = null)
		{
			if (InitializeEventImpl != null) {
				try {
					args = new TweenEventArgs(tween, TweenEvent.Initialize);
					InitializeEventImpl(this, args);
				} catch (Exception e) {
					Log(TweenLogLevel.Error, 
						"Exception in Initialize event handler: {0}", e
					);
				}
			}

			if (_parent != null) {
				(_parent as TweenOptions).TriggerInitialize(tween, args);
			}
		}

		// Called when the tween starts
		private event EventHandler<TweenEventArgs> StartEventImpl;
		event EventHandler<TweenEventArgs> ITweenOptions.StartEvent {
			add { StartEventImpl += value; }
			remove { StartEventImpl -= value; }
		}

		// Trigger Start event
		protected void TriggerStart(ITween tween, TweenEventArgs args = null)
		{
			if (StartEventImpl != null) {
				try {
					args = new TweenEventArgs(tween, TweenEvent.Start);
					StartEventImpl(this, args);
				} catch (Exception e) {
					Log(TweenLogLevel.Error, 
						"Exception in Start event handler: {0}", e
					);
				}
			}

			if (_parent != null) {
				(_parent as TweenOptions).TriggerInitialize(tween, args);
			}
		}

		// Called every time the tween updates
		private event EventHandler<TweenEventArgs> UpdateEventImpl;
		event EventHandler<TweenEventArgs> ITweenOptions.UpdateEvent {
			add { UpdateEventImpl += value; }
			remove { UpdateEventImpl -= value; }
		}

		// Trigger Start event
		protected void TriggerUpdate(ITween tween, TweenEventArgs args = null)
		{
			if (UpdateEventImpl != null) {
				try {
					args = new TweenEventArgs(tween, TweenEvent.Update);
					UpdateEventImpl(this, args);
				} catch (Exception e) {
					Log(TweenLogLevel.Error, 
						"Exception in Update event handler: {0}", e
					);
				}
			}

			if (_parent != null) {
				(_parent as TweenOptions).TriggerUpdate(tween, args);
			}
		}

		// Check if there are currently update listeners registered
		protected bool HasUpdateListeners()
		{
			return (
				UpdateEventImpl != null 
				|| (
					_parent != null
					&& (_parent as TweenOptions).HasUpdateListeners()
				)
			);
		}

		// Called once the tween completed (or was stopped)
		private event EventHandler<TweenEventArgs> CompleteEventImpl;
		event EventHandler<TweenEventArgs> ITweenOptions.CompleteEvent {
			add { CompleteEventImpl += value; }
			remove { CompleteEventImpl -= value; }
		}

		// Trigger Start event
		protected void TriggerComplete(
			ITween tween, 
			TweenCompletedBy stoppedBy, 
			TweenEventArgs args = null
		) {
			if (CompleteEventImpl != null) {
				try {
					args = new TweenEventArgs(tween, TweenEvent.Complete, null, stoppedBy);
					CompleteEventImpl(this, args);
				} catch (Exception e) {
					Log(TweenLogLevel.Error, 
						"Exception in Complete event handler: {0}", e
					);
				}
			}

			if (_parent != null) {
				(_parent as TweenOptions).TriggerComplete(tween, stoppedBy, args);
			}
		}

		// Called every time the tween updates
		private event EventHandler<TweenEventArgs> ErrorEventImpl;
		event EventHandler<TweenEventArgs> ITweenOptions.ErrorEvent {
			add { ErrorEventImpl += value; }
			remove { ErrorEventImpl -= value; }
		}

		// Trigger Start event
		protected void TriggerError(ITween tween, string error, TweenEventArgs args = null)
		{
			if (ErrorEventImpl != null) {
				try {
					args = new TweenEventArgs(tween, TweenEvent.Error, error);
					ErrorEventImpl(this, args);
				} catch (Exception e) {
					Log(TweenLogLevel.Error, 
						"Exception in Error event handler: {0}", e
					);
				}
			}

			if (_parent != null) {
				(_parent as TweenOptions).TriggerError(tween, error, args);
			}
		}

		///////////////////
		// Logging

		// Log level
		TweenLogLevel ITweenOptions.LogLevel {
			get {
				if (_logLevel != TweenLogLevel.Undefined) {
					return _logLevel;
				} else if (_parent != null) {
					return _parent.LogLevel;
				} else {
					return Sttz.Tweener.TweenLogLevel.Undefined;
				}
			}
			set {
				_logLevel = value;
			}
		}

		// Log a message
		public void Log(TweenLogLevel level, string message, params object[] args)
		{
			// Only log messages with apropriate level
			if (level < Options.LogLevel) return;

			message = string.Format("[{0}] {1}", level, string.Format(message, args));
			if (level == TweenLogLevel.Debug) {
				Debug.Log(message);
			} else if (level == TweenLogLevel.Warning) {
				Debug.LogWarning(message);
			} else {
				Debug.LogError(message);
			}
		}
	}

	/// <summary>
	/// Fluid interface on top of bare TweenOptions.
	/// </summary>
	public abstract class TweenOptionsFluid<TContainer> : TweenOptions, ITweenOptionsFluid<TContainer> where TContainer : class
	{
		///////////////////
		// Properites

		// Set duration of tween
		public TContainer Over(float duration)
		{
			Options.Duration = duration;
			return this as TContainer;
		}

		// Set easing method
		public TContainer Ease(EasingMethod easing)
		{
			Options.Easing = easing;
			return this as TContainer;
		}

		// Set Timing
		public TContainer Timing(TweenTiming timing)
		{
			Options.TweenTiming = timing;
			return this as TContainer;
		}

		// Set start delay
		public TContainer Delay(float seconds)
		{
			Options.StartDelay = seconds;
			return this as TContainer;
		}

		// Set overwrite settings
		public TContainer Overwrite(TweenOverwrite settings)
		{
			Options.OverwriteSettings = settings;
			return this as TContainer;
		}

		// Set overwrite settings
		public TContainer Recycle(TweenRecycle recycle)
		{
			Options.Recycle = recycle;
			return this as TContainer;
		}

		// Set overwrite settings
		public TContainer Retain()
		{
			Options.RetainCount += 1;
			return this as TContainer;
		}

		// Set overwrite settings
		public TContainer Release()
		{
			Options.RetainCount -= 1;
			return this as TContainer;
		}

		///////////////////
		// Events

		public TContainer OnInitialize(EventHandler<TweenEventArgs> handler)
		{
			Options.InitializeEvent += handler;
			return this as TContainer;
		}

		public TContainer OnInitialize(Action handler)
		{
			Options.InitializeEvent += (sender, args) => handler();
			return this as TContainer;
		}

		public TContainer OnInitialize(Action<TweenEventArgs> handler)
		{
			Options.InitializeEvent += (sender, args) => handler(args);
			return this as TContainer;
		}

		public TContainer OnStart(EventHandler<TweenEventArgs> handler)
		{
			Options.StartEvent += handler;
			return this as TContainer;
		}

		public TContainer OnStart(Action handler)
		{
			Options.StartEvent += (sender, args) => handler();
			return this as TContainer;
		}

		public TContainer OnStart(Action<TweenEventArgs> handler)
		{
			Options.StartEvent += (sender, args) => handler(args);
			return this as TContainer;
		}

		public TContainer OnUpdate(EventHandler<TweenEventArgs> handler)
		{
			Options.UpdateEvent += handler;
			return this as TContainer;
		}

		public TContainer OnUpdate(Action handler)
		{
			Options.UpdateEvent += (sender, args) => handler();
			return this as TContainer;
		}

		public TContainer OnUpdate(Action<TweenEventArgs> handler)
		{
			Options.UpdateEvent += (sender, args) => handler(args);
			return this as TContainer;
		}

		public TContainer OnComplete(EventHandler<TweenEventArgs> handler)
		{
			Options.CompleteEvent += handler;
			return this as TContainer;
		}

		public TContainer OnComplete(Action handler)
		{
			Options.CompleteEvent += (sender, args) => handler();
			return this as TContainer;
		}

		public TContainer OnComplete(Action<TweenEventArgs> handler)
		{
			Options.CompleteEvent += (sender, args) => handler(args);
			return this as TContainer;
		}

		public TContainer OnError(EventHandler<TweenEventArgs> handler)
		{
			Options.ErrorEvent += handler;
			return this as TContainer;
		}

		public TContainer OnError(Action handler)
		{
			Options.ErrorEvent += (sender, args) => handler();
			return this as TContainer;
		}

		public TContainer OnError(Action<TweenEventArgs> handler)
		{
			Options.ErrorEvent += (sender, args) => handler(args);
			return this as TContainer;
		}

		///////////////////
		// Debug

		// Set overwrite settings
		public TContainer LogLevel(TweenLogLevel level)
		{
			Options.LogLevel = level;
			return this as TContainer;
		}

		///////////////////
		// Finalizers (end the fluid chain)

		// Return a WaitForSeconds set for the full tween duration (delay + duration)
		public WaitForSeconds WaitForTweenDuration()
		{
			// Check duration is set
			if (float.IsNaN(Options.Duration)) {
				Log(TweenLogLevel.Error,
				    "No duration set on {0} for WaitForDuration()", this);
				return null;
			}

			// Add delay to duration
			var duration = Options.Duration;
			if (!float.IsNaN(Options.StartDelay)) {
				duration += Options.StartDelay;
			}

			// Return yield instruction for the duration
			return new WaitForSeconds(duration);
		}
	}

}
