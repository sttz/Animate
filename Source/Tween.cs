using System;
using System.Collections;
using UnityEngine;
using System.Text.RegularExpressions;

using Sttz.Tweener.Core;

namespace Sttz.Tweener {

/// <summary>
/// Individual tween.
/// </summary>
/// <remarks>
/// There are several ways to create tweens. Use the methods on 
/// <see cref="Animate"/> to create single tweens that use the global
/// default options. Use <see cref="Animate.On"/> or 
/// <see cref="Animate.Group"/> to create a group and then add tweens
/// to the group using the methods on <see cref="TweenGroup"/>. This 
/// allows to set options on the group, which will then apply to all 
/// tweens in that group.
/// 
/// You can also create tweens using the static methods on <see cref="Tween"/>.
/// Note that those tweens are not automatically registered with Animate,
/// you'll have to add them to a group using <see cref="TweenGroup{TTarget}.Add"/>.
/// This allows to set individual tween options for tweens in a group.
/// 
/// All the events of the tween will be cleared once it's being recycled,
/// you normally don't have to unregister event handlers you register.
/// </remarks>
public abstract class Tween : TweenOptionsContainer
{
	// -------- Properties --------

	/// <summary>
	/// Current state of the tween.
	/// </summary>
	public TweenState State {
		get {
			return _state;
		}
	}

	/// <summary>
	/// When an error occurred (<see cref="State"/> is set to <see cref="TweenState.Error"/>), this
	/// property contains an error description.
	/// </summary>
	public string Error {
		get {
			return _error;
		}
	}

	/// <summary>
	/// Explains how the tween was completed.
	/// </summary>
	/// <remarks>
	/// This field is set as soon as the tween's state becomes 
	/// <see cref="TweenState.Complete"/> and <see cref="TweenOptions.CompleteEvent"/> fires.
	/// </remarks>
	/// <seealso cref="TweenEventArgs.CompletedBy"/>
	public TweenCompletedBy CompletedBy {
		get {
			return _completedBy;
		}
	}

	/// <summary>
	/// The time the tween was created (in the tween's time).
	/// </summary>
	/// <seealso cref="TweenTiming"/>
	public float CreationTime {
		get {
			if ((Options.TweenTiming & TweenTiming.UnscaledTime) > 0) {
				return _creationTimeUnscaled;
			} else if ((Options.TweenTiming & TweenTiming.RealTime) > 0) {
				return _creationTimeReal;
			} else {
				return _creationTime;
			}
		}
	}

	/// <summary>
	/// The time the tween will be or was started (in the tween's time).
	/// </summary>
	/// <seealso cref="TweenTiming"/>
	public float StartTime {
		get {
			var startTime = CreationTime;
			var startDelay = Options.StartDelay;
			if (!float.IsNaN(startDelay)) {
				startTime += startDelay;
			}
			return startTime;
		}
	}

	/// <summary>
	/// Time the tween will be or was started, in unscaled time.
	/// </summary>
	public float StartTimeUnscaled {
		get {
			// Creation time in unscaled time
			var startTime = _creationTimeUnscaled;
			// Start delay (convert to unscaled time if necessary)
			var startDelay = Options.StartDelay;
			if (!float.IsNaN(startDelay)) {
				if ((Options.TweenTiming & (TweenTiming.UnscaledTime | TweenTiming.RealTime)) > 0) {
					startTime += startDelay;
				} else {
					if (Time.timeScale > 0) {
						startTime += startDelay / Time.timeScale;
					}
				}
			}
			return startTime;
		}
	}

	/// <summary>
	/// Duration of the tween, in unscaled time.
	/// </summary>
	/// <seealso cref="TweenOptions.Duration"/>
	public float DurationUnscaled {
		get {
			var duration = Options.Duration;
			// Convert duration to unscaled time
			if ((Options.TweenTiming & (TweenTiming.UnscaledTime | TweenTiming.RealTime)) == 0) {
				if (Time.timeScale > 0) {
					duration /= Time.timeScale;
				} else {
					duration = 0;
				}
			}
			return duration;
		}
	}

