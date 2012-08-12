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
		/// TweenPluginInfo that can be used for automatic activation.
		/// </summary>
		/// <seealso cref="ITweenOptions.SetAutomatic"/>
		/// <seealso cref="ITweenOptionsFluid<TContainer>.Automate"/>
		public static TweenPluginInfo Automatic()
		{
			return DefaultInfo;
		}

		/// <summary>
		/// Use the TweenSlerp plugin for the current tween.
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
			pluginType = typeof(TweenSlerpImpl<>),
			// Plugin needs to calculate the value
			hooks = TweenPluginHook.CalculateValue,
			// Delegate to select proper plugin type for manual mode
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
					"TweenSlerp: Tween value needs to be either Vector3 or Quaternion,"
					+ " got {0} for {1} on {2}",
					tween.ValueType, tween.Property, tween.Target);
			}

			return info;
		}

		// Returns if the plugin should activate automatically
		private static TweenPluginInfo ShouldActivate(ITween tween, TweenPluginInfo info)
		{
			if (tween.ValueType == typeof(Vector3)) {
				info.pluginType = typeof(TweenSlerpImplVector3);
			} else if (tween.ValueType == typeof(Quaternion)) {
				info.pluginType =  typeof(TweenSlerpImplQuaternion);
			} else {
				info.pluginType = null;
			}

			return info;
		}

		///////////////////
		// Generial

		// Tween Slerp base implementation
		private class TweenSlerpImpl<TValue> : TweenPlugin<TValue>
		{
			// Initialize the plugin for the given tween,
			// returns null on success or an error message on failure.
			public override string Initialize(ITween tween, TweenPluginHook hook, ref object userData)
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

			// Return the difference between start and end
			public override TValue DiffValue(TValue start, TValue end, ref object userData)
			{
				// Slerp doesn't need diff, skip it...
				return default(TValue);
			}
		}

		// Vector3 implementation
		private class TweenSlerpImplVector3 : TweenSlerpImpl<Vector3>
		{
			// Return the end value
			public override Vector3 EndValue(Vector3 start, Vector3 diff, ref object userData)
			{
				return start + diff;
			}

			// Return the value at the current position
			public override Vector3 ValueAtPosition(Vector3 start, Vector3 end, Vector3 diff, float position, ref object userData)
			{
				return Vector3.Slerp(start, end, position);
			}
		}

		// Quaternion implementation
		private class TweenSlerpImplQuaternion : TweenSlerpImpl<Quaternion>
		{
			// Return the end value
			public override Quaternion EndValue(Quaternion start, Quaternion diff, ref object userData)
			{
				return start * diff;
			}

			// Return the value at the current position
			public override Quaternion ValueAtPosition(Quaternion start, Quaternion end, Quaternion diff, float position, ref object userData)
			{
				return Quaternion.Slerp(start, end, position);
			}
		}

	}
}

