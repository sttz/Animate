using System;
using System.Collections.Generic;

namespace Sttz.Tweener.Core.Static
{
	public delegate TValue GetAccessor<TTarget, TValue>(TTarget target)
		where TTarget : class;
	
	public delegate void SetAccessor<TTarget, TValue>(TTarget target, TValue value)
		where TTarget : class;

	/// <summary>
	/// Default accessor plugin using precompiled methods.
	/// </summary>
	public static class TweenStaticAccessorPlugin
	{
		///////////////////
		// Usage

		/// <summary>
		/// Teach the static accessor plugin to access a property on a type.
		/// </summary>
		public static void Teach<TTarget, TValue>(
			string propertyName, 
			GetAccessor<TTarget, TValue> getter, SetAccessor<TTarget, TValue> setter
		)
			where TTarget : class
		{
			var key = PluginKey(typeof(TTarget), typeof(TValue), propertyName);
			plugins[key] = new TweenStaticAccessorPlugin<TTarget, TValue>(getter, setter);
		}

		public static bool Load(ITween tween, bool weak = true)
		{
			ITweenPlugin plugin;
			if (!plugins.TryGetValue(PluginKey(tween.TargetType, tween.ValueType, tween.Property), out plugin)) {
				return false;
			}

			tween.Internal.LoadPlugin(plugin, weak: weak);
			return true;
		}

		public static Tween<TTarget, TValue> PluginStaticAccessor<TTarget, TValue> (
			this Tween<TTarget, TValue> tween
		)
			where TTarget : class
		{
			if (!Load(tween, weak: false)) {
				tween.PluginError("PluginStaticAccessor",
					"Cannot tween property {0} on {1}, use TweenStaticAccessorPlugin.Teach() " +
					"to add support for more properties and targets.",
					tween.Property, tween.Target
				);
			}
			return tween;
		}

		///////////////////
		// Internals

		static string PluginKey(Type targetType, Type valueType, string property)
		{
			return targetType.FullName + "/" + valueType.FullName + "/" + property;
		}

		static Dictionary<string, ITweenPlugin> plugins = new Dictionary<string, ITweenPlugin>();
	}

	/// <summary>
	/// Default accessor plugin using precompiled methods.
	/// </summary>
	public class TweenStaticAccessorPlugin<TTarget, TValue> :
		ITweenGetterPlugin<TTarget, TValue>, ITweenSetterPlugin<TTarget, TValue>
		where TTarget : class
	{
		///////////////////
		// General

		GetAccessor<TTarget, TValue> get;
		SetAccessor<TTarget, TValue> set;

		public TweenStaticAccessorPlugin(GetAccessor<TTarget, TValue> getter, SetAccessor<TTarget, TValue> setter)
		{
			this.get = getter;
			this.set = setter;
		}

		// Initialize
		public string Initialize(ITween tween, TweenPluginType initForType, ref object userData)
		{
			return null;
		}

		///////////////////
		// Get Value Hook

		// Get the value of a plugin property
		public TValue GetValue(TTarget target, string property, ref object userData)
		{
			return get(target);
		}

		///////////////////
		// Set Value Hook

		// Set the value of a plugin property
		public void SetValue(TTarget target, string property, TValue value, ref object userData)
		{
			set(target, value);
		}
	}

	public delegate TValue DiffValue<TValue>(TValue start, TValue end);
	public delegate TValue EndValue<TValue>(TValue start, TValue diff);
	public delegate TValue ValueAtPosition<TValue>(TValue start, TValue end, TValue diff, float position);

	/// <summary>
	/// Default arithmetic plugin using precompiled arithmetic.
	/// </summary>
	public static class TweenStaticArithmeticPlugin
	{
		///////////////////
		// Usage

		/// <summary>
		/// Teach the static arithmetic plugin to calculate a property of a type.
		/// </summary>
		public static void Teach<TValue>(
			DiffValue<TValue> diff, EndValue<TValue> end, ValueAtPosition<TValue> valueAt
		) {
			supportedTypes[typeof(TValue)] = new TweenStaticArithmeticPlugin<TValue>(diff, end, valueAt);
		}

		public static bool Load(ITween tween, bool weak = true)
		{
			if (tween == null) return false;

			ITweenPlugin plugin;
			if (supportedTypes.TryGetValue(tween.ValueType, out plugin)) {
				tween.Internal.LoadPlugin(plugin, weak: weak);
				return true;
			}

			return false;
		}

		public static Tween<TTarget, TValue> PluginStaticArithmetic<TTarget, TValue> (
			this Tween<TTarget, TValue> tween
		)
			where TTarget : class
		{
			if (!Load(tween, weak: false)) {
				tween.PluginError("PluginStaticAccessor",
				    "Cannot tween value {0} ({0} on {1}), use TweenStaticArithmeticPlugin.RegisterSupport() " +
					"to add a new ITweenPlugin supporting this type.",
					typeof(TValue).Name, tween.Property, tween.Target
				);
			}
			return tween;
		}

		///////////////////
		// Internals

		static Dictionary<Type, ITweenPlugin> supportedTypes = new Dictionary<Type, ITweenPlugin>() {
			{ typeof(int),    new TweenStaticArithmeticPluginInt() },
			{ typeof(float),  new TweenStaticArithmeticPluginFloat() },
			{ typeof(double), new TweenStaticArithmeticPluginDouble() }
		};

		public static bool SupportsType(Type type)
		{
			return supportedTypes.ContainsKey(type);
		}

		public static void RegisterSupport(Type type, ITweenPlugin plugin)
		{
			supportedTypes[type] = plugin;
		}
	}

	/// <summary>
	/// Default arithmetic plugin using precompiled methods.
	/// </summary>
	public class TweenStaticArithmeticPlugin<TValue> : ITweenArithmeticPlugin<TValue>
	{
		///////////////////
		// General

		DiffValue<TValue> diff;
		EndValue<TValue> end;
		ValueAtPosition<TValue> valueAt;

		public TweenStaticArithmeticPlugin(DiffValue<TValue> diff, EndValue<TValue> end, ValueAtPosition<TValue> valueAt)
		{
			this.diff = diff;
			this.end = end;
			this.valueAt = valueAt;
		}

		// Initialize
		public string Initialize(ITween tween, TweenPluginType initForType, ref object userData)
		{
			return null;
		}

		///////////////////
		// Calculate Value Hook

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
	public class TweenStaticArithmeticPluginInt : ITweenArithmeticPlugin<int>
	{
		// Initialize
		public string Initialize(ITween tween, TweenPluginType initForType, ref object userData)
		{
			return null;
		}

		///////////////////
		// Calculate Value Hook

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
	public class TweenStaticArithmeticPluginFloat : ITweenArithmeticPlugin<float>
	{
		// Initialize
		public string Initialize(ITween tween, TweenPluginType initForType, ref object userData)
		{
			return null;
		}

		///////////////////
		// Calculate Value Hook

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
	public class TweenStaticArithmeticPluginDouble : ITweenArithmeticPlugin<double>
	{
		// Initialize
		public string Initialize(ITween tween, TweenPluginType initForType, ref object userData)
		{
			return null;
		}

		///////////////////
		// Calculate Value Hook

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

