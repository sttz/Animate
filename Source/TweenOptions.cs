using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Sttz.Tweener.Core;
using System.ComponentModel;

namespace Sttz.Tweener {

/// <summary>
/// Set of options that can be used when creating a tween or group.
/// </summary>
/// <remarks>
/// Use <see cref="Animate.Template"/> to create templates.
/// </remarks>
public class TweenTemplate : TweenOptionsContainer
{
	// Constructor (internal to force use of Animate.Template)
	internal TweenTemplate(TweenOptions parent = null)
	{
		Options.ParentOptions = parent;
	}

	protected override void ReturnToPool()
	{
		// Templates aren't recycled
	}
}

/// <summary>
/// Base class for <see cref="Tween"/> and <see cref="TweenGroup"/> that
/// contains the options and reference counting.
/// </summary>
public abstract class TweenOptionsContainer
{
	/// <summary>
	/// Options of the tween or group.
	/// </summary>
	public readonly TweenOptions Options = new TweenOptions();

	/// <summary>
	/// Retain count that prevents recycling of a tween or group when it's greater than zero.
	/// </summary>
	/// <remarks>
	/// By default, tweens and groups are recycled after they've completed. If you want to
	/// hang on to a tween or group, there are two options:
	/// - Disable recycling completely for a tween or group using <see cref="TweenOptions.Recycle"/>.
	/// - Increase the retain count as long as you're using a tween or group and then
	///   decrease it again to allow the tween or group to be recycled.
	/// </remarks>
	public uint RetainCount {
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
	protected uint _retainCount;

	/// <summary>
	/// Called when <see cref="RetainCount"/> reaches zero.
	/// </summary>
	protected abstract void ReturnToPool();
}

/// <summary>
/// Options for tweens or groups.
/// </summary>
/// <remarks>
/// Most options related to tweens are located in this separate class. Using 
/// <see cref="ParentOptions"/>, this allows options to be stacked:
/// - Global (<see cref="Animate.Options"/>)
/// - Template (optional, <see cref="Animate.Template"/>)
/// - Group (`group.Options` via <see cref="TweenOptionsContainer.Options"/>)
/// - Tween (`tween.Options` via <see cref="TweenOptionsContainer.Options"/>)
/// 
/// This stacking allows options to be set globally, per group or per tween.
/// It also allows options set on a higher level to be overwritten.
/// 
/// This class allows options to be read and set. For most use cases, the
/// fluid interface defined by <see cref="TweenOptionsFluid"/> is intended
/// to be used to set options.
/// </remarks>
public class TweenOptions
{
	// -------- Lifecycle --------

	/// <summary>
	/// Reset the options instance, so it can be reused.
	/// </summary>
	public void Reset()
	{
		_parent = null;
		_recycle = TweenRecycle.Undefined;
		_duration = float.NaN;
		_easing = null;
		_tweenTiming = TweenTiming.Undefined;
		_startDelay = float.NaN;
		_overwrite = TweenOverwrite.Undefined;
		_logLevel = TweenLogLevel.Undefined;
		if (_plugins != null) _plugins.Clear();
		_defaultPluginRequired = null;

		ResetEvents();
	}

	/// <summary>
	/// Remove all event listeners.
	/// </summary>
	internal void ResetEvents()
	{
		InitializeEvent = null;
		StartEvent = null;
		UpdateEvent = null;
		CompleteEvent = null;
		ErrorEvent = null;
	}

	// -------- Properties --------

	/// <summary>
	/// Duration of the tween in seconds.
	/// </summary>
	public float Duration
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

	/// <summary>
	/// Easing to apply to the tween. See <see cref="Easing"/> for
	/// possible values. <see cref="Easing.Linear"/> disables easing.
	/// </summary>
	public EasingMethod Easing
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

	/// <summary>
	/// Timing options of the tween. This sets if the tween is
	/// updated during `Update`, `FixedUpdate` or `LateUpdate`
	/// and which game time it's based on.
	/// </summary>
	/// <seealso cref="TweenTiming"/>
	public TweenTiming TweenTiming
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

	/// <summary>
	/// Delay before the tween starts (in seconds).
	/// </summary>
	public float StartDelay
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

