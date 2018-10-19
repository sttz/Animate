#if !ENABLE_IL2CPP && !NET_STANDARD_2_0

using System;
using System.Collections.Generic;

using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

/* Runtime code generation helper methods / classes
 * Since AOT targets don't support codegen, everything
 * is collected here so it can be easily excluded.
 */

namespace Sttz.Tweener.Core.Codegen {

	/// <summary>
	/// Tweening helper methods for code generation.
	/// </summary>
	/// <remarks>
	/// These helper methods are not available with AOT compilation
	/// or .Net Standard.
	/// </remraks>
	public static class TweenCodegen
	{
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
	}

	/* Code taken from Stackoverflow:
	 * http://stackoverflow.com/a/756979/202741
	 * http://stackoverflow.com/users/88558/lucero
	 * 
	 * Licensed under Stackoverflow's cc-wiki license:
	 * http://creativecommons.org/licenses/by-sa/3.0/
	 */

	public delegate TResult BinaryOperator<TLeft, TRight, TResult>(TLeft left, TRight right);

	/// <summary>
	/// Provide efficient generic access to either native or static operators for the given type combination.
	/// </summary>
	/// <typeparam name="TLeft">The type of the left operand.</typeparam>
	/// <typeparam name="TRight">The type of the right operand.</typeparam>
	/// <typeparam name="TResult">The type of the result value.</typeparam>
	/// <remarks>Inspired by Keith Farmer's code on CodeProject:<br/>http://www.codeproject.com/KB/cs/genericoperators.aspx</remarks>
	public static class Operator<TLeft, TRight, TResult>
	{
		private static BinaryOperator<TLeft, TRight, TResult> addition;
		private static BinaryOperator<TLeft, TRight, TResult> bitwiseAnd;
		private static BinaryOperator<TLeft, TRight, TResult> bitwiseOr;
		private static BinaryOperator<TLeft, TRight, TResult> division;
		private static BinaryOperator<TLeft, TRight, TResult> exclusiveOr;
		private static BinaryOperator<TLeft, TRight, TResult> leftShift;
		private static BinaryOperator<TLeft, TRight, TResult> modulus;
		private static BinaryOperator<TLeft, TRight, TResult> multiply;
		private static BinaryOperator<TLeft, TRight, TResult> rightShift;
		private static BinaryOperator<TLeft, TRight, TResult> subtraction;

		/// <summary>
		/// Gets the addition operator + (either native or "op_Addition").
		/// </summary>
		/// <value>The addition operator.</value>
		public static BinaryOperator<TLeft, TRight, TResult> Addition {
			get {
				if (addition == null) {
					addition = CreateOperator("op_Addition", OpCodes.Add);
				}
				return addition;
			}
		}

		/// <summary>
		/// Gets the modulus operator % (either native or "op_Modulus").
		/// </summary>
		/// <value>The modulus operator.</value>
		public static BinaryOperator<TLeft, TRight, TResult> Modulus {
			get {
				if (modulus == null) {
					modulus = CreateOperator("op_Modulus", OpCodes.Rem);
				}
				return modulus;
			}
		}

		/// <summary>
		/// Gets the exclusive or operator ^ (either native or "op_ExclusiveOr").
		/// </summary>
		/// <value>The exclusive or operator.</value>
		public static BinaryOperator<TLeft, TRight, TResult> ExclusiveOr {
			get {
				if (exclusiveOr == null) {
					exclusiveOr = CreateOperator("op_ExclusiveOr", OpCodes.Xor);
				}
				return exclusiveOr;
			}
		}

		/// <summary>
		/// Gets the bitwise and operator &amp; (either native or "op_BitwiseAnd").
		/// </summary>
		/// <value>The bitwise and operator.</value>
		public static BinaryOperator<TLeft, TRight, TResult> BitwiseAnd {
			get {
				if (bitwiseAnd == null) {
					bitwiseAnd = CreateOperator("op_BitwiseAnd", OpCodes.And);
				}
				return bitwiseAnd;
			}
		}

