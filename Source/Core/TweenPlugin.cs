using System;
using System.Text.RegularExpressions;
using Sttz.Tweener;

namespace Sttz.Tweener.Core {

/// <summary>
/// Plugin load callback, called by the tween on initialization.
/// </summary>
/// <param name="tween">Tween to load the plugin for</param>
/// <param name="weak">Wether to load the plugin is required</param>
/// <returns>The result of the loading</returns>
public delegate PluginResult PluginLoader(Tween tween, bool required);

/// <summary>
/// Result of a <see cref="PluginLoader"/> method.
/// </summary>
public struct PluginResult
{
	/// <summary>
	/// Create a new result representing an error.
	/// </summary>
	/// <param name="format">Error string format</param>
	/// <param name="args">Format string arguments</param>
	public static PluginResult Error(string format, params object[] args)
	{
		return new PluginResult() {
			error = format,
			errorArgs = args
		};
	}

	/// <summary>
	/// Create a new result representing success.
	/// </summary>
	/// <param name="plugin">Plugin instance to use</param>
	/// <param name="userData">User data to pass to the plugin instance</param>
	public static PluginResult Load(ITweenPlugin plugin, object userData = null)
	{
		return new PluginResult() {
			plugin = plugin,
			userData = userData
		};
	}

	public string error;
	public object[] errorArgs;
	public ITweenPlugin plugin;
	public object userData;

	/// <summary>
	/// Get the formatted error string.
	/// </summary>
	public string GetError()
	{
		return string.Format(error, errorArgs);
	}
}

/// <summary>
/// Plugin types a single plugin instance covers.
/// </summary>
[Flags]
public enum TweenPluginType
{
	None = 0,
	Getter = 1<<0,
	Setter = 1<<1,
	Arithmetic = 1<<2
}

/// <summary>
/// Base interface for Animate plugins.
/// </summary>
/// <remarks>
/// The entry point for a plugin is its <see cref="PluginLoader"/> method. The loader
/// gets called with a tween and should then check if the plugin applies to the 
/// given tween. If everything is fine, it should call <see cref="Tween.LoadPlugin"/>
/// to actually register itself with the tween.
/// 
/// In addition to the loader, the plugin should also provide and extension method
/// for <see cref="Tween{TTarget,TValue}"/>, that calls the loader for the current
/// tween and requires the plugin.
/// 
/// All built-in plugins follow these conventions:
/// - The plugin container is a static class that contains the loader method, the
///   tween extension method and private plugin implementation classes.
/// - If the plugin has options the user can set, it should optionally provide a 
///   property inline-syntax via <see cref="Tween.PropertyOptions"/>, a `CustomLoader`
///   method that returns a loader with the given options applied (.e.g. see
///   <see cref="TweenMaterial.CustomLoader"/>) and an extension method on `Tween`.
///   The extension method can use concrete types to provide type-checking (e.g.
///   extend `Tween&lt;Transform, TValue&gt;` if the plugin only works on transforms).
/// - The plugin can contain multiple private implementations that are chosen
///   by the loader method.
/// - The error returned by the loader is handled depending on wether the plugin
///   is required. If it's required, the error is fatal and logged as error. If it's
///   not required, the error is silent only only logged as debug message.
///   If the plugin wants additional logging, it can call <see cref="TweenOptions.Log"/>
///   on the tween.
/// </remarks>
public interface ITweenPlugin
{
	/// <summary>
	/// Initialize the plugin.
	/// </summary>
	/// <remarks>
	/// In contrast to the loader, this method is not called for every tween and only
	/// if the plugin is actually used. Heavy initialization should be deferred to this
	/// method.
	/// </remarks>
	/// <param name="tween">Tween the plugin is loaded for</param>
	/// <param name="initForType">The types the plugin is actually used for</param>
	/// <param name="userData">User data set by the loader</param>
	/// <returns>null on success or an error on failure</returns>
	string Initialize(Tween tween, TweenPluginType initForType, ref object userData);
}

/// <summary>
/// Plugin providing get access to a property.
/// </summary>
public interface ITweenGetterPlugin<TTarget, TValue> : ITweenPlugin
{
	/// <summary>
	/// Get the value of the property on the target object.
	/// </summary>
	/// <param name="target">The target object</param>
	/// <param name="property">The property name</param>
	/// <param name="userData">User data set by the loader</param>
	/// <returns>The current value of the property</returns>
	TValue GetValue(TTarget target, string property, ref object userData);
}

/// <summary>
/// Plugin providing set access to a property.
/// </summary>
public interface ITweenSetterPlugin<TTarget, TValue> : ITweenPlugin
{
	/// <summary>
	/// Set the value of the property on the target object.
	/// </summary>
	/// <param name="target">The target object</param>
	/// <param name="property">The property name</param>
	/// <param name="value">The value to set</param>
	/// <param name="userData">User data set by the loader</param>
	void SetValue(TTarget target, string property, TValue value, ref object userData);
}

/// <summary>
/// Plugin providing calculations for a type.
/// </summary>
public interface ITweenArithmeticPlugin<TValue> : ITweenPlugin
{
	/// <summary>
	/// Calculate the difference between start and end.
	/// </summary>
	/// <param name="start">The start value</param>
	/// <param name="end">The end value</param>
	/// <param name="userData">User data set by the loader</param>
	/// <returns>The difference between start and end</returns>
	TValue DiffValue(TValue start, TValue end, ref object userData);
	
	/// <summary>
	/// Calculate the end value.
	/// </summary>
	/// <param name="start">The start value</param>
	/// <param name="diff">The difference from start to end</param>
	/// <param name="userData">User data set by the loader</param>
	/// <returns>The end value</returns>
	TValue EndValue(TValue start, TValue diff, ref object userData);

	/// <summary>
	/// Calculate the value at the given position.
	/// </summary>
	/// <remarks>
	/// Depending on the tween type, the user and the accessor plugin provide
	/// some combination of start, diff and end values. The arithmetic plugin
	/// is then asked via <see cref="DiffValue"/> and <see cref="EndValue"/>
	/// to calculate the missing values. Those values are only for the plugin
	/// itself and it can opt to not calculate them.
	/// </remarks>
	/// <param name="start">The start value</param>
	/// <param name="end">The end value</param>
	/// <param name="diff">The difference from start to end</param>
	/// <param name="position">The normalized position between 0 and 1</param>
	/// <param name="userData">User data set by the loader</param>
	/// <returns>The value at the given position</returns>
	TValue ValueAtPosition(TValue start, TValue end, TValue diff, float position, ref object userData);
}

}
