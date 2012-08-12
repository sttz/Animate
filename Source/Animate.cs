using System;
using System.Collections.Generic;
using UnityEngine;

using Sttz.Tweener;
using Sttz.Tweener.Core;

// TODO: Group events
// TODO: Handlers without parameters?

/// @mainpage
/// <summary>
/// Animate Tweening Engine.
/// </summary>
/// <remarks>
/// <para>version 3.0.0 beta 1</para>
/// <para>
/// Animate is a high-performance generalized tweening engine written in C#
/// and optimized to use in the Unity game engine.
/// </para>
/// ### Introduction
/// <para>
/// Thanks to extensive use of generics in its design, Animate avoids 
/// expensive boxing and unboxing of value types. Objects are being recycled,
/// shared and structs used where possible. After an initial warm-up phase,
/// Animate won't allocate any more objects, avoiding to put stress on the
/// grabage collection.
/// </para>
/// <para>
/// A powerful plugin system allows to extend Animate's functionality in
/// many different ways. The core tweening functionality is just a plugin
/// as well, the <c>TweenDefaultPlugin</c>.
/// </para>
/// ### Usage
/// <para>
/// The quickest way to create a tween using Animate are the 
/// <see cref="Animate.To<TValue>"/>, <see cref="Animate.From<TValue>"/>, 
/// <see cref="Animate.FromTo<TValue>"/> and <see cref="Animate.By<TValue>"/> 
/// methods. Those methods create a single tween that will inherit the global
/// options.
/// </para>
/// <para>
/// The most flexible way to create a tween is the <see cref="Animate.On"/>
/// method. It returns a group with a default target and allows to create
/// multiple tweens in one go and define options that apply to all of those tweens.
/// </para>
/// <code>
/// // Shortest tween invocation, using global options:
/// Animate.To(transform, 5f, "position", Vector3.one);
/// 
/// // Override options for that tween:
/// Animate.To(transform, 5f, "position", Vector3.one)
///     .Ease(Easing.QuadraticOut)
///     .OnComplete((sender, args) => {
///         Debug.Log("Tween completed!");
///     });
/// 
/// // Create a group, options apply to all tweens in the group:
/// Animate.On(transform).Over(5f)
///     .To("position", Vector3.one)
///     .To("rotation", Quaternion.identity)
///     .To(3f, "localScale", Vector3.one)
///     .Ease(Easing.QuinticInOut);
/// 
/// // Set options for an individual tween in the group:
/// Animate.On(transform).Over(5f)
///     .To("position", Vector3.one)
///     .Add(Tween.To("rotation", Quaternion.identity)
///         .Ease(Easing.Linear)
///     )
///     .Ease(Easing.QuinticInOut);
/// </code>
/// ### Options
/// <para>
/// A flexible options system makes it easy to define default options globally
/// or for a group of tweens and allows to override options at many different
/// levels. There are no exclusively global options in Animate, everything can
/// be fine-tuned down to a single tween. Events are also part of this system:
/// You can register event handlers globally, on a group or on an individual 
/// tween.
/// </para>
/// <para>
/// Options can be set in two different ways: Using properties on the 
/// <c>Options</c> property on <see cref="Aniamte"/>, <see cref="Sttz.Tweener.ITweenGroup"/>
/// or <see cref="Sttz.Tweener.ITween"/>, or using the fluid interface on groups and tweens
/// (see <see cref="Sttz.Tweener.ITweenOptionsFluid"/>).
/// </para>
/// <para>
/// A lot of Animate's options use flags that can be combined to define
/// the excact behavior. Most of those options also provide default combinations
/// that should cover most use cases. To combine different flags to create
/// your own combination, use the binary-or operator, e.g:
/// <code>Animate.Options.OverwriteSettings = 
///     TweenOverwrite.OnStart | TweenOverwrite.Finish | TweenOverwrite.Overlapping;</code>
/// </para>
/// ### Recycling
/// <para>
/// Note that groups and tweens will be recycled by default, meaning you cannot
/// re-use them after they completed. Use <see cref="Animate.Group"/> to create
/// groups that won't be recycled or manually turn off recycling using
/// <see cref="Sttz.Tweener.ITweenOptions.Recycle"/>.
/// </para>
/// ### Plugins
/// <para>
/// Plugins can be enabled for automatic activation or activated for a single
/// tween.
/// </para>
/// <para>
/// The automatic activation can be enabled at any scope, e.g. globally on
/// <see cref="Animate.Options"/> or just for a group or even a tween (where
/// manual activation is recommended instead). Use 
/// <see cref="Sttz.Tweener.ITweenOptions.SetAutomatic"/> or 
/// <see cref="Sttz.Tweener.ITweenOptionsFluid<TContainer>.Automate"/> to enable automatic
/// activation.
/// </para>
/// <para>Using auto-activation, the plugin will try to detect when it's needed 
/// and activate automatically. Refer to the plugin's documentation for possible 
/// additional requirements. If an automatic activation fails, 
/// it usually fails silently.
/// </para>
/// <para>
/// Plugins are activated for a tween by passing it as the last parameter
/// to all <c>To</c>, <c>From</c>, <c>FromTo</c> and <c>By</c> methods. If
/// a plugin activated this way fails, it will produce an error.
/// </para>
/// <code>
/// // Globally enable automatic activation
/// Animate.Options.SetAutomatic(TweenSlerp.Automatic());
/// 
/// // Disable (or enable) automatic activation for a group
/// Animate.On(transform).Over(5f)
///     .Automate(TweenSlerp.Automatic(), false)
///     .To("rotation", Quaternion.identity);
/// 
/// // Manually activate a plugin
/// Animate.To(transform, 5f, "rotation", Quaternion.identity, TweenSlerp.Use());
/// </code>
/// </remarks>
 
