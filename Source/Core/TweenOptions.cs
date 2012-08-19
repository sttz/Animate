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
		// Collect all automatic plugins
		HashSet<TweenPluginInfo> GetAutomaticPlugins(HashSet<TweenPluginInfo> current = null);
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

		// The default plugin, handling basic operation
		protected TweenPluginInfo _defaultPlugin;

		// Automatic plugins
		protected Dictionary<TweenPluginInfo, bool> _autoPlugins;

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
			_duration = float.NaN;
			_easing = null;
			_tweenTiming = TweenTiming.Undefined;
			_startDelay = float.NaN;
			_overwrite = TweenOverwrite.Undefined;
			_defaultPlugin = default(TweenPluginInfo);
			_autoPlugins = null;
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
		// Plugins

		// The default plugin, handling basic operation
		TweenPluginInfo ITweenOptions.DefaultPlugin {
			get {
				if (_defaultPlugin.pluginType != null) {
					return _defaultPlugin;
				} else if (_parent != null) {
					return _parent.DefaultPlugin;
				} else {
					return TweenPluginInfo.None;
				}
			}
			set {
				_defaultPlugin = value;
			}
		}

		// Register an automatic plugin
		void ITweenOptions.SetAutomatic(TweenPluginInfo plugin, bool enable)
		{
			// Check for auto activation delegate
			if (plugin.autoActivation == null) {
				Log(TweenLogLevel.Error,
				    "Plugin {0} does not provide automation.", plugin.pluginType);
				return;
			}

			// Initialize plugin dictionary
			if (_autoPlugins == null) {
				_autoPlugins = new Dictionary<TweenPluginInfo, bool>();
			}

			// Set enabled state
			_autoPlugins[plugin] = enable;
		}

		// Get all automatic plugins registered in the current scope
		public HashSet<TweenPluginInfo> GetAutomaticPlugins(
			HashSet<TweenPluginInfo> current = null
		) {
			// Top instance creates hash set
			if (current == null) {
				current = new HashSet<TweenPluginInfo>();
			}

			// Let parents recursively add their instances
			if (_parent != null) {
				_parent.Internal.GetAutomaticPlugins(current);
			}

			// Remove disabled and add enabled instances from current scope
			if (_autoPlugins != null) {
				foreach (var pair in _autoPlugins) {
					if (pair.Value) {
						current.Add(pair.Key);
					} else {
						current.Remove(pair.Key);
					}
				}
			}

			return current;
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
				args = new TweenEventArgs(tween, TweenEvent.Initialize);
				InitializeEventImpl(this, args);
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
				args = new TweenEventArgs(tween, TweenEvent.Start);
				StartEventImpl(this, args);
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
				args = new TweenEventArgs(tween, TweenEvent.Update);
				UpdateEventImpl(this, args);
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
				args = new TweenEventArgs(tween, TweenEvent.Complete, null, stoppedBy);
				CompleteEventImpl(this, args);
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
				args = new TweenEventArgs(tween, TweenEvent.Error, error);
				ErrorEventImpl(this, args);
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

		///////////////////
		// Events

		// Set overwrite settings
		public TContainer OnInitialize(EventHandler<TweenEventArgs> handler)
		{
			Options.InitializeEvent += handler;
			return this as TContainer;
		}

		// Set overwrite settings
		public TContainer OnStart(EventHandler<TweenEventArgs> handler)
		{
			Options.StartEvent += handler;
			return this as TContainer;
		}

		// Set overwrite settings
		public TContainer OnUpdate(EventHandler<TweenEventArgs> handler)
		{
			Options.UpdateEvent += handler;
			return this as TContainer;
		}

		// Set overwrite settings
		public TContainer OnComplete(EventHandler<TweenEventArgs> handler)
		{
			Options.CompleteEvent += handler;
			return this as TContainer;
		}

		// Set overwrite settings
		public TContainer OnError(EventHandler<TweenEventArgs> handler)
		{
			Options.ErrorEvent += handler;
			return this as TContainer;
		}

		///////////////////
		// Plugins

		// Make a plugin automatic (or revert)
		public TContainer Automate(TweenPluginInfo plugin, bool enable = true)
		{
			Options.SetAutomatic(plugin, enable);
			return this as TContainer;
		}

		///////////////////
		// Debug

		// Set overwrite settings
		public TContainer Log(TweenLogLevel level)
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
