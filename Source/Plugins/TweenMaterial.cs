using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sttz.Tweener.Core;

namespace Sttz.Tweener {

/// <summary>
/// Tween a material property.
/// </summary>
/// <remarks>
/// This plugin enables to tweening of any custom property on a Material.
/// 
/// Since Unity doesn't expose a property's type at runtime, it's up
/// to the user to make sure the tween type matches the property's type.
/// 
/// When not specifying a property type with <see cref="CustomLoader"/>
/// or using one of the <see cref="Material"/> extension methods, following
/// types are inferred from the tween type:
/// - Color: Color
/// - Vector2: TextureOffset
/// - Vector4: Vector
/// - Float: Float
/// Note that TextureScale always requires specifying the type.
/// 
/// When the plugin is enabled globally or for a group, it also requires
/// properties to be prefixed with an underscore (.e.g. "_Color", which
/// is also the Unity convention for all material properties).
/// 
/// ```cs
/// // Enable globally
/// Animate.Options.EnablePlugin(TweenMaterial.Load);
/// 
/// // Enable for a specific tween
/// Animate.To(material, 2f, "_Color", Color.white)
/// 	.Material();
/// 
/// // Specifying a property type
/// Animate.To(material, 2f, "_MainTex", Vector2.one)
/// 	.Material(TweenMaterial.PropertyType.TextureScale);
/// ```
/// </remarks>
public static class TweenMaterial
{
	// -------- Plugin Use --------

	/// <summary>
	/// Type of the material property.
	/// </summary>
	public enum PropertyType
	{
		Undefined,
		Color,
		Vector,
		TextureOffset,
		TextureScale,
		Float
	}

	/// <summary>
	/// TweenMaterial plugin loader.
	/// </summary>
	/// <remarks>
	/// Pass this method to <see cref="TweenOptions.EnablePlugin"/> to enable the
	/// plugin for the options scope.
	/// </remarks>
	public static PluginResult Loader(Tween tween, bool required)
	{
		return Loader(tween, required, PropertyType.Undefined);
	}

	/// <summary>
	/// Create a custom TweenMaterial plugin loader that forces a 
	/// specific material property type.
	/// </summary>
	/// <remarks>
	/// Pass the result of this method to <see cref="TweenOptions.EnablePlugin"/> to enable the
	/// plugin for the options scope.
	/// </remarks>
	/// <param name="forceType">Type of the material property</param>
	/// <returns>A custom plugin loader that forces the property type</returns>
	public static PluginLoader CustomLoader(PropertyType forceType)
	{
		if (forceType == PropertyType.Undefined) {
			return Loader;
		} else {
			return (tween, required) => {
				return Loader(tween, required, forceType);
			};
		}
	}

	/// <summary>
	/// Require the <see cref="TweenMaterial"/> plugin for the current tween.
	/// </summary>
	/// <remarks>
	/// Shorthand for using <see cref="TweenOptions.EnablePlugin"/> with type-checking.
	/// </remarks>
	/// <param name="type">Type of the material property</param>
	public static Tween<Material, Color> Material(this Tween<Material, Color> tween, PropertyType type = PropertyType.Undefined) {
		tween.Options.EnablePlugin(CustomLoader(type), true, true);
		return tween;
	}

	/// <summary>
	/// Require the <see cref="TweenMaterial"/> plugin for the current tween.
	/// </summary>
	/// <remarks>
	/// Shorthand for using <see cref="TweenOptions.EnablePlugin"/> with type-checking.
	/// </remarks>
	/// <param name="type">Type of the material property</param>
	public static Tween<Material, Vector2> Material(this Tween<Material, Vector2> tween, PropertyType type = PropertyType.Undefined) {
		tween.Options.EnablePlugin(CustomLoader(type), true, true);
		return tween;
	}

	/// <summary>
	/// Require the <see cref="TweenMaterial"/> plugin for the current tween.
	/// </summary>
	/// <remarks>
	/// Shorthand for using <see cref="TweenOptions.EnablePlugin"/> with type-checking.
	/// </remarks>
	/// <param name="type">Type of the material property</param>
	public static Tween<Material, Vector4> Material(this Tween<Material, Vector4> tween, PropertyType type = PropertyType.Undefined) {
		tween.Options.EnablePlugin(CustomLoader(type), true, true);
		return tween;
	}

	/// <summary>
	/// Require the <see cref="TweenMaterial"/> plugin for the current tween.
	/// </summary>
	/// <remarks>
	/// Shorthand for using <see cref="TweenOptions.EnablePlugin"/> with type-checking.
	/// </remarks>
	/// <param name="type">Type of the material property</param>
	public static Tween<Material, float> Material(this Tween<Material, float> tween, PropertyType type = PropertyType.Undefined) {
		tween.Options.EnablePlugin(CustomLoader(type), true, true);
		return tween;
	}

	// -------- Internals --------

	static PluginResult Loader(Tween tween, bool required, PropertyType forceType)
	{
		// Check if target is Material
		if (!(tween.Target is Material)) {
			return PluginResult.Error("TweenMaterial must be used on a Material, got {0}.".LazyFormat(tween.TargetType));
		}

		// Since we cannot get the type of a material property
		// from unity, we have to get it as parameter or
		// assume it from the tween type
		var type = PropertyType.Color;

		// Property type has been forced
		if (forceType != PropertyType.Undefined) {
			type = forceType;

		} else {
			// Get option, removes it from the property for check below
			var option = tween.PropertyOptions;

			// Only activate for properties starting with an underscore
			if (!required && tween.Property[0] != '_') {
				return PluginResult.Error("TweenMaterial: Non-required loading needs name to start with an underscore.");
			}

			// Validate and parse option
			if (option != string.Empty) {
				if (!validOptions.Contains(option, StringComparer.OrdinalIgnoreCase)) {
					return PluginResult.Error("TweenMaterial: Invalid property type option '{0}'.".LazyFormat(option));
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
		}

		// Check tween value type
		if (typeToValueType[type] != tween.ValueType) {
			return PluginResult.Error(
				"TweenMaterial: Invalid value type '{0}' for property type {1}."
				.LazyFormat(tween.ValueType, type)
			);
		}

		// Set plugin type to use
		return PluginResult.Load(typeToPluginInstance[type], userData: type);
	}

	// Possible plugin options
	private static string[] validOptions = Enum.GetNames(typeof(PropertyType));

	// Mapping of property type to value type
	private static Dictionary<PropertyType, Type> typeToValueType
		= new Dictionary<PropertyType, Type> {
		{ PropertyType.Color, typeof(Color) },
		{ PropertyType.Vector, typeof(Vector4) },
		{ PropertyType.TextureOffset, typeof(Vector2) },
		{ PropertyType.TextureScale, typeof(Vector2) },
		{ PropertyType.Float, typeof(float) }
	};

	// Mapping of property type to plugin type
	private static Dictionary<PropertyType, ITweenPlugin> typeToPluginInstance
		= new Dictionary<PropertyType, ITweenPlugin> {
		{ PropertyType.Color, new TweenMaterialImplColor() },
		{ PropertyType.Vector, new TweenMaterialImplVector() },
		{ PropertyType.TextureOffset, new TweenMaterialImplTexture() },
		{ PropertyType.TextureScale, new TweenMaterialImplTexture() },
		{ PropertyType.Float, new TweenMaterialImplFloat() }
	};

	/// <summary>
	/// Tween material properties.
	/// </summary>
	private abstract class TweenMaterialImpl
	{
		// -------- General --------

		// Initialize
		public string Initialize(Tween tween, TweenPluginType initForType, ref object userData)
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
