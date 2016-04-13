using System;
using System.Text.RegularExpressions;

namespace Sttz.Tweener.Core {

	// Tween plugin hooks
	[Flags]
	public enum TweenPluginHook {
		None = 0,
		// Get Value hook
		GetValue = 2,
		GetValueWeak = (GetValue | 4),
		// Set Value hook
		SetValue = 8,
		SetValueWeak = (SetValue | 16),
		// Calcualte value hook
		CalculateValue = 32,
		CalculateValueWeak = (CalculateValue | 64)
	}

	/// <summary>
	/// I tween plugin.
	/// </summary>
	public interface ITweenPlugin<TValue>
	{
		// Initialize the plugin for the given tween,
		// returns null on success or an error message on failure.
		string Initialize(ITween tween, TweenPluginHook hook, ref object userData);

		///////////////////
		// Get Hook

		// Get the value of a plugin property
		TValue GetValue(object target, string property, ref object userData);

		///////////////////
		// Set Hook

		// Set the value of a plugin property
		void SetValue(object target, string property, TValue value, ref object userData);

		///////////////////
		// Calculate Hook

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
		public TweenPluginHook hooks;
		public TweenPluginDelegate manualActivation;
		public TweenPluginDelegate autoActivation;

		public object getValueUserData;
		public object setValueUserData;
		public object calculateValueUserData;
	}

	/// <summary>
	/// Abstract base class for tween plugins.
	/// </summary>
	public abstract class TweenPlugin<TValue> : ITweenPlugin<TValue>
	{
		///////////////////
		// General

		// Initialize the plugin for the given tween,
		// returns null on success or an error message on failure.
		public virtual string Initialize(ITween tween, TweenPluginHook hook, ref object userData)
		{
			return null;
		}

		///////////////////
		// Get Hook

		// Get the value of a plugin property
		public virtual TValue GetValue(object target, string property, ref object userData)
		{
			throw new NotImplementedException();
		}

		///////////////////
		// Set Hook

		// Set the value of a plugin property
		public virtual void SetValue(object target, string property, TValue value, ref object userData)
		{
			throw new NotImplementedException();
		}

		///////////////////
		// Calcualte Hook

		// Return the difference between start and end
		public virtual TValue DiffValue(TValue start, TValue end, ref object userData)
		{
			throw new NotImplementedException();
		}

		// Return the end value
		public virtual TValue EndValue(TValue start, TValue diff, ref object userData)
		{
			throw new NotImplementedException();
		}

		// Return the value at the current position
		public virtual TValue ValueAtPosition(TValue start, TValue end, TValue diff, float position, ref object userData)
		{
			throw new NotImplementedException();
		}
	}
}