/// <summary>
/// Animate tweening engine.
/// </summary>
public class Animate : UnityTweenEngine
{
	///////////////////
	// Configuration

	// Configure global default
	static Animate()
	{
		Options.Easing = Easing.QuadraticIn;
		Options.LogLevel = TweenLogLevel.Warning;

		Options.TweenTiming = TweenTiming.Default;
		Options.OverwriteSettings = TweenOverwrite.Default;

		Options.DefaultPlugin = TweenDefaultPlugin.Use();
		Options.Recycle = TweenRecycle.All;
		Pool = new TweenPool();
	}

	///////////////////
	// Main Methods

	/// <summary>
	/// Create a new tween group with a default target.
	/// </summary>
	/// <param name='target'>
	/// Default target used for all tweens created on the group.
	/// </param>
	/// <param name='template'>
	/// Optional template providing opotions for this group.
	/// </param>
	public static TweenGroup On(object target, TweenTemplate template = null)
	{
		var parentOptions = Options;
		if (template != null) parentOptions = template;

		TweenGroup tweenGroup = null;
		if (Pool != null) {
			tweenGroup = Pool.GetGroup();
		} else {
			tweenGroup = new TweenGroup();
		}
		tweenGroup.Use(target, parentOptions, Instance);

		return tweenGroup;
	}

	/// <summary>
	/// Create a new tween group without a default target.
	/// </summary>
	/// <remarks>
	/// <para>You'll have to specify a target for each individual tween added
	/// to this gorup.</para>
	/// <para>Groups returned by this method are not recycled so you can
	/// reuse the group even after all of its tweens have completed.</para>
	/// </remarks>
	/// <param name='template'>
	/// Optional template providing opotions for this group.
	/// </param>
	/// <seealso cref="Animate.On"/>
	public static TweenGroup Group(TweenTemplate template = null)
	{
		var parentOptions = Options;
		if (template != null) parentOptions = template;

		TweenGroup tweenGroup = null;
		if (Pool != null) {
			tweenGroup = Pool.GetGroup();
		} else {
			tweenGroup = new TweenGroup();
		}
		tweenGroup.Use(null, parentOptions, Instance);
		tweenGroup.Options.Recycle = TweenRecycle.Tweens;

		return tweenGroup;
	}

