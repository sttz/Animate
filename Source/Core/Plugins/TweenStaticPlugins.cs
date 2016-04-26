using System;
using System.Collections.Generic;

namespace Sttz.Tweener.Core.Static
{
	/// <summary>
	/// Default accessor plugin using precompiled methods.
	/// </summary>
	public static class TweenStaticAccessorPlugin
	{
		///////////////////
		// Usage

		public static bool Load<TTarget, TValue>(Tween<TTarget, TValue> tween)
			where TTarget : class
		{
			return TweenStaticAccessorPlugin<TTarget, TValue>.Load(tween);
		}

		///////////////////
		// Internals

		public delegate TValue GetAccessor<TTarget, TValue>(TTarget target)
			where TTarget : class;
		public delegate void SetAccessor<TTarget, TValue>(TTarget target, TValue value)
			where TTarget : class;

		/// <summary>
		/// Teach the static accessor plugin to access a property on a type.
		/// </summary>
		public static void Teach<TTarget, TValue>(
			string propertyName, 
			GetAccessor<TTarget, TValue> getter, SetAccessor<TTarget, TValue> setter
		)
			where TTarget : class
		{
			accessors[PairKey<TTarget, TValue>(propertyName)] = new AccessorPair<TTarget, TValue> {
				getter = getter,
				setter = setter
			};
		}

		internal static AccessorPair<TTarget, TValue> GetAccessorPair<TTarget, TValue>(string propertyName)
			where TTarget : class
		{
			object pair;
			if (accessors.TryGetValue(PairKey<TTarget, TValue>(propertyName), out pair)) {
				return (AccessorPair<TTarget, TValue>)pair;
			} else {
				return default(AccessorPair<TTarget, TValue>);
			}
		}

		static string PairKey<TTarget, TValue>(string propertyName)
		{
			return typeof(TTarget).FullName + "/" + typeof(TValue).FullName + "/" + propertyName;
		}

		internal struct AccessorPair<TTarget, TValue>
			where TTarget : class
		{
			public GetAccessor<TTarget, TValue> getter;
			public SetAccessor<TTarget, TValue> setter;
		}

		static Dictionary<string, object> accessors = new Dictionary<string, object>();
	}

	/// <summary>
	/// Default accessor plugin using precompiled methods.
	/// </summary>
	public class TweenStaticAccessorPlugin<TTarget, TValue> :
		ITweenGetterPlugin<TTarget, TValue>, ITweenSetterPlugin<TTarget, TValue>
		where TTarget : class
	{
		///////////////////
		// Usage

		static TweenStaticAccessorPlugin<TTarget, TValue> _sharedInstance
			= new TweenStaticAccessorPlugin<TTarget, TValue>();

		public static bool Load(Tween<TTarget, TValue> tween)
		{
			tween.LoadPlugin(_sharedInstance, weak: true);
			return true;
		}

		///////////////////
		// General

		// Initialize
		public string Initialize(ITween tween, TweenPluginType initForType, ref object userData)
		{
			var accessor = TweenStaticAccessorPlugin.GetAccessorPair<TTarget, TValue>(tween.Property);
			if (accessor.getter == null || accessor.setter == null) {
				return string.Format(
					"Cannot tween property {0} on {1}, use TweenStaticAccessorPlugin.Teach() " +
					"to add support for more properties and targets.",
					tween.Property, tween.Target
				);
			}

			userData = accessor;
			return null;
		}

		///////////////////
		// Get Value Hook

		// Get the value of a plugin property
		public TValue GetValue(TTarget target, string property, ref object userData)
		{
			return ((TweenStaticAccessorPlugin.AccessorPair<TTarget, TValue>)userData).getter(target);
		}

		///////////////////
		// Set Value Hook

		// Set the value of a plugin property
		public void SetValue(TTarget target, string property, TValue value, ref object userData)
		{
			((TweenStaticAccessorPlugin.AccessorPair<TTarget, TValue>)userData).setter(target, value);
		}
	}

	/// <summary>
	/// Default arithmetic plugin using precompiled arithmetic.
	/// </summary>
	public static class TweenStaticArithmeticPlugin
	{
		///////////////////
		// Usage

		public static bool Load<TTarget, TValue>(Tween<TTarget, TValue> tween)
			where TTarget : class
		{
			var sharedInstance = GetImplementationForValueType(tween.ValueType);

			if (sharedInstance != null) {
				tween.LoadPlugin(sharedInstance, weak: true);
				return true;
			}

			return false;
		}

		///////////////////
		// Internals

		static Dictionary<Type, ITweenPlugin> supportedTypes = new Dictionary<Type, ITweenPlugin>() {
			{ typeof(float), new TweenStaticArithmeticPluginFloat() }
		};

		public static bool SupportsType(Type type)
		{
			return supportedTypes.ContainsKey(type);
		}

		public static void RegisterSupport(Type type, ITweenPlugin plugin)
		{
			supportedTypes[type] = plugin;
		}

		public static ITweenPlugin GetImplementationForValueType(Type type)
		{
			ITweenPlugin instance = null;
			if (supportedTypes.TryGetValue(type, out instance)) {
				return instance;
			} else {
				return null;
			}
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