	/// <summary>
	/// Now in the tween's time.
	/// </summary>
	/// <seealso cref="TweenTiming"/>
	public float TweenTime {
		get {
			if ((Options.TweenTiming & TweenTiming.UnscaledTime) > 0) {
				return Time.unscaledTime;
			} else if ((Options.TweenTiming & TweenTiming.RealTime) > 0) {
				return Time.realtimeSinceStartup;
			} else {
				return Time.time;
			}
		}
	}

	/// <summary>
	/// The tween engine used for this tween.
	/// </summary>
	public ITweenEngine TweenEngine {
		get {
			return _engine;
		}
		set {
			_engine = value;
		}
	}

	/// <summary>
	/// Method of the tween (To, From, FromTo or By).
	/// </summary>
	public TweenMethod TweenMethod {
		get {
			return _tweenMethod;
		}
	}

	/// <summary>
	/// Target object the tweened property is located on.
	/// </summary>
	/// <exception cref="System.InvalidCastException">
	/// Thrown when the object's type does not match the tween's target type.
	/// </exception>
	public abstract object Target { get; set; }

	/// <summary>
	/// Name of the target property on the target object.
	/// </summary>
	public string Property {
		get {
			return _property;
		}
	}

	/// <summary>
	/// Type of the target the value is tweened on.
	/// </summary>
	public abstract Type TargetType { get; }

	/// <summary>
	/// Type of tweened value of the tween.
	/// </summary>
	/// <remarks>
	/// A tween's type has to match the type of the target property.
	/// Make sure the value you supply when creating a tween has the
	/// same type as the property that's being tweened.
	/// </remarks>
	public abstract Type ValueType { get; }

	// -------- Control Tween --------

	/// <summary>
	/// Stop the tween (leave the property at its current value).
	/// </summary>
	public void Stop()
	{
		Complete(TweenCompletedBy.Stop);
	}

	/// <summary>
	/// Finish the tween (set the property to its end value).
	/// </summary>
	public void Finish()
	{
		Complete(TweenCompletedBy.Finish);
	}

	/// <summary>
	/// Cancel the tween (set the property to its start value).
	/// </summary>
	public void Cancel()
	{
		Complete(TweenCompletedBy.Cancel);
	}

	/// <summary>
	/// Check if two tween's durations overlap (without start delay).
	/// </summary>
	/// <param name="other">Tween to check against this tween</param>
	/// <returns>Wether the tween's durations overlap</returns>
	public bool Overlaps(Tween other)
	{
		if (_state >= TweenState.Complete) return false;

		float startTime, duration, endTime, otherStartTime, otherDuration, otherEndTime;

		// Calculate in unscaled time if one tween is running in unscaled or real time
		if (((Options.TweenTiming | other.Options.TweenTiming) & (TweenTiming.UnscaledTime | TweenTiming.RealTime)) > 0) {
			startTime = StartTimeUnscaled;
			duration = DurationUnscaled;
			otherStartTime = other.StartTimeUnscaled;
			otherDuration = other.DurationUnscaled;

		} else {
			startTime = StartTime;
			duration = Options.Duration;
			otherStartTime = other.StartTime;
			otherDuration = other.Options.Duration;
		}

		// If Either duration is 0 (possible if Time.timeScale == 0),
		// we consider the tween as disabled
		if (duration == 0 || otherDuration == 0) {
			return false;
		}

		endTime = startTime + duration;
		otherEndTime = otherStartTime + otherDuration;

		return (
			(startTime >= otherStartTime && startTime < otherEndTime)
			|| (endTime >= otherStartTime && endTime < otherEndTime)
		);
	}

