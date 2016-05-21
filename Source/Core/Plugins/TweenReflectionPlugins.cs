using System;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Sttz.Tweener.Core.Static;

namespace Sttz.Tweener.Core.Reflection
{
	/// <summary>
	/// Helper class for basic reflection.
	/// </summary>
	public static class TweenReflection
	{
		// Binding flags used to look for properties
		private static BindingFlags bindingFlags =
			  BindingFlags.Public
			| BindingFlags.NonPublic
			| BindingFlags.Instance
			| BindingFlags.Static;

		// Find a member on the target type
		public static MemberInfo FindMember(Type type, string name)
		{
			MemberInfo info = type.GetProperty(name, bindingFlags);
			if (info == null) {
				info = type.GetField(name, bindingFlags);
			}
			return info;
		}

		// Type of property or field
		public static Type MemberType(MemberInfo member)
		{
			if (member is PropertyInfo) {
				return (member as PropertyInfo).PropertyType;
			} else {
				return (member as FieldInfo).FieldType;
			}
		}
	}

	public static class TweenReflectionAccessorPlugin
	{
		public static bool Load<TTarget, TValue>(Tween<TTarget, TValue> tween, bool automatic = true)
			where TTarget : class
		{
			return TweenReflectionAccessorPlugin<TTarget, TValue>.Load(tween, automatic: automatic);
		}

		public static Tween<TTarget, TValue> PluginReflectionAccessor<TTarget, TValue> (
			this Tween<TTarget, TValue> tween
		)
			where TTarget : class
		{
			Load(tween, automatic: false);
			return tween;
		}
	}

	/// <summary>
	/// Base class for the default plugin, providing generic get/set implementations.
	/// </summary>
	public class TweenReflectionAccessorPlugin<TTarget, TValue>
		: ITweenGetterPlugin<TTarget, TValue>, ITweenSetterPlugin<TTarget, TValue>
		where TTarget : class
	{
		///////////////////
		// Usage

		static TweenReflectionAccessorPlugin<TTarget, TValue> _sharedInstance
			= new TweenReflectionAccessorPlugin<TTarget, TValue>();

		public static bool Load(Tween<TTarget, TValue> tween, bool automatic = true)
		{
			if (tween == null) return false;
			tween.LoadPlugin(_sharedInstance, weak: automatic);
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

			userData = memberInfo;
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
			if (userData is PropertyInfo) {
				(userData as PropertyInfo).SetValue(target, value, null);
			} else {
				(userData as FieldInfo).SetValue(target, value);
			}
		}
	}

	/// <summary>
	/// Default arithmetic plugin using reflection.
	/// </summary>
	/// <remarks>
	/// Due to limitations in C#, this plugin only works with custom types
	/// and not with C#'s built-in basic types. The plugin relies on
	/// TweenStaticArithmeticPlugin to provide the arithmetic for those types.
	/// </remarks>
	public static class TweenReflectionArithmeticPlugin
	{
		///////////////////
		// Usage

		public static bool Load<TTarget, TValue>(Tween<TTarget, TValue> tween, bool automatic = true)
			where TTarget : class
		{
			return TweenReflectionArithmeticPlugin<TValue>.Load(tween, automatic: automatic);
		}

		public static Tween<TTarget, TValue> PluginReflectionArithmetic<TTarget, TValue> (
			this Tween<TTarget, TValue> tween
		)
			where TTarget : class
		{
			if (!Load(tween, automatic: false)) {
				tween.PluginError("PluginReflectionArithmetic",
				    "Cannot tween value {0} ({0} on {1}): TweenReflectionArithmeticPlugin " +
					"only supports non-basic types with op_Addition, op_Subtraction and op_Multiply " +
				    "operator overloads.",
					tween.Property, tween.Target
				);
			}
			return tween;
		}
	}

	/// <summary>
	/// Calculation implementation using reflection.
	/// </summary>
	public class TweenReflectionArithmeticPlugin<TValue> : ITweenArithmeticPlugin<TValue>
	{
		///////////////////
		// Usage

		static TweenReflectionArithmeticPlugin<TValue> _sharedInstance
			= new TweenReflectionArithmeticPlugin<TValue>();

		public static bool Load<TTarget>(Tween<TTarget, TValue> tween, bool automatic = true)
			where TTarget : class
		{
			if (tween == null) return false;

			// Use the static plugin if possible
			if (TweenStaticArithmeticPlugin.Load(tween)) {
				return true;
			}

			// Look for necessary op_* methods
			var data = new TweenReflectionUserData ();
			data.opAddition = GetOperatorMethod(tween.ValueType, "op_Addition");
			data.opSubtraction = GetOperatorMethod(tween.ValueType, "op_Subtraction");
			data.opMultiply = GetOperatorMethod(tween.ValueType, "op_Multiply", typeof (float));

			if (data.opAddition == null || data.opSubtraction == null || data.opMultiply == null) {
				return false;
			}

			// Fall back to the reflection plugin
			tween.LoadPlugin(_sharedInstance, weak: automatic, userData: data);
			return true;
		}

		///////////////////
		// General

		// User data
		private class TweenReflectionUserData
		{
			public MethodInfo opAddition;
			public MethodInfo opSubtraction;
			public MethodInfo opMultiply;
		}

		public static MethodInfo GetOperatorMethod(Type type, string name, Type secondArgumentType = null)
		{
			if (secondArgumentType == null)
				secondArgumentType = typeof(TValue);

			return type.GetMethod(
				name,
				BindingFlags.Static | BindingFlags.Public,
				null,
				new Type[] { typeof(TValue), secondArgumentType },
				null
			);
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
			return (TValue)((TweenReflectionUserData)userData).opSubtraction.Invoke(null, new object[] { end, start });
		}

		// Return the end value
		public TValue EndValue(TValue start, TValue diff, ref object userData)
		{
			return (TValue)((TweenReflectionUserData)userData).opAddition.Invoke(null, new object[] { start, diff });
		}

		// Return the value at the current position
		public TValue ValueAtPosition(TValue start, TValue end, TValue diff, float position, ref object userData)
		{
			var data = (TweenReflectionUserData)userData;
			var offset = data.opMultiply.Invoke(null, new object[] { diff, position });
			return (TValue)data.opAddition.Invoke(null, new object[] { start, offset });
		}
	}
}
