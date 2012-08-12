using System;
using UnityEngine;

namespace Sttz.Tweener.Core {

	/// <summary>
	/// Implementation of easing methods.
	/// </summary>
	internal static class EasingImpl
	{
		///////////////////
		// Linear

		public static float LinearImpl(float position)
		{
			return position;
		}

		///////////////////
		// Quadratic

		public static float QuadraticInImpl(float position)
		{
			return (position * position);
		}

		public static float QuadraticOutImpl(float position)
		{
			return (position * (position - 2f) * -1f);
		}

		public static float QuadraticInOutImpl(float position)
		{
			return InOut(QuadraticInImpl, QuadraticOutImpl, position);
		}

		///////////////////
		// Cubic

		public static float CubicInImpl(float position)
		{
			return (position * position * position);
		}

		public static float CubicOutImpl(float position)
		{
			return (Mathf.Pow(position - 1f, 3f) + 1f);
		}

		public static float CubicInOutImpl(float position)
		{
			return InOut(CubicInImpl, CubicOutImpl, position);
		}

		///////////////////
		// Quartic

		public static float QuarticInImpl(float position)
		{
			return (position * position * position * position);
		}

		public static float QuarticOutImpl(float position)
		{
			return (1f - Mathf.Pow(position - 1f, 4f));
		}

		public static float QuarticInOutImpl(float position)
		{
			return InOut(QuarticInImpl, QuarticOutImpl, position);
		}

		///////////////////
		// Quintic

		public static float QuinticInImpl(float position)
		{
			return (position * position * position * position * position);
		}

		public static float QuinticOutImpl(float position)
		{
			return (Mathf.Pow(position - 1f, 5f) + 1f);
		}

		public static float QuinticInOutImpl(float position)
		{
			return InOut(QuinticInImpl, QuinticOutImpl, position);
		}

		///////////////////
		// Sinusoidal

		public static float SinusoidalInImpl(float position)
		{
			return (Mathf.Sin((position - 1f) * (Mathf.PI / 2f)) + 1f);
		}

		public static float SinusoidalOutImpl(float position)
		{
			return Mathf.Sin(position * (Mathf.PI / 2f));
		}

		public static float SinusoidalInOutImpl(float position)
		{
			return InOut(SinusoidalInImpl, SinusoidalOutImpl, position);
		}

		///////////////////
		// Exponential

		public static float ExponentialInImpl(float position)
		{
			return Mathf.Pow(2f, 10f * (position - 1f));
		}

		public static float ExponentialOutImpl(float position)
		{
			return (-1f * Mathf.Pow(2f, -10f * position) + 1f);
		}

		public static float ExponentialInOutImpl(float position)
		{
			return InOut(ExponentialInImpl, ExponentialOutImpl, position);
		}

		///////////////////
		// Circular

		public static float CircularInImpl(float position)
		{
			return (-1f * Mathf.Sqrt(1f - position * position) + 1f);
		}

		public static float CircularOutImpl(float position)
		{
			return Mathf.Sqrt(1f - Mathf.Pow(position - 1f, 2f));
		}

		public static float CircularInOutImpl(float position)
		{
			return InOut(CircularInImpl, CircularOutImpl, position);
		}

		///////////////////
		// Back

		public static float BackInImpl(float position)
		{
			return (position * position * ((Easing.BackDefaultSwing + 1f) * position - Easing.BackDefaultSwing));
		}

		public static float BackOutImpl(float position)
		{
			position = position - 1f;
			return (position * position * ((Easing.BackDefaultSwing + 1f) * position + Easing.BackDefaultSwing) + 1f);
		}

		public static float BackInOutImpl(float position)
		{
			return InOut(BackInImpl, BackOutImpl, position);
		}

		public static float BackInImpl(float position, float swing, float arg2)
		{
			return (position * position * ((swing + 1f) * position - swing));
		}

		public static float BackOutImpl(float position, float swing, float arg2)
		{
			position = position - 1f;
			return (position * position * ((swing + 1f) * position + swing) + 1f);
		}

		public static float BackInOutImpl(float position, float swing, float arg2)
		{
			return InOut(BackInImpl, BackOutImpl, position, swing, arg2);
		}

		///////////////////
		// Bounce

		public static float BounceInImpl(float position)
		{
			return (1f - BounceOutImpl(1f - position));
		}

		public static float BounceOutImpl(float position)
		{
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
		}

		public static float BounceInOutImpl(float position)
		{
			return InOut(BounceInImpl, BounceOutImpl, position);
		}

		///////////////////
		// Elastic

		public static float ElasticInternal(float position, bool easingIn, float amplitude, float period)
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

		public static float ElasticInImpl(float position)
		{
			return ElasticInternal(position, true, Easing.ElasticDefaultAmplitude, Easing.ElasticDefaultPeriod);
		}

		public static float ElasticOutImpl(float position)
		{
			return ElasticInternal(position, false, Easing.ElasticDefaultAmplitude, Easing.ElasticDefaultPeriod);
		}

		public static float ElasticInOutImpl(float position)
		{
			return InOut(ElasticInImpl, ElasticOutImpl, position);
		}

		public static float ElasticInImpl(float position, float amplitude, float period)
		{
			return ElasticInternal(position, true, amplitude, period);
		}

		public static float ElasticOutImpl(float position, float amplitude, float period)
		{
			return ElasticInternal(position, false, amplitude, period);
		}

		public static float ElasticInOutImpl(float position, float amplitude, float period)
		{
			return InOut(ElasticInImpl, ElasticOutImpl, position, amplitude, period);
		}

		///////////////////
		// Helper Methods

		// Delegate for internal easing methods
		public delegate float EasingMethodArgs(float position, float arg1, float arg2);

		// Helper that constructs InOut form In and Out
		public static float InOut(EasingMethod In, EasingMethod Out, float position) {
			if (position <= 0.5f) {
				return In(position * 2f) / 2f;
			} else {
				return Out(2f * position - 1f) / 2f + 0.5f;
			}
		}

		public static float InOut(
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