	/// <summary>
	/// Check if another tween overwrites this one and complete this
	/// tween as necessary.
	/// </summary>
	/// <param name="other">Tween that potentially overwrites this one</param>
	internal void Overwrite(Tween other)
	{
		if (_state >= TweenState.Complete) return;

		// Don't overwrite ourself
		if (this == other) return;

		var settings = other.Options.OverwriteSettings;

		// Check overlapping
		if ((settings & TweenOverwrite.Overlapping) > 0
				&& !Overlaps(other)) {
			return;
		}

		// Call appropriate method
		if ((settings & TweenOverwrite.Cancel) > 0) {
			Options.Log(TweenLogLevel.Debug, "Overwrite {0} on {1} with Cancel.".LazyFormat(_property, Target));
			Complete(TweenCompletedBy.Cancel, true);
		} else if ((settings & TweenOverwrite.Finish) > 0) {
			Options.Log(TweenLogLevel.Debug, "Overwrite {0} on {1} with Finish.".LazyFormat(_property, Target));
			Complete(TweenCompletedBy.Finish, true);
		} else {
			Options.Log(TweenLogLevel.Debug, "Overwrite {0} on {1} with Stop.".LazyFormat(_property, Target));
			Complete(TweenCompletedBy.Stop, true);
		}
	}

	/// <summary>
	/// Create a coroutine that will wait until the tween has completed.
	/// </summary>
	/// <returns>
	/// A coroutine you can yield in one of your own to wait until
	/// the tween has completed.
	/// </returns>
	/// <seealso cref="TweenOptionsFluid.WaitForTweenDuration"/>
	public Coroutine WaitForEndOfTween()
	{
		if (_engine == null) {
			Options.Log(TweenLogLevel.Error, 
				"Tween of {0} on {1} needs to be added to a group "
				+ "before WaitForEndOfTween() can be used."
				.LazyFormat(_property, Target));
			return null;
		}
		return (_engine as MonoBehaviour).StartCoroutine(WaitFOrEndOfTWeenCoroutine());
	}

	// Coroutine implementation for WaitForEndOfTween
	protected IEnumerator WaitFOrEndOfTWeenCoroutine()
	{
		RetainCount++;

		while (_state < TweenState.Complete) {
			if ((_timing & TweenTiming.LateUpdate) > 0) {
				yield return new WaitForEndOfFrame();
			} else if ((_timing & TweenTiming.FixedUpdate) > 0) {
				yield return new WaitForFixedUpdate();
			} else {
				yield return null;
			}
		}

		RetainCount--;
	}

	// -------- Lifecycle --------

	/// <summary>
	/// Reset the tween so that it can be used again.
	/// </summary>
	public virtual void Reset()
	{
		Options.Reset();

		_engine = null;
		_retainCount = 0;
		_tweenMethod = TweenMethod.To;
		_property = null;
		_options = null;

		_targetIsUnityObject = false;
		_targetIsUnityRef = false;
		_targetUnityObject = null;
		_targetUnityReference = null;

		_error = null;
		_completedBy = TweenCompletedBy.Undefined;
		_creationTime = 0;
		_creationTimeUnscaled = 0;
		_creationTimeReal = 0;
		_startTime = 0;
		_validated = false;
		_valuesPrepared = false;

		_state = TweenState.Unused;
	}

	protected override void ReturnToPool()
	{
		// Return to pool
		if (_engine.Pool != null
				&& Options.Recycle != TweenRecycle.None
				&& (Options.Recycle & TweenRecycle.Tweens) > 0) {
			_engine.Pool.Return(this);
		}
	}

	// -------- Initialization --------

