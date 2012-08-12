using System;

using Sttz.Tweener.Core;
namespace Sttz.Tweener {

	/// <summary>
	/// Delegate for easing methods.
	/// </summary>
	/// <remarks>
	/// You can create custom easing methods simply by implementing
	/// a method with this delegate signature.
	/// </remarks>
	public delegate float EasingMethod(float position);

	/// <summary>
	/// Collection of easing methods.
	/// </summary>
	/// <remarks>
	/// <para>Easing methods change the speed of the tween over its duration.</para>
	/// 
	/// <para><see cref="Easing.Linear"/> is the same as applying no easing:
	/// the speed of the tween will stay constant over its duration.</para>
	/// 
	/// <para>The regular set of easing methods will start slow and speed up (In)
	/// until reaching full speed at the end, start at full speed and slow down
	/// towards the end (Out) or start and end slow and reach full speed at
	/// their halfpoint (InOut).</para>
	/// 
	/// <para>The regular easing methods, sorted by strength:
	/// <list type="bullet">
	/// <item><see cref="QuadraticOut"/></item>
	/// <item><see cref="CubicOut"/></item>
	/// <item><see cref="QuarticOut"/></item>
	/// <item><see cref="QuinticOut"/></item>
	/// <item><see cref="SinusoidalOut"/></item>
	/// <item><see cref="ExponentialOut"/></item>
	/// <item><see cref="CircularOut"/></item>
	/// </list></para>
	/// 
	/// 
	/// <para>There are also three special easing methods: <see cref="BackOut"/>,
	/// <see cref="BounceOut"/> and <see cref="ElasticOut"/>. Back first moves
	/// away from the target before starting to move towards it, Bounce bounces
	/// off the start or end, reaching the start/end value multiple times and
	/// Elastic is similar to bounce but moves beyond the start/end values.</para>
	/// 
	/// <para>These easing methods are based on Robert Penner's easing
	/// equations, first published in his Flash programming book in 2002. See 
	/// <see cref="http://www.robertpenner.com/easing/"/> for more information
	/// as well as an interactive visualization of these methods.</para>
	/// </remarks>
	/// <seealso cref="ITweenOptions.Easing"/>
	/// <seealso cref="ITweenOptionsFluid<TContainer>.Ease"/>
	/// <seealso cref="EasingMethod"/>
	public static class Easing
	{
		///////////////////
		// Easings

		/// <summary>
		/// Linear easing (no easing).
		/// </summary>
		public static EasingMethod Linear = EasingImpl.LinearImpl;

		/// <summary>
		/// Quadratic easing, in direction.
		/// </summary>
		public static EasingMethod QuadraticIn = EasingImpl.QuadraticInImpl;
		/// <summary>
		/// Quadratic easing, out direction.
		/// </summary>
		public static EasingMethod QuadraticOut = EasingImpl.QuadraticOutImpl;
		/// <summary>
		/// Quadratic easing, in-out direction.
		/// </summary>
		public static EasingMethod QuadraticInOut = EasingImpl.QuadraticInOutImpl;

		/// <summary>
		/// Cubic easing, in direction.
		/// </summary>
		public static EasingMethod CubicIn = EasingImpl.CubicInImpl;
		/// <summary>
		/// Cubic easing, out direction.
		/// </summary>
		public static EasingMethod CubicOut = EasingImpl.CubicOutImpl;
		/// <summary>
		/// Cubic easing, in-out direction.
		/// </summary>
		public static EasingMethod CubicInOut = EasingImpl.CubicInOutImpl;

		/// <summary>
		/// Quartic easing, in direction.
		/// </summary>
		public static EasingMethod QuarticIn = EasingImpl.QuarticInImpl;
		/// <summary>
		/// Quartic easing, out direction.
		/// </summary>
		public static EasingMethod QuarticOut = EasingImpl.QuarticOutImpl;
		/// <summary>
		/// Quartic easing, in-out direction.
		/// </summary>
		public static EasingMethod QuarticInOut = EasingImpl.QuarticInOutImpl;

		/// <summary>
		/// Quintic easing, in direction.
		/// </summary>
		public static EasingMethod QuinticIn = EasingImpl.QuinticInImpl;
		/// <summary>
		/// Quintic easing, out direction.
		/// </summary>
		public static EasingMethod QuinticOut = EasingImpl.QuinticInOutImpl;
		/// <summary>
		/// Quintic easing, in-out direction.
		/// </summary>
		public static EasingMethod QuinticInOut = EasingImpl.QuinticInOutImpl;

