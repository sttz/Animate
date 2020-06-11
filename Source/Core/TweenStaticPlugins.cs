using System;
using System.Collections.Generic;

namespace Sttz.Tweener.Core {

/// <summary>
/// Default accessor plugin using user-provided accessor methods.
/// </summary>
/// <remarks>
/// To access properties on arbitrary objects without using reflection,
/// this plugin allows the user to provide getter and setter callbacks.
/// This allows Animate to be used without using reflection or codegen
/// but requires a bit of setup for each property that is tweened.
/// 
/// It also allows to set up virtual properties that don't actually
/// exist on a type and do some additional processing. E.g. defining
/// a "position.x" on Transform to tween only the x coordinate.
/// </remarks>
public static class TweenStaticAccessorPlugin
{
	// -------- Usage --------

	/// <summary>
	/// Delegate for callbacks accessing a property.
	/// </summary>
	/// <param name="target">Target to get the property from</param>
	/// <typeparam name="TTarget">The type of the target</typeparam>
	/// <typeparam name="TValue">The type of the value</typeparam>
	/// <returns>The property value</returns>
	public delegate TValue GetAccessor<TTarget, TValue>(TTarget target) where TTarget : class;

	/// <summary>
	/// Delegate for callback setting a property.
	/// </summary>
	/// <param name="target">Target to set the property on</param>
	/// <param name="value">Value to set the property to</param>
	/// <typeparam name="TTarget">The type of the target</typeparam>
	/// <typeparam name="TValue">The type of the value</typeparam>
	public delegate void SetAccessor<TTarget, TValue>(TTarget target, TValue value) where TTarget : class;

	/// <summary>
	/// Teach the static accessor plugin to access a property on a type.
	/// </summary>
	/// <seealso cref="Animate.EnableAccess"/>
	public static void EnableAccess<TTarget, TValue>(
		string propertyName, 
		GetAccessor<TTarget, TValue> getter, SetAccessor<TTarget, TValue> setter
	)
		where TTarget : class
	{
		var key = PluginKey(typeof(TTarget), typeof(TValue), propertyName);
		plugins[key] = new TweenStaticAccessorPluginImpl<TTarget, TValue>(getter, setter);
	}

	/// <summary>
	/// TweenStaticAccessorPlugin plugin loader.
	/// </summary>
	/// <remarks>
	/// Pass this method to <see cref="TweenOptions.EnablePlugin"/> to enable the
	/// plugin for the options scope.
	/// </remarks>
	public static PluginResult Loader(Tween tween, bool required)
	{
		ITweenPlugin plugin;
		if (!plugins.TryGetValue(PluginKey(tween.TargetType, tween.ValueType, tween.Property), out plugin)) {
			return PluginResult.Error(
				"TweenStaticAccessorPlugin: No accessor registered for property {0} ({1}) on type {2}."
				.LazyFormat(tween.Property, tween.ValueType, tween.TargetType)
			);
		}

		return PluginResult.Load(plugin);
	}

	// -------- Internals --------

	static string PluginKey(Type targetType, Type valueType, string property)
	{
		return targetType.FullName + "/" + valueType.FullName + "/" + property;
	}

	static Dictionary<string, ITweenPlugin> plugins = new Dictionary<string, ITweenPlugin>();

	/// <summary>
	/// Default accessor plugin using precompiled methods.
	/// </summary>
	private class TweenStaticAccessorPluginImpl<TTarget, TValue> :
		ITweenGetterPlugin<TTarget, TValue>, ITweenSetterPlugin<TTarget, TValue>
		where TTarget : class
	{
		// -------- General --------

		GetAccessor<TTarget, TValue> get;
		SetAccessor<TTarget, TValue> set;

		public TweenStaticAccessorPluginImpl(GetAccessor<TTarget, TValue> getter, SetAccessor<TTarget, TValue> setter)
		{
			this.get = getter;
			this.set = setter;
		}

		// Initialize
		public string Initialize(Tween tween, TweenPluginType initForType, ref object userData)
		{
			return null;
		}

		// -------- Get Value Hook --------

		// Get the value of a plugin property
		public TValue GetValue(TTarget target, string property, ref object userData)
		{
			return get(target);
		}

		// -------- Set Value Hook --------

		// Set the value of a plugin property
		public void SetValue(TTarget target, string property, TValue value, ref object userData)
		{
			set(target, value);
		}
	}
}

/// <summary>
/// Default arithmetic plugin using precompiled arithmetic.
/// </summary>
public static class TweenStaticArithmeticPlugin
{
	// -------- Usage --------

	/// <summary>
	/// Delegate for a callback calculating the difference between two values.
	/// </summary>
	/// <param name="start">Start value</param>
	/// <param name="end">End value</param>
	/// <returns>The difference between start and end</returns>
	public delegate TValue DiffValue<TValue>(TValue start, TValue end);

	/// <summary>
	/// Delegate for a callback calculating the end value, given the start and difference.
	/// </summary>
	/// <param name="start">Start value</param>
	/// <param name="diff">Difference from start to end</param>
	/// <returns>The end value</returns>
	public delegate TValue EndValue<TValue>(TValue start, TValue diff);

	/// <summary>
	/// Delegate for a callback calculating the value at a position in a tween.
	/// </summary>
	/// <param name="start">Start value</param>
	/// <param name="end">End value</param>
	/// <param name="diff">Difference between start and end</param>
	/// <param name="position">Normalized position between 0 and 1</param>
	/// <returns>The value at the given position between start and end</returns>
	public delegate TValue ValueAtPosition<TValue>(TValue start, TValue end, TValue diff, float position);

