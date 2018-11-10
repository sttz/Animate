using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Sttz.Tweener.Core {

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
	// -------- Usage --------

	/// <summary>
	/// TweenReflectionAccessorPlugin plugin loader.
	/// </summary>
	/// <remarks>
	/// This loader cannot be used with <see cref="TweenOptions.EnablePlugin"/> and is 
	/// called internally in <see cref="ITweenEngine.LoadDynamicPlugins"/>.
	/// </remarks>
	public static void Load<TTarget, TValue>(Tween<TTarget, TValue> tween, bool required)
		where TTarget : class
	{
		tween.LoadPlugin(TweenReflectionAccessorPluginImpl<TTarget,TValue>.sharedInstance, weak: !required);
	}

	// -------- Internals --------

	/// <summary>
	/// Base class for the default plugin, providing generic get/set implementations.
	/// </summary>
	private class TweenReflectionAccessorPluginImpl<TTarget, TValue>
		: ITweenGetterPlugin<TTarget, TValue>, ITweenSetterPlugin<TTarget, TValue>
		where TTarget : class
	{

		public static TweenReflectionAccessorPluginImpl<TTarget, TValue> sharedInstance
			= new TweenReflectionAccessorPluginImpl<TTarget, TValue>();

		// Initialize
		public string Initialize(Tween tween, TweenPluginType initForType, ref object userData)
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

		// -------- Get Value Hook --------

		// Get the value of a plugin property
		public TValue GetValue(TTarget target, string property, ref object userData)
		{
			if (userData is PropertyInfo) {
				return (TValue)(userData as PropertyInfo).GetValue(target, null);
			} else {
				return (TValue)(userData as FieldInfo).GetValue(target);
			}
		}

		// -------- Set Value Hook --------

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
	// -------- Usage --------

	/// <summary>
	/// TweenReflectionArithmeticPlugin plugin loader.
	/// </summary>
	/// <remarks>
	/// This loader cannot be used with <see cref="TweenOptions.EnablePlugin"/> and is 
	/// called internally in <see cref="ITweenEngine.LoadDynamicPlugins"/>.
	/// </remarks>
	public static void Load<TTarget, TValue>(Tween<TTarget, TValue> tween, bool required)
		where TTarget : class
	{
		// Look for necessary op_* methods
		var data = new TweenReflectionUserData();
		data.opAddition = GetOperatorMethod("op_Addition", tween.ValueType);
		data.opSubtraction = GetOperatorMethod("op_Subtraction", tween.ValueType);
		data.opMultiply = GetOperatorMethod("op_Multiply", tween.ValueType, typeof (float));

		if (data.opAddition == null || data.opSubtraction == null || data.opMultiply == null) {
			tween.Options.Log(
				TweenLogLevel.Debug,
				"TweenReflectionArithmeticPlugin requires op_Addition, op_Subtraction and op_Multiply "
				+ "methods on the type {0}.", tween.ValueType
			);
			return;
		}

		tween.LoadPlugin(TweenReflectionArithmeticPluginImpl<TValue>.sharedInstance, weak: !required, userData: data);
	}

	static MethodInfo GetOperatorMethod(string name, Type valueType, Type secondArgumentType = null)
	{
		if (secondArgumentType == null)
			secondArgumentType = valueType;

		return valueType.GetMethod(
			name,
			BindingFlags.Static | BindingFlags.Public,
			null,
			new Type[] { valueType, secondArgumentType },
			null
		);
	}

	// User data
	private class TweenReflectionUserData
	{
		public MethodInfo opAddition;
		public MethodInfo opSubtraction;
		public MethodInfo opMultiply;
	}

	/// <summary>
	/// Calculation implementation using reflection.
	/// </summary>
	private class TweenReflectionArithmeticPluginImpl<TValue> : ITweenArithmeticPlugin<TValue>
	{
		public static TweenReflectionArithmeticPluginImpl<TValue> sharedInstance
			= new TweenReflectionArithmeticPluginImpl<TValue>();

		// Initialize
		public string Initialize(Tween tween, TweenPluginType initForType, ref object userData)
		{
			return null;
		}

		// -------- Calculate Value Hook --------

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

}