	/// <summary>
	/// How conflicting tweens are resolved. See <see cref="TweenOverwrite"/>
	/// for all available options.
	/// </summary>
	public TweenOverwrite OverwriteSettings {
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

	/// <summary>
	/// The parent options instance, used when an option
	/// is not set in this instance.
	/// </summary>
	public TweenOptions ParentOptions
	{
		get {
			return _parent;
		}
		internal set {
			_parent = value;
		}
	}

	/// <summary>
	/// Wether groups and tweens are recycled.
	/// </summary>
	/// <seealso cref="TweenRecycle"/>
	public TweenRecycle Recycle {
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

	// -------- Events --------

	/// <summary>
	/// Event triggered when the tween initializes (usually in the frame
	/// after it was created or when calling <see cref="Tween.Validate"/>.)
	/// </summary>
	/// <remarks>
	/// Events bubble up the options instances, i.e. registering for a 
	/// group's initialize event will trigger for each tween in the group.
	/// 
	/// Listeners don't need to be removed. Once a tween completes or a
	/// group is recycled, all of its listeners will be removed.
	/// </remarks>
	public event EventHandler<TweenEventArgs> InitializeEvent;

	/// <summary>
	/// Trigger Initialize event.
	/// </summary>
	internal void TriggerInitialize(Tween tween, TweenEventArgs args = null)
	{
		if (InitializeEvent != null) {
			try {
				args = new TweenEventArgs(tween, TweenEvent.Initialize);
				InitializeEvent(this, args);
			} catch (Exception e) {
				Log(TweenLogLevel.Error, 
					"Exception in Initialize event handler: {0}".LazyFormat(e)
				);
			}
		}

		if (_parent != null) {
			(_parent as TweenOptions).TriggerInitialize(tween, args);
		}
	}

	/// <summary>
	/// Event triggered when the tween starts (after the start delay).
	/// </summary>
	/// <remarks>
	/// Events bubble up the options instances, i.e. registering for a 
	/// group's start event will trigger for each tween in the group.
	/// 
	/// Listeners don't need to be removed. Once a tween completes or a
	/// group is recycled, all of its listeners will be removed.
	/// </remarks>
	public event EventHandler<TweenEventArgs> StartEvent;

	/// <summary>
	/// Trigger Start event
	/// </summary>
	internal void TriggerStart(Tween tween, TweenEventArgs args = null)
	{
		if (StartEvent != null) {
			try {
				args = new TweenEventArgs(tween, TweenEvent.Start);
				StartEvent(this, args);
			} catch (Exception e) {
				Log(TweenLogLevel.Error, 
					"Exception in Start event handler: {0}".LazyFormat(e)
				);
			}
		}

		if (_parent != null) {
			(_parent as TweenOptions).TriggerInitialize(tween, args);
		}
	}

	/// <summary>
	/// Event triggered every time the tween has been updated.
	/// </summary>
	/// <remarks>
	/// Events bubble up the options instances, i.e. registering for a 
	/// group's update event will trigger for each tween in the group.
	/// 
	/// Listeners don't need to be removed. Once a tween completes or a
	/// group is recycled, all of its listeners will be removed.
	/// 
	/// Note that the Update event is only triggered if there are
	/// listeners registered when the tween starts. If you register
	/// a listener after the tween has started, it might not get called.
	/// </remarks>
	public event EventHandler<TweenEventArgs> UpdateEvent;

	/// <summary>
	/// Trigger Update event
	/// </summary>
	internal void TriggerUpdate(Tween tween, TweenEventArgs args = null)
	{
		if (UpdateEvent != null) {
			try {
				args = new TweenEventArgs(tween, TweenEvent.Update);
				UpdateEvent(this, args);
			} catch (Exception e) {
				Log(TweenLogLevel.Error, 
					"Exception in Update event handler: {0}".LazyFormat(e)
				);
			}
		}

		if (_parent != null) {
			(_parent as TweenOptions).TriggerUpdate(tween, args);
		}
	}

	/// <summary>
	/// Returns wether there are any Update listeners registered.
	/// </summary>
	internal bool HasUpdateListeners()
	{
		return (
			UpdateEvent != null 
			|| (
				_parent != null
				&& (_parent as TweenOptions).HasUpdateListeners()
			)
		);
	}

	/// <summary>
	/// Event triggered when the tween completes (either normally,
	/// by being stopped or overwritten, check 
	/// <see cref="TweenEventArgs.CompletedBy"/> if you need to know
	/// what completed the tween.
	/// </summary>
	/// <remarks>
	/// Events bubble up the options instances, i.e. registering for a 
	/// group's complete event will trigger for each tween in the group.
	/// 
	/// Listeners don't need to be removed. Once a tween completes or a
	/// group is recycled, all of its listeners will be removed.
	/// </remarks>
	public event EventHandler<TweenEventArgs> CompleteEvent;

	/// <summary>
	/// Trigger Complete event
	/// </summary>
	internal void TriggerComplete(
		Tween tween, 
		TweenCompletedBy stoppedBy, 
		TweenEventArgs args = null
	) {
		if (CompleteEvent != null) {
			try {
				args = new TweenEventArgs(tween, TweenEvent.Complete, null, stoppedBy);
				CompleteEvent(this, args);
			} catch (Exception e) {
				Log(TweenLogLevel.Error, 
					"Exception in Complete event handler: {0}".LazyFormat(e)
				);
			}
		}

		if (_parent != null) {
			(_parent as TweenOptions).TriggerComplete(tween, stoppedBy, args);
		}
	}

	/// <summary>
	/// Triggered when an error occurs. <see cref="TweenEventArgs.Error"/>
	/// or <see cref="Tween.Error"/> contain a description of the error.
	/// </summary>
	/// <remarks>
	/// Events bubble up the options instances, i.e. registering for a 
	/// group's error event will trigger for each tween in the group.
	/// 
	/// Listeners don't need to be removed. Once a tween completes or a
	/// group is recycled, all of its listeners will be removed.
	/// </remarks>
	public event EventHandler<TweenEventArgs> ErrorEvent;

	/// <summary>
	/// Trigger Error event
	/// </summary>
	internal void TriggerError(Tween tween, string error, TweenEventArgs args = null)
	{
		if (ErrorEvent != null) {
			try {
				args = new TweenEventArgs(tween, TweenEvent.Error, error);
				ErrorEvent(this, args);
			} catch (Exception e) {
				Log(TweenLogLevel.Error, 
					"Exception in Error event handler: {0}".LazyFormat(e)
				);
			}
		}

		if (_parent != null) {
			(_parent as TweenOptions).TriggerError(tween, error, args);
		}
	}

	// -------- Plugins --------

	/// <summary>
	/// Wether plugins loaded by <see cref="EnablePlugin"/> are required
	/// by default.
	/// </summary>
	/// <remarks>
	/// This gets enabled automatically for tween options
	/// instances.
	/// </remarks>
	public bool DefaultPluginRequired {
		get {
			if (_defaultPluginRequired != null) {
				return (bool)_defaultPluginRequired;
			} else if (_parent != null) {
				return _parent.DefaultPluginRequired;
			} else {
				return false;
			}
		}
		set {
			_defaultPluginRequired = value;
		}
	}

	/// <summary>
	/// Enable a plugin for automatic loading.
	/// </summary>
	/// <remarks>
	/// The order in which plugins are enabled matters. Plugins that are
	/// enabled later can override plugins enabled earlier.
	/// 
	/// Also, the options level they are defined it matters. Plugins that
	/// are loaded in child levels can override plugins in parent levels.
	/// 
	/// Finally, plugins can be re-enabled or re-disabled on lower
	/// levels, e.g. a plugin enabled at the global options level can be
	/// disabled in a group's options and then re-enabled in a tween's
	/// options.
	/// </remarks>
	/// <param name="loader">Plugin's loader callback</param>
	/// <param name="enabled">Wether to enable or disable the plugin (null = use parent's state)</param>
	/// <param name="required">Wether the tween fails if the plugin cannot be loaded</param>
	/// <seealso cref="TweenOptionsFluid.Plugin"/>
	public void EnablePlugin(PluginLoader loader, bool? enabled = true, bool? required = null)
	{
		if (enabled == null) {
			var index = _plugins.FindIndex(s => s.loader == loader);
			if (index >= 0) {
				_plugins.RemoveAt(index);
			}
		} else {
			var index = GetOrCreatePluginStateIndex(loader);
			var state = _plugins[index];
			state.enabled = (bool)enabled;
			state.required = required ?? DefaultPluginRequired;
			_plugins[index] = state;
		}
	}

	// -------- Logging --------

	/// <summary>
	/// Log level of the current scope. Only messages with equal
	/// or higher level will be logged to the console.
	/// </summary>
	/// <remarks>
	/// Since the log level is also part of the options stack, it's
	/// possible to raise the log level for only a single group
	/// or tween.
	/// </remarks>
	public TweenLogLevel LogLevel {
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

	/// <summary>
	/// Log a message on the current option instance, taking its
	/// log level into account.
	/// </summary>
	public void Log(TweenLogLevel level, string message)
	{
		if (level < LogLevel) return;

		message = string.Format("[{0}] {1}", level, message);
		if (level == TweenLogLevel.Debug) {
			Debug.Log(message);
		} else if (level == TweenLogLevel.Warning) {
			Debug.LogWarning(message);
		} else {
			Debug.LogError(message);
		}
	}

	public void Log(TweenLogLevel level, LazyFormatString message)
	{
		if (level >= LogLevel) Log(level, message.ToString());
	}

	// -------- Fields --------

	struct PluginState {
		public PluginLoader loader;
		public bool enabled;
		public bool required;

		public PluginState(PluginLoader loader)
		{
			this.loader = loader;
			this.enabled = false;
			this.required = false;
		}
	}

	TweenOptions _parent;
	TweenRecycle _recycle;
	float _duration = float.NaN;
	EasingMethod _easing;
	TweenTiming _tweenTiming;
	float _startDelay = float.NaN;
	TweenOverwrite _overwrite;
	TweenLogLevel _logLevel;
	List<PluginState> _plugins;
	bool? _defaultPluginRequired;

	// -------- Internals --------

	int GetOrCreatePluginStateIndex(PluginLoader loader)
	{
		if (_plugins == null) {
			_plugins = new List<PluginState>();
			_plugins.Add(new PluginState(loader));
			return 0;
		}

		var index = _plugins.FindIndex(s => s.loader == loader);
		if (index < 0) {
			_plugins.Add(new PluginState(loader));
			return _plugins.Count - 1;
		}

		return index;
	}

	static List<PluginState> _pluginList = new List<PluginState>();
	static List<int> _pluginListIndices = new List<int>();

	internal void LoadPlugins(Tween tween)
	{
		// This is a bit complicated because we want two properties:
		// 1. Plugins should be loaded in the order they were enabled and parents' 
		//    plugins before children's plugins
		// 2. Child plugin enabled/disabled state has priority over parent's
		//
		// E.g. (+ = enabled, - = disabled)
		// Global: PluginA+, PluginB+, PluginC+
		// Group: PluginB-, PluginD+
		// Tween: PluginA-, PluginB+
		// Actual load order: PluginC, PluginD, (PluginA), PluginB
		// -> PluginC is loaded first, because it's defined in parent, PluginA and PluginB are 
		// loaded last, because they are re-defined in the child.
		//
		// Therefore we first resolve the second, by creating a list of plugins,
		// recursing from the child to the parent, ignoring plugins that have already
		// been added previously. This ensures child state prevails and preserves 
		// order inside levels.
		//
		// E.g. PluginA-, PluginB+, PluginD+, PluginC+
		//
		// The list then contains only the states that are relevant but the order
		// of the levels is reversed. Therefore, we also need to save the level indices,
		// so that we can iterate the levels in reversed but the states in normal order.
		//
		// E.g. 0, 2, 3 -> iterate 3..3, 2..2 and then 0..1

		_pluginListIndices.Add(0);
		TweenOptions current = this;
		do {
			if (current._plugins != null) {
				for (int i = 0; i < current._plugins.Count; i++) {
					var state = current._plugins[i];
					var exists = false;
					for (int j = 0; j < _pluginList.Count; j++) {
						if (_pluginList[j].loader == state.loader) {
							exists = true;
							break;
						}
					}
					if (!exists) {
						_pluginList.Add(state);
					}
				}
				_pluginListIndices.Add(_pluginList.Count);
			}
			current = current._parent;
		} while (current != null);

		var lastIndex = _pluginList.Count;
		for (int i = _pluginListIndices.Count - 1; i >= 0; i--) {
			var index = _pluginListIndices[i];

			for (int j = index; j < lastIndex; j++) {
				var state = _pluginList[j];
				if (!state.enabled) continue;
				
				var result = state.loader(tween, state.required);
				if (result.isError) {
					if (state.required) {
						tween.PluginError(result.GetError());
						return;
					} else {
						tween.Options.Log(
							TweenLogLevel.Debug, 
							result.error
						);
						continue;
					}
				}

				tween.LoadPlugin(result.plugin, !state.required, result.userData);
			}

			lastIndex = index;
		}

		_pluginList.Clear();
		_pluginListIndices.Clear();
	}
}

/// <summary>
/// Extension methods implementing fluid <see cref="TweenOptions"/> interface.
/// </summary>
public static class TweenOptionsFluid
{
	// -------- Properties --------

	/// <summary>
	/// Duration of the tween (in seconds).
	/// </summary>
	/// <seealso cref="TweenOptions.Duration"/>
	public static TContainer Over<TContainer>(this TContainer container, float duration)
		where TContainer : TweenOptionsContainer
	{
		container.Options.Duration = duration;
		return container;
	}

	/// <summary>
	/// Easing to apply to the tween. See <see cref="Easing"/> for
	/// possible values. <see cref="Easing.Linear"/> disables easing.
	/// </summary>
	/// <seealso cref="TweenOptions.Easing"/>
	public static TContainer Ease<TContainer>(this TContainer container, EasingMethod easing)
		where TContainer : TweenOptionsContainer
	{
		container.Options.Easing = easing;
		return container;
	}

	/// <summary>
	/// Timing options of the tween. This sets if the tween is
	/// updated during `Update`, `FixedUpdate` or `LateUpdate`
	/// and which game time it's based on.
	/// </summary>
	/// <seealso cref="TweenOptions.TweenTiming"/>
	public static TContainer Timing<TContainer>(this TContainer container, TweenTiming timing)
		where TContainer : TweenOptionsContainer
	{
		container.Options.TweenTiming = timing;
		return container;
	}

	/// <summary>
	/// Delay before the tween starts (in seconds).
	/// </summary>
	/// <seealso cref="TweenOptions.StartDelay"/>
	public static TContainer Delay<TContainer>(this TContainer container, float seconds)
		where TContainer : TweenOptionsContainer
	{
		container.Options.StartDelay = seconds;
		return container;
	}

	/// <summary>
	/// How conflicting tweens are resolved. See <see cref="TweenOverwrite"/>
	/// for all available options.
	/// </summary>
	/// <seealso cref="TweenOptions.OverwriteSettings"/>
	public static TContainer Overwrite<TContainer>(this TContainer container, TweenOverwrite settings)
		where TContainer : TweenOptionsContainer
	{
		container.Options.OverwriteSettings = settings;
		return container;
	}

	/// <summary>
	/// Enable or disable a plugin for all tweens these options apply to.
	/// </summary>
	/// <param name="loader">The loader of the plugin</param>
	/// <param name="enable">Wether to enable or disable the plugin</param>
	/// <seealso cref="TweenOptions.EnablePlugin"/>
	public static TContainer Plugin<TContainer>(this TContainer container, PluginLoader loader, bool enable = true)
		where TContainer : TweenOptionsContainer
	{
		container.Options.EnablePlugin(loader, enable);
		return container;
	}

	/// <summary>
	/// Wether groups and tweens are recycled.
	/// </summary>
	/// <seealso cref="TweenOptions.Recycle"/>
	public static TContainer Recycle<TContainer>(this TContainer container, TweenRecycle recycle)
		where TContainer : TweenOptionsContainer
	{
		container.Options.Recycle = recycle;
		return container;
	}

	/// <summary>
	/// Increase the retain count, preventing the group or tween from being
	/// recycled. Pair this call with one to <see cref="Release"/> allow
	/// recycling again.
	/// </summary>
	/// <seealso cref="TweenOptionsContainer.RetainCount"/>
	public static TContainer Retain<TContainer>(this TContainer container)
		where TContainer : TweenOptionsContainer
	{
		container.RetainCount++;
		return container;
	}

	/// <summary>
	/// Decrease the retain count. Only call this method after having
	/// called <see cref="Retain"/> before.
	/// </summary>
	/// <seealso cref="TweenOptionsContainer.RetainCount"/>
	public static TContainer Release<TContainer>(this TContainer container)
		where TContainer : TweenOptionsContainer
	{
		container.RetainCount--;
		return container;
	}

	// -------- Events --------

	/// <summary>
	/// Event triggered when the tween initializes (usually in the frame
	/// after it was created or when calling <see cref="Tween.Validate"/>.)
	/// </summary>
	/// <seealso cref="TweenOptions.InitializeEvent"/>
	public static TContainer OnInitialize<TContainer>(this TContainer container, EventHandler<TweenEventArgs> handler)
		where TContainer : TweenOptionsContainer
	{
		container.Options.InitializeEvent += handler;
		return container;
	}

	/// <summary>
	/// Event triggered when the tween initializes (usually in the frame
	/// after it was created or when calling <see cref="Tween.Validate"/>.)
	/// </summary>
	/// <seealso cref="TweenOptions.InitializeEvent"/>
	public static TContainer OnInitialize<TContainer>(this TContainer container, Action<TweenEventArgs> handler)
		where TContainer : TweenOptionsContainer
	{
		container.Options.InitializeEvent += (sender, args) => handler(args);
		return container;
	}

	/// <summary>
	/// Event triggered when the tween initializes (usually in the frame
	/// after it was created or when calling <see cref="Tween.Validate"/>.)
	/// </summary>
	/// <seealso cref="TweenOptions.InitializeEvent"/>
	public static TContainer OnInitialize<TContainer>(this TContainer container, Action handler)
		where TContainer : TweenOptionsContainer
	{
		container.Options.InitializeEvent += (sender, args) => handler();
		return container;
	}

	/// <summary>
	/// Event triggered when the tween starts (after the start delay).
	/// </summary>
	/// <seealso cref="TweenOptions.StartEvent"/>
	public static TContainer OnStart<TContainer>(this TContainer container, EventHandler<TweenEventArgs> handler)
		where TContainer : TweenOptionsContainer
	{
		container.Options.StartEvent += handler;
		return container;
	}

	/// <summary>
	/// Event triggered when the tween starts (after the start delay).
	/// </summary>
	/// <seealso cref="TweenOptions.StartEvent"/>
	public static TContainer OnStart<TContainer>(this TContainer container, Action<TweenEventArgs> handler)
		where TContainer : TweenOptionsContainer
	{
		container.Options.StartEvent += (sender, args) => handler(args);
		return container;
	}

	/// <summary>
	/// Event triggered when the tween starts (after the start delay).
	/// </summary>
	/// <seealso cref="TweenOptions.StartEvent"/>
	public static TContainer OnStart<TContainer>(this TContainer container, Action handler)
		where TContainer : TweenOptionsContainer
	{
		container.Options.StartEvent += (sender, args) => handler();
		return container;
	}

	/// <summary>
	/// Event triggered every time the tween has been updated.
	/// </summary>
	/// <seealso cref="TweenOptions.UpdateEvent"/>
	public static TContainer OnUpdate<TContainer>(this TContainer container, EventHandler<TweenEventArgs> handler)
		where TContainer : TweenOptionsContainer
	{
		container.Options.UpdateEvent += handler;
		return container;
	}

	/// <summary>
	/// Event triggered every time the tween has been updated.
	/// </summary>
	/// <seealso cref="TweenOptions.UpdateEvent"/>
	public static TContainer OnUpdate<TContainer>(this TContainer container, Action<TweenEventArgs> handler)
		where TContainer : TweenOptionsContainer
	{
		container.Options.UpdateEvent += (sender, args) => handler(args);
		return container;
	}

	/// <summary>
	/// Event triggered every time the tween has been updated.
	/// </summary>
	/// <seealso cref="TweenOptions.UpdateEvent"/>
	public static TContainer OnUpdate<TContainer>(this TContainer container, Action handler)
		where TContainer : TweenOptionsContainer
	{
		container.Options.UpdateEvent += (sender, args) => handler();
		return container;
	}

	/// <summary>
	/// Event triggered when the tween completes (either normally,
	/// by being stopped or overwritten, check 
	/// <see cref="TweenEventArgs.CompletedBy"/> if you need to know
	/// what completed the tween.
	/// </summary>
	/// <seealso cref="TweenOptions.CompleteEvent"/>
	public static TContainer OnComplete<TContainer>(this TContainer container, EventHandler<TweenEventArgs> handler)
		where TContainer : TweenOptionsContainer
	{
		container.Options.CompleteEvent += handler;
		return container;
	}

	/// <summary>
	/// Event triggered when the tween completes (either normally,
	/// by being stopped or overwritten, check 
	/// <see cref="TweenEventArgs.CompletedBy"/> if you need to know
	/// what completed the tween.
	/// </summary>
	/// <seealso cref="TweenOptions.CompleteEvent"/>
	public static TContainer OnComplete<TContainer>(this TContainer container, Action<TweenEventArgs> handler)
		where TContainer : TweenOptionsContainer
	{
		container.Options.CompleteEvent += (sender, args) => handler(args);
		return container;
	}

	/// <summary>
	/// Event triggered when the tween completes (either normally,
	/// by being stopped or overwritten, check 
	/// <see cref="TweenEventArgs.CompletedBy"/> if you need to know
	/// what completed the tween.
	/// </summary>
	/// <seealso cref="TweenOptions.CompleteEvent"/>
	public static TContainer OnComplete<TContainer>(this TContainer container, Action handler)
		where TContainer : TweenOptionsContainer
	{
		container.Options.CompleteEvent += (sender, args) => handler();
		return container;
	}

	/// <summary>
	/// Triggered when an error occurs. <see cref="TweenEventArgs.Error"/>
	/// or <see cref="Tween.Error"/> contain a description of the error.
	/// </summary>
	/// <seealso cref="TweenOptions.ErrorEvent"/>
	public static TContainer OnError<TContainer>(this TContainer container, EventHandler<TweenEventArgs> handler)
		where TContainer : TweenOptionsContainer
	{
		container.Options.ErrorEvent += handler;
		return container;
	}

	/// <summary>
	/// Triggered when an error occurs. <see cref="TweenEventArgs.Error"/>
	/// or <see cref="Tween.Error"/> contain a description of the error.
	/// </summary>
	/// <seealso cref="TweenOptions.ErrorEvent"/>
	public static TContainer OnError<TContainer>(this TContainer container, Action<TweenEventArgs> handler)
		where TContainer : TweenOptionsContainer
	{
		container.Options.ErrorEvent += (sender, args) => handler(args);
		return container;
	}

	/// <summary>
	/// Triggered when an error occurs. <see cref="TweenEventArgs.Error"/>
	/// or <see cref="Tween.Error"/> contain a description of the error.
	/// </summary>
	/// <seealso cref="TweenOptions.ErrorEvent"/>
	public static TContainer OnError<TContainer>(this TContainer container, Action handler)
		where TContainer : TweenOptionsContainer
	{
		container.Options.ErrorEvent += (sender, args) => handler();
		return container;
	}

	// -------- Debug --------

	/// <summary>
	/// Log level of the current scope. Only messages with equal
	/// or higher level will be logged to the console.
	/// </summary>
	/// <seealso cref="TweenOptions.LogLevel"/>
	public static TContainer LogLevel<TContainer>(this TContainer container, TweenLogLevel level)
		where TContainer : TweenOptionsContainer
	{
		container.Options.LogLevel = level;
		return container;
	}

	// -------- Chain Finalizers --------

	/// <summary>
	/// Return a <c>WaitForSeconds</c> instruction that can be used
	/// in Unity coroutines to wait for the duration of the tween
	/// (delay + duration).
	/// </summary>
	/// <seealso cref="Tween.WaitForEndOfTween"/>
	public static WaitForSeconds WaitForTweenDuration<TContainer>(this TContainer container)
		where TContainer : TweenOptionsContainer
	{
		// Check duration is set
		if (float.IsNaN(container.Options.Duration)) {
			container.Options.Log(TweenLogLevel.Error,
				"No duration set on {0} for WaitForDuration()".LazyFormat(container));
			return null;
		}

		// Add delay to duration
		var duration = container.Options.Duration;
		if (!float.IsNaN(container.Options.StartDelay)) {
			duration += container.Options.StartDelay;
		}

		// Return yield instruction for the duration
		return new WaitForSeconds(duration);
	}
}

}
