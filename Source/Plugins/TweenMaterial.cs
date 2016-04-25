using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Sttz.Tweener.Core;
namespace Sttz.Tweener.Plugins {

	/// <summary>
	/// Animate plugin to tween material properties.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Only the main material properties are available as actual properties
	/// on the <c>Material</c> class. Secondary properties have to be accessed
	/// using the <c>Get*</c> and <c>Set*</c> methods.
	/// </para>
	/// <para>
	/// The TweenMaterial plugin makes tweening those properties easy. There's
	/// an explicit and an automatic way to use the plugin:
	/// </para>
	/// <code>
	/// // Explicit usage, material property type is inferred from tween type:
	/// Animate.To(material, 2f, "_SpecColor", Color.red, TweenMaterial.Use());
	/// 
	/// // Automatic usage, plugin auto-detects when it's needed:
	/// Animate.Options.SetAutomatic(TweenMaterial.Automatic());
	/// Animate.To(material, 2f, "_SpecColor", Color.red);
	/// </code>
	/// <para>
	/// Make sure the type of the tween matches the type of the property or
	/// set the property type explicitly using the <see cref="TweenMaterial.Use"/>
	/// method.
	/// </para>
	/// </remarks>
	public static class TweenMaterial
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
		/// Use the TweenMaterial plugin for the current tween.
		/// </summary>
		/// <param name='type'>
		/// Optionally specify the type of the property to override auto-detection.
		/// </param>
		public static TweenPluginInfo Use(PropertyType type = PropertyType.Undefined)
		{
			var info = DefaultInfo;

			// Forced property type
			if (type != PropertyType.Undefined) {
				// Insert proper implementation type
				info.pluginType = typeToPluginType[type];
				// Pass on type as user data
				info.getValueUserData = type;
				info.setValueUserData = type;
				// Disable auto-activation since this is a typed sub-class
				info.autoActivation = null;
			}

			return info;
		}

		///////////////////
		// Activation

		// Default plugin info
		private static TweenPluginInfo DefaultInfo = new TweenPluginInfo() {
			// Generic plugin type
			pluginType = typeof(TweenMaterialImpl),
			// Plugin needs to set and get the value
			hooks = TweenPluginType.Getter | TweenPluginType.Setter,
			// Enable automatic activation
			autoActivation = ShouldActivate,
			// Manual activation
			manualActivation = ManualActivation
		};

		// Callback for manual activation
		private static TweenPluginInfo ManualActivation(ITween tween, TweenPluginInfo info)
		{
			// Auto-detect property type
			if (info.pluginType == typeof(TweenMaterialImpl)) {
				info = ShouldActivate(tween, info);
			}

			// Print error if activation failed
			if (info.pluginType == null) {
				tween.Internal.Log(TweenLogLevel.Error,
					"Could not activate tween material plugin for tween of {0} on {1}.",
					tween.Property, tween.Target);
			}

			return info;
		}

		// Returns if the plugin should activate automatically
		private static TweenPluginInfo ShouldActivate(ITween tween, TweenPluginInfo info)
		{
			// Check if target is Material
			if (!(tween.Target is Material)) {
				return TweenPluginInfo.None;
			}

			// Get option, removes it from the property for chek below
			var option = TweenPluginInfo.GetOption(tween);

			// Only activate for properties starting with an underscore
			if (tween.Property[0] != '_') {
				return TweenPluginInfo.None;
			}

			// Since we cannot get the type of a material property
			// from unity, we have to get it as parameter or
			// assume it from the tween type
			PropertyType type = PropertyType.Color;

			// Validate and parse option
			if (option != null) {
				if (!validOptions.Contains(option, StringComparer.OrdinalIgnoreCase)) {
					tween.Internal.Log(TweenLogLevel.Warning,
						"TweenMaterial: Invalid property type option '{0}'.", option);
					return TweenPluginInfo.None;
				}
				type = (PropertyType)Enum.Parse(typeof(PropertyType), option, true);
			
			// Assume property type based on input type
			} else {
				if (tween.ValueType == typeof(Color)) {
					type = PropertyType.Color;
				} else if (tween.ValueType == typeof(Vector4)) {
					type = PropertyType.Vector;
				} else if (tween.ValueType == typeof(Vector2)) {
					type = PropertyType.TextureOffset;
				} else if (tween.ValueType == typeof(float)) {
					type = PropertyType.Float;
				}
			}

			// Check tween value type
			if (typeToValueType[type] != tween.ValueType) {
				tween.Internal.Log(TweenLogLevel.Warning,
					"TweenMaterial: Invalid value type '{0}' for property type {1}.",
					tween.ValueType, type);
				return TweenPluginInfo.None;
			}

			// Set plugin type to use
			info.pluginType = typeToPluginType[type];
			info.getValueUserData = info.setValueUserData = type;
			return info;
		}

