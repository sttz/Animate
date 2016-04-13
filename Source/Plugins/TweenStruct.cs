using System;
using System.Reflection;

using Sttz.Tweener.Core;
using Sttz.Tweener.Core.Codegen;

namespace Sttz.Tweener.Plugins {

	/// <summary>
	/// Tween a single member in a struct (value type).
	/// </summary>
	/// <remarks>
	/// <para>
	/// Structs or Value Types are special types in .Net that are copied
	/// when used and teherefore aren't referenced and don't need to be
	/// garbage collected. This can speed up some operations but comes with
	/// the downside that it's not easily possible to tween a single member
	/// of a struct while leaving the other members alone.
	/// </para>
	/// <para>
	/// E.g. Unity's Vector3 and Quaternion types are value types. Trying to
	/// tween a property on Vector3 or Quaternion will produce an error in
	/// Animate. If it proceeded, Animeate would only update it's copy but would
	/// never actually update the value on the target object.
	/// </para>
	/// <para>
	/// Generally, it's faster to tween the whole value type instead of
	/// tweening a single of its properties using TweenStruct. E.g. instead of 
	/// tweening <c>transform.position.x</c>, tween <c>transform.position</c>
	/// instead. Sometimes it's necessary to tween only a single property
	/// of a value type and allow the other properties to be modified during
	/// the tween. In this case, the TweenStruct plugin can be used.
	/// </para>
	/// <code>
	/// // If possible, tween whole struct:
	/// Animate.To(transform, 2f, "position", Vector3.zero);
	/// 
	/// // Explicit usage, only tween the x value:
	/// Animate.To(transform, 2f, "position", Vector3.zero, TweenStruct.Use("x"));
	/// 
	/// // Automatic usage, plugin auto-detects when it's needed:
	/// Animate.Options.SetAutomatic(TweenStruct.Automatic());
	/// Animate.To(transform, 2f, "position.x", Vector3.zero);
	/// </code>
	/// </remarks>
	public static class TweenStruct
	{
		///////////////////
		// Plugin Use

		/// <summary>
		/// TweenPluginInfo that can be used for automatic activation.
		/// </summary>
		/// <seealso cref="ITweenOptions.SetAutomatic"/>
		/// <seealso cref="ITweenOptionsFluid<TContainer>.Automate"/>
		public static TweenPluginInfo Automatic()
		{
			return DefaultInfo;
		}

		/// <summary>
		/// Use the TweenStruct plugin for the current tween.
		/// </summary>
		/// <param name='property'>
		/// Name of the property on the struct. If you don't set the property
		/// here, you need to use the "struct.property" syntax for the tween's
		/// property.
		/// </param>
		public static TweenPluginInfo Use(string property = null)
		{
			var info = DefaultInfo;
			info.getValueUserData = info.setValueUserData = property;
			return info;
		}

		///////////////////
		// Activation

		// Default plugin info
		private static TweenPluginInfo DefaultInfo = new TweenPluginInfo() {
			// Generic plugin type
			pluginType = typeof(TweenStructImpl<>),
			// Plugin needs to get and set the value
			hooks = TweenPluginHook.GetValue | TweenPluginHook.SetValue,
			// Delegate to select proper plugin type for manual mode
			manualActivation = ManualActivation,
			// Enable automatic activation
			autoActivation = ShouldActivate
		};

		// Callback for manual activation
		private static TweenPluginInfo ManualActivation(ITween tween, TweenPluginInfo info)
		{
			info = ShouldActivate(tween, info);

			if (info.pluginType == null) {
				tween.Internal.Log(TweenLogLevel.Error,
					"TweenStruct: Struct or struct property could not be found "
					+ "for tween of {0} on {1}.",
					tween.Property, tween.Target);
			}

			return info;
		}

