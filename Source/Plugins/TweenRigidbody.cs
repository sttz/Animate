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

		/// <summary>
		/// TweenPluginInfo that can be used for automatic activation.
		/// </summary>
		/// <seealso cref="ITweenOptions.SetAutomatic"/>
		/// <seealso cref="ITweenOptionsFluid<TContainer>.Automate"/>
		public static TweenPluginInfo Automatic()
		{
			return DefaultInfo;
		}

		/// <summary>
		/// Use the TweenRigidbody plugin for the current tween.
		/// </summary>
		public static TweenPluginInfo Use()
		{
			return DefaultInfo;
		}

		///////////////////
		// Activation

		// Default plugin info
		private static TweenPluginInfo DefaultInfo = new TweenPluginInfo() {
			// Generic plugin type
			pluginType = typeof(TweenRigidbody),
			// Plugin needs to set the value
			hooks = TweenPluginHook.SetValue,
			// Choose proper type for manual activation
			manualActivation = ManualActivation,
			// Enable automatic activation
			autoActivation = ShouldActivate
		};

		// Callback for manual activation
		private static TweenPluginInfo ManualActivation(ITween tween, TweenPluginInfo info)
		{
			info = ShouldActivate(tween, info);

			if (info.pluginType == null) {
				tween.Internal.Log(TweenLogLevel.Error,
					"TweenRigidbody: Tween property can only be 'position', "
					+ "'rotation' or 'eulerAngles' on a Transform with a "
					+ "kinematic Rigidbody, got {0} on {1}.",
					tween.Property, tween.Target);
			}

			return info;
		}

		// Returns if the plugin should activate automatically
		private static TweenPluginInfo ShouldActivate(ITween tween, TweenPluginInfo info)
		{
			// Check if target is Transform and has a kinematic rigidbody
			if (!(tween.Target is Transform)
					|| (tween.Target as Transform).rigidbody == null
					|| !(tween.Target as Transform).rigidbody.isKinematic) {
				return TweenPluginInfo.None;
			}

			// Supported properties on Transform
			if (tween.Property == "position" 
					|| tween.Property == "eulerAngles") {
				info.pluginType = typeof(TweenRigidbodyImplVector3);
			} else if (tween.Property == "rotation") {
				info.pluginType = typeof(TweenRigidbodyImplQuaternion);
			} else {
				info.pluginType = null;
			}

			info.setValueUserData = (tween.Target as Transform).rigidbody;
			return info;
		}

		///////////////////
		// Implementation

		// Open generic implementation
		private class TweenRigidbodyImpl<TValue> : TweenPlugin<TValue>
		{
			// Initialize
			public override string Initialize(ITween tween, TweenPluginHook hook, ref object userData)
			{
				// Check if target is Transform and has a rigidbody
				if (!(tween.Target is Transform)
						|| (tween.Target as Transform).rigidbody == null
						|| !(tween.Target as Transform).rigidbody.isKinematic) {
					return string.Format(
						"Target {0} must be a Transform and have a kinematic Rigidbody attached.",
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
				userData = (tween.Target as Transform).rigidbody;

				// All ok!
				return null;
			}
		}

		// Vector3 implementation
		private class TweenRigidbodyImplVector3 : TweenRigidbodyImpl<Vector3>
		{
			// Write value to material
			public override void SetValue(object target, string property, Vector3 value, ref object userData)
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
		private class TweenRigidbodyImplQuaternion : TweenRigidbodyImpl<Quaternion>
		{
			// Write value to material
			public override void SetValue(object target, string property, Quaternion value, ref object userData)
			{
				(userData as Rigidbody).MoveRotation(value);
			}
		}
	}

}
