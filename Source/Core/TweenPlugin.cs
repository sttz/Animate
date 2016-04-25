using System;
using System.Text.RegularExpressions;

namespace Sttz.Tweener.Core {

	[Flags]
	public enum TweenPluginType
	{
		None = 0,
		Getter = 1<<0,
		Setter = 1<<1,
		Arithmetic = 1<<2
	}

	/// <summary>
	/// I tween plugin.
	/// </summary>
	public interface ITweenPlugin
	{
		// Initialize the plugin for the given tween,
		// returns null on success or an error message on failure.
		string Initialize(ITween tween, TweenPluginType initForType, ref object userData);
	}

	public interface ITweenGetterPlugin<TTarget, TValue> : ITweenPlugin
	{
		// Get the value of a plugin property
		TValue GetValue(TTarget target, string property, ref object userData);
	}

	public interface ITweenSetterPlugin<TTarget, TValue> : ITweenPlugin
	{
		// Set the value of a plugin property
		void SetValue(TTarget target, string property, TValue value, ref object userData);
	}

	public interface ITweenArithmeticPlugin<TValue> : ITweenPlugin
	{
		// Return the difference between start and end
		TValue DiffValue(TValue start, TValue end, ref object userData);
		// Return the end value
		TValue EndValue(TValue start, TValue diff, ref object userData);
		// Return the value at the current position
		TValue ValueAtPosition(TValue start, TValue end, TValue diff, float position, ref object userData);
	}

	// Delegate used for automatic plugins
	public delegate TweenPluginInfo TweenPluginDelegate(ITween tween, TweenPluginInfo info);

	/// <summary>
	/// Tween plugin data.
	/// </summary>
	public struct TweenPluginInfo
	{
		///////////////////
		// Option parsing

		// Regex used to parse options from property string
		// for automatic plugins.
		private static readonly Regex OptionsRegex = new Regex(
			@"^:(?<options>\w+):(?<property>.*)$",
			RegexOptions.ExplicitCapture
		);

		// Parse and remove option from tween property
		public static string GetOption(ITween tween)
		{
			// Try to parse options from property
			var match = OptionsRegex.Match(tween.Property);
			if (!match.Success) return null;

			// Remove options from tween property
			tween.Internal.Property = match.Groups["property"].Value;

			return match.Groups["options"].Value;
		}

		///////////////////
		// Plugin Info

		public static readonly TweenPluginInfo None;

		public Type pluginType;
		public TweenPluginType hooks;
		public bool canBeOverwritten;
		public TweenPluginDelegate manualActivation;
		public TweenPluginDelegate autoActivation;

		public object getValueUserData;
		public object setValueUserData;
		public object calculateValueUserData;
	}
}