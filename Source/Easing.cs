using System;
using UnityEngine;

namespace Sttz.Tweener {

/// <summary>
/// Delegate for easing methods.
/// </summary>
using EasingMethod = Func<float, float>;

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
/// their half-point (InOut).</para>
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
/// http://www.robertpenner.com/easing/ for more information
/// as well as an interactive visualization of these methods.</para>
/// </remarks>
/// <seealso cref="TweenOptions.Easing"/>
/// <seealso cref="TweenOptionsFluid.Ease"/>
public static class Easing
{
	// -------- Easings --------

	/// <summary>
	/// Linear easing (no easing).
	/// </summary>
	public static readonly EasingMethod Linear = (position) => {
		return position;
	};

	/// <summary>
	/// Quadratic easing, in direction.
	/// </summary>
	public static readonly EasingMethod QuadraticIn = (position) => {
		return (position * position);
	};
	/// <summary>
	/// Quadratic easing, out direction.
	/// </summary>
	public static readonly EasingMethod QuadraticOut = (position) => {
		return (position * (position - 2f) * -1f);
	};
	/// <summary>
	/// Quadratic easing, in-out direction.
	/// </summary>
	public static readonly EasingMethod QuadraticInOut = (position) => {
		return InOut(QuadraticIn, QuadraticOut, position);
	};

	/// <summary>
	/// Cubic easing, in direction.
	/// </summary>
	public static readonly EasingMethod CubicIn = (position) => {
		return (position * position * position);
	};
	/// <summary>
	/// Cubic easing, out direction.
	/// </summary>
	public static readonly EasingMethod CubicOut = (position) => {
		return (Mathf.Pow(position - 1f, 3f) + 1f);
	};
	/// <summary>
	/// Cubic easing, in-out direction.
	/// </summary>
	public static readonly EasingMethod CubicInOut = (position) => {
		return InOut(CubicIn, CubicOut, position);
	};

	/// <summary>
	/// Quartic easing, in direction.
	/// </summary>
	public static readonly EasingMethod QuarticIn = (position) => {
		return (position * position * position * position);
	};
	/// <summary>
	/// Quartic easing, out direction.
	/// </summary>
	public static readonly EasingMethod QuarticOut = (position) => {
		return (1f - Mathf.Pow(position - 1f, 4f));
	};
	/// <summary>
	/// Quartic easing, in-out direction.
	/// </summary>
	public static readonly EasingMethod QuarticInOut = (position) => {
		return InOut(QuarticIn, QuarticOut, position);
	};

	/// <summary>
	/// Quintic easing, in direction.
	/// </summary>
	public static readonly EasingMethod QuinticIn = (position) => {
		return (position * position * position * position * position);
	};
	/// <summary>
	/// Quintic easing, out direction.
	/// </summary>
	public static readonly EasingMethod QuinticOut = (position) => {
		return (Mathf.Pow(position - 1f, 5f) + 1f);
	};
	/// <summary>
	/// Quintic easing, in-out direction.
	/// </summary>
	public static readonly EasingMethod QuinticInOut = (position) => {
		return InOut(QuinticIn, QuinticOut, position);
	};

	/// <summary>
	/// Sinusoidal easing, in direction.
	/// </summary>
	public static readonly EasingMethod SinusoidalIn = (position) => {
		return (Mathf.Sin((position - 1f) * (Mathf.PI / 2f)) + 1f);
	};
	/// <summary>
	/// Sinusoidal easing, out direction.
	/// </summary>
	public static readonly EasingMethod SinusoidalOut = (position) => {
		return Mathf.Sin(position * (Mathf.PI / 2f));
	};
	/// <summary>
	/// Sinusoidal easing, in-out direction.
	/// </summary>
	public static readonly EasingMethod SinusoidalInOut = (position) => {
		return InOut(SinusoidalIn, SinusoidalOut, position);
	};