		/// <summary>
		/// Gets the division operator / (either native or "op_Division").
		/// </summary>
		/// <value>The division operator.</value>
		public static BinaryOperator<TLeft, TRight, TResult> Division {
			get {
				if (division == null) {
					division = CreateOperator("op_Division", OpCodes.Div);
				}
				return division;
			}
		}

		/// <summary>
		/// Gets the multiplication operator * (either native or "op_Multiply").
		/// </summary>
		/// <value>The multiplication operator.</value>
		public static BinaryOperator<TLeft, TRight, TResult> Multiply {
			get {
				if (multiply == null) {
					multiply = CreateOperator("op_Multiply", OpCodes.Mul);
				}
				return multiply;
			}
		}

		/// <summary>
		/// Gets the bitwise or operator | (either native or "op_BitwiseOr").
		/// </summary>
		/// <value>The bitwise or operator.</value>
		public static BinaryOperator<TLeft, TRight, TResult> BitwiseOr {
			get {
				if (bitwiseOr == null) {
					bitwiseOr = CreateOperator("op_BitwiseOr", OpCodes.Or);
				}
				return bitwiseOr;
			}
		}

		/// <summary>
		/// Gets the left shift operator &lt;&lt; (either native or "op_LeftShift").
		/// </summary>
		/// <value>The left shift operator.</value>
		public static BinaryOperator<TLeft, TRight, TResult> LeftShift {
			get {
				if (leftShift == null) {
					leftShift = CreateOperator("op_LeftShift", OpCodes.Shl);
				}
				return leftShift;
			}
		}

		/// <summary>
		/// Gets the right shift operator &gt;&gt; (either native or "op_RightShift").
		/// </summary>
		/// <value>The right shift operator.</value>
		public static BinaryOperator<TLeft, TRight, TResult> RightShift {
			get {
				if (rightShift == null) {
					rightShift = CreateOperator("op_RightShift", OpCodes.Shr);
				}
				return rightShift;
			}
		}

		/// <summary>
		/// Gets the subtraction operator - (either native or "op_Addition").
		/// </summary>
		/// <value>The subtraction operator.</value>
		public static BinaryOperator<TLeft, TRight, TResult> Subtraction {
			get {
				if (subtraction == null) {
					subtraction = CreateOperator("op_Subtraction", OpCodes.Sub);
				}
				return subtraction;
			}
		}

