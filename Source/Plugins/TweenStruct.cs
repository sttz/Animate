#if !ENABLE_IL2CPP && !NET_STANDARD_2_0

using System;
using System.Collections.Generic;
using System.Reflection;

using Sttz.Tweener.Core;

namespace Sttz.Tweener {

/// <summary>
/// Tween a single member in a struct (value type).
/// </summary>
/// <remarks>
/// > [!NOTE]
/// > TweenStruct uses codegen and is therefore not available with
/// > AOT/IL2CPP or .Net Standard.
/// >
/// > Instead, you can use a virtual property to tween a struct member:
/// > ```cs
/// > Animate.EnableAccess("field.x",
/// >     (Example t) => t.field.x,
/// >     (t, v) => { var field = t.field; field.x = v; t.field = field; });
/// > ```
/// 
/// Structs or Value Types are special types in .Net that are copied
/// when used and therefore aren't referenced and don't need to be
/// garbage collected. This can speed up some operations but comes with
/// the downside that it's not easily possible to tween a single member
/// of a struct while leaving the other members alone.
/// 
/// E.g. Unity's Vector3 and Quaternion types are value types. Trying to
/// tween a property on Vector3 or Quaternion will produce an error in
/// Animate. If it proceeded, Animate would only update its copy but would
/// never actually update the value on the target object.
/// 
/// Generally, it's faster to tween the whole value type instead of
/// tweening a single of its properties using TweenStruct. E.g. instead of 
/// tweening <c>transform.position.x</c>, tween <c>transform.position</c>
/// instead. Sometimes it's necessary to tween only a single property
/// of a value type and allow the other properties to be modified during
/// the tween. In this case, the TweenStruct plugin can be used.
/// 
/// ```cs
/// // If possible, tween the whole struct:
/// Animate.To(transform, 2f, "position", Vector3.zero);
/// 
/// // Explicit usage, only tween the x value:
/// Animate.To(transform, 2f, "position.x", Vector3.zero)
/// 	.Struct();
/// 
/// // Automatic usage, plugin auto-detects when it's needed:
/// Animate.Options.EnablePlugin(TweenStruct.Loader);
/// Animate.To(transform, 2f, "position.x", Vector3.zero);
/// ```
/// </remarks>
public static class TweenStruct
{
	// -------- Plugin Use --------

	/// <summary>
	/// TweenStruct plugin loader.
	/// </summary>
	/// <remarks>
	/// Pass this method to <see cref="TweenOptions.EnablePlugin"/> to enable the
	/// plugin for the options scope.
	/// </remarks>
	public static PluginResult Loader(Tween tween, bool required)
	{
		return Loader(tween, required, null);
	}

	/// <summary>
	/// Create a custom TweenStruct plugin loader that defines the
	/// nested struct property name.
	/// </summary>
	/// <remarks>
	/// Pass the result of this method to <see cref="TweenOptions.EnablePlugin"/> to enable the
	/// plugin for the options scope.
	/// </remarks>
	/// <param name="property">Name of the property on the struct</param>
	/// <returns>A custom plugin loader that forces the property type</returns>
	public static PluginLoader CustomLoader(string property)
	{
		if (property == null) {
			return Loader;
		} else {
			return (tween, required) => {
				return Loader(tween, required, property);
			};
		}
	}

	/// <summary>
	/// Require the <see cref="TweenStruct"/> plugin for the current tween.
	/// </summary>
	/// <remarks>
	/// Shorthand for using <see cref="TweenOptions.EnablePlugin"/> with type-checking.
	/// </remarks>
	/// <param name="property">Name of the property on the struct</param>
	public static Tween<TTarget, TValue> Struct<TTarget, TValue>(
		this Tween<TTarget, TValue> tween, string property = null
	)
		where TTarget : class
		where TValue : struct
	{
		tween.Options.EnablePlugin(CustomLoader(property), true, true);
		return tween;
	}

	// -------- Implementation --------

