using System;
using UnityEngine;
using System.Collections.Generic;

namespace Sttz.Tweener.Core.Static
{
	/// <summary>
	/// Default accessor plugin using precompiled methods.
	/// </summary>
	public static class TweenStaticAccessorPlugin
	{
		public delegate TValue GetAccessor<TValue>(object target);
		public delegate void SetAccessor<TValue>(object target, TValue value);

		static TweenStaticAccessorPlugin()
		{
			Teach(typeof(Transform), "rotation",
				 (tf) => ((Transform)tf).rotation,
				 (tf, value) => ((Transform)tf).rotation = value);
			Teach(typeof(Transform), "position",
				 (tf) => ((Transform)tf).position,
				 (tf, value) => ((Transform)tf).position = value);
		}

		/// <summary>
		/// Teach the static accessor plugin to access a property on a type.
		/// </summary>
		public static void Teach<TValue>(Type targetType, string propertyName, GetAccessor<TValue> getter, SetAccessor<TValue> setter)
		{
			// Check target isn't a value type
			if (targetType.IsValueType) {
				throw new Exception(string.Format(
					"Cannot teach to tween {0} on {1}: Target type is a value type.",
					propertyName, targetType
				));
			}

			accessors[PairKey<TValue>(targetType, propertyName)] = new AccessorPair<TValue> {
				getter = getter,
				setter = setter
			};
		}

		internal static AccessorPair<TValue> GetAccessorPair<TValue>(Type targetType, string propertyName)
		{
			object pair;
			if (accessors.TryGetValue(PairKey<TValue>(targetType, propertyName), out pair)) {
				return (AccessorPair<TValue>)pair;
			} else {
				return default(AccessorPair<TValue>);
			}
		}

		static string PairKey<TValue>(Type targetType, string propertyName)
		{
			return targetType.FullName + "/" + typeof(TValue).FullName + "/" + propertyName;
		}

		internal struct AccessorPair<TValue> {
			public GetAccessor<TValue> getter;
			public SetAccessor<TValue> setter;
		}

		static Dictionary<string, object> accessors = new Dictionary<string, object>();

		// Return the plugin info structure
		public static TweenPluginInfo Use()
		{
			return new TweenPluginInfo {
				pluginType = typeof(TweenStaticAccessorPlugin<>),
				hooks = TweenPluginHook.CalculateValueWeak
			};
		}
	}

	/// <summary>
	/// Default accessor plugin using precompiled methods.
	/// </summary>
	public class TweenStaticAccessorPlugin<TValue> : TweenPlugin<TValue>
	{
		///////////////////
		// General

		// Initialize
		public override string Initialize(ITween tween, TweenPluginHook hook, ref object userData)
		{
			if (hook != TweenPluginHook.GetValue && hook != TweenPluginHook.SetValue) {
				return string.Format(
					"TweenReflectionAccessorPlugin only supports GetValue and SetValue hooks (got {0}).",
					hook
				);
			}

			var accessor = TweenStaticAccessorPlugin.GetAccessorPair<TValue>(tween.Target.GetType(), tween.Property);
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
		public override TValue GetValue(object target, string property, ref object userData)
		{
			return ((TweenStaticAccessorPlugin.AccessorPair<TValue>)userData).getter(target);
		}

		///////////////////
		// Set Value Hook

		// Set the value of a plugin property
		public override void SetValue(object target, string property, TValue value, ref object userData)
		{
			((TweenStaticAccessorPlugin.AccessorPair<TValue>)userData).setter(target, value);
		}
	}

	/// <summary>
	/// Default arithmetic plugin using precompiled arithmetic.
	/// </summary>
	public static class TweenStaticArithmeticPlugin
	{
		static Dictionary<Type, Type> supportedTypes = new Dictionary<Type, Type>() {
			{ typeof(float), typeof(TweenStaticArithmeticPluginFloat) },
			{ typeof(Vector3), typeof(TweenStaticArithmeticPluginVector3) },
			{ typeof(Color), typeof(TweenStaticArithmeticPluginColor) }
		};

		// Return the plugin info structure
		public static TweenPluginInfo Use()
		{
			return new TweenPluginInfo {
				pluginType = typeof(TweenStaticArithmeticPlugin),
				hooks = TweenPluginHook.CalculateValueWeak,
				manualActivation = ManualActivation
			};
		}

		// Callback for manual activation
		private static TweenPluginInfo ManualActivation(ITween tween, TweenPluginInfo info)
		{
			var pluginType = GetImplementationForValueType(tween.ValueType);
			info.pluginType = pluginType;
			return info;
		}

		public static bool SupportsType(Type type)
		{
			return supportedTypes.ContainsKey(type);
		}

		public static Type GetImplementationForValueType(Type type)
		{
			Type pluginType = null;
			if (supportedTypes.TryGetValue(type, out pluginType)) {
				return pluginType;
			} else {
				return null;
			}
		}
	}

	/// <summary>
	/// Base class for default arithmetic plugin using precompiled arithmetic.
	/// </summary>
	public abstract class TweenStaticArithmeticPlugin<TValue> : TweenPlugin<TValue>
	{
		// Initialize
		public override string Initialize(ITween tween, TweenPluginHook hook, ref object userData)
		{
			if (hook != TweenPluginHook.CalculateValue) {
				return string.Format(
					"TweenStaticArithmeticPlugin only supports the CalculateValue hook (got {0}).",
					hook
				);
			}

			return null;
		}
	}

	/// <summary>
	/// Specialized implementation of arithmetic plugin for Vector3.
	/// </summary>
	public class TweenStaticArithmeticPluginFloat : TweenStaticArithmeticPlugin<float>
	{
		///////////////////
		// Calculate Value Hook

		// Return the difference between start and end
		public override float DiffValue(float start, float end, ref object userData)
		{
			return end - start;
		}

		// Return the end value
		public override float EndValue(float start, float diff, ref object userData)
		{
			return start * diff;
		}

		// Return the value at the current position
		public override float ValueAtPosition(float start, float end, float diff, float position, ref object userData)
		{
			return start + diff * position;
		}
	}

	/// <summary>
	/// Specialized implementation of arithmetic plugin for Vector3.
	/// </summary>
	public class TweenStaticArithmeticPluginVector3 : TweenStaticArithmeticPlugin<Vector3>
	{
		///////////////////
		// Calculate Value Hook

		// Return the difference between start and end
		public override Vector3 DiffValue(Vector3 start, Vector3 end, ref object userData)
		{
			return end - start;
		}

		// Return the end value
		public override Vector3 EndValue(Vector3 start, Vector3 diff, ref object userData)
		{
			return start + diff;
		}

		// Return the value at the current position
		public override Vector3 ValueAtPosition(Vector3 start, Vector3 end, Vector3 diff, float position, ref object userData)
		{
			return start + diff * position;
		}
	}

	/// <summary>
	/// Specialized implementation of arithmetic plugin for Vector3.
	/// </summary>
	public class TweenStaticArithmeticPluginColor : TweenStaticArithmeticPlugin<Color>
	{
		///////////////////
		// Calculate Value Hook

		// Return the difference between start and end
		public override Color DiffValue(Color start, Color end, ref object userData)
		{
			return end - start;
		}

		// Return the end value
		public override Color EndValue(Color start, Color diff, ref object userData)
		{
			return start * diff;
		}

		// Return the value at the current position
		public override Color ValueAtPosition(Color start, Color end, Color diff, float position, ref object userData)
		{
			return start + diff * position;
		}
	}
}