	/// <summary>
	/// Trigger the validation of the tween and optionally force 
	/// it to be rendered.
	/// </summary>
	/// <remarks>
	/// Usually, tweens are validated during initialization in the frame
	/// after they were created. You can call this method to validate
	/// the tween right after creating it. This will make
	/// the stack trace of validation errors more useful as it will point
	/// to where the tween was created instead of to the where it was 
	/// initialized.
	/// 
	/// Tweens will first render (set their target property) when
	/// they are started, which is usually during the next frame after they
	/// were created or after their start delay. Especially when doing a From
	/// or FromTo tween, you might want the initial value to be set 
	/// immediately to avoid visual glitches. Validate the tween and 
	/// set the <c>forceRender</c> parameter to true in this case.
	/// </remarks>
	/// <param name='forceRender'>
	/// Forces the tween to render (set its target property to the initial 
	/// value) after it has been validated.
	/// </param>
	public bool Validate(bool forceRender = false)
	{
		// Check conditions
		if (float.IsNaN(Options.Duration)) {
			Fail("Duration not set for tween of {0} on {1}.", _property, Target);
			return false;
		}
		if (Options.Duration == 0) {
			Fail("Zero durations set for tween of {0} on {1}.", _property, Target);
			return false;
		}

		// Load plugins
		if (!LoadPlugins()) return false;

		// Force a render
		if (forceRender) {
			PrepareValues();
			ApplyValue(0);
		}

		// All good!
		_validated = true;
		return true;
	}

	// -------- Plugin API --------

	/// <summary>
	/// The getter plugin being used.
	/// </summary>
	public abstract ITweenPlugin GetterPlugin { get; }

	/// <summary>
	/// The setter plugin being used.
	/// </summary>
	public abstract ITweenPlugin SetterPlugin { get; }

	/// <summary>
	/// The arithmetic plugin being used.
	/// </summary>
	public abstract ITweenPlugin ArithmeticPlugin { get; }

	// Regex used to parse options from property string
	// for automatic plugins.
	private static readonly Regex OptionsRegex = new Regex(
		@"^:(?<options>\w+):(?<property>.*)$",
		RegexOptions.ExplicitCapture
	);

	/// <summary>
	/// Parse the options in the property name in the format
	/// ":options:propertyName".
	/// </summary>
	/// <value></value>
	public string PropertyOptions {
		get {
			if (_options == null) {
				// Try to parse options from property
				var match = OptionsRegex.Match(_property);
				if (!match.Success) {
					_options = "";
				} else {
					// Remove options from tween property
					_property = match.Groups["property"].Value;
					_options = match.Groups["options"].Value;
				}
			}
			return _options;
		}
	}

	/// <summary>
	/// Load a plugin.
	/// </summary>
	/// <remarks>
	/// This needs to be called before the tween is initialized.
	/// </remarks>
	/// <param name="plugin">The plugin to load</param>
	/// <param name="weak">Wether the plugin can be overwritten by other plugins</param>
	/// <param name="userData">Plugin user data passed to the plugin when called</param>
	/// <returns>Wether the plugin was loaded. Note that the plugin can be overwritten later.</returns>
	internal abstract bool LoadPlugin(ITweenPlugin plugin, bool weak, object userData = null);

	/// <summary>
	/// Use this method to trigger a fatal error from a plugin.
	/// </summary>
	/// <param name="format">Format string</param>
	/// <param name="args">Format arguments</param>
	internal void PluginError(string format, params object[] args)
	{
		if (_state == TweenState.Error) return;

		Fail("Error from plugin {0}", string.Format(format, args));
	}

	// -------- Internals --------

	protected ITweenEngine _engine;
	protected TweenMethod _tweenMethod;
	protected string _property;
	protected string _options;

	protected TweenState _state;
	protected string _error;
	protected TweenCompletedBy _completedBy;
	protected float _creationTime;
	protected float _creationTimeUnscaled;
	protected float _creationTimeReal;
	protected float _startTime;
	protected bool _validated;
	protected bool _valuesPrepared;

	protected float _oneOverDuration;
	protected bool _targetIsUnityObject;
	protected UnityEngine.Object _targetUnityObject;
	protected bool _targetIsUnityRef;
	protected UnityEngine.TrackedReference _targetUnityReference;
	protected TweenTiming _timing;
	protected bool _triggerUpdate;