	private static PluginResult Loader(Tween tween, bool required, string property)
	{
		string structProperty = null;
		string nestedProperty = null;
		MemberInfo member;

		// Manual activation path
		if (property != null) {
			structProperty = tween.Property;
			nestedProperty = property;

		// Automatic activation path
		} else {
			// Split path, e.g. "struct.property"
			var parts = tween.Property.Split('.');
			if (parts.Length != 2) {
				return PluginResult.Error("TweenStruct: Property doesn't contain a single dot: {0}".LazyFormat(tween.Property));
			}
			structProperty = parts[0];
			nestedProperty = parts[1];
		}

		// Look for struct
		member = TweenReflection.FindMember(tween.TargetType, structProperty);
		if (member == null) {
			return PluginResult.Error("TweenStruct: Member {0} not found on target type {1}".LazyFormat(structProperty, tween.TargetType));
		}

		// Check type
		var memberType = TweenReflection.MemberType(member);
		if (!memberType.IsValueType) {
			return PluginResult.Error("TweenStruct: Member {0} is not a value type.".LazyFormat(structProperty));
		}

		// Look for struct value
		var nestedMember = TweenReflection.FindMember(memberType, nestedProperty);
		if (nestedMember == null) {
			return PluginResult.Error("TweenStruct: Member {0} not found on struct {1}".LazyFormat(nestedProperty, memberType));
		}

		// So far ok!
		var userData = new TweenStructArguments() {
			memberInfo = member,
			nestedMemberInfo = nestedMember
		};
		var pluginType = typeof(TweenStructImpl<,>).MakeGenericType(tween.TargetType, tween.ValueType);
		var instance = GetInstance(pluginType);

		// Set plugin type to use
		return PluginResult.Load(instance, userData: userData);
	}

	static Dictionary<Type, ITweenPlugin> instances;

	// Shared plugin instances
	private static ITweenPlugin GetInstance(Type pluginType)
	{
		if (instances == null) instances = new Dictionary<Type, ITweenPlugin>();

		ITweenPlugin instance;
		if (instances.TryGetValue(pluginType, out instance)) {
			return instance;
		}

		instance = (ITweenPlugin)Activator.CreateInstance(pluginType);
		instances[pluginType] = instance;
		return instance;
	}

	// Arguments
	private class TweenStructArguments
	{
		// MemberInfo (struct)
		public MemberInfo memberInfo;
		// MemberInfo for value on struct
		public MemberInfo nestedMemberInfo;
	}

	// User data
	private class TweenStructUserData<TTarget, TValue>
	{
		// Arguments
		public TweenStructArguments arguments;
		// Handler to set struct on target
		public TweenCodegen.SetHandler<TTarget, object> setter;
		// Setter for value on struct
		public TweenCodegen.SetHandler<object, TValue> nestedSetter;
	}

	// Plugin implementation
	private class TweenStructImpl<TTarget, TValue>
		: ITweenGetterPlugin<TTarget, TValue>, ITweenSetterPlugin<TTarget, TValue>
		where TTarget : class
	{
		// Initialize
		public string Initialize(Tween tween, TweenPluginType initForType, ref object userData)
		{
			TweenStructUserData<TTarget, TValue> data = null;

			// Initialize user data
			if (userData is TweenStructArguments) {
				data = new TweenStructUserData<TTarget, TValue>();
				data.arguments = (userData as TweenStructArguments);
				userData = data;
			} else {
				data = userData as TweenStructUserData<TTarget, TValue>;
			}

			// Check type
			var memberType = TweenReflection.MemberType(data.arguments.nestedMemberInfo);
			if (memberType != tween.ValueType) {
				return string.Format(
					"Mismatching types: Property type is {0} but tween type is {1} "
					+ "for tween of {2} on {3}.",
					memberType, tween.ValueType, tween.Property, tween.Target
				);
			}

			// Generate setters
			if (initForType == TweenPluginType.Setter) {
				try {
					data.setter = TweenCodegen.GenerateSetMethod<TTarget, object>(data.arguments.memberInfo);
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

		// -------- Get Value Hook --------

		// Get the value of a plugin property
		public TValue GetValue(TTarget target, string property, ref object userData)
		{
			var data = (userData as TweenStructUserData<TTarget, TValue>);

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

		// -------- Set Value Hook --------

		// Set the value of a plugin property
		public void SetValue(TTarget target, string property, TValue value, ref object userData)
		{
			var data = (userData as TweenStructUserData<TTarget, TValue>);

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

#endif
