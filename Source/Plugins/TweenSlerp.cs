using System;
using UnityEngine;

using Sttz.Tweener.Core;

namespace Sttz.Tweener.Plugins {

	/// <summary>
	/// Tween a rotation using the shortest path from start to end.
	/// </summary>
	/// <remarks>
	/// <para>
	/// TweenSlerp uses Spherical Linear Interpolation (Slerp) to tween
	/// a rotation from start to end using the shortest path possible.
	/// </para>
	/// <para>
	/// Eg. when not using TweenSlerp, tweening from 10° to 350° will take
	/// the long way around through 180°, whereas when using TweenSlerp, the
	/// tween will go the short way through 0°.
	/// </para>
	/// <code>
	/// // Explicit usage:
	/// Animate.To(transform, 2f, "rotation", Quaternion.identity, TweenSlerp.Use());
	/// 
	/// // Automatic usage, plugin auto-detects when it's needed:
	/// Animate.Options.SetAutomatic(TweenSlerp.Automatic());
	/// Animate.To(transform, 2f, "eulerAngles", Vector3.zero);
	/// </code>
	/// </remarks>
	public static class TweenSlerp
	{
		///////////////////
		// Plugin Use

		/// <summary>
		/// TweenSlerp plugin loader.
		/// </summary>
		/// <remarks>
		/// Use this method together with <see cref="ITweenEngine.AddDefaultPlugin"/> to 
		/// load this plugin for all tweens. For individual tweens, use the
		/// <see cref="TweenSlerp.PluginSlerp"/> extension methods instead.
		/// </remarks>
		public static bool Load(ITween tween, bool weak = true) 
		{
			if (tween == null) return false;

			if (tween.ValueType == typeof(Vector3)
					&& (!weak || tween.Property.EndsWith("EulerAngles", StringComparison.OrdinalIgnoreCase))) {
				tween.Internal.LoadPlugin(sharedVector3Plugin, weak: weak);
			} else if (tween.ValueType == typeof(Quaternion)) {
				tween.Internal.LoadPlugin(sharedQuaternionPlugin, weak: weak);
			} else {
				return false;
			}

			return true;
		}

		/// <summary>
		/// Use the TweenSlerp plugin on the current tween.
		/// </summary>
		public static Tween<TTarget, Quaternion> PluginSlerp<TTarget>(this Tween<TTarget, Quaternion> tween)
			where TTarget : class
		{
			if (!Load(tween, weak: false)) {
				tween.PluginError("TweenSlerp", "Plugin could not be loaded");
			}
			return tween;
		}

		/// <summary>
		/// Use the TweenSlerp plugin on the current tween.
		/// </summary>
		public static Tween<TTarget, Vector3> PluginSlerp<TTarget>(this Tween<TTarget, Vector3> tween)
			where TTarget : class
		{
			if (!Load(tween, weak: false)) {
				tween.PluginError("TweenSlerp", "Plugin could not be loaded");
			}
			return tween;
		}

		///////////////////
		// Generial

		static TweenSlerpImplVector3 sharedVector3Plugin = new TweenSlerpImplVector3();
		static TweenSlerpImplQuaternion sharedQuaternionPlugin = new TweenSlerpImplQuaternion();

		// Tween Slerp base implementation
		private class TweenSlerpImpl
		{
			// Initialize the plugin for the given tween,
			// returns null on success or an error message on failure.
			public string Initialize(ITween tween, TweenPluginType initForType, ref object userData)
			{
				// Check for correct value
				if (tween.ValueType != typeof(Vector3) 
						&& tween.ValueType != typeof(Quaternion)) {
					return string.Format(
						"Target property must either be a Vector3 or Quaternion, got {0}.",
						tween.ValueType);
				}

				return null;
			}
		}

		// Vector3 implementation
		private class TweenSlerpImplVector3 : TweenSlerpImpl, ITweenArithmeticPlugin<Vector3>
		{
			// Return the end value
			public Vector3 EndValue(Vector3 start, Vector3 diff, ref object userData)
			{
				return start + diff;
			}

			// Return the value at the current position
			public Vector3 ValueAtPosition(Vector3 start, Vector3 end, Vector3 diff, float position, ref object userData)
			{
				return Vector3.Slerp(start, end, position);
			}

			// Return the difference between start and end
			public Vector3 DiffValue(Vector3 start, Vector3 end, ref object userData)
			{
				// Slerp doesn't need diff, skip it...
				return default(Vector3);
			}
		}

		// Quaternion implementation
		private class TweenSlerpImplQuaternion : TweenSlerpImpl, ITweenArithmeticPlugin<Quaternion>
		{
			// Return the end value
			public Quaternion EndValue(Quaternion start, Quaternion diff, ref object userData)
			{
				return start * diff;
			}

			// Return the value at the current position
			public Quaternion ValueAtPosition(Quaternion start, Quaternion end, Quaternion diff, float position, ref object userData)
			{
				return Quaternion.Slerp(start, end, position);
			}
			// Return the difference between start and end
			public Quaternion DiffValue(Quaternion start, Quaternion end, ref object userData)
			{
				// Slerp doesn't need diff, skip it...
				return default(Quaternion);
			}
		}

	}
}