	// Trigger an error and abort the tween
	protected void Fail(string message, params object[] args)
	{
		Fail(TweenLogLevel.Error, message, args);
	}

	// Trigger a silent error with custom log level
	protected void Fail(TweenLogLevel level, string message, params object[] args)
	{
		_state = TweenState.Error;

		_error = string.Format(message, args);
		Options.Log(level, _error);

		Options.TriggerError(this, _error);

		// Remove all local event listeners
		Options.ResetEvents();
	}

	// Overwrite existing tweens
	protected void DoOverwrite()
	{
		var settings = Options.OverwriteSettings;

		// Check if enabled
		if (settings == TweenOverwrite.Undefined 
				|| settings == TweenOverwrite.None) return;

		// OnInitialize only if state == Waiting
		if ((settings & TweenOverwrite.OnInitialize) > 0
				&& _state != TweenState.Waiting) {
			return;
		
		// Default OnStart only if state == Tweening
		} else if ((settings & TweenOverwrite.OnInitialize) == 0
				&& _state != TweenState.Tweening) {
			return;
		}

		_engine.Overwrite(this);
	}

	/// <summary>
	/// Load the tween's plugins.
	/// </summary>
	/// <remarks>
	/// This calls <see cref="ITweenEngine.LoadDynamicPlugins"/> and
	/// <see cref="TweenOptions.LoadPlugins"/> to load the default
	/// plugins. Manual plugins should be loaded using a extension
	/// method on <c>Tween</c>.
	/// </remarks>
	protected abstract bool LoadPlugins();

	// Check a single combination of hook flags
	// Returns: 1 = overwrite, 0 = don't overwrite, -1 = error
	protected int GetPluginOverride(bool installedWeak, bool overwriterWeak)
	{
		// weak <- weak		true
		// weak <- strong	true
		// strong <- weak	false
		// strong <- strong	error

		// Installed is weak: Always overwrite
		if (installedWeak) {
			return 1;
		// Overwriter is weak: Don't overwrite
		} else if (overwriterWeak) {
			return 0;
		// Two is strong: Error
		} else {
			return -1;
		}
	}

	// Prepare the from, to, diff values
	protected abstract void PrepareValues();

	// Initialize the tween
	protected void Initialize()
	{
		_state = TweenState.Waiting;

		// Validate tween
		if (!_validated && !Validate()) {
			return;
		}

		DoOverwrite();
		Options.TriggerInitialize(this);

		// Cache start time
		_startTime = StartTime;
	}

	// Start the tween
	protected void Start()
	{
		_state = TweenState.Tweening;

		DoOverwrite();
		PrepareValues();

		// Cache most frequently used options
		_oneOverDuration = 1f / Options.Duration;
		_timing = Options.TweenTiming;
		_triggerUpdate = Options.HasUpdateListeners();

		Options.TriggerStart(this);
	}

	// Complete the tween
	protected void Complete(TweenCompletedBy completedBy, bool fromOverwrite = false)
	{
		if (_state >= TweenState.Complete) return;

		_state = TweenState.Complete;
		_completedBy = completedBy;

		// Set to start / end value
		if (completedBy == TweenCompletedBy.Cancel || completedBy == TweenCompletedBy.Finish) {
			if (!_validated && !Validate())
				return;

			if (completedBy == TweenCompletedBy.Cancel) {
				ApplyValue(0);
			} else {
				ApplyValue(1);
			}

			if (_state == TweenState.Error)
				return;
		}

		// Add overwrite to cause
		if (fromOverwrite) {
			completedBy |= TweenCompletedBy.Overwrite;
		}

		// Trigger event
		Options.TriggerComplete(this, completedBy);

		// Remove all local event listeners
		Options.ResetEvents();
	}

