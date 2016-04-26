using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Sttz.Tweener.Core {

	/// <summary>
	/// Internal tween options.
	/// </summary>
	public interface ITweenInternal : ITweenOptionsInternal
	{
		// Tween engine used by the tween
		ITweenEngine TweenEngine { get; set; }

		// Method of tween (To, From, FromTo, By)
		TweenMethod TweenMethod { get; set; }
		// Target object the property is modified on
		object Target { get; set; }
		// Name of property to tween
		string Property { get; set; }

		// Overwrite this tween by another one based on its settings
		void Overwrite(ITween other);

		// Reset the tween, Use() needs to be called before it can be used again
		void Reset();
		// Update tween
		bool Update();
	}

	// Single tween
	public class Tween<TTarget, TValue> : TweenOptionsFluid<Tween<TTarget, TValue>>, ITween, ITweenInternal
		where TTarget : class
	{
		///////////////////
		// Fields

		// Tween engine
		protected ITweenEngine _engine;

		// Tweening method
		protected TweenMethod _tweenMethod;
		// Target object of the tween
		protected TTarget _target;
		// Property on the tween to animate
		protected string _property;
		// Options parsed from the property
		protected string _options;

		// First value (To, From, By)
		protected TValue _startValue;
		// Second value (To in FromTo)
		protected TValue _endValue;
		// Difference between start and end
		protected TValue _diffValue;

		// Tween state
		protected TweenState _state;
		// Description of the error if _state == TweenState.Error
		protected string _error;
		// Reason tween was completed
		protected TweenCompletedBy _completedBy;
		// Time the tween was created (Time.time)
		protected float _creationTime;
		// Time the tween was created (Time.unscaledTime)
		protected float _creationTimeUnscaled;
		// Time the tween was created (Time.realTimeSinceStartup)
		protected float _creationTimeReal;
		// Cached start time
		protected float _startTime;
		// Is the tween already validated?
		protected bool _validated;
		// Have values already been prepared?
		protected bool _valuesPrepared;

		// Plugin hook to get value
		protected bool _hookGetWeak;
		protected object _hookGetUserData;
		protected ITweenGetterPlugin<TTarget, TValue> _hookGet;
		// Plugin hook to set value
		protected bool _hookSetWeak;
		protected object _hookSetUserData;
		protected ITweenSetterPlugin<TTarget, TValue> _hookSet;
		// Plugin hook to calculate value
		protected bool _hookCalculateWeak;
		protected object _hookCalculateUserData;
		protected ITweenArithmeticPlugin<TValue> _hookCalculate;

		// Cached (1 / _duration) for performance
		protected float _oneOverDuration;
		// Cached typed reference to check if Unity objects are destroyed
		protected bool _targetIsUnityObject;
		protected UnityEngine.Object _targetUnityObject;
		protected bool _targetIsUnityRef;
		protected UnityEngine.TrackedReference _targetUnityReference;
		// Cached teen timing value
		protected TweenTiming _timing;
		// Cached flag if udpate event needs to be triggered
		protected bool _triggerUpdate;

		///////////////////
		// Constructor

		// Constructor, using of the Tween.* static methods
		// is strongly encouraged.
		public static Tween<TTarget, TValue> Create(
			TweenMethod tweenMethod,
			TTarget target, 
			float duration,
			string property, 
			TValue startValue,
			TValue endValue,
			TValue diffValue,
			ITweenOptions parentOptions = null
		) {
			// Basic sanity checks
			if (target == null) {
				Animate.Options.Internal.Log(
					TweenLogLevel.Error, 
					"Trying to tween {0} on a null object.", property
				);
				return null;
			}
			if (property == null) {
				Animate.Options.Internal.Log(
					TweenLogLevel.Error, 
					"Property to tween on object {0} is null.", target
				);
				return null;
			}

			// Get instance from pool or create new one
			Tween<TTarget, TValue> tween = null;
			if (Animate.Pool != null) {
				tween = Animate.Pool.GetTween<TTarget, TValue>();
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

		// Initialize an instance returned from the pool
		public void Use(
			TweenMethod tweenMethod,
			TTarget target, 
			float duration,
			string property,
			TValue startValue,
			TValue endValue,
			TValue diffValue,
			ITweenOptions parentOptions
		) {
			if (_state != TweenState.Unused) {
				throw new Exception("Trying to re-use a tween that hasn't been reset yet.");
			}

			_state = TweenState.Uninitialized;

			// Initialize values
			_tweenMethod = tweenMethod;
			_target = target;
			_duration = duration;
			_property = property;
			_startValue = startValue;
			_endValue = endValue;
			_diffValue = diffValue;
			_parent = parentOptions;

			// Set creation time
			_creationTime = Time.time;
			_creationTimeUnscaled = Time.unscaledTime;
			_creationTimeReal = Time.realtimeSinceStartup;
		}

		// Reset the tween back to an uninitialized state
		public override void Reset()
		{
			base.Reset();

			_engine = null;
			_tweenMethod = TweenMethod.To;
			_target = null;
			_property = null;
			_options = null;

			_targetIsUnityObject = false;
			_targetIsUnityRef = false;
			_targetUnityObject = null;
			_targetUnityReference = null;

			_startValue = default(TValue);
			_endValue = default(TValue);
			_diffValue = default(TValue);

			_error = null;
			_completedBy = TweenCompletedBy.Undefined;
			_creationTime = 0;
			_creationTimeUnscaled = 0;
			_creationTimeReal = 0;
			_startTime = 0;
			_validated = false;
			_valuesPrepared = false;

			_hookGetWeak = false;
			_hookGetUserData = null;
			_hookGet = null;
			_hookSetWeak = false;
			_hookSetUserData = null;
			_hookSet = null;
			_hookCalculateWeak = false;
			_hookCalculateUserData = null;
			_hookCalculate = null;

			_state = TweenState.Unused;
		}

		protected override void ReturnToPool()
		{
			// Return to pool
			if (Animate.Pool != null
					&& Options.Recycle != TweenRecycle.None
					&& (Options.Recycle & TweenRecycle.Tweens) > 0) {
				Animate.Pool.Return(this);
			}
		}

		///////////////////
		// Initialization

		// Validate the tween early, optionally forcing a render of the tween
		public bool Validate(bool forceRender = false)
		{
			// Check conditions
			if (float.IsNaN(Options.Duration)) {
				Fail("Duration not set for tween of {0} on {1}.", _property, _target);
				return false;
			}
			if (Options.Duration == 0) {
				Fail("Zero durations set for tween of {0} on {1}.", _property, _target);
				return false;
			}

			// Load plugins
			// TODO: Make this independent of UnityTweenEngine
			UnityTweenEngine.Instance.LoadPlugins(this);

			if (_state == TweenState.Error)
				return false;

			if (_hookGet == null || _hookSet == null || _hookCalculate == null) {
				Fail("Missing plugins: Getter = {0}, Setter = {1}, Arithmetic = {2}",
					_hookGet, _hookSet, _hookCalculate);
				return false;
			}

			Log(TweenLogLevel.Debug,
				"Tweening {0} on {1} with {2}, {3} and {4}.",
				_property, _target, _hookGet, _hookSet, _hookCalculate
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

			// Force a render
			if (forceRender) {
				PrepareValues();
				Value = ValueAtPosition(0);
			}

			// All good!
			_validated = true;
			return true;
		}

		///////////////////
		// Plugin API

		// Regex used to parse options from property string
		// for automatic plugins.
		private static readonly Regex OptionsRegex = new Regex(
			@"^:(?<options>\w+):(?<property>.*)$",
			RegexOptions.ExplicitCapture
		);

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

		public bool LoadPlugin(ITweenPlugin plugin, bool weak, object userData = null)
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
						Log(TweenLogLevel.Debug, error);
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
						Log(TweenLogLevel.Debug, error);
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
						Log(TweenLogLevel.Debug, error);
					} else {
						Fail(error);
						return false;
					}
				}
			}

			return true;
		}

		public void PluginError(string pluginName, string format, params object[] args)
		{
			if (_state == TweenState.Error) return;

			Fail(
				"Error from plugin {0}: {1}", 
			    pluginName, string.Format(format, args)
			);
		}

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

		///////////////////
		// Properties

		public ITweenInternal Internal {
			get {
				return this;
			}
		}

		// Current state of the tween
		public TweenState State {
			get {
				return _state;
			}
		}

		// Error description, if one occured
		public string Error {
			get {
				return _error;
			}
		}

		// Reason the tween was completed
		public TweenCompletedBy CompletedBy {
			get {
				return _completedBy;
			}
		}

		// Target object of the tween
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

		// Time the tween will start or has started
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

		// Time the tween will start or has started in unscaled time (unaffected by Time.timeScale)
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

		// Duration of the tween in unscaled time (unaffected by Time.timeScale)
		public float DurationUnscaled {
			get {
				var duration = Options.Duration;
				// Conver duration to unscaled time
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

		// Time based on the timing
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

		// Target object of the tween
		public ITweenEngine TweenEngine {
			get {
				return _engine;
			}
			set {
				_engine = value;
			}
		}

		// Target object of the tween
		public TweenMethod TweenMethod {
			get {
				return _tweenMethod;
			}
			set {
				_tweenMethod = value;
			}
		}

		// Target object of the tween
		public TTarget Target {
			get {
				return _target;
			}
			set {
				_target = value;
			}
		}

		// Target object of the tween
		object ITween.Target {
			get {
				return _target;
			}
		}

		// Target object of the tween
		object ITweenInternal.Target {
			get {
				return _target;
			}
			set {
				_target = (TTarget)value;
			}
		}

		// Property to tween on the target
		public string Property {
			get {
				return _property;
			}
			set {
				_property = value;
			}
		}

		// Target of tween
		public Type TargetType {
			get {
				return typeof(TTarget);
			}
		}

		// Value of tween
		public Type ValueType {
			get {
				return typeof(TValue);
			}
		}

		// First value
		public TValue StartValue {
			get {
				return _startValue;
			}
		}

		// Second value
		public TValue EndValue {
			get {
				return _endValue;
			}
		}

		// Diff value
		public TValue DiffValue {
			get {
				return _diffValue;
			}
		}

		///////////////////
		// Control Tween

		// Stop the tween, leaving the target at the current value
		public void Stop()
		{
			Complete(TweenCompletedBy.Stop);
		}

		// Finish the tween, jumping to the final value
		public void Finish()
		{
			Complete(TweenCompletedBy.Finish);
		}

		// Cancel the tween, jumping back to the initial value
		public void Cancel()
		{
			Complete(TweenCompletedBy.Cancel);
		}

		// Check if this tween overlaps another
		public bool Overlaps(ITween other)
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
			otherEndTime = otherStartTime + duration;

			return (
				(startTime >= otherStartTime && startTime < otherEndTime)
				|| (endTime >= otherStartTime && endTime < otherEndTime)
			);
		}

		// Let tween be overridden by another one
		public void Overwrite(ITween other)
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
			if ((settings & Sttz.Tweener.TweenOverwrite.Cancel) > 0) {
				Log(TweenLogLevel.Debug, "Overwrite {0} on {1} with Cancel.", _property, _target);
				Complete(TweenCompletedBy.Cancel, true);
			} else if ((settings & Sttz.Tweener.TweenOverwrite.Finish) > 0) {
				Log(TweenLogLevel.Debug, "Overwrite {0} on {1} with Finish.", _property, _target);
				Complete(TweenCompletedBy.Finish, true);
			} else {
				Log(TweenLogLevel.Debug, "Overwrite {0} on {1} with Stop.", _property, _target);
				Complete(TweenCompletedBy.Stop, true);
			}
		}

		// Return a coroutine that waits for the tween to complete
		public Coroutine WaitForEndOfTween()
		{
			if (_engine == null) {
				Log(TweenLogLevel.Error, 
					"Tween of {0} on {1} needs to be added to a group "
					+ "before WaitForEndOfTween() can be used.",
					_property, _target);
				return null;
			}
			return (_engine as MonoBehaviour).StartCoroutine(WaitFOrEndOfTWeenCoroutine());
		}

		// Coroutine implementation for WaitForEndOfTween
		protected IEnumerator WaitFOrEndOfTWeenCoroutine()
		{
			Retain();

			while (_state < TweenState.Complete) {
				if ((_timing & TweenTiming.LateUpdate) > 0) {
					yield return new WaitForEndOfFrame();
				} else if ((_timing & TweenTiming.FixedUpdate) > 0) {
					yield return new WaitForFixedUpdate();
				} else {
					yield return null;
				}
			}

			Release();
		}

		///////////////////
		// Value wrapper

		// Read / Write the target property value 
		// using reflection and generated IL code
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

		///////////////////
		// Engine

		// Trigger an error and abort the tween
		protected void Fail(string message, params object[] args)
		{
			Fail(TweenLogLevel.Error, message, args);
		}

		// Trigger a silent error with custom log level
		protected void Fail(TweenLogLevel level, string message, params object[] args)
		{
			_state = TweenState.Error;

			_error = message;
			Log(level, message, args);

			TriggerError(this, message);

			// Remove all local event listeners
			ResetEvents();
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

		// Prepare the from, to, diff values
		protected void PrepareValues()
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

		// Initialize the tween
		protected void Initialize()
		{
			_state = TweenState.Waiting;

			// Validate tween
			if (!_validated && !Validate()) {
				return;
			}

			DoOverwrite();
			TriggerInitialize(this);
		}

		// Start the tween
		protected void Start()
		{
			_state = TweenState.Tweening;

			PrepareValues();

			// Cache most frequently used options
			_startTime = StartTime;
			_oneOverDuration = 1f / Options.Duration;
			_easing = Options.Easing;
			_timing = Options.TweenTiming;
			_triggerUpdate = HasUpdateListeners();

			_targetUnityObject = (Target as UnityEngine.Object);
			_targetIsUnityObject = (_targetUnityObject != null);
			_targetUnityReference = (Target as UnityEngine.TrackedReference);
			_targetIsUnityRef = (_targetUnityReference != null);

			DoOverwrite();
			TriggerStart(this);
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

				TValue finalValue;
				if (completedBy == TweenCompletedBy.Cancel) {
					finalValue = ValueAtPosition(0f);
				} else {
					finalValue = ValueAtPosition(1f);
				}

				if (_state == TweenState.Error)
					return;

				Value = finalValue;
			}

			// Add overwrite to cause
			if (fromOverwrite) {
				completedBy |= TweenCompletedBy.Overwrite;
			}

			// Trigger event
			TriggerComplete(this, completedBy);

			// Remove all local event listeners
			ResetEvents();
		}

		// Get tweened value at position
		protected TValue ValueAtPosition(float position)
		{
			try {
				return _hookCalculate.ValueAtPosition(
					_startValue, _endValue, _diffValue,
					position,
					ref _hookCalculateUserData
				);
			} catch (Exception e) {
				Fail("Tween stopped because of exception: {0}", e);
				return default(TValue);
			}
		}

		// Upate tween
		public bool Update()
		{
			// Check if unity object was destroyed
			// In this case we get == null only if the target is typed
			// to UnityEngine.Object, otherwise it will never be null.
			if ((_targetIsUnityObject && _targetUnityObject == null)
					|| (_targetIsUnityRef && _targetUnityReference == null)) {
				Fail(TweenLogLevel.Debug,
					"Tween of {0} on {1} stopped because unity object was destroyed.",
					_property, _target);
				return false;
			}

			// Current time
			float time = TweenTime;

			// Handle non-tweening state
			if (_state != TweenState.Tweening) {
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
					}

					// Wait to start tween
					if (_state == TweenState.Waiting) {
						if (time >= _startTime) {
							Start();
						}
					}
				}

				// Check for error during init/start
				if (_state == TweenState.Error) {
					return false;
				}
			}

			// Update tween
			var position = Mathf.Clamp01((time - _startTime) * _oneOverDuration);
			var easedPosition = position;
			if (_easing != null) {
				easedPosition = _easing(position);
			}

			// Apply value
			Value = ValueAtPosition(easedPosition);

			if (_triggerUpdate) {
				TriggerUpdate(this);
			}

			// Complete tween
			if (position >= 1) {
				Complete(TweenCompletedBy.Complete);
				return false;
			}

			return true;
		}

		public override string ToString()
		{
			return string.Format("[Tween: {0} '{1}' on '{2}' from {3} by {4} in {5}]", _tweenMethod, _property, _target, _startValue, _diffValue, _state);
		}
	}

}