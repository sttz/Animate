using System;
using UnityEngine;

using Sttz.Tweener.Core;

namespace Sttz.Tweener.Plugins
{

	public static class TweenFollow
	{
		///////////////////
		// Plugin Use

		public static bool Load(ITween tween, Transform targetSpace)
		{
			if (tween == null || targetSpace == null) return false;

			if (tween.ValueType != typeof(Vector3)) {
				tween.Internal.PluginError("TweenFollow", 
					"Tween target value needs to be Vector3, {0} on {1} is {2}.",
					tween.Property, tween.Target, tween.ValueType.Name
				);
				return false;
			}

			if (targetSpace == null) {
				tween.Internal.PluginError("TweenFollow", "targetSpace is null.");
				return false;
			}

			if (tween.TweenMethod != TweenMethod.To) {
				tween.Internal.PluginError("TweenFollow", "Tween needs to be a To tween.");
				return false;
			}

			tween.Internal.LoadPlugin(sharedPlugin, weak: false, userData: targetSpace);
			return true;
		}

		public static Tween<TTarget, TValue> PluginFollow<TTarget, TValue>(this Tween<TTarget, TValue> tween, Transform targetSpace)
			where TTarget : class
		{
			Load(tween, targetSpace);
			return tween;
		}

		///////////////////
		// Generial

		static TweenFollowImpl sharedPlugin = new TweenFollowImpl();

		// Vector3 implementation
		private class TweenFollowImpl : ITweenArithmeticPlugin<Vector3>
		{
			// Initialize the plugin for the given tween,
			// returns null on success or an error message on failure.
			public string Initialize(ITween tween, TweenPluginType initForType, ref object userData)
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