	protected abstract void ApplyValue(float position);

	// Update tween
	internal bool Update()
	{
		// Check if unity object was destroyed
		// In this case we get == null only if the target is typed
		// to UnityEngine.Object, otherwise it will never be null.
		if ((_targetIsUnityObject && _targetUnityObject == null)
				|| (_targetIsUnityRef && _targetUnityReference == null)) {
			Fail(TweenLogLevel.Debug,
				"Tween of {0} on {1} stopped because unity object was destroyed.",
				_property, Target);
			return false;
		}

		// Current time
		float time = TweenTime;

		// Handle non-tweening state
		if (_state == TweenState.Error) {
			return false;
		} else if (_state != TweenState.Tweening) {
			// Already over
			if (_state >= TweenState.Complete) {
				return false;
			
			// Prepare for tweening
			} else if (_state < TweenState.Tweening) {
				// Already completed, error or pooled instance
				if (_state == TweenState.Unused) {
					return false;
				}

				// Initialize tween
				if (_state == TweenState.Uninitialized) {
					Initialize();
					if (_state == TweenState.Error) {
						return false;
					}
				}

				// Wait to start tween
				if (_state == TweenState.Waiting) {
					if (time < _startTime) {
						return true;
					} else {
						Start();
						if (_state == TweenState.Error) {
							return false;
						}
					}
				}
			}
		}

		// Update tween
		var position = Mathf.Clamp01((time - _startTime) * _oneOverDuration);
		var easedPosition = position;
		if (Options.Easing != null) {
			easedPosition = Options.Easing(position);
		}

		// Apply value
		ApplyValue(easedPosition);

		if (_triggerUpdate) {
			Options.TriggerUpdate(this);
		}

		// Complete tween
		if (position >= 1) {
			Complete(TweenCompletedBy.Complete);
			return false;
		}

		return true;
	}
}

/// <summary>
/// Concrete generic implementation of <see cref="Tween"/>.
/// </summary>
/// <remarks>
/// You don't normally need to interact with this subtype. Refer to <see cref="Tween"/>
/// or the helper methods on <see cref="Animate"/> instead.
/// </remarks>
/// <typeparam name="TTarget">Type of the target object (needs to be a reference type)</typeparam>
/// <typeparam name="TValue">Type of the target property</typeparam>
public class Tween<TTarget, TValue> : Tween where TTarget : class
{
	// -------- Properties --------

	/// <summary>
	/// Start value of the tween (set when the tween is started).
	/// </summary>
	public TValue StartValue {
		get {
			return _startValue;
		}
	}

	/// <summary>
	/// End value of the tween (set when the tween is started).
	/// </summary>
	public TValue EndValue {
		get {
			return _endValue;
		}
	}

	/// <summary>
	/// Difference between start and end value of the tween (set when the tween is started).
	/// </summary>
	public TValue DiffValue {
		get {
			return _diffValue;
		}
	}

	/// <summary>
	/// Current value of the target property.
	/// </summary>
	public TValue Value {
		get {
			try {
				return _hookGet.GetValue(_target, _property, ref _hookGetUserData);
			} catch (Exception e) {
				Fail("Tween stopped because of exception: {0}", e);
				return default(TValue);
			}
		}
		set {
			try {
				_hookSet.SetValue(_target, _property, value, ref _hookSetUserData);
			} catch (Exception e) {
				Fail("Tween stopped because of exception: {0}", e);
			}
		}
	}

	public override object Target {
		get {
			return _target;
		}
		set {
			_target = (TTarget)value;
		}
	}

	public override Type TargetType {
		get {
			return typeof(TTarget);
		}
	}

	public override Type ValueType {
		get {
			return typeof(TValue);
		}
	}

	public override ITweenPlugin GetterPlugin {
		get { return _hookGet; }
	}

	public override ITweenPlugin SetterPlugin {
		get { return _hookSet; }
	}

