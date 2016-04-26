using System;
using System.Linq;
using UnityEngine;

using Sttz.Tweener.Core;

namespace Sttz.Tweener.Plugins {

	/// <summary>
	/// Tween a kinematic rigidbody using its <c>MovePosition</c> and 
	/// <c>MoveRotation</c> methods.
	/// </summary>
	/// <remarks>
	/// <para>
	/// When tweening kinematic rigidbodies it's recommended to use the 
	/// rigidbody's <c>MovePosition</c> and <c>MoveRotation</c> methods
	/// to properly apply friction. This is important if you e.g. want to
	/// tween a moving elevator platform.
	/// </para>
	/// <para>
	/// To use this plugin, tween the rigidbody's transform's <c>position</c>,
	/// <c>rotation</c> or <c>eulerAngles</c> property. You should also set
	/// the <see cref="TweenTiming"/> to <c>Physics</c> to update the tween
	/// during <c>FixedUpdate</c>.
	/// </para>
	/// <code>
	/// // Explicit usage:
	/// Animate.To(transform, 2f, "position", Vector3.one, TweenRigidbody.Use())
	///     .Timing(TweenTiming.Physics);
	/// 
	/// // Automatic usage, plugin auto-detects when it's needed:
	/// Animate.Options.SetAutomatic(TweenRigidbody.Automatic());
	/// Animate.To(transform, 2f, "position", Vector3.one)
	///     .Timing(TweenTiming.Physics);
	/// </code>
	/// </remarks>
	public static class TweenRigidbody
	{
		///////////////////
		// Plugin Use

		public static bool Load<TTarget, TValue>(Tween<TTarget, TValue> tween, bool automatic = false)
			where TTarget : class
		{
			// Check if target is Transform and has a kinematic rigidbody
			var targetTf = tween.Target as Transform;
			if (targetTf == null) {
				return false;
			}

			var targetRb = targetTf.GetComponent<Rigidbody>();
			if (targetRb == null || !targetRb.isKinematic) {
				return false;
			}

			// Supported properties on Transform
			ITweenPlugin instance;
			if (tween.Property == "position"
					|| tween.Property == "eulerAngles") {
				instance = TweenRigidbodyImplVector3<TTarget>.sharedInstance;
			} else if (tween.Property == "rotation") {
				instance = TweenRigidbodyImplQuaternion<TTarget>.sharedInstance;
			} else {
				return false;
			}

			// Set plugin type to use
			tween.LoadPlugin(instance, weak: automatic);
			return true;
		}

		public static Tween<TTarget, TValue> PluginRigidbody<TTarget, TValue>(this Tween<TTarget, TValue> tween)
			where TTarget : class
		{
			if (!Load(tween)) {
				tween.PluginError("TweenMaterial",
					"TweenRigidbody: Tween property can only be 'position', "
					+ "'rotation' or 'eulerAngles' on a Transform with a "
					+ "kinematic Rigidbody, got {0} on {1}.",
					tween.Property, tween.Target
				);
			}
			return tween;
		}

		///////////////////
		// Implementation

		// Open generic implementation
		private class TweenRigidbodyImpl
		{
			// Initialize
			public string Initialize(ITween tween, TweenPluginType initForType, ref object userData)
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
		private class TweenRigidbodyImplVector3<TTarget> : TweenRigidbodyImpl,
			ITweenSetterPlugin<TTarget, Vector3>
			where TTarget : class
		{
			internal static TweenRigidbodyImplVector3<TTarget> sharedInstance
				= new TweenRigidbodyImplVector3<TTarget>();

			public void SetValue(TTarget target, string property, Vector3 value, ref object userData)
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
		private class TweenRigidbodyImplQuaternion<TTarget> : TweenRigidbodyImpl,
			ITweenSetterPlugin<TTarget, Quaternion>
			where TTarget : class
		{
			internal static TweenRigidbodyImplQuaternion<TTarget> sharedInstance
				= new TweenRigidbodyImplQuaternion<TTarget>();

			public void SetValue(TTarget target, string property, Quaternion value, ref object userData)
			{
				(userData as Rigidbody).MoveRotation(value);
			}
		}
	}

}
