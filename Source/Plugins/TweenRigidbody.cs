using System;
using System.Linq;
using UnityEngine;

using Sttz.Tweener.Core;

namespace Sttz.Tweener {

/// <summary>
/// Tween a kinematic Rigidbody's position or rotation with properly
/// applying physics.
/// </summary>
/// <remarks>
/// To make a tweened kinematic Rigidbody interact properly with
/// other physics objects, there are two requirements:
/// - Use `Rigidbody.MovePosition` and `Rigidbody.MoveRotation` to
///   update the rigidbody's position (instead of `Transform`).
/// - Update the rigidbody in `FixedUpdate` (instead of `Update`).
/// 
/// TweenRigidbody ensures both. It updates the Rigidbody with the
/// proper methods and ensures the tween is set to update in
/// `FixedUpdate`.
/// 
/// TweenRigidbody requires tweening 'position', 'rotation' or 
/// 'eulerAngles' on the transform the kinematic rigidbody is 
/// attached to. It's not possible to TweenRigidbody with local
/// position or rotation.
/// 
/// ```cs
/// // Enable TweenRigidbody globally
/// Animate.Options.EnablePlugin(TweenRigidbody.Loader);
/// 
/// // Enable TweenRigidbody for a specific tween
/// Animate.To(transform, 5f, "position", Vector3(0, 0, 50))
/// 	.Rigidbody();
/// ```
/// </remarks>
public static class TweenRigidbody
{
	// -------- Plugin Use --------

	/// <summary>
	/// TweenRigidbody plugin loader.
	/// </summary>
	/// <remarks>
	/// Pass this method to <see cref="TweenOptions.EnablePlugin"/> to enable the
	/// plugin for the options scope.
	/// </remarks>
	public static PluginResult Loader(Tween tween, bool required)
	{
		// Check if target is Transform and has a kinematic rigidbody
		var targetTf = tween.Target as Transform;
		if (targetTf == null) {
			return PluginResult.Error("TweenRigidbody: Target needs to be a transform, got {0}.", tween.Target);
		}

		var targetRb = targetTf.GetComponent<Rigidbody>();
		if (targetRb == null || !targetRb.isKinematic) {
			return PluginResult.Error("TweenRigidbody: Target needs to have a kinematic rigidbody attached.");
		}

		// Supported properties on Transform
		ITweenPlugin instance;
		if (tween.Property == "position"
				|| tween.Property == "eulerAngles") {
			instance = TweenRigidbodyImplVector3.sharedInstance;
		} else if (tween.Property == "rotation") {
			instance = TweenRigidbodyImplQuaternion.sharedInstance;
		} else {
			return PluginResult.Error(
				"TweenRigidbody: Can only used with properties "
				+ "'position', 'rotation' or 'eulerAngles', got {0}.",
				tween.Property
			);
		}

		// Make sure the tween is set to FixedUpdate
		tween.Options.TweenTiming &= ~(TweenTiming.Update | TweenTiming.LateUpdate);
		tween.Options.TweenTiming |= TweenTiming.FixedUpdate;

		return PluginResult.Load(instance);
	}

	/// <summary>
	/// Require the <see cref="TweenRigidbody"/> plugin for the current tween.
	/// </summary>
	/// <remarks>
	/// Shorthand for using <see cref="TweenOptions.EnablePlugin"/> with type-checking.
	/// </remarks>
	public static Tween<Transform, Vector3> Rigidbody(this Tween<Transform, Vector3> tween)
	{
		tween.Options.EnablePlugin(Loader, true, true);
		return tween;
	}

	/// <summary>
	/// Require the <see cref="TweenRigidbody"/> plugin for the current tween.
	/// </summary>
	/// <remarks>
	/// Shorthand for using <see cref="TweenOptions.EnablePlugin"/> with type-checking.
	/// </remarks>
	public static Tween<Transform, Quaternion> Rigidbody(this Tween<Transform, Quaternion> tween)
	{
		tween.Options.EnablePlugin(Loader, true, true);
		return tween;
	}

	// -------- Implementation --------

	// Open generic implementation
	private class TweenRigidbodyImpl
	{
		// Initialize
		public string Initialize(Tween tween, TweenPluginType initForType, ref object userData)
		{
			// Check if target is Transform and has a rigidbody
			var targetTf = tween.Target as Transform;
			if (targetTf == null) {
				return string.Format(
					"Target {0} must be a Transform with a kinematic Rigidbody attached.",
					tween.Target);
			}

			var targetRb = targetTf.GetComponent<Rigidbody>();
			if (targetRb == null || !targetRb.isKinematic) {
				return string.Format(
					"Target transform {0} must have a kinematic Rigidbody attached.",
					tween.Target);
			}

			// Supported properties on Transform
			if (tween.Property != "position" 
					&& tween.Property != "rotation"
					&& tween.Property != "eulerAngles") {
				return string.Format(
					"Only 'position', 'rotation' or 'eulerAngles' are supported, got {0}.",
					tween.Property);
			}

			// Set rigidbody as user data
			userData = targetRb;

			// All ok!
			return null;
		}
	}

	// Vector3 implementation
	private class TweenRigidbodyImplVector3 : TweenRigidbodyImpl,
		ITweenSetterPlugin<Transform, Vector3>
	{
		internal static TweenRigidbodyImplVector3 sharedInstance
			= new TweenRigidbodyImplVector3();

		public void SetValue(Transform target, string property, Vector3 value, ref object userData)
		{
			var rigidbody = (userData as Rigidbody);
			if (property == "position") {
				rigidbody.MovePosition(value);
			} else if (property == "eulerAngles") {
				rigidbody.MoveRotation(Quaternion.Euler(value));
			}
		}
	}

	// Quaternion implementation
	private class TweenRigidbodyImplQuaternion : TweenRigidbodyImpl,
		ITweenSetterPlugin<Transform, Quaternion>
	{
		internal static TweenRigidbodyImplQuaternion sharedInstance
			= new TweenRigidbodyImplQuaternion();

		public void SetValue(Transform target, string property, Quaternion value, ref object userData)
		{
			(userData as Rigidbody).MoveRotation(value);
		}
	}
}

}
