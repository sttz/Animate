using System;
using UnityEngine;

using Sttz.Tweener.Core;

namespace Sttz.Tweener {

/// <summary>
/// Plugin to tween a global position to a moving transform's local position.
/// </summary>
/// <remarks>
/// This plugin can only be used with <see cref="CustomLoader"/> or the 
/// <see cref="Follow"/> extension method because it always requires an 
/// argument.
/// 
/// To use this plugin, tween a global position to a local position and
/// set the target transform as argument. The property will always be 
/// set to a global position and the local position transformed each frame.
/// 
/// ```cs
/// otherTransform.rotation = Quaternion.identity;
/// otherTransform.position = new Vector3(10, 0, 10);
/// 
/// // Require TweenFollow for a single tween
/// Animate.To(transform, 2f, "position", new Vector3(0, 10, 0))
/// 	.Follow(otherTransform);
/// 
/// // transform will end up at (10, 10, 10) at the end of the tween.
/// ```
/// </remarks>
public static class TweenFollow
{
	// -------- Plugin Use --------

	/// <summary>
	/// Create a custom TweenFollow plugin loader that uses the
	/// given target space.
	/// </summary>
	/// <remarks>
	/// Pass the result of this method to <see cref="TweenOptions.EnablePlugin"/> to enable the
	/// plugin for the options scope.
	/// </remarks>
	/// <param name="targetSpace">Target transform to follow</param>
	/// <returns>A custom plugin loader</returns>
	public static PluginLoader CustomLoader(Transform targetSpace)
	{
		return (tween, required) => {
			if (tween.ValueType != typeof(Vector3)) {
				return PluginResult.Error("TweenFollow: Tween value needs to be Vector3, got {0}.", tween.ValueType);
			}

			if (targetSpace == null) {
				return PluginResult.Error("TweenFollow: Target space is null.");
			}

			if (tween.TweenMethod != TweenMethod.To) {
				return PluginResult.Error("TweenFollow: Tween needs to be a To tween.");
			}

			return PluginResult.Load(sharedPlugin, userData: targetSpace);
		};
	}

	/// <summary>
	/// Require the <see cref="TweenFollow"/> plugin for the current tween.
	/// </summary>
	/// <remarks>
	/// Shorthand for using <see cref="TweenOptions.EnablePlugin"/> with type-checking.
	/// </remarks>
	/// <param name="targetSpace">Target transform to follow</param>
	public static Tween<TTarget, Vector3> Follow<TTarget>(this Tween<TTarget, Vector3> tween, Transform targetSpace)
		where TTarget : class
	{
		tween.Options.EnablePlugin(CustomLoader(targetSpace), true, true);
		return tween;
	}

	// -------- Implementation --------

	static TweenFollowImpl sharedPlugin = new TweenFollowImpl();

	// Vector3 implementation
	private class TweenFollowImpl : ITweenArithmeticPlugin<Vector3>
	{
		// Initialize the plugin for the given tween,
		// returns null on success or an error message on failure.
		public string Initialize(Tween tween, TweenPluginType initForType, ref object userData)
		{
			return null;
		}

		// Return the value at the current position
		public Vector3 ValueAtPosition(Vector3 start, Vector3 end, Vector3 diff, float position, ref object userData)
		{
			var targetSpace = (Transform)userData;
			var endGlobal = targetSpace.TransformPoint(end);
			return Vector3.Lerp(start, endGlobal, position);
		}
		
		// Return the end value
		public Vector3 EndValue(Vector3 start, Vector3 diff, ref object userData)
		{
			// Not used
			return Vector3.zero;
		}

		// Return the difference between start and end
		public Vector3 DiffValue(Vector3 start, Vector3 end, ref object userData)
		{
			// Not used
			return Vector3.zero;
		}
	}
}

}