		// Returns if the plugin should activate automatically
		private static TweenPluginInfo ShouldActivate(ITween tween, TweenPluginInfo info)
		{
			string structProperty = null;
			string nestedProperty = null;
			MemberInfo member;

			// Manual activation path
			if (info.getValueUserData is string) {
				structProperty = tween.Property;
				nestedProperty = (info.getValueUserData as string);
			
			// Automatic activation path
			} else {
				// Split path, e.g. "struct.property"
				var parts = tween.Property.Split('.');
				if (parts.Length != 2) {
					return TweenPluginInfo.None;
				}
				structProperty = parts[0];
				nestedProperty = parts[1];
			}

			// Look for struct
			member = TweenCodegen.FindMember(tween.Target.GetType(), structProperty);
			if (member == null) {
				return TweenPluginInfo.None;
			}

			// Check type
			var memberType = TweenCodegen.MemberType(member);
			if (!memberType.IsValueType) {
				return TweenPluginInfo.None;
			}

			// Look for struct value
			var nestedMember = TweenCodegen.FindMember(memberType, nestedProperty);
			if (nestedMember == null) {
				return TweenPluginInfo.None;
			}

			// So far ok!
			info.getValueUserData = info.setValueUserData = new TweenStructArguments() {
				memberInfo = member,
				nestedMemberInfo = nestedMember
			};
			return info;
		}

		///////////////////
		// Implementation

		// Arguments
		private class TweenStructArguments
		{
			// MemberInfo (struct)
			public MemberInfo memberInfo;
			// MemberInfo for value on struct
			public MemberInfo nestedMemberInfo;
		}

		// User data
		private class TweenStructUserData<TValue>
		{
			// Arguments
			public TweenStructArguments arguments;
			// Handler to set struct on target
			public TweenCodegen.SetHandler<object, object> setter;
			// Setter for value on struct
			public TweenCodegen.SetHandler<object, TValue> nestedSetter;
		}

		// Plugin implementation
		private class TweenStructImpl<TValue> : TweenPlugin<TValue>
		{
			// Initialize
			public override string Initialize(ITween tween, TweenPluginHook hook, ref object userData)
			{
				TweenStructUserData<TValue> data = null;

				// Initialize user data
				if (userData is TweenStructArguments) {
					data = new TweenStructUserData<TValue>();
					data.arguments = (userData as TweenStructArguments);
					userData = data;
				} else {
					data = userData as TweenStructUserData<TValue>;
				}

				// Check type
				var memberType = TweenCodegen.MemberType(data.arguments.nestedMemberInfo);
				if (memberType != tween.ValueType) {
					return string.Format(
						"Mismatching types: Property type is {0} but tween type is {1} "
						+ "for tween of {2} on {3}.",
						memberType, tween.ValueType, tween.Property, tween.Target
					);
				}

				// Generate setters
				if (hook == TweenPluginHook.SetValue) {
					try {
						data.setter = TweenCodegen.GenerateSetMethod<object, object>(data.arguments.memberInfo);
						data.nestedSetter = TweenCodegen.GenerateSetMethod<object, TValue>(data.arguments.nestedMemberInfo);
					} catch (Exception e) {
						return string.Format(
							"Failed to generate setter methods for tween of {0} on {1}: {2}.",
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
				var data = (userData as TweenStructUserData<TValue>);

				// Get struct
				object value = null;
				MemberInfo member = data.arguments.memberInfo;
				if (member is PropertyInfo) {
					value = (member as PropertyInfo).GetValue(target, null);
				} else {
					value = (member as FieldInfo).GetValue(target);
				}

				// Get value in struct
				member = data.arguments.nestedMemberInfo;
				if (member is PropertyInfo) {
					return (TValue)(member as PropertyInfo).GetValue(value, null);
				} else {
					return (TValue)(member as FieldInfo).GetValue(value);
				}
			}

			///////////////////
			// Set Value Hook

			// Set the value of a plugin property
			public override void SetValue(object target, string property, TValue value, ref object userData)
			{
				var data = (userData as TweenStructUserData<TValue>);

				// Get struct
				object structValue = null;
				MemberInfo member = data.arguments.memberInfo;
				if (member is PropertyInfo) {
					structValue = (member as PropertyInfo).GetValue(target, null);
				} else {
					structValue = (member as FieldInfo).GetValue(target);
				}

				// Set value on struct
				data.nestedSetter(ref structValue, value);
				// Save struct back to target
				data.setter(ref target, structValue);
			}
		}

	}
}