	public override ITweenPlugin ArithmeticPlugin {
		get { return _hookCalculate; }
	}

	// -------- Lifecycle --------

	public void Use(
		TweenMethod tweenMethod,
		TTarget target, 
		float duration,
		string property,
		TValue startValue,
		TValue endValue,
		TValue diffValue,
		TweenOptions parentOptions
	) {
		if (_state != TweenState.Unused) {
			throw new Exception("Trying to re-use a tween that hasn't been reset yet.");
		}

		_state = TweenState.Uninitialized;

		// Initialize values
		_tweenMethod = tweenMethod;
		_target = target;
		Options.Duration = duration;
		_property = property;
		_startValue = startValue;
		_endValue = endValue;
		_diffValue = diffValue;
		Options.ParentOptions = parentOptions;
		Options.DefaultPluginRequired = true;

		if (target is UnityEngine.Object) {
			_targetIsUnityObject = true;
			_targetUnityObject = target as UnityEngine.Object;
		} else if (target is UnityEngine.TrackedReference) {
			_targetIsUnityRef = true;
			_targetUnityReference = target as UnityEngine.TrackedReference;
		}

		// Set creation time
		_creationTime = Time.time;
		_creationTimeUnscaled = Time.unscaledTime;
		_creationTimeReal = Time.realtimeSinceStartup;
	}

	public override void Reset()
	{
		base.Reset();

		_target = null;

		_startValue = default(TValue);
		_endValue = default(TValue);
		_diffValue = default(TValue);

		_hookGetWeak = false;
		_hookGetUserData = null;
		_hookGet = null;
		_hookSetWeak = false;
		_hookSetUserData = null;
		_hookSet = null;
		_hookCalculateWeak = false;
		_hookCalculateUserData = null;
		_hookCalculate = null;
	}

	// -------- Initialization --------

	protected override bool LoadPlugins()
	{
		_engine.LoadDynamicPlugins(this);
		Options.LoadPlugins(this);

		if (_state == TweenState.Error)
			return false;

		if (_hookGet == null || _hookSet == null || _hookCalculate == null) {
			Fail("Missing plugins for tween of {0} ({1}) on {2} ({3}): Getter = {4}, Setter = {5}, Arithmetic = {6}",
				_property, ValueType, _target, TargetType, _hookGet, _hookSet, _hookCalculate);
			return false;
		}

		Options.Log(TweenLogLevel.Debug,
			"Tweening {0} on {1} with {2}, {3} and {4}."
			.LazyFormat(_property, _target, _hookGet, _hookSet, _hookCalculate)
		);

		// Initialize plugins
		string error;

		// TODO: Combine init?
		error = _hookGet.Initialize(this, TweenPluginType.Getter, ref _hookGetUserData);
		if (error != null) {
			Fail("{0}: {1}", _hookGet, error);
			return false;
		}

		error = _hookSet.Initialize(this, TweenPluginType.Setter, ref _hookSetUserData);
		if (error != null) {
			Fail("{0}: {1}", _hookSet, error);
			return false;
		}

		error = _hookCalculate.Initialize(this, TweenPluginType.Arithmetic, ref _hookCalculateUserData);
		if (error != null) {
			Fail("{0}: {1}", _hookCalculate, error);
			return false;
		}

		return true;
	}