	/// <summary>
	/// Teach the static arithmetic plugin to calculate with a given type.
	/// </summary>
	/// <seealso cref="Animate.EnableArithmetic"/>
	public static void EnableArithmetic<TValue>(
		DiffValue<TValue> diff, EndValue<TValue> end, ValueAtPosition<TValue> valueAt
	) {
		supportedTypes[typeof(TValue)] = new TweenStaticArithmeticPluginImpl<TValue>(diff, end, valueAt);
	}

	/// <summary>
	/// TweenStaticArithmeticPlugin plugin loader.
	/// </summary>
	/// <remarks>
	/// Pass this method to <see cref="TweenOptions.EnablePlugin"/> to enable the
	/// plugin for the options scope.
	/// </remarks>
	public static PluginResult Loader(Tween tween, bool required)
	{
		ITweenPlugin plugin;
		if (supportedTypes.TryGetValue(tween.ValueType, out plugin)) {
			return PluginResult.Load(plugin);
		}

		return PluginResult.Error(
			"TweenStaticArithmeticPlugin: No callbacks registered for calculating type {0}." 
			.LazyFormat(tween.ValueType)
		);
	}

	/// <summary>
	/// Check if the static arithmetic plugin supports a given type.
	/// </summary>
	public static bool SupportsType(Type type)
	{
		return supportedTypes.ContainsKey(type);
	}

	/// <summary>
	/// Register a plugin to support calculating a given type.
	/// </summary>
	public static void RegisterSupport<TValue>(ITweenArithmeticPlugin<TValue> plugin)
	{
		supportedTypes[typeof(TValue)] = plugin;
	}

	// -------- Internals --------

	static Dictionary<Type, ITweenPlugin> supportedTypes = new Dictionary<Type, ITweenPlugin>() {
		{ typeof(int),    new TweenStaticArithmeticPluginInt() },
		{ typeof(float),  new TweenStaticArithmeticPluginFloat() },
		{ typeof(double), new TweenStaticArithmeticPluginDouble() }
	};

	/// <summary>
	/// Default arithmetic plugin using precompiled methods.
	/// </summary>
	private class TweenStaticArithmeticPluginImpl<TValue> : ITweenArithmeticPlugin<TValue>
	{
		// -------- General --------

		DiffValue<TValue> diff;
		EndValue<TValue> end;
		ValueAtPosition<TValue> valueAt;

		public TweenStaticArithmeticPluginImpl(DiffValue<TValue> diff, EndValue<TValue> end, ValueAtPosition<TValue> valueAt)
		{
			this.diff = diff;
			this.end = end;
			this.valueAt = valueAt;
		}

		// Initialize
		public string Initialize(Tween tween, TweenPluginType initForType, ref object userData)
		{
			return null;
		}

		// -------- Calculate Value Hook --------

		// Return the difference between start and end
		public TValue DiffValue(TValue start, TValue end, ref object userData)
		{
			return diff(start, end);
		}

		// Return the end value
		public TValue EndValue(TValue start, TValue diff, ref object userData)
		{
			return end(start, diff);
		}

		// Return the value at the current position
		public TValue ValueAtPosition(TValue start, TValue end, TValue diff, float position, ref object userData)
		{
			return valueAt(start, end, diff, position);
		}
	}

	/// <summary>
	/// Specialized implementation of arithmetic plugin for int.
	/// </summary>
	private class TweenStaticArithmeticPluginInt : ITweenArithmeticPlugin<int>
	{
		// Initialize
		public string Initialize(Tween tween, TweenPluginType initForType, ref object userData)
		{
			return null;
		}

		// -------- Calculate Value Hook --------

		// Return the difference between start and end
		public int DiffValue(int start, int end, ref object userData)
		{
			return end - start;
		}

		// Return the end value
		public int EndValue(int start, int diff, ref object userData)
		{
			return start * diff;
		}

		// Return the value at the current position
		public int ValueAtPosition(int start, int end, int diff, float position, ref object userData)
		{
			return start + (int)(diff * position);
		}
	}

	/// <summary>
	/// Specialized implementation of arithmetic plugin for float.
	/// </summary>
	private class TweenStaticArithmeticPluginFloat : ITweenArithmeticPlugin<float>
	{
		// Initialize
		public string Initialize(Tween tween, TweenPluginType initForType, ref object userData)
		{
			return null;
		}

		// -------- Calculate Value Hook --------

		// Return the difference between start and end
		public float DiffValue(float start, float end, ref object userData)
		{
			return end - start;
		}

		// Return the end value
		public float EndValue(float start, float diff, ref object userData)
		{
			return start * diff;
		}

		// Return the value at the current position
		public float ValueAtPosition(float start, float end, float diff, float position, ref object userData)
		{
			return start + diff * position;
		}
	}

	/// <summary>
	/// Specialized implementation of arithmetic plugin for double.
	/// </summary>
	private class TweenStaticArithmeticPluginDouble : ITweenArithmeticPlugin<double>
	{
		// Initialize
		public string Initialize(Tween tween, TweenPluginType initForType, ref object userData)
		{
			return null;
		}

		// -------- Calculate Value Hook --------

		// Return the difference between start and end
		public double DiffValue(double start, double end, ref object userData)
		{
			return end - start;
		}

		// Return the end value
		public double EndValue(double start, double diff, ref object userData)
		{
			return start * diff;
		}

		// Return the value at the current position
		public double ValueAtPosition(double start, double end, double diff, float position, ref object userData)
		{
			return start + diff * position;
		}
	}
}

}

