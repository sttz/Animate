using System;
using UnityEngine;

using Sttz.Tweener.Core;

namespace Sttz.Tweener {

/// <summary>
/// Tween a rotation using the shortest path from start to end.
/// </summary>
/// <remarks>
/// TweenSlerp is enabled by default when tweening Quaternions, it's 
/// also used automatically when tweening a Vector3 where the property
/// ends in "eulerAngles".
/// 
/// Vector3 can represent positions as well as rotations but tweening
/// them does not work the same way. Positions can usually just be
/// linearly interpolated (lerp - a basic tween), whereas that leads
/// to unexpected behavior for rotations. E.g. tweening from
/// (0, 10, 0) to (0, 350, 0) will take the long way around, while
/// tweening (0, -20, 0) would be the shortest path.
/// 
/// Slerp (spherical lerp) solves this by always tweening the 
/// shortest possible path between two rotations.
/// 
/// ```cs
/// // Disable TweenSlerp
/// Animate.Options.EnablePlugin(TweenSlerp.Loader, false);
/// 
/// // Require TweenSlerp for a single tween
/// Animate.To(transform, 2f, "eulerAngles", new Vector3(0, 180, 0))
///     .Slerp();
/// ```
/// </remarks>
public static class TweenSlerp
{
	// -------- Plugin Use --------

	/// <summary>
	/// TweenSlerp plugin loader.
	/// </summary>
	/// <remarks>
	/// Pass this method to <see cref="TweenOptions.EnablePlugin"/> to enable the
	/// plugin for the options scope.
	/// </remarks>
	public static PluginResult Loader(Tween tween, bool required)
	{
		if (tween.ValueType == typeof(Vector3)) {
			if (required || tween.Property.EndsWith("EulerAngles", StringComparison.OrdinalIgnoreCase)) {
				return PluginResult.Load(sharedVector3Plugin);
			}
		} else if (tween.ValueType == typeof(Quaternion)) {
			return PluginResult.Load(sharedQuaternionPlugin);
		}

		return PluginResult.Error("TweenSlerp: Can only slerp Vector3 or Quaternion, got {0}.".LazyFormat(tween.ValueType));
	}

	/// <summary>
	/// Require the <see cref="TweenSlerp"/> plugin for the current tween.
	/// </summary>
	/// <remarks>
	/// Shorthand for using <see cref="TweenOptions.EnablePlugin"/> with type-checking.
	/// </remarks>
	public static Tween<TTarget, Quaternion> Slerp<TTarget>(this Tween<TTarget, Quaternion> tween)
		where TTarget : class
	{
		tween.Options.EnablePlugin(Loader, true, true);
		return tween;
	}

	/// <summary>
	/// Require the <see cref="TweenSlerp"/> plugin for the current tween.
	/// </summary>
	/// <remarks>
	/// Shorthand for using <see cref="TweenOptions.EnablePlugin"/> with type-checking.
	/// </remarks>
	public static Tween<TTarget, Vector3> Slerp<TTarget>(this Tween<TTarget, Vector3> tween)
		where TTarget : class
	{
		tween.Options.EnablePlugin(Loader, true, true);
		return tween;
	}

	// -------- Implementation --------

	static TweenSlerpImplVector3 sharedVector3Plugin = new TweenSlerpImplVector3();
	static TweenSlerpImplQuaternion sharedQuaternionPlugin = new TweenSlerpImplQuaternion();

	// Tween Slerp base implementation
	private abstract class TweenSlerpImpl
	{
		// Initialize the plugin for the given tween,
		// returns null on success or an error message on failure.
		public string Initialize(Tween tween, TweenPluginType initForType, ref object userData)
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

