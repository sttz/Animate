using System;
using UnityEngine;
using UnityEngine.UI;

namespace Sttz.Tweener.Core.Static
{
	/// <summary>
	/// Plugin providing static accessor and arithmetic plugins for
	/// use cases in Unity.
	/// </summary>
	public static class TweenStaticUnityPlugin
	{
		static bool loaded;

		public static void Load()
		{
			if (loaded) return;
			loaded = true;

			// ------ Accessors -------

			// Transform
			TweenStaticAccessorPlugin.Teach("rotation",
				(Transform tf) => tf.rotation,
				(tf, value) => tf.rotation = value);
			TweenStaticAccessorPlugin.Teach("localRotation",
				(Transform tf) => tf.localRotation,
				(tf, value) => tf.localRotation = value);
			TweenStaticAccessorPlugin.Teach("eulerAngles",
				(Transform tf) => tf.eulerAngles,
				(tf, value) => tf.eulerAngles = value);
			TweenStaticAccessorPlugin.Teach("localEulerAngles",
				(Transform tf) => tf.localEulerAngles,
				(tf, value) => tf.localEulerAngles = value);
			TweenStaticAccessorPlugin.Teach("localScale",
				(Transform tf) => tf.localScale,
				(tf, value) => tf.localScale = value);
			TweenStaticAccessorPlugin.Teach("position",
				(Transform tf) => tf.position,
				(tf, value) => tf.position = value);
			TweenStaticAccessorPlugin.Teach("localPosition",
				(Transform tf) => tf.localPosition,
				(tf, value) => tf.localPosition = value);

			TweenStaticAccessorPlugin.Teach("anchoredPosition",
				(RectTransform tf) => tf.anchoredPosition,
				(tf, value) => tf.anchoredPosition = value);

			TweenStaticAccessorPlugin.Teach("color",
			    (Graphic t) => t.color,
				(t, value) => t.color = value);
			TweenStaticAccessorPlugin.Teach("color",
				(Material t) => t.color,
				(t, value) => t.color = value);

			TweenStaticAccessorPlugin.Teach("weight",
				(AnimationState tf) => tf.weight,
				(tf, value) => tf.weight = value);
			TweenStaticAccessorPlugin.Teach("alpha",
				(CanvasGroup tf) => tf.alpha,
				(tf, value) => tf.alpha = value);

			// ------ Arithmetic -------

			TweenStaticArithmeticPlugin.RegisterSupport(typeof(Vector2), new TweenStaticArithmeticPluginVector2());
			TweenStaticArithmeticPlugin.RegisterSupport(typeof(Vector3), new TweenStaticArithmeticPluginVector3());
			TweenStaticArithmeticPlugin.RegisterSupport(typeof(Vector4), new TweenStaticArithmeticPluginVector4());
			TweenStaticArithmeticPlugin.RegisterSupport(typeof(Color), new TweenStaticArithmeticPluginColor());
		}

		/// <summary>
		/// Specialized implementation of arithmetic plugin for Vector2.
		/// </summary>
		public class TweenStaticArithmeticPluginVector2 : ITweenArithmeticPlugin<Vector2>
		{
			// Initialize
			public string Initialize(ITween tween, TweenPluginType initForType, ref object userData)
			{
				return null;
			}

			///////////////////
			// Calculate Value Hook

			// Return the difference between start and end
			public Vector2 DiffValue(Vector2 start, Vector2 end, ref object userData)
			{
				return end - start;
			}

			// Return the end value
			public Vector2 EndValue(Vector2 start, Vector2 diff, ref object userData)
			{
				return start + diff;
			}

			// Return the value at the current position
			public Vector2 ValueAtPosition(Vector2 start, Vector2 end, Vector2 diff, float position, ref object userData)
			{
				return start + diff * position;
			}
		}

		/// <summary>
		/// Specialized implementation of arithmetic plugin for Vector3.
		/// </summary>
		public class TweenStaticArithmeticPluginVector3 : ITweenArithmeticPlugin<Vector3>
		{
			// Initialize
			public string Initialize(ITween tween, TweenPluginType initForType, ref object userData)
			{
				return null;
			}

			///////////////////
			// Calculate Value Hook

			// Return the difference between start and end
			public Vector3 DiffValue(Vector3 start, Vector3 end, ref object userData)
			{
				return end - start;
			}

			// Return the end value
			public Vector3 EndValue(Vector3 start, Vector3 diff, ref object userData)
			{
				return start + diff;
			}

			// Return the value at the current position
			public Vector3 ValueAtPosition(Vector3 start, Vector3 end, Vector3 diff, float position, ref object userData)
			{
				return start + diff * position;
			}
		}

		/// <summary>
		/// Specialized implementation of arithmetic plugin for Vector4.
		/// </summary>
		public class TweenStaticArithmeticPluginVector4 : ITweenArithmeticPlugin<Vector4>
		{
			// Initialize
			public string Initialize(ITween tween, TweenPluginType initForType, ref object userData)
			{
				return null;
			}

			///////////////////
			// Calculate Value Hook

			// Return the difference between start and end
			public Vector4 DiffValue(Vector4 start, Vector4 end, ref object userData)
			{
				return end - start;
			}

			// Return the end value
			public Vector4 EndValue(Vector4 start, Vector4 diff, ref object userData)
			{
				return start + diff;
			}

			// Return the value at the current position
			public Vector4 ValueAtPosition(Vector4 start, Vector4 end, Vector4 diff, float position, ref object userData)
			{
				return start + diff * position;
			}
		}

		/// <summary>
		/// Specialized implementation of arithmetic plugin for Color.
		/// </summary>
		public class TweenStaticArithmeticPluginColor : ITweenArithmeticPlugin<Color>
		{
			// Initialize
			public string Initialize(ITween tween, TweenPluginType initForType, ref object userData)
			{
				return null;
			}

			///////////////////
			// Calculate Value Hook

			// Return the difference between start and end
			public Color DiffValue(Color start, Color end, ref object userData)
			{
				return end - start;
			}

			// Return the end value
			public Color EndValue(Color start, Color diff, ref object userData)
			{
				return start * diff;
			}

			// Return the value at the current position
			public Color ValueAtPosition(Color start, Color end, Color diff, float position, ref object userData)
			{
				return start + diff * position;
			}
		}
	}

}