	/// <summary>
	/// Create a new options template.
	/// </summary>
	/// <remarks>
	/// You can specify a template when
	/// creating a group. The template's options will override the global
	/// defaults but options you set on the group or on individual tweens
	/// will still override the template's.
	/// </remarks>
	public static ITweenTemplate Template()
	{
		return new TweenTemplate(Options);
	}

	///////////////////
	// Single Tweens

	/// <summary>
	/// Create a new To tween without a group.
	/// </summary>
	/// <param name='target'>
	/// The tween's target object which contains the property to tween.
	/// </param>
	/// <param name='duration'>
	/// The duration of the tween in seconds.
	/// </param>
	/// <param name='property'>
	/// The name of the property to tween on the target.
	/// </param>
	/// <param name='toValue'>
	/// The target value you want to tween the property to. 
	/// The type of this value has to exactly match the property's type.
	/// </param>
	/// <param name='plugins'>
	/// Plugins you want to use with this tween. Call the plugin's <c>Use()</c>
	/// method with the appropriate options (e.g. <c>MyPlugin.Use(option)</c>).
	/// </param>
	/// <typeparam name='TValue'>
	/// The value type of the tween. Has to match the target property
	/// type's value exactly.
	/// </typeparam>
	/// <seealso cref="Sttz.Tweener.ITweenGroup.To<TValue>"/>
	/// <seealso cref="Sttz.Tweener.Tween.To<TValue>"/>
	public static Tween<TValue> To<TValue>(
		object target, float duration, string property, TValue toValue, 
		params TweenPluginInfo[] plugins
	) {
		var tween = Tween.To(target, duration, property, toValue, plugins);
		Instance.SinglesGroup.Add(tween);
		return tween;
	}

	/// <summary>
	/// Create a new From tween without a group.
	/// </summary>
	/// <param name='target'>
	/// The tween's target object which contains the property to tween.
	/// </param>
	/// <param name='duration'>
	/// The duration of the tween in seconds.
	/// </param>
	/// <param name='property'>
	/// The name of the property to tween on the target.
	/// </param>
	/// <param name='fromValue'>
	/// The target value you want to tween the property from, to its current value. 
	/// The type of this value has to exactly match the property's type.
	/// </param>
	/// <param name='plugins'>
	/// Plugins you want to use with this tween. Call the plugin's <c>Use()</c>
	/// method with the appropriate options (e.g. <c>MyPlugin.Use(option)</c>).
	/// </param>
	/// <typeparam name='TValue'>
	/// The value type of the tween. Has to match the target property
	/// type's value exactly.
	/// </typeparam>
	/// <seealso cref="Sttz.Tweener.ITweenGroup.From<TValue>"/>
	/// <seealso cref="Sttz.Tweener.Tween.From<TValue>"/>
	public static Tween<TValue> From<TValue>(
		object target, float duration, string property, TValue fromValue, 
		params TweenPluginInfo[] plugins
	) {
		var tween = Tween.From(target, duration, property, fromValue, plugins);
		Instance.SinglesGroup.Add(tween);
		return tween;
	}

	/// <summary>
	/// Create a new FromTo tween without a group.
	/// </summary>
	/// <param name='target'>
	/// The tween's target object which contains the property to tween.
	/// </param>
	/// <param name='duration'>
	/// The duration of the tween in seconds.
	/// </param>
	/// <param name='property'>
	/// The name of the property to tween on the target.
	/// </param>
	/// <param name='fromValue'>
	/// The value you want to tween the property from. 
	/// The type of this value has to exactly match the property's type.
	/// </param>
	/// <param name='toValue'>
	/// The value you want to tween the property to. 
	/// The type of this value has to exactly match the property's type.
	/// </param>
	/// <param name='plugins'>
	/// Plugins you want to use with this tween. Call the plugin's <c>Use()</c>
	/// method with the appropriate options (e.g. <c>MyPlugin.Use(option)</c>).
	/// </param>
	/// <typeparam name='TValue'>
	/// The value type of the tween. Has to match the target property
	/// type's value exactly.
	/// </typeparam>
	/// <seealso cref="Sttz.Tweener.ITweenGroup.FromTo<TValue>"/>
	/// <seealso cref="Sttz.Tweener.Tween.FromTo<TValue>"/>
	public static Tween<TValue> FromTo<TValue>(
		object target, float duration, string property, TValue fromValue, TValue toValue, 
		params TweenPluginInfo[] plugins
	) {
		var tween = Tween.FromTo(target, duration, property, fromValue, toValue, plugins);
		Instance.SinglesGroup.Add(tween);
		return tween;
	}