	/// <summary>
	/// Exponential easing, in direction.
	/// </summary>
	public static readonly EasingMethod ExponentialIn = (position) => {
		return Mathf.Pow(2f, 10f * (position - 1f));
	};
	/// <summary>
	/// Exponential easing, out direction.
	/// </summary>
	public static readonly EasingMethod ExponentialOut = (position) => {
		return (-1f * Mathf.Pow(2f, -10f * position) + 1f);
	};
	/// <summary>
	/// Exponential easing, in-out direction.
	/// </summary>
	public static readonly EasingMethod ExponentialInOut = (position) => {
		return InOut(ExponentialIn, ExponentialOut, position);
	};

	/// <summary>
	/// Circular easing, in direction.
	/// </summary>
	public static readonly EasingMethod CircularIn = (position) => {
		return (-1f * Mathf.Sqrt(1f - position * position) + 1f);
	};
	/// <summary>
	/// Circular easing, out direction.
	/// </summary>
	public static readonly EasingMethod CircularOut = (position) => {
		return Mathf.Sqrt(1f - Mathf.Pow(position - 1f, 2f));
	};
	/// <summary>
	/// Circular easing, in-out direction.
	/// </summary>
	public static readonly EasingMethod CircularInOut = (position) => {
		return InOut(CircularIn, CircularOut, position);
	};

	/// <summary>
	/// Back easing, in direction.
	/// </summary>
	public static readonly EasingMethod BackIn = (position) => {
		return (position * position * ((Easing.BackDefaultSwing + 1f) * position - Easing.BackDefaultSwing));
	};
	/// <summary>
	/// Back easing, out direction.
	/// </summary>
	public static readonly EasingMethod BackOut = (position) => {
		position = position - 1f;
		return (position * position * ((Easing.BackDefaultSwing + 1f) * position + Easing.BackDefaultSwing) + 1f);
	};
	/// <summary>
	/// Back easing, in-out direction.
	/// </summary>
	public static readonly EasingMethod BackInOut = (position) => {
		return InOut(BackIn, BackOut, position);
	};

	/// <summary>
	/// Bounce easing, in direction.
	/// </summary>
	public static readonly EasingMethod BounceIn = (position) => {
		return (1f - BounceOut(1f - position));
	};
	/// <summary>
	/// Bounce easing, out direction.
	/// </summary>
	public static readonly EasingMethod BounceOut = (position) => {
		if (position < (1f / 2.75f)) {
			return (7.5625f * position * position);
		} else if (position < (2f / 2.75f)) {
			position -= (1.5f / 2.75f);
			return (7.5625f * position * position + 0.75f);
		} else if (position < (2.5f / 2.75f)) {
			position -= (2.25f / 2.75f);
			return (7.5625f* position * position + 0.9375f);
		} else {
			position -= (2.625f / 2.75f);
			return (7.5625f * position * position + 0.984375f);
		}
	};
	/// <summary>
	/// Bounce easing, in-out direction.
	/// </summary>
	public static readonly EasingMethod BounceInOut = (position) => {
		return InOut(BounceIn, BounceOut, position);
	};

	/// <summary>
	/// Elastic easing, in direction.
	/// </summary>
	public static readonly EasingMethod ElasticIn = (position) => {
		return ElasticInternal(position, true, Easing.ElasticDefaultAmplitude, Easing.ElasticDefaultPeriod);
	};
	/// <summary>
	/// Elastic easing, out direction.
	/// </summary>
	public static readonly EasingMethod ElasticOut = (position) => {
		return ElasticInternal(position, false, Easing.ElasticDefaultAmplitude, Easing.ElasticDefaultPeriod);
	};
	/// <summary>
	/// Elastic easing, in-out direction.
	/// </summary>
	public static readonly EasingMethod ElasticInOut = (position) => {
		return InOut(ElasticIn, ElasticOut, position);
	};

