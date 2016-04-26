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
}