		/// <summary>
		/// Sinusoidal easing, in direction.
		/// </summary>
		public static EasingMethod SinusoidalIn = EasingImpl.SinusoidalInImpl;
		/// <summary>
		/// Sinusoidal easing, out direction.
		/// </summary>
		public static EasingMethod SinusoidalOut = EasingImpl.SinusoidalOutImpl;
		/// <summary>
		/// Sinusoidal easing, in-out direction.
		/// </summary>
		public static EasingMethod SinusoidalInOut = EasingImpl.SinusoidalInOutImpl;

		/// <summary>
		/// Exponential easing, in direction.
		/// </summary>
		public static EasingMethod ExponentialIn = EasingImpl.ExponentialInImpl;
		/// <summary>
		/// Exponential easing, out direction.
		/// </summary>
		public static EasingMethod ExponentialOut = EasingImpl.ExponentialOutImpl;
		/// <summary>
		/// Exponential easing, in-out direction.
		/// </summary>
		public static EasingMethod ExponentialInOut = EasingImpl.ExponentialInOutImpl;

		/// <summary>
		/// Circular easing, in direction.
		/// </summary>
		public static EasingMethod CircularIn = EasingImpl.CircularInImpl;
		/// <summary>
		/// Circular easing, out direction.
		/// </summary>
		public static EasingMethod CircularOut = EasingImpl.CircularOutImpl;
		/// <summary>
		/// Circular easing, in-out direction.
		/// </summary>
		public static EasingMethod CircularInOut = EasingImpl.CircularInOutImpl;

		/// <summary>
		/// Back easing, in direction.
		/// </summary>
		public static EasingMethod BackIn = EasingImpl.BackInImpl;
		/// <summary>
		/// Back easing, out direction.
		/// </summary>
		public static EasingMethod BackOut = EasingImpl.BackInOutImpl;
		/// <summary>
		/// Back easing, in-out direction.
		/// </summary>
		public static EasingMethod BackInOut = EasingImpl.BackInOutImpl;

		/// <summary>
		/// Bounce easing, in direction.
		/// </summary>
		public static EasingMethod BounceIn = EasingImpl.BounceInImpl;
		/// <summary>
		/// Bounce easing, out direction.
		/// </summary>
		public static EasingMethod BounceOut = EasingImpl.BounceOutImpl;
		/// <summary>
		/// Bounce easing, in-out direction.
		/// </summary>
		public static EasingMethod BounceInOut = EasingImpl.BounceInOutImpl;

		/// <summary>
		/// Elastic easing, in direction.
		/// </summary>
		public static EasingMethod ElasticIn = EasingImpl.ElasticInImpl;
		/// <summary>
		/// Elastic easing, out direction.
		/// </summary>
		public static EasingMethod ElasticOut = EasingImpl.ElasticInOutImpl;
		/// <summary>
		/// Elastic easing, in-out direction.
		/// </summary>
		public static EasingMethod ElasticInOut = EasingImpl.ElasticInOutImpl;

		///////////////////
		// Easings with custom parameters

		/// <summary>
		/// Default back swing amount.
		/// </summary>
		public const float BackDefaultSwing = 1.70158f;

		/// <summary>
		/// Back easing with custom swing amount, in direction.
		/// </summary>
		/// <param name='swing'>
		/// Swing amount.
		/// </param>
		public static EasingMethod BackInCustom(float swing = BackDefaultSwing)
		{
			return delegate(float position) {
				return EasingImpl.BackInImpl(position, swing, 0);
			};
		}

		/// <summary>
		/// Back easing with custom swing amount, out direction.
		/// </summary>
		/// <param name='swing'>
		/// Swing amount.
		/// </param>
		public static EasingMethod BackOutCustom(float swing = BackDefaultSwing)
		{
			return delegate(float position) {
				return EasingImpl.BackOutImpl(position, swing, 0);
			};
		}

		/// <summary>
		/// Back easing with custom swing amount, in-out direction.
		/// </summary>
		/// <param name='swing'>
		/// Swing amount.
		/// </param>
		public static EasingMethod BackInOutCustom(float swing = BackDefaultSwing)
		{
			return delegate(float position) {
				return EasingImpl.BackInOutImpl(position, swing, 0);
			};
		}

		/// <summary>
		/// Default elastic amplitude (1 = full target value, < 1 never reaches target).
		/// </summary>
		public const float ElasticDefaultAmplitude = 1f;
		/// <summary>
		/// Default elastic period (number of swings = 1 / p).
		/// </summary>
		public const float ElasticDefaultPeriod = 0.3f;

