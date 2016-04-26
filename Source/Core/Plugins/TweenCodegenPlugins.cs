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
		public static bool Load<TTarget, TValue>(Tween<TTarget, TValue> tween)
			where TTarget : class
		{
			return TweenCodegenAccessorPlugin<TTarget, TValue>.Load(tween);
		}
	}

	/// <summary>
	/// Base class for the default plugin, providing generic get/set implementations.
	/// </summary>
	public class TweenCodegenAccessorPlugin<TTarget, TValue>
		: ITweenGetterPlugin<TTarget, TValue>, ITweenSetterPlugin<TTarget, TValue>
		where TTarget : class
	{
		///////////////////
		// Usage

		static TweenCodegenAccessorPlugin<TTarget, TValue> _sharedInstance
			= new TweenCodegenAccessorPlugin<TTarget, TValue>();

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
			if (initForType == TweenPluginType.Getter) {
				userData = memberInfo;

			// Generate set handler
			} else if (initForType == TweenPluginType.Setter) {
				try {
					userData = TweenCodegen.GenerateSetMethod<TTarget, TValue>(memberInfo);
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
		public TValue GetValue(TTarget target, string property, ref object userData)
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
		public void SetValue(TTarget target, string property, TValue value, ref object userData)
		{
			(userData as TweenCodegen.SetHandler<TTarget, TValue>)(ref target, value);
		}
	}

	/// <summary>
	/// Default arithmetic plugin using code generation.
	/// </summary>
	public static class TweenCodegenArithmeticPlugin
	{
		///////////////////
		// Usage

		public static bool Load<TTarget, TValue>(Tween<TTarget, TValue> tween)
			where TTarget : class
		{
			return TweenCodegenArithmeticPlugin<TValue>.Load(tween);
		}
	}

	/// <summary>
	/// Generic calculation implementation for default plugin.
	/// </summary>
	public class TweenCodegenArithmeticPlugin<TValue> : ITweenArithmeticPlugin<TValue>
	{
		///////////////////
		// Usage

		static TweenCodegenArithmeticPlugin<TValue> _sharedInstance
			= new TweenCodegenArithmeticPlugin<TValue>();

		public static bool Load<TTarget>(Tween<TTarget, TValue> tween)
			where TTarget : class
		{
			// Use the static plugin if possible
			if (TweenStaticArithmeticPlugin.Load(tween)) {
				return true;
			}

			// Fall back to the reflection plugin
			tween.LoadPlugin(_sharedInstance, weak: true);
			return true;
		}

		///////////////////
		// General

		// Initialize
		public string Initialize(ITween tween, TweenPluginType initForType, ref object userData)
		{
			// Check if calculation is possible
			try {
				Operator<TValue, TValue, TValue>.Addition(default(TValue), default(TValue));
				Operator<TValue, TValue, TValue>.Subtraction(default(TValue), default(TValue));
				Operator<TValue, float, TValue>.Multiply(default(TValue), 0.5f);
			} catch (Exception e) {
				return string.Format(
					"Property {0} on {1} cannot bet tweened, "
					+ "type {2} does not support addition, "
					+ "subtraction or multiplication. ({3})",
					tween.Property, tween.Target, typeof(TValue), e.Message
				);
			}

			return null;
		}

		///////////////////
		// Calculate Value Hook

		// Return the difference between start and end
		public TValue DiffValue(TValue start, TValue end, ref object userData)
		{
			return Operator<TValue, TValue, TValue>.Subtraction(end, start);
		}

		// Return the end value
		public TValue EndValue(TValue start, TValue diff, ref object userData)
		{
			return Operator<TValue, TValue, TValue>.Addition(start, diff);
		}

		// Return the value at the current position
		public TValue ValueAtPosition(TValue start, TValue end, TValue diff, float position, ref object userData)
		{
			return Operator<TValue, TValue, TValue>.Addition(start, 
						Operator<TValue, float, TValue>.Multiply(diff, position));
		}
	}
}