	// -------- Easings with custom parameters --------

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
		return (position) => {
			return BackInImpl(position, swing, 0);
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
		return (position) => {
			return BackOutImpl(position, swing, 0);
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
		return (position) => {
			return InOut(BackInImpl, BackOutImpl, position, swing, 0);
		};
	}

	static float BackInImpl(float position, float swing, float arg2)
	{
		return (position * position * ((swing + 1f) * position - swing));
	}

	static float BackOutImpl(float position, float swing, float arg2)
	{
		position = position - 1f;
		return (position * position * ((swing + 1f) * position + swing) + 1f);
	}

	/// <summary>
	/// Default elastic amplitude (1 = full target value, &lt; 1 never reaches target).
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
	/// Amplitude of the elasticity (1 = full target value, &lt; 1 never reaches target).
	/// </param>
	/// <param name='period'>
	/// Period of the elasticity (1 / period = number of swings).
	/// </param>
	public static EasingMethod ElasticInCustom(float amplitude = ElasticDefaultAmplitude, float period = ElasticDefaultPeriod)
	{
		return delegate(float position) {
			return ElasticInImpl(position, amplitude, period);
		};
	}

	/// <summary>
	/// Elastic easing with custom amplitude and period, out direction.
	/// </summary>
	/// <param name='amplitude'>
	/// Amplitude of the elasticity (1 = full target value, &lt; 1 never reaches target).
	/// </param>
	/// <param name='period'>
	/// Period of the elasticity (1 / period = number of swings).
	/// </param>
	public static EasingMethod ElasticOutCustom(float amplitude = ElasticDefaultAmplitude, float period = ElasticDefaultPeriod)
	{
		return delegate(float position) {
			return ElasticOutImpl(position, amplitude, period);
		};
	}

	/// <summary>
	/// Elastic easing with custom amplitude and period, in-out direction.
	/// </summary>
	/// <param name='amplitude'>
	/// Amplitude of the elasticity (1 = full target value, &lt; 1 never reaches target).
	/// </param>
	/// <param name='period'>
	/// Period of the elasticity (1 / period = number of swings).
	/// </param>
	public static EasingMethod ElasticInOutCustom(float amplitude = ElasticDefaultAmplitude, float period = ElasticDefaultPeriod)
	{
		return delegate(float position) {
			return InOut(ElasticInImpl, ElasticOutImpl, position, amplitude, period);
		};
	}

	static float ElasticInImpl(float position, float amplitude, float period)
	{
		return ElasticInternal(position, true, amplitude, period);
	}

	static float ElasticOutImpl(float position, float amplitude, float period)
	{
		return ElasticInternal(position, false, amplitude, period);
	}

	static float ElasticInternal(float position, bool easingIn, float amplitude, float period)
	{
		if (position == 0f || position == 1f) {
			return position;
		}
		
		var s = 0f;
		if (amplitude < 1f) {
			s = period / 4f;
		} else {
			s = period / (2f * Mathf.PI) * Mathf.Asin(1f / amplitude);
		}
		if (easingIn) {
			position -= 1f;
			return -(amplitude * Mathf.Pow(2f, 10f * position)) 
					* Mathf.Sin((position - s) * (2f * Mathf.PI) / period);
		} else {
			return amplitude * Mathf.Pow(2f, -10f * position) 
					* Mathf.Sin((position - s) * (2f * Mathf.PI) / period) + 1f;
		}
	}

	// -------- Type mapping --------

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

	// -------- Helper Methods --------

	// Delegate for internal easing methods
	delegate float EasingMethodArgs(float position, float arg1, float arg2);

	// Helper that constructs InOut form In and Out
	static float InOut(EasingMethod In, EasingMethod Out, float position) {
		if (position <= 0.5f) {
			return In(position * 2f) / 2f;
		} else {
			return Out(2f * position - 1f) / 2f + 0.5f;
		}
	}

	static float InOut(
		EasingMethodArgs In, EasingMethodArgs Out, 
		float position, 
		float arg1, float arg2
	) {
		if (position <= 0.5f) {
			return In(position * 2f, arg1, arg2) / 2f;
		} else {
			return Out(2f * position - 1f, arg1, arg2) / 2f + 0.5f;
		}
	}
}

}
