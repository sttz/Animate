using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace Sttz.Tweener.Core {

	/// <summary>
	/// Tween reflection.
	/// </summary>
	public static class TweenReflection
	{
		// Binding flags used to look for properties
		private static BindingFlags bindingFlags = 
			  BindingFlags.Public 
			| BindingFlags.NonPublic 
			| BindingFlags.Instance 
			| BindingFlags.Static;

		// Delegate use to create IL set handler
		public delegate void SetHandler<TTarget, TValue>(ref TTarget target, TValue value);

		// Enable caching for generated setters
		public static bool enableCaching = true;

		// Cached set handlers
		// Handlers are stored with a string concatenated from the two 
		// argument types and the two actual actual types, separated by ;
		// e.g. TTarget;TValue;declaringType;valueType
		private static Dictionary<string, object> cachedHandlers
			 = new Dictionary<string, object>();

		// Method to generate set handler
		public static SetHandler<TTarget, TValue> GenerateSetMethod<TTarget, TValue>(MemberInfo info)
		{
			// Get type info
			PropertyInfo pInfo = null;
			FieldInfo fInfo = null;
			Type valueType = null;
			Type declaringType = info.DeclaringType;

			if (info is PropertyInfo) {
				pInfo = info as PropertyInfo;
				valueType = pInfo.PropertyType;
			} else {
				fInfo = info as FieldInfo;
				valueType = fInfo.FieldType;
			}

			// Return cached setter if enabled and existing
			string identifier = null;
			if (enableCaching) {
				identifier = string.Format("{0};{1};{2};{3}",
					typeof(TValue).FullName, typeof(TTarget).FullName, 
					declaringType.FullName, info.Name);
				if (cachedHandlers.ContainsKey(identifier)) {
					return (SetHandler<TTarget, TValue>)cachedHandlers[identifier];
				}
			}

			// Create setter type
			var args = new Type[] { typeof(TTarget).MakeByRefType(), typeof(TValue) };
			var setterType = typeof(SetHandler<,>).MakeGenericType(
				new Type[] { typeof(TTarget), typeof(TValue) }
			);

			// Create setter
			var dynamicSet = new DynamicMethod(
				"SetHandler", typeof(void), args, typeof(TTarget), true
			);
			var setGenerator = dynamicSet.GetILGenerator();

			// Generate method body
			setGenerator.Emit(OpCodes.Ldarg_0);
			setGenerator.Emit(OpCodes.Ldind_Ref);
			if (!typeof(TTarget).IsValueType && declaringType.IsValueType) {
				setGenerator.Emit(OpCodes.Unbox, declaringType);
			}
			setGenerator.Emit(OpCodes.Ldarg_1);
			if (!typeof(TValue).IsValueType && valueType.IsValueType) {
				setGenerator.Emit(OpCodes.Unbox_Any, valueType);
			}
			if (pInfo != null) {
				setGenerator.Emit(OpCodes.Callvirt, pInfo.GetSetMethod(true));
			} else {
				setGenerator.Emit(OpCodes.Stfld, fInfo);
			}
			setGenerator.Emit(OpCodes.Ret);

			// Return delegate
			var setter = (SetHandler<TTarget, TValue>)dynamicSet.CreateDelegate(setterType);

			// Cache setter if enabled
			if (enableCaching) {
				cachedHandlers[identifier] = setter;
			}

			return setter;
		}

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
}