		private static BinaryOperator<TLeft, TRight, TResult> CreateOperator(string operatorName, OpCode opCode)
		{
			if (operatorName == null) {
				throw new ArgumentNullException("operatorName");
			}
			bool isPrimitive = true;
			bool isLeftNullable;
			bool isRightNullable = false;
			Type leftType = typeof(TLeft);
			Type rightType = typeof(TRight);
			MethodInfo operatorMethod = LookupOperatorMethod(ref leftType, operatorName, ref isPrimitive, out isLeftNullable) ??
							LookupOperatorMethod(ref rightType, operatorName, ref isPrimitive, out isRightNullable);
			DynamicMethod method = new DynamicMethod(string.Format("{0}:{1}:{2}:{3}", operatorName, typeof(TLeft).FullName, typeof(TRight).FullName, typeof(TResult).FullName), typeof(TResult),
								 new Type[] { typeof(TLeft), typeof(TRight) });
			//Debug.WriteLine(method.Name, "Generating operator method");
			ILGenerator generator = method.GetILGenerator();
			if (isPrimitive) {
				//Debug.WriteLine("Primitives using opcode", "Emitting operator code");
				generator.Emit(OpCodes.Ldarg_0);
				if (isLeftNullable) {
					generator.EmitCall(OpCodes.Call, typeof(TLeft).GetMethod("op_Explicit", BindingFlags.Public | BindingFlags.Static), null);
				}
				IlTypeHelper.ILType stackType = IlTypeHelper.EmitWidening(generator, IlTypeHelper.GetILType(leftType), IlTypeHelper.GetILType(rightType));
				generator.Emit(OpCodes.Ldarg_1);
				if (isRightNullable) {
					generator.EmitCall(OpCodes.Call, typeof(TRight).GetMethod("op_Explicit", BindingFlags.Public | BindingFlags.Static), null);
				}
				stackType = IlTypeHelper.EmitWidening(generator, IlTypeHelper.GetILType(rightType), stackType);
				generator.Emit(opCode);
				if (typeof(TResult) == typeof(object)) {
					generator.Emit(OpCodes.Box, IlTypeHelper.GetPrimitiveType(stackType));
				} else {
					Type resultType = typeof(TResult);
					if (IsNullable(ref resultType)) {
						generator.Emit(OpCodes.Newobj, typeof(TResult).GetConstructor(new Type[] { resultType }));
					} else {
						IlTypeHelper.EmitExplicit(generator, stackType, IlTypeHelper.GetILType(resultType));
					}
				}
			} else if (operatorMethod != null) {
				//Debug.WriteLine("Call to static operator method", "Emitting operator code");
				generator.Emit(OpCodes.Ldarg_0);
				generator.Emit(OpCodes.Ldarg_1);
				generator.EmitCall(OpCodes.Call, operatorMethod, null);
				if (typeof(TResult).IsPrimitive && operatorMethod.ReturnType.IsPrimitive) {
					IlTypeHelper.EmitExplicit(generator, IlTypeHelper.GetILType(operatorMethod.ReturnType), IlTypeHelper.GetILType(typeof(TResult)));
				} else if (!typeof(TResult).IsAssignableFrom(operatorMethod.ReturnType)) {
					//Debug.WriteLine("Conversion to return type", "Emitting operator code");
					generator.Emit(OpCodes.Ldtoken, typeof(TResult));
					generator.EmitCall(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) }), null);
					generator.EmitCall(OpCodes.Call, typeof(Convert).GetMethod("ChangeType", new Type[] { typeof(object), typeof(Type) }), null);
				}
			} else {
				//Debug.WriteLine("Throw NotSupportedException", "Emitting operator code");
				generator.ThrowException(typeof(NotSupportedException));
			}
			generator.Emit(OpCodes.Ret);
			return (BinaryOperator<TLeft, TRight, TResult>)method.CreateDelegate(typeof(BinaryOperator<TLeft, TRight, TResult>));
		}

		private static bool IsNullable(ref Type type)
		{
			if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>))) {
				type = type.GetGenericArguments()[0];
				return true;
			}
			return false;
		}

		private static MethodInfo LookupOperatorMethod(ref Type type, string operatorName, ref bool isPrimitive, out bool isNullable)
		{
			isNullable = IsNullable(ref type);
			if (!type.IsPrimitive) {
				isPrimitive = false;
				foreach (MethodInfo methodInfo in type.GetMethods(BindingFlags.Static | BindingFlags.Public)) {
					if (methodInfo.Name == operatorName) {
						bool isMatch = true;
						foreach (ParameterInfo parameterInfo in methodInfo.GetParameters()) {
							switch (parameterInfo.Position) {
								case 0:
									if (parameterInfo.ParameterType != typeof(TLeft)) {
										isMatch = false;
									}
									break;
								case 1:
									if (parameterInfo.ParameterType != typeof(TRight)) {
										isMatch = false;
									}
									break;
								default:
									isMatch = false;
									break;
							}
						}
						if (isMatch) {
							if (typeof(TResult).IsAssignableFrom(methodInfo.ReturnType) || typeof(IConvertible).IsAssignableFrom(methodInfo.ReturnType)) {
								return methodInfo; // full signature match
							}
						}
					}
				}
			}
			return null;
		}
	}

	internal static class IlTypeHelper
	{
		[Flags]
		public enum ILType
		{
			None = 0,
			Unsigned = 1,
			B8 = 2,
			B16 = 4,
			B32 = 8,
			B64 = 16,
			Real = 32,
			I1 = B8, // 2
			U1 = B8 | Unsigned, // 3
			I2 = B16, // 4
			U2 = B16 | Unsigned, // 5
			I4 = B32, // 8
			U4 = B32 | Unsigned, // 9
			I8 = B64, //16
			U8 = B64 | Unsigned, //17
			R4 = B32 | Real, //40
			R8 = B64 | Real //48
		}

		public static ILType GetILType(Type type)
		{
			if (type == null) {
				throw new ArgumentNullException("type");
			}
			if (!type.IsPrimitive) {
				throw new ArgumentException("IL native operations requires primitive types", "type");
			}
			if (type == typeof(double)) {
				return ILType.R8;
			}
			if (type == typeof(float)) {
				return ILType.R4;
			}
			if (type == typeof(ulong)) {
				return ILType.U8;
			}
			if (type == typeof(long)) {
				return ILType.I8;
			}
			if (type == typeof(uint)) {
				return ILType.U4;
			}
			if (type == typeof(int)) {
				return ILType.I4;
			}
			if (type == typeof(short)) {
				return ILType.U2;
			}
			if (type == typeof(ushort)) {
				return ILType.I2;
			}
			if (type == typeof(byte)) {
				return ILType.U1;
			}
			if (type == typeof(sbyte)) {
				return ILType.I1;
			}
			return ILType.None;
		}

		public static Type GetPrimitiveType(ILType iLType)
		{
			switch (iLType) {
				case ILType.R8:
					return typeof(double);
				case ILType.R4:
					return typeof(float);
				case ILType.U8:
					return typeof(ulong);
				case ILType.I8:
					return typeof(long);
				case ILType.U4:
					return typeof(uint);
				case ILType.I4:
					return typeof(int);
				case ILType.U2:
					return typeof(short);
				case ILType.I2:
					return typeof(ushort);
				case ILType.U1:
					return typeof(byte);
				case ILType.I1:
					return typeof(sbyte);
			}
			throw new ArgumentOutOfRangeException("iLType");
		}

		public static ILType EmitWidening(ILGenerator generator, ILType onStackIL, ILType otherIL)
		{
			if (generator == null) {
				throw new ArgumentNullException("generator");
			}
			if (onStackIL == ILType.None) {
				throw new ArgumentException("Stack needs a value", "onStackIL");
			}
			if (onStackIL < ILType.I8) {
				onStackIL = ILType.I8;
			}
			if ((onStackIL < otherIL) && (onStackIL != ILType.R4)) {
				switch (otherIL) {
					case ILType.R4:
					case ILType.R8:
						if ((onStackIL & ILType.Unsigned) == ILType.Unsigned) {
							generator.Emit(OpCodes.Conv_R_Un);
						} else if (onStackIL != ILType.R4) {
							generator.Emit(OpCodes.Conv_R8);
						} else {
							return ILType.R4;
						}
						return ILType.R8;
					case ILType.U8:
					case ILType.I8:
						if ((onStackIL & ILType.Unsigned) == ILType.Unsigned) {
							generator.Emit(OpCodes.Conv_U8);
							return ILType.U8;
						}
						if (onStackIL != ILType.I8) {
							generator.Emit(OpCodes.Conv_I8);
						}
						return ILType.I8;
				}
			}
			return onStackIL;
		}

		public static void EmitExplicit(ILGenerator generator, ILType onStackIL, ILType otherIL)
		{
			if (otherIL != onStackIL) {
				switch (otherIL) {
					case ILType.I1:
						generator.Emit(OpCodes.Conv_I1);
						break;
					case ILType.I2:
						generator.Emit(OpCodes.Conv_I2);
						break;
					case ILType.I4:
						generator.Emit(OpCodes.Conv_I4);
						break;
					case ILType.I8:
						generator.Emit(OpCodes.Conv_I8);
						break;
					case ILType.U1:
						generator.Emit(OpCodes.Conv_U1);
						break;
					case ILType.U2:
						generator.Emit(OpCodes.Conv_U2);
						break;
					case ILType.U4:
						generator.Emit(OpCodes.Conv_U4);
						break;
					case ILType.U8:
						generator.Emit(OpCodes.Conv_U8);
						break;
					case ILType.R4:
						generator.Emit(OpCodes.Conv_R4);
						break;
					case ILType.R8:
						generator.Emit(OpCodes.Conv_R8);
						break;
				}
			}
		}
	}
}

#endif