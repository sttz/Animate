using System;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Sttz.Tweener.Core.Reflection;
using Sttz.Tweener.Core.Static;

namespace Sttz.Tweener.Core.Codegen {

	/// <summary>
	/// Default accessor plugin using code generation.
	/// </summary>
	public static class TweenCodegenAccessorPlugin
	{
		// Return the plugin info structure
		public static TweenPluginInfo Use()
		{
			return new TweenPluginInfo {
				pluginType = typeof(TweenCodegenAccessorPlugin<>),
				hooks =
					  TweenPluginHook.GetValueWeak
					| TweenPluginHook.SetValueWeak
			};
		}
	}

	/// <summary>
	/// Base class for the default plugin, providing generic get/set implementations.
	/// </summary>
	public class TweenCodegenAccessorPlugin<TValue> : TweenPlugin<TValue>
	{
		///////////////////
		// General

		// Initialize
		public override string Initialize(ITween tween, TweenPluginHook hook, ref object userData)
		{
			if (hook != TweenPluginHook.GetValue && hook != TweenPluginHook.SetValue) {
				return string.Format(
					"TweenCodegenAccessorPlugin only supports GetValue and SetValue hooks (got {0}).",
					hook
				);
			}

			var memberInfo = TweenReflection.FindMember(tween.Target.GetType(), tween.Property);

			// Get / Set need MemberInfo
			if (memberInfo == null) {
				return string.Format(
					"Property {0} on {1} could not be found.",
					tween.Property, tween.Target
				);
			}

			// Check types match
			var memberType = TweenReflection.MemberType(memberInfo);
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

			// Set member info to userData for get hook
			if (hook == TweenPluginHook.GetValue) {
				userData = memberInfo;

			// Generate set handler
			} else if (hook == TweenPluginHook.SetValue) {
				try {
					userData = TweenCodegen.GenerateSetMethod<object, TValue>(memberInfo);
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
	/// Default arithmetic plugin using code generation.
	/// </summary>
	public static class TweenCodegenArithmeticPlugin
	{
		// Return the plugin info structure
		public static TweenPluginInfo Use()
		{
			return new TweenPluginInfo {
				pluginType = typeof(TweenCodegenArithmeticPlugin<>),
				hooks = TweenPluginHook.CalculateValueWeak,
				manualActivation = ManualActivation
			};
		}

		// Callback for manual activation
		private static TweenPluginInfo ManualActivation(ITween tween, TweenPluginInfo info)
		{
			// Use the static plugin if possible
			var staticPlugin = TweenStaticArithmeticPlugin.GetImplementationForValueType(tween.ValueType);
			if (staticPlugin != null) {
				info.pluginType = staticPlugin;
			}

			return info;
		}
	}

	/// <summary>
	/// Generic calculation implementation for default plugin.
	/// </summary>
	public class TweenCodegenArithmeticPlugin<TValue> : TweenPlugin<TValue>
	{
		///////////////////
		// General

		// Initialize
		public override string Initialize(ITween tween, TweenPluginHook hook, ref object userData)
		{
			if (hook != TweenPluginHook.CalculateValue) {
				return string.Format(
					"TweenCodegenArithmeticPlugin only supports the CalculateValue hook (got {0}).",
					hook
				);
			}

			// Check if calculation is possible
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

			return null;
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
}
