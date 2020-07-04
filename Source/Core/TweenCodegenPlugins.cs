#if !ENABLE_IL2CPP && !NET_STANDARD_2_0

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Sttz.Tweener.Core {

/// <summary>
/// Default accessor plugin using code generation.
/// </summary>
public static class TweenCodegenAccessorPlugin
{
	// -------- Usage --------

	/// <summary>
	/// TweenCodegenAccessorPlugin plugin loader.
	/// </summary>
	/// <remarks>
	/// This loader cannot be used with <see cref="TweenOptions.EnablePlugin"/> and is 
	/// called internally in <see cref="ITweenEngine.LoadDynamicPlugins"/>.
	/// </remarks>
	public static void Load<TTarget, TValue>(Tween<TTarget, TValue> tween, bool required)
		where TTarget : class
	{
		tween.LoadPlugin(TweenCodegenAccessorPluginImpl<TTarget, TValue>.sharedInstance, weak: !required);
	}

	// -------- Implementation --------

	private class TweenCodegenAccessorPluginImpl<TTarget, TValue>
		: ITweenGetterPlugin<TTarget, TValue>, ITweenSetterPlugin<TTarget, TValue>
		where TTarget : class
	{
		public static TweenCodegenAccessorPluginImpl<TTarget, TValue> sharedInstance
			= new TweenCodegenAccessorPluginImpl<TTarget, TValue>();

		// -------- General --------

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
			(userData as TweenCodegen.SetHandler<TTarget, TValue>)(ref target, value);
		}
	}
}

/// <summary>
/// Default arithmetic plugin using code generation.
/// </summary>
public static class TweenCodegenArithmeticPlugin
{
	// -------- Usage --------

	/// <summary>
	/// TweenCodegenArithmeticPlugin plugin loader.
	/// </summary>
	/// <remarks>
	/// This loader cannot be used with <see cref="TweenOptions.EnablePlugin"/> and is 
	/// called internally in <see cref="ITweenEngine.LoadDynamicPlugins"/>.
	/// </remarks>
	public static void Load<TTarget, TValue>(Tween<TTarget, TValue> tween, bool required)
		where TTarget : class
	{
		// Check if calculation is possible
		try {
			Operator<TValue, TValue, TValue>.Addition(default (TValue), default (TValue));
			Operator<TValue, TValue, TValue>.Subtraction(default (TValue), default (TValue));
			Operator<TValue, float, TValue>.Multiply(default (TValue), 0.5f);
		} catch (Exception e) {
			tween.Options.Log(TweenLogLevel.Debug, 
				"TweenCodegenArithmeticPlugin encountered exception: {0}".LazyFormat(e)
			);
			return;
		}

		tween.LoadPlugin(TweenCodegenArithmeticPluginImpl<TValue>.sharedInstance, weak: !required);
	}

	// -------- Implementation --------

	private class TweenCodegenArithmeticPluginImpl<TValue> : ITweenArithmeticPlugin<TValue>
	{
		public static TweenCodegenArithmeticPluginImpl<TValue> sharedInstance
			= new TweenCodegenArithmeticPluginImpl<TValue>();

		// -------- General --------

		// Initialize
		public string Initialize(Tween tween, TweenPluginType initForType, ref object userData)
		{
			return null;
		}

		// -------- Calculate Value Hook --------

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

}

#endif