	internal override bool LoadPlugin(ITweenPlugin plugin, bool weak, object userData = null)
	{
		if (_state >= TweenState.Complete || plugin == null) {
			return false;
		}

		var getterPlugin = plugin as ITweenGetterPlugin<TTarget, TValue>;
		if (getterPlugin != null) {
			var action = GetPluginOverride(_hookGetWeak || _hookGet == null, weak);
			if (action == 1) {
				_hookGet = getterPlugin;
				_hookGetWeak = weak;
				_hookGetUserData = userData;
			
			} else if (action == -1) {
				var error = string.Format(
					"Load plugin: Getter hook required by {0} already used by {1}.",
					plugin.GetType().Name, _hookGet.GetType().Name
				);
				if (weak) {
					Options.Log(TweenLogLevel.Debug, error);
				} else {
					Fail(error);
					return false;
				}
			}
		}

		var setterPlugin = plugin as ITweenSetterPlugin<TTarget, TValue>;
		if (setterPlugin != null) {
			var action = GetPluginOverride(_hookSetWeak || _hookSet == null, weak);
			if (action == 1) {
				_hookSet = setterPlugin;
				_hookSetWeak = weak;
				_hookSetUserData = userData;

			} else if (action == -1) {
				var error = string.Format(
					"Load Plugin: Setter hook required by {0} already used by {1}.",
					plugin.GetType().Name, _hookSet.GetType().Name
				);
				if (weak) {
					Options.Log(TweenLogLevel.Debug, error);
				} else {
					Fail(error);
					return false;
				}
			}
		}

		var arithmeticPlugin = plugin as ITweenArithmeticPlugin<TValue>;
		if (arithmeticPlugin != null) {
			var action = GetPluginOverride(_hookCalculateWeak || _hookCalculate == null, weak);
			if (action == 1) {
				_hookCalculate = arithmeticPlugin;
				_hookCalculateWeak = weak;
				_hookCalculateUserData = userData;

			} else if (action == -1) {
				var error = string.Format(
					"Load Plugin: Arithmetic hook required by {0} already used by {1}.",
					plugin.GetType().Name, _hookCalculate.GetType().Name
				);
				if (weak) {
					Options.Log(TweenLogLevel.Debug, error);
				} else {
					Fail(error);
					return false;
				}
			}
		}

		return true;
	}

	// -------- Internals --------

	protected TTarget _target;

	protected TValue _startValue;
	protected TValue _endValue;
	protected TValue _diffValue;

	protected bool _hookGetWeak;
	protected object _hookGetUserData;
	protected ITweenGetterPlugin<TTarget, TValue> _hookGet;
	protected bool _hookSetWeak;
	protected object _hookSetUserData;
	protected ITweenSetterPlugin<TTarget, TValue> _hookSet;
	protected bool _hookCalculateWeak;
	protected object _hookCalculateUserData;
	protected ITweenArithmeticPlugin<TValue> _hookCalculate;

	protected override void PrepareValues()
	{
		if (_valuesPrepared) return;

		try {
			if (_tweenMethod == TweenMethod.To) {
				_startValue = Value;
				_diffValue = _hookCalculate.DiffValue(_startValue, _endValue, ref _hookCalculateUserData);
			} else if (_tweenMethod == TweenMethod.From) {
				_endValue = Value;
				_diffValue = _hookCalculate.DiffValue(_startValue, _endValue, ref _hookCalculateUserData);
			} else if (_tweenMethod == TweenMethod.FromTo) {
				_diffValue = _hookCalculate.DiffValue(_startValue, _endValue, ref _hookCalculateUserData);
			} else if (_tweenMethod == TweenMethod.By) {
				_startValue = Value;
				_endValue = _hookCalculate.EndValue(_startValue, _diffValue, ref _hookCalculateUserData);
			}
		} catch (Exception e) {
			Fail("Tween stopped because of exception: {0}", e);
			return;
		}

		_valuesPrepared = true;
	}

	protected override void ApplyValue(float position)
	{
		try {
			Value = _hookCalculate.ValueAtPosition(
				_startValue, _endValue, _diffValue,
				position,
				ref _hookCalculateUserData
			);
		} catch (Exception e) {
			Fail("Tween stopped because of exception: {0}", e);
		}
	}

	public override string ToString()
	{
		return string.Format("[Tween: {0} '{1}' on '{2}' from {3} by {4} in {5}]", _tweenMethod, _property, _target, _startValue, _diffValue, _state);
	}
}

}
