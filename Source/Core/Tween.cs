using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Sttz.Tweener.Core {

	/// <summary>
	/// Internal tween options.
	/// </summary>
	public interface ITweenInternal : ITweenOptionsInternal
	{
		// Tween engine used by the tween
		ITweenEngine TweenEngine { get; set; }
		// Member info of the target property (if one exists)
		MemberInfo MemberInfo { get; }

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
	public class Tween<TValue> : TweenOptionsFluid<ITween>, ITween, ITweenInternal
	{
		///////////////////
		// Fields

		// Tween engine
		protected ITweenEngine _engine;

		// Tweening method
		protected TweenMethod _tweenMethod;
		// Target object of the tween
		protected object _target;
		// Property on the tween to animate
		protected string _property;
		// Member info of the target property (if one exists)
		protected MemberInfo _memberInfo;

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
		// Time the tween was created (Time.realtimeSinceStartup)
		protected float _creationTimeReal;
		// Cached start time
		protected float _startTime;
		// Is the tween already validated?
		protected bool _validated;
		// Have values already been prepared?
		protected bool _valuesPrepared;

		// Plugins requested to be activated for this tween
		protected TweenPluginInfo[] _plugins;

		// Plugin hook to get value
		protected TweenPluginInfo _hookGetInfo;
		protected ITweenPlugin<TValue> _hookGet;
		// Plugin hook to set value
		protected TweenPluginInfo _hookSetInfo;
		protected ITweenPlugin<TValue> _hookSet;
		// Plugin hook to calculate value
		protected TweenPluginInfo _hookCalculateInfo;
		protected ITweenPlugin<TValue> _hookCalculate;

		// Cached (1 / _duration) for performance
		protected float _oneOverDuration;
		// Cached typed reference to check if Unity objects are destroyed
		protected bool _targetIsUnityObject;
		protected UnityEngine.Object _targetUnityObject;
		protected bool _targetIsUnityRef;
		protected UnityEngine.TrackedReference _targetUnityReference;
		// Cached flag if real time should be used
		protected bool _realTime;
		// Cached flag if udpate event needs to be triggered
		protected bool _triggerUpdate;

		///////////////////
		// Constructor

		// Constructor, using of the Tween.* static methods
		// is strongly encouraged.
		public static Tween<TValue> Create(
			TweenMethod tweenMethod,
			object target, 
			float duration,
			string property, 
			TValue startValue,
			TValue endValue,
			TValue diffValue,
			TweenPluginInfo[] plugins,
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
			Tween<TValue> tween = null;
			if (Animate.Pool != null) {
				tween = Animate.Pool.GetTween<TValue>();
			} else {
				tween = new Tween<TValue>();
			}

			// Setup instance
			tween.Use(
				tweenMethod, target, duration, property, 
				startValue, endValue, diffValue, 
				plugins, parentOptions
			);
			return tween;
		}

		// Initialize an instance returned from the pool
		public void Use(
			TweenMethod tweenMethod,
			object target, 
			float duration,
			string property,
			TValue startValue,
			TValue endValue,
			TValue diffValue,
			TweenPluginInfo[] plugins,
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
			_plugins = plugins;
			_parent = parentOptions;

			// Set creation time
			_creationTime = Time.time;
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
			_memberInfo = null;

			_startValue = default(TValue);
			_endValue = default(TValue);
			_diffValue = default(TValue);

			_error = null;
			_completedBy = TweenCompletedBy.Undefined;
			_creationTime = 0;
			_creationTimeReal = 0;
			_startTime = 0;
			_validated = false;
			_valuesPrepared = false;

			_plugins = null;
			_hookGet = null;
			_hookGetInfo = default(TweenPluginInfo);
			_hookSet = null;
			_hookSetInfo = default(TweenPluginInfo);
			_hookCalculate = null;
			_hookCalculateInfo = default(TweenPluginInfo);

			_state = TweenState.Unused;
		}

		///////////////////
		// Initialization

		// Validate the tween early, optionally forcing a render of the tween
		public bool Validate(bool forceRender = false)
		{
			// Load default plugin
			var defaultInfo = Options.DefaultPlugin;
			if (defaultInfo.manualActivation != null) {
				defaultInfo = defaultInfo.manualActivation(this, defaultInfo);
			}
			_hookGetInfo = _hookSetInfo = _hookCalculateInfo = defaultInfo;

			// Register automatic plugins
			var autoPlugins = GetAutomaticPlugins();
			foreach (var pluginInfo in autoPlugins) {
				// Check plugin type
				if (!PluginCheckType(pluginInfo)) continue;
				// Check if plugin should be registered
				var data = pluginInfo.autoActivation(this, pluginInfo);
				if (data.pluginType == null) continue;
				// Try to register plugin
				if (!RegisterPlugin(data, false)) {
					// Automatic plugins are optional
					continue;
				}
			}

			// Register manual plugins
			if (_plugins != null) {
				foreach (var pluginInfo in _plugins) {
					var data = pluginInfo;
					// Let plugin choose concrete type
					if (data.manualActivation != null) {
						data = data.manualActivation(this, data);
					}
					// Plugin manual activation failed
					if (data.pluginType == null) {
						// TODO: Error message from plugin
						Fail("Plugin {0} failed to activate.",  pluginInfo.pluginType);
						return false;
					}
					// Validate type
					if (!PluginCheckType(data)) {
						Fail("Plugin {0} cannot handle tween type {1} of property {2} on {3}.",
						     data.pluginType, typeof(TValue), _property, _target);
						return false;
					}
					if (!RegisterPlugin(data, true)) {
						// Manual plugins are required
						return false;
					}
				}
			}

			// Check conditions
			if (_hookGetInfo.pluginType == null) {
				Fail("No plugin registered for get value hook on tween of {0} on {1}.", _property, _target);
				return false;
			}
			if (_hookSetInfo.pluginType == null) {
				Fail("No plugin registered for set value hook on tween of {0} on {1}.", _property, _target);
				return false;
			}
			if (_hookCalculateInfo.pluginType == null) {
				Fail("No plugin registered for calculate value hook on tween of {0} on {1}.", _property, _target);
				return false;
			}
			if (float.IsNaN(Options.Duration)) {
				Fail("Duration not set for tween of {0} on {1}.", _property, _target);
				return false;
			}
			if (Options.Duration == 0) {
				Fail("Zero durations set for tween of {0} on {1}.", _property, _target);
				return false;
			}

			// Load plugins
			_hookGet = GetPlugin(_hookGetInfo);
			_hookSet = GetPlugin(_hookSetInfo);
			_hookCalculate = GetPlugin(_hookCalculateInfo);
			if (_hookGet == null || _hookSet == null || _hookCalculate == null) return false;

			Log(TweenLogLevel.Debug, 
			    "Tweening {0} on {1} with {2}, {3} and {4}.",
			    _property, _target, _hookGet, _hookSet, _hookCalculate
			);

			// Initialize plugins
			string error;

			error = _hookGet.Initialize(this, TweenPluginHook.GetValue, ref _hookGetInfo.getValueUserData);
			if (error != null) {
				Fail("{0}: {1}", _hookGet, error);
				return false;
			}

			error = _hookSet.Initialize(this, TweenPluginHook.SetValue, ref _hookSetInfo.setValueUserData);
			if (error != null) {
				Fail("{0}: {1}", _hookSet, error);
				return false;
			}

			error = _hookCalculate.Initialize(this, TweenPluginHook.CalculateValue, ref _hookCalculateInfo.calculateValueUserData);
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

		// Check if the plugin type is compatible
		protected bool PluginCheckType(TweenPluginInfo info)
		{
			// No type given
			if (info.pluginType == null) {
				return false;
			// If it's open type, assume we can close it
			} else if (info.pluginType.ContainsGenericParameters) {
				return true;
			}

			// Look for ITweenPlugin interface and check its type parameter
			var interfaces = info.pluginType.GetInterfaces();
			foreach (var iface in interfaces) {
				if (iface.GetGenericTypeDefinition() == typeof(ITweenPlugin<>)) {
					// Check type argument of interface
					if (iface.GetGenericArguments()[0] == typeof(TValue)) {
						return true;
					} else {
						return false;
					}
				}
			}

			// Type does not implement ITweenPlugin!
			return false;
		}

		// Get a shared plugin instance
		protected ITweenPlugin<TValue> GetPlugin(TweenPluginInfo info)
		{
			// Get closed type
			var type = info.pluginType;
			if (type.ContainsGenericParameters) {
				type = type.MakeGenericType(typeof(TValue));
			}
			
			// Create new instance
			if (!_sharedPlugins.ContainsKey(type)) {
				var instance = Activator.CreateInstance(type);
				_sharedPlugins[type] = instance;
			}

			// Return existing instance
			return _sharedPlugins[type] as ITweenPlugin<TValue>;
		}

		// Register plugins
		protected bool RegisterPlugin(TweenPluginInfo info, bool fail)
		{
			int getHook = 0, setHook = 0, calcHook = 0;

			// Check if hooks are available, automatic plugins fail silently
			if ((info.hooks & TweenPluginHook.GetValue) > 0) {
				if (_hookGetInfo.pluginType == null) {
					getHook = 1;
				} else {
					getHook = CheckHook(_hookGetInfo.hooks, info.hooks, TweenPluginHook.GetValue);
					if (getHook == -1) {
						RegisterPluginError(fail, 
							"GetValueHook required by {0} already used by {1}.",
							info.pluginType, _hookGetInfo.pluginType);
						return false;
					}
				}
			}

			if ((info.hooks & TweenPluginHook.SetValue) > 0) {
				if (_hookSetInfo.pluginType == null) {
					setHook = 1;
				} else {
					setHook = CheckHook(_hookSetInfo.hooks, info.hooks, TweenPluginHook.SetValue);
					if (setHook == -1) {
						RegisterPluginError(fail, 
							"SetValueHook required by {0} already used by {1}.",
							info.pluginType, _hookSetInfo.pluginType);
						return false;
					}
				}
			}

			if ((info.hooks & TweenPluginHook.CalculateValue) > 0) {
				if (_hookCalculateInfo.pluginType == null) {
					calcHook = 1;
				} else {
					calcHook = CheckHook(_hookCalculateInfo.hooks, info.hooks, TweenPluginHook.CalculateValue);
					if (calcHook == -1) {
						RegisterPluginError(fail, 
							"CalculateValueHook required by {0} already used by {1}.",
							info.pluginType, _hookCalculateInfo.pluginType);
						return false;
					}
				}
			}

			// Plugin doesn't want any hooks
			if (getHook != 1 && setHook != 1 && calcHook != 1) return true;

			// Register hooks
			if (getHook == 1) {
				_hookGetInfo = info;
			}
			if (setHook == 1) {
				_hookSetInfo = info;
			}
			if (calcHook == 1) {
				_hookCalculateInfo = info;
			}
			return true;
		}

		// Plugin error that optionally fails
		protected void RegisterPluginError(bool fail, string message, params object[] args)
		{
			if (!fail) {
				Log(TweenLogLevel.Debug, message, args);
			} else {
				Fail(message, args);
			}
		}

		// Check a single combination of hook flags
		// Returns: 1 = overwrite, 0 = don't overwrite, -1 = error
		protected int CheckHook(TweenPluginHook one, TweenPluginHook two, TweenPluginHook flag)
		{
			// weak <- weak		true
			// weak <- strong	true
			// strong <- weak	false
			// strong <- strong	error

			// Determine weak flag
			TweenPluginHook flagWeak;
			if (flag == TweenPluginHook.GetValue) {
				flagWeak = TweenPluginHook.GetValueWeak;
			} else if (flag == TweenPluginHook.SetValue) {
				flagWeak = TweenPluginHook.SetValueWeak;
			} else {
				flagWeak = TweenPluginHook.CalculateValueWeak;
			}

			// Check if flag is set in both masks
			if ((one & two & flag) > 0) {
				// One is weak: Always overwrite
				if ((one & flagWeak) == flagWeak) {
					return 1;
				// Two is weak: Don't overwrite
				} else if ((two & flagWeak) == flagWeak) {
					return 0;
				// Two is string: Error
				} else {
					return -1;
				}
			}

			// No conflicting flags: All ok
			return 1;
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
				if ((Options.TweenTiming & TweenTiming.RealTime) > 0) {
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

		// Time the tween will start or has started in real time (unaffected by Time.timeScale)
		public float StartTimeReal {
			get {
				// Creation time in real time
				var startTime = _creationTimeReal;
				// Start delay (convert to real time if necessary)
				var startDelay = Options.StartDelay;
				if (!float.IsNaN(startDelay)) {
					if ((Options.TweenTiming & TweenTiming.RealTime) > 0) {
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

		// Duration of the tween in real time (unaffected by Time.timeScale)
		public float DurationReal {
			get {
				var duration = Options.Duration;
				// Conver duration to real time
				if ((Options.TweenTiming & TweenTiming.RealTime) == 0) {
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
				if ((Options.TweenTiming & TweenTiming.RealTime) > 0) {
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
		public object Target {
			get {
				return _target;
			}
			set {
				_target = value;
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

		// Target member info, if property exists
		public MemberInfo MemberInfo {
			get {
				// Try to get member
				if (_memberInfo == null) {
					_memberInfo = TweenReflection.FindMember(_target.GetType(), _property);
				}
				return _memberInfo;
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

			// Calculate in real time if one tween is running in real time
			if (((Options.TweenTiming | other.Options.TweenTiming) & TweenTiming.RealTime) > 0) {
				startTime = StartTimeReal;
				duration = DurationReal;
				otherStartTime = other.StartTimeReal;
				otherDuration = other.DurationReal;

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
			while (_state <= TweenState.Complete) {
				if ((_tweenTiming & TweenTiming.LateUpdate) > 0) {
					yield return new WaitForEndOfFrame();
				} else if ((_tweenTiming & TweenTiming.FixedUpdate) > 0) {
					yield return new WaitForFixedUpdate();
				} else {
					yield return null;
				}
			}
		}

		///////////////////
		// Value wrapper

		// Read / Write the target property value 
		// using reflection and generated IL code
		public TValue Value {
			get {
				try {
					return _hookGet.GetValue(_target, _property, ref _hookGetInfo.getValueUserData);
				} catch (Exception e) {
					Fail("Tween stopped because of exception: {0}", e);
					return default(TValue);
				}
			}
			set {
				try {
					_hookSet.SetValue(_target, _property, value, ref _hookSetInfo.setValueUserData);
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
					_diffValue = _hookCalculate.DiffValue(_startValue, _endValue, ref _hookCalculateInfo.calculateValueUserData);
				} else if (_tweenMethod == TweenMethod.From) {
					_endValue = Value;
					_diffValue = _hookCalculate.DiffValue(_startValue, _endValue, ref _hookCalculateInfo.calculateValueUserData);
				} else if (_tweenMethod == TweenMethod.FromTo) {
					_diffValue = _hookCalculate.DiffValue(_startValue, _endValue, ref _hookCalculateInfo.calculateValueUserData);
				} else if (_tweenMethod == TweenMethod.By) {
					_startValue = Value;
					_endValue = _hookCalculate.EndValue(_startValue, _diffValue, ref _hookCalculateInfo.calculateValueUserData);
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
			_realTime = ((_tweenTiming & TweenTiming.RealTime) > 0);
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
			if (completedBy == TweenCompletedBy.Cancel) {
				Value = ValueAtPosition(0f);
			} else if (completedBy == TweenCompletedBy.Finish) {
				Value = ValueAtPosition(1f);
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
					ref _hookCalculateInfo.calculateValueUserData
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
			float time = (_realTime ? Time.realtimeSinceStartup : Time.time);

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
			if (_easing != null) {
				position = _easing(position);
			}

			// Apply value
			Value = ValueAtPosition(position);

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
	}

}