		///////////////////
		// Plugin Data

		// Material property types
		public enum PropertyType
		{
			Undefined,
			Color,
			Vector,
			TextureOffset,
			TextureScale,
			Float
		}

		// Possible plugin optoins
		private static string[] validOptions = Enum.GetNames(typeof(PropertyType));

		// Mapping of property type to value type
		private static Dictionary<PropertyType, Type> typeToValueType
			= new Dictionary<PropertyType, Type>() {
			{ PropertyType.Color, typeof(Color) },
			{ PropertyType.Vector, typeof(Vector4) },
			{ PropertyType.TextureOffset, typeof(Vector2) },
			{ PropertyType.TextureScale, typeof(Vector2) },
			{ PropertyType.Float, typeof(float) }
		};

		// Mapping of property type to plugin type
		private static Dictionary<PropertyType, Type> typeToPluginType
			= new Dictionary<PropertyType, Type>() {
			{ PropertyType.Color, typeof(TweenMaterialImplColor) },
			{ PropertyType.Vector, typeof(TweenMaterialImplVector) },
			{ PropertyType.TextureOffset, typeof(TweenMaterialImplTexture) },
			{ PropertyType.TextureScale, typeof(TweenMaterialImplTexture) },
			{ PropertyType.Float, typeof(TweenMaterialImplFloat) }
		};

		/// <summary>
		/// Tween material properties.
		/// </summary>
		private class TweenMaterialImpl
		{
			///////////////////
			// General

			// Initialize
			public string Initialize(ITween tween, TweenPluginType initForType, ref object userData)
			{
				// Make sure we get a material
				if (!(tween.Target is Material)) {
					return string.Format(
						"Target must be a Material, got {0}.", 
						tween.Target);
				}

				var material = tween.Target as Material;

				// Check property exists on material
				if (!material.HasProperty(tween.Property)) {
					return string.Format(
						"Property {0} not found on Material {1}.", 
						tween.Property, tween.Target);
				}

				// All ok!
				return null;
			}
		}

		/// <summary>
		/// Tween material properties.
		/// </summary>
		private class TweenMaterialImplColor : TweenMaterialImpl,
			ITweenGetterPlugin<Material, Color>, ITweenSetterPlugin<Material, Color>
		{
			// Read value from material
			public Color GetValue(Material target, string property, ref object userData)
			{
				return target.GetColor(property);
			}

			// Write value to material
			public void SetValue(Material target, string property, Color value, ref object userData)
			{
				target.SetColor(property, value);
			}
		}

		/// <summary>
		/// Tween material properties.
		/// </summary>
		private class TweenMaterialImplVector : TweenMaterialImpl,
			ITweenGetterPlugin<Material, Vector2>, ITweenSetterPlugin<Material, Vector2>
		{
			// Read value from material
			public Vector2 GetValue(Material target, string property, ref object userData)
			{
				return target.GetVector(property);
			}

			// Write value to material
			public void SetValue(Material target, string property, Vector2 value, ref object userData)
			{
				target.SetVector(property, value);
			}
		}

		/// <summary>
		/// Tween material properties.
		/// </summary>
		private class TweenMaterialImplTexture : TweenMaterialImpl,
			ITweenGetterPlugin<Material, Vector2>, ITweenSetterPlugin<Material, Vector2>
		{
			// Read value from material
			public Vector2 GetValue(Material target, string property, ref object userData)
			{
				if ((PropertyType)userData == PropertyType.TextureOffset) {
					return target.GetTextureOffset(property);
				} else {
					return target.GetTextureScale(property);
				}
			}

			// Write value to material
			public void SetValue(Material target, string property, Vector2 value, ref object userData)
			{
				if ((PropertyType)userData == PropertyType.TextureOffset) {
					target.SetTextureOffset(property, value);
				} else {
					target.SetTextureScale(property, value);
				}
			}
		}

		/// <summary>
		/// Tween material properties.
		/// </summary>
		private class TweenMaterialImplFloat : TweenMaterialImpl,
			ITweenGetterPlugin<Material, float>, ITweenSetterPlugin<Material, float>
		{
			// Read value from material
			public float GetValue(Material target, string property, ref object userData)
			{
				return target.GetFloat(property);
			}

			// Write value to material
			public void SetValue(Material target, string property, float value, ref object userData)
			{
				target.SetFloat(property, value);
			}
		}
	}

}

