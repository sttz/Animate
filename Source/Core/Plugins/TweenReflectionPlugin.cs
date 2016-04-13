using System;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

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

	/// <summary>
	/// Default accessor plugin using code generation.
	/// </summary>
	public static class TweenReflectionAccessorPlugin
	{
		// Return the plugin info structure
		public static TweenPluginInfo Use()
		{
			return new TweenPluginInfo {
				pluginType = typeof(TweenReflectionAccessorPlugin<>),
				hooks =
					  TweenPluginHook.GetValueWeak
					| TweenPluginHook.SetValueWeak
			};
		}
	}

	/// <summary>
	/// Base class for the default plugin, providing generic get/set implementations.
	/// </summary>
	public class TweenReflectionAccessorPlugin<TValue> : TweenPlugin<TValue>
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
			if (userData is PropertyInfo) {
				(userData as PropertyInfo).SetValue(target, value, null);
			} else {
				(userData as FieldInfo).SetValue(target, value);
			}
		}
	}
}