		/// <summary>
		/// Elastic easing with custom amplitude and period, in direction.
		/// </summary>
		/// <param name='amplitude'>
		/// Amplitude of the elasticity (1 = full target value, < 1 never reaches target).
		/// </param>
		/// <param name='period'>
		/// Period of the elasticity (1 / period = number of swings).
		/// </param>
		public static EasingMethod ElasticInCustom(float amplitude = 0, float period = 0)
		{
			return delegate(float position) {
				return EasingImpl.ElasticInImpl(position, amplitude, period);
			};
		}

		/// <summary>
		/// Elastic easing with custom amplitude and period, out direction.
		/// </summary>
		/// <param name='amplitude'>
		/// Amplitude of the elasticity (1 = full target value, < 1 never reaches target).
		/// </param>
		/// <param name='period'>
		/// Period of the elasticity (1 / period = number of swings).
		/// </param>
		public static EasingMethod ElasticOutCustom(float amplitude = 0, float period = 0)
		{
			return delegate(float position) {
				return EasingImpl.ElasticOutImpl(position, amplitude, period);
			};
		}

		/// <summary>
		/// Elastic easing with custom amplitude and period, in-out direction.
		/// </summary>
		/// <param name='amplitude'>
		/// Amplitude of the elasticity (1 = full target value, < 1 never reaches target).
		/// </param>
		/// <param name='period'>
		/// Period of the elasticity (1 / period = number of swings).
		/// </param>
		public static EasingMethod ElasticInOutCustom(float amplitude = 0, float period = 0)
		{
			return delegate(float position) {
				return EasingImpl.ElasticInOutImpl(position, amplitude, period);
			};
		}

		///////////////////
		// Type mapping

		/// <summary>
		/// Return the easing method for the given enum values. This allows
		/// to e.g. choose the easing in the Unity editor using the provided enums.
		/// </summary>
		/// <param name='type'>
		/// Easing type.
		/// </param>
		/// <param name='direction'>
		/// Easing direction.
		/// </param>
		public static EasingMethod EasingForType(EasingType type, EasingDirection direction)
		{
			switch (type) {
			case EasingType.Linear:
				return Linear;
			
			case EasingType.Quadratic:
				switch (direction) {
				case EasingDirection.In:
					return QuadraticIn;
				case EasingDirection.Out:
					return QuadraticOut;
				default:
					return QuadraticInOut;
				}
			
			case EasingType.Cubic:
				switch (direction) {
				case EasingDirection.In:
					return CubicIn;
				case EasingDirection.Out:
					return CubicOut;
				default:
					return CubicInOut;
				}

			case EasingType.Quartic:
				switch (direction) {
				case EasingDirection.In:
					return QuarticIn;
				case EasingDirection.Out:
					return QuarticOut;
				default:
					return QuarticInOut;
				}

			case EasingType.Quintic:
				switch (direction) {
				case EasingDirection.In:
					return QuinticIn;
				case EasingDirection.Out:
					return QuinticOut;
				default:
					return QuinticInOut;
				}

			case EasingType.Sinusoidal:
				switch (direction) {
				case EasingDirection.In:
					return SinusoidalIn;
				case EasingDirection.Out:
					return SinusoidalOut;
				default:
					return SinusoidalInOut;
				}

			case EasingType.Exponential:
				switch (direction) {
				case EasingDirection.In:
					return ExponentialIn;
				case EasingDirection.Out:
					return ExponentialOut;
				default:
					return ExponentialInOut;
				}

			case EasingType.Circular:
				switch (direction) {
				case EasingDirection.In:
					return CircularIn;
				case EasingDirection.Out:
					return CircularOut;
				default:
					return CircularInOut;
				}

			case EasingType.Back:
				switch (direction) {
				case EasingDirection.In:
					return BackIn;
				case EasingDirection.Out:
					return BackOut;
				default:
					return BackInOut;
				}

			case EasingType.Bounce:
				switch (direction) {
				case EasingDirection.In:
					return BounceIn;
				case EasingDirection.Out:
					return BounceOut;
				default:
					return BounceInOut;
				}

			case EasingType.Elastic:
				switch (direction) {
				case EasingDirection.In:
					return ElasticIn;
				case EasingDirection.Out:
					return ElasticOut;
				default:
					return ElasticInOut;
				}
			}

			return null;
		}
	}

}