	/// <summary>
	/// Create a new By tween without a group.
	/// </summary>
	/// <param name='target'>
	/// The tween's target object which contains the property to tween.
	/// </param>
	/// <param name='duration'>
	/// The duration of the tween in seconds.
	/// </param>
	/// <param name='property'>
	/// The name of the property to tween on the target.
	/// </param>
	/// <param name='byValue'>
	/// The target value you want to tween the property by, starting from its current value. 
	/// The type of this value has to exactly match the property's type.
	/// </param>
	/// <param name='plugins'>
	/// Plugins you want to use with this tween. Call the plugin's <c>Use()</c>
	/// method with the appropriate options (e.g. <c>MyPlugin.Use(option)</c>).
	/// </param>
	/// <typeparam name='TValue'>
	/// The value type of the tween. Has to match the target property
	/// type's value exactly.
	/// </typeparam>
	/// <seealso cref="Sttz.Tweener.ITweenGroup.By<TValue>"/>
	/// <seealso cref="Sttz.Tweener.Tween.By<TValue>"/>
	public static Tween<TValue> By<TValue>(
		object target, float duration, string property, TValue byValue, 
		params TweenPluginInfo[] plugins
	) {
		var tween = Tween.By(target, duration, property, byValue, plugins);
		Instance.SinglesGroup.Add(tween);
		return tween;
	}

	///////////////////
	// Manage Tweens

	/// <summary>
	/// Check if there are any existing tweens on the given target with
	/// the optional property.
	/// </summary>
	/// <returns>
	/// <c>true</c> if tweens exist, <c>false</c> otherwise.
	/// </returns>
	/// <param name='target'>
	/// Target object the tweens run on.
	/// </param>
	/// <param name='property'>
	/// Optional property name. When null, <c>Has()</c> returns true 
	/// if any tween is running on the target.
	/// </param>
	public static bool Has(object target, string property = null)
	{
		return Instance.Has(target, property);
	}

	/// <summary>
	/// Stop all tweens on a target with an optional property. Stopping
	/// a tween will leave it at it's intermediate value.
	/// </summary>
	/// <param name='target'>
	/// The target to stop the tweens on.
	/// </param>
	/// <param name='property'>
	/// Optional property name. If null, all tweens on target are stopped.
	/// </param>
	/// <seealso cref="Sttz.Tweener.ITweenGroup.Stop"/>
	/// <seealso cref="Sttz.Tweener.ITween.Stop"/>
	public static void Stop(object target, string property = null)
	{
		Instance.Stop(target, property);
	}

	/// <summary>
	/// Finish all tweens on a target with an optional property. Finishing
	/// a tween will leave set it to its end value.
	/// </summary>
	/// <param name='target'>
	/// The target to finish the tweens on.
	/// </param>
	/// <param name='property'>
	/// Optional property name. If null, all tweens on target are finished.
	/// </param>
	/// <seealso cref="Sttz.Tweener.ITweenGroup.Finish"/>
	/// <seealso cref="Sttz.Tweener.ITween.Finish"/>
	public static void Finish(object target, string property = null)
	{
		Instance.Finish(target, property);
	}

	/// <summary>
	/// Cancel all tweens on a target with an optional property. Cancelling
	/// a tween will leave set it to its initial value.
	/// </summary>
	/// <param name='target'>
	/// The target to cancel the tweens on.
	/// </param>
	/// <param name='property'>
	/// Optional property name. If null, all tweens on target are cancelled.
	/// </param>
	/// <seealso cref="Sttz.Tweener.ITweenGroup.Cancel"/>
	/// <seealso cref="Sttz.Tweener.ITween.Cancel"/>
	public static void Cancel(object target, string property = null)
	{
		Instance.Cancel(target, property);
	}
}
