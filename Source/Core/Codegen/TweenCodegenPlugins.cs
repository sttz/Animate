using System;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Sttz.Tweener.Core.Codegen {

	/// <summary>
	/// Tween default plugin.
	/// </summary>
	public class TweenCodegenPlugin
	{
		// Return the plugin info structure
		public static TweenPluginInfo Use()
		{
			return new TweenPluginInfo() {
				pluginType = typeof(TweenCodegenPlugin<>),
				hooks = 
					  TweenPluginHook.GetValueWeak
					| TweenPluginHook.SetValueWeak
					| TweenPluginHook.CalculateValueWeak,
				manualActivation = ManualActivation
			};
		}

		// Callback for manual activation
		private static TweenPluginInfo ManualActivation(ITween tween, TweenPluginInfo info)
		{
			// Swap in specialized plugin versions
			if (tween.ValueType == typeof(float)) {
				info.pluginType = typeof(TweenDefaultPluginFloat);
			} else if (tween.ValueType == typeof(Vector3)) {
				info.pluginType = typeof(TweenDefaultPluginVector3);
			} else if (tween.ValueType == typeof(Color)) {
				info.pluginType = typeof(TweenDefaultPluginColor);
			}

			return info;
		}
	}

	/// <summary>
	/// Base class for the default plugin, providing generic get/set implementations.
	/// </summary>
	public class TweenAccessorPlugin<TValue> : TweenPlugin<TValue>
	{
		///////////////////
		// General

		// Initialize
		public override string Initialize(ITween tween, TweenPluginHook hook, ref object userData)
		{
			// Get / Set need MemberInfo
			if ((hook == TweenPluginHook.GetValue 
					|| hook == TweenPluginHook.SetValue)) {
				if (tween.Internal.MemberInfo == null) {
					return string.Format(
						"Property {0} on {1} could not be found.",
						tween.Property, tween.Target
					);
				}
				// Check types match
				var memberType = TweenCodegen.MemberType(tween.Internal.MemberInfo);
				if (memberType != tween.ValueType) {
					return string.Format(
						"Mismatching types: Property type is {0} but tween type is {1} "
						+ "for tween of {2} on {3}.",
						memberType, tween.ValueType, tween.Property, tween.Target
					);
				}
				// Check target isn't a value type
				if (tween.Target.GetType().IsValueType) {
					return string.Format(
						"Cannot tween property {0} on value type {1}, " +
						"maybe use TweenStruct plugin?",
						tween.Property, tween.Target
					);
				}
			}

			// Set member info to userData for get hook
			if (hook == TweenPluginHook.GetValue) {
				userData = tween.Internal.MemberInfo;

			// Generate set handler
			} if (hook == TweenPluginHook.SetValue) {
				try {
					userData = TweenCodegen.GenerateSetMethod<object, TValue>(tween.Internal.MemberInfo);
				} catch (Exception e) {
					return string.Format(
						"Failed to generate setter method for tween of {0} on {1}: {2}.",
						tween.Property, tween.Target, e
					);
				}
			
			}

			return null;
		}

		///////////////////
		// Get Value Hook

		// Get the value of a plugin property
		public override TValue GetValue(object target, string property, ref object userData)
		{
			if (userData is PropertyInfo) {
				return (TValue)(userData as PropertyInfo).GetValue(target, null);
			} else {
				return (TValue)(userData as FieldInfo).GetValue(target);
			}
		}

		///////////////////
		// Set Value Hook

		// Set the value of a plugin property
		public override void SetValue(object target, string property, TValue value, ref object userData)
		{
			(userData as TweenCodegen.SetHandler<object, TValue>)(ref target, value);
		}
	}

	/// <summary>
	/// Generic calculation implementation for default plugin.
	/// </summary>
	public class TweenCodegenPlugin<TValue> : TweenAccessorPlugin<TValue>
	{
		///////////////////
		// General

		// Initialize
		public override string Initialize(ITween tween, TweenPluginHook hook, ref object userData)
		{
			var error = base.Initialize(tween, hook, ref userData);
			if (error != null) return error;

			// Check if calculation is possible
			if (hook == TweenPluginHook.CalculateValue) {
				try {
					Operator<TValue, TValue, TValue>.Addition(default(TValue), default(TValue));
					Operator<TValue, TValue, TValue>.Subtraction(default(TValue), default(TValue));
					Operator<TValue, float, TValue>.Multiply(default(TValue), 0.5f);
				} catch {
					return string.Format(
						"Property {0} on {1} cannot bet tweened, "
						+ "type {2} does not support addition, "
						+ "subtraction or multiplication.",
						tween.Property, tween.Target, typeof(TValue)
					);
				}
			}

			return null;
		}

		///////////////////
		// Get Value Hook

		// Get the value of a plugin property
		public override TValue GetValue(object target, string property, ref object userData)
		{
			if (userData is PropertyInfo) {
				return (TValue)(userData as PropertyInfo).GetValue(target, null);
			} else {
				return (TValue)(userData as FieldInfo).GetValue(target);
			}
		}

		///////////////////
		// Set Value Hook

		// Set the value of a plugin property
		public override void SetValue(object target, string property, TValue value, ref object userData)
		{
			(userData as TweenCodegen.SetHandler<object, TValue>)(ref target, value);
		}

		///////////////////
		// Calculate Value Hook

		// Return the difference between start and end
		public override TValue DiffValue(TValue start, TValue end, ref object userData)
		{
			return Operator<TValue, TValue, TValue>.Subtraction(end, start);
		}

		// Return the end value
		public override TValue EndValue(TValue start, TValue diff, ref object userData)
		{
			return Operator<TValue, TValue, TValue>.Addition(start, diff);
		}

		// Return the value at the current position
		public override TValue ValueAtPosition(TValue start, TValue end, TValue diff, float position, ref object userData)
		{
			return Operator<TValue, TValue, TValue>.Addition(start, 
						Operator<TValue, float, TValue>.Multiply(diff, position));
		}
	}

	/// <summary>
	/// Specialized implementation of default plugin for Vector3.
	/// </summary>
	public class TweenDefaultPluginFloat : TweenAccessorPlugin<float>
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
	/// Specialized implementation of default plugin for Vector3.
	/// </summary>
	public class TweenDefaultPluginVector3 : TweenAccessorPlugin<Vector3>
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
	/// Specialized implementation of default plugin for Vector3.
	/// </summary>
	public class TweenDefaultPluginColor : TweenAccessorPlugin<Color>
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
