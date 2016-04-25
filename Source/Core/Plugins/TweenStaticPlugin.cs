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
		public delegate TValue GetAccessor<TTarget, TValue>(TTarget target)
			where TTarget : class;
		public delegate void SetAccessor<TTarget, TValue>(TTarget target, TValue value)
			where TTarget : class;

		static TweenStaticAccessorPlugin()
		{
			Teach("rotation",
				 (Transform tf) => tf.rotation,
				 (tf, value) => tf.rotation = value);
			Teach("position",
				 (Transform tf) => tf.position,
				 (tf, value) => tf.position = value);
		}

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

		// Return the plugin info structure
		public static TweenPluginInfo Use()
		{
			return new TweenPluginInfo {
				pluginType = typeof(TweenStaticAccessorPlugin<,>),
				canBeOverwritten = true,
				hooks = TweenPluginType.Getter | TweenPluginType.Setter
			};
		}
	}

	/// <summary>
	/// Default accessor plugin using precompiled methods.
	/// </summary>
	public class TweenStaticAccessorPlugin<TTarget, TValue> 
		: ITweenGetterPlugin<TTarget, TValue>, ITweenSetterPlugin<TTarget, TValue>
		where TTarget : class
	{
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
				canBeOverwritten = true,
				hooks = TweenPluginType.Arithmetic,
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
	/// Specialized implementation of arithmetic plugin for Vector3.
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
	/// Specialized implementation of arithmetic plugin for Vector3.
	/// </summary>
	public class TweenStaticArithmeticPluginVector3 : ITweenArithmeticPlugin<Vector3>
	{
		// Initialize
		public string Initialize(ITween tween, TweenPluginType initForType, ref object userData)
		{
			return null;
		}

		///////////////////
		// Calculate Value Hook

		// Return the difference between start and end
		public Vector3 DiffValue(Vector3 start, Vector3 end, ref object userData)
		{
			return end - start;
		}

		// Return the end value
		public Vector3 EndValue(Vector3 start, Vector3 diff, ref object userData)
		{
			return start + diff;
		}

		// Return the value at the current position
		public Vector3 ValueAtPosition(Vector3 start, Vector3 end, Vector3 diff, float position, ref object userData)
		{
			return start + diff * position;
		}
	}

	/// <summary>
	/// Specialized implementation of arithmetic plugin for Vector3.
	/// </summary>
	public class TweenStaticArithmeticPluginColor : ITweenArithmeticPlugin<Color>
	{
		// Initialize
		public string Initialize(ITween tween, TweenPluginType initForType, ref object userData)
		{
			return null;
		}

		///////////////////
		// Calculate Value Hook

		// Return the difference between start and end
		public Color DiffValue(Color start, Color end, ref object userData)
		{
			return end - start;
		}

		// Return the end value
		public Color EndValue(Color start, Color diff, ref object userData)
		{
			return start * diff;
		}

		// Return the value at the current position
		public Color ValueAtPosition(Color start, Color end, Color diff, float position, ref object userData)
		{
			return start + diff * position;
		}
	}
}

