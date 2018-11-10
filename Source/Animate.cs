using System;
using Sttz.Tweener.Core;

namespace Sttz.Tweener {

/// <summary>
/// Animate Tweening Engine.
/// </summary>
/// <remarks>
/// version 3.0.0 beta 3
/// 
/// Animate is a high-performance generic tweening engine written in C#
/// and optimized to use in the Unity game engine.
/// 
/// ### Introduction
/// 
/// What separates Animate from most other C# tweening engines is that it
/// uses generics for the main tweening operations. This avoids expensive
/// boxing of value types and casting when accessing the properties.
/// 
/// Animate also recycles its objects and uses values types where possible
/// to avoid creating work for the garbage collector.
/// 
/// There exist three main tweening modes, that can be used depending on
/// the target platform:
/// - Code generation creates accessors and arithmetic operations dynamically
///   as needed. This is the most convenient and pretty fast but takes
///   more time for warm up and is not available on AOT (IL2CPP) platforms
///   or with .Net Standard.
/// - Reflection dynamically accesses properties and arithmetic operators.
///   This is convenient but slow and mostly useful during development.
/// - Static mode uses pre-defined and user-provided callbacks for accessing
///   properties and arithmetics. This is very fast but requires a bit
///   of set up per property and arithmetic type.
/// 
/// Animate provides a plugin system to extend the basic behavior. All 
/// the modes described above are implemented as plugins as well.
/// 
/// ### Usage
/// 
/// The quickest way to create a tween using Animate are the 
/// <see cref="Animate.To"/>, <see cref="Animate.From"/>, 
/// <see cref="Animate.FromTo"/> and <see cref="Animate.By"/> 
/// methods. Those methods create a single tween that will inherit the global
/// options.
/// 
/// The most flexible way to create a tween is the <see cref="Animate.On"/>
/// method. It returns a group with a default target, allows to create
/// multiple tweens in one go and define options that apply to all of those tweens.
/// 
/// ```cs
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
///     .To("rotation", Quaternion.identity, 
///         t => t.Ease(Easing.Linear)
///     )
///     .Ease(Easing.QuinticInOut);
/// ```
/// 
/// ### Options
/// 
/// A tween's options can be set on the <see cref="TweenOptions"/> class. Options
/// are stacked, allowing to set an option on a global, template, group or tween's
/// level. Options in a lower level override their parent's options.
/// 
/// - **Global options**: <see cref="ITweenEngine.Options"/> (available via 
///   <see cref="Animate.Engine"/>)
/// - **Template options**: Create a template using <see cref="Animate.Template"/> 
///   and then use it with <see cref="Animate.On"/> and <see cref="Animate.Group"/>.
/// - **Group options**: Groups extend <see cref="TweenOptionsContainer.Options"/>.
/// - **Tween options**: Tweens extend <see cref="TweenOptionsContainer.Options"/>.
/// 
/// For Templates, Groups and Tweens, <see cref="TweenOptionsFluid"/> implements
/// the fluid interface for setting options.
/// 
/// Events are also part of the options stack, meaning that events bubble up the stack
/// and e.g. listening for global or a group's <see cref="TweenOptions.ErrorEvent"/>
/// will trigger for any tween error globally or inside the group.
/// 
/// Events for groups and tweens are reset once the group is recycled or the tween
/// completes. Therefore it's typically not necessary to unregister event handlers.
/// 
/// A lot of Animate's options use flags that can be combined to define
/// the exact behavior. Most of those options also provide default combinations
/// that should cover most use cases. To combine different flags to create
/// your own combination, use the binary-or operator, e.g:
/// `Animate.Options.OverwriteSettings = 
///     TweenOverwrite.OnStart | TweenOverwrite.Finish | TweenOverwrite.Overlapping;`
/// 
/// ### Recycling
/// 
/// Groups and tweens will be recycled by default to reduce heap memory pressure.
/// This means you can't reuse groups or tweens and should typically just create
/// new groups or tweens (which will use recycled instances).
/// 
/// Note that groups from <see cref="Animate.Group"/> and templates are not 
/// recycled (<see cref="TweenRecycle.Groups"/> is unset on 
/// <see cref="TweenOptions.Recycle"/>) to allow creating and reusing groups with
/// custom settings.
/// 
/// If you want to hold on to a tween or group, there are two main options:
/// - Increase the group's or tween's <see cref="TweenOptionsContainer.RetainCount"/>
///   until you don't need it anymore and then decrease it again. This will
///   prevent it from being recycled for that duration.
/// - Set <see cref="TweenOptions.Recycle"/>. This allows to disable recycling for 
///   groups and/or tweens on any level (global, group, tween).
/// - Set <see cref="ITweenEngine.Pool"/> to `null` to disable recycling completely.
/// 
/// You need to increase the retain count when you e.g. wait for a tween to
/// complete by checking it repeatedly. If not retained, the tween will be 
/// recycled immediately on completion and the check will return invalid information.
/// 
/// ### Default Plugins
/// 
/// Animate provides three sets of default plugins, each with different convenience,
/// speed and compatibility tradeoff:
/// - **Static**: The static plugins use no reflection, are performant but require
///   some setup per tweened property or tween type.
/// - **Reflection**: The reflection plugin is relatively slow but works on 
///   AOT/IL2CPP platforms.
/// - **Codegen**: The codegen reflection plugin is fast but only works on non-AOT
///   platforms and not on .Net Standard.
/// 
/// By default, reflection is disabled and only the static plugin used. Animate 
/// comes with static support for many Unity properties and types built-in but
/// will have to enable support for custom properties, types or ones not covered
/// by the default support.
/// 
/// To enable reflection, set the `ANIMATE_REFLECTION` compilation define (.e.g. 
/// in Unity's player settings). This will enable codegen where enable and fall
/// back to plain reflection on other platforms.
/// 
/// To extend support of the static plugin, use the <see cref="Animate.EnableAccess"/>
/// and <see cref="Animate.EnableArithmetic"/> methods to enable access to a
/// property or tweening a type respectively.
/// 
/// ```cs
/// class Example {
/// 	public float field;
/// }
/// 
/// // Enable tweening "field" on the class "Example"
/// Animate.EnableAccess("field",
/// 	(Example t) => t.field,
/// 	(t, v) => t.field = v);
/// 
/// // Add support for tweening long
/// Animate.EnableArithmetic&lt;long&gt;(
///     (start, end) => end - start,
///     (start, diff) => start + diff,
///     (start, end, diff, position) => start + (long)(diff * (double)position)
/// );
/// ```
/// 
/// ### Plugins
/// 
/// There are two main ways to use plugins, enabling them for automatic loading
/// on any options level or requiring them on a single tween.
/// 
/// The first allows the plugin to check if it's needed and load itself automatically
/// if that's the case. If an automatic plugin does not load, it fails silently.
/// 
/// Loading a plugin on a single tween, however, requires the plugin to be used
/// and an error will be raised if the plugin fails to load.
/// 
/// ```cs
/// // Enable a plugin globally
/// Animate.Engine.Options.EnablePlugin(TweenRigidbody.Load);
/// 
/// // Enable a plugin for a group
/// Animate.On(transform).Over(5f)
/// 	.EnablePlugin(TweenRigidbody.Load);
/// 	.To("position", Vector3.one);
/// 
/// // Requiring a plugin for a tween (will raise an error if it fails)
/// Animate.To(transform, 5f, "position", Vector3.one)
/// 	.PluginRigidbody();
/// ```
/// 
/// The order in which plugins are enabled matters, if multiple plugins
/// could be used. In case of conflict, the plugin loaded last will be
/// used.
/// 
/// Plugins can also be re-enabled or re-disabled on the different options levels,
/// i.e. a plugin can be enabled globally but disabled for a specific group.
/// </remarks>
public static class Animate
{
	/// <summary>
	/// Animate version string.
	/// </summary>
	public const string Version = "3.0.0b3";
	/// <summary>
	/// Animate version number.
	/// </summary>
	public const int VersionNumber = 3003;

	// -------- Configuration --------

	/// <summary>
	/// Default tween engine factory method.
	/// </summary>
	public static readonly Func<ITweenEngine> DefaultEngineFactory = () => {
		var engine = UnityTweenEngine.Create();

		// Recycle tweens and groups
		engine.Options.Recycle = TweenRecycle.All;
		engine.Pool = new TweenPool();

		// Default settings
		engine.Options.Easing = Easing.QuadraticOut;
		engine.Options.LogLevel = TweenLogLevel.Warning;

		engine.Options.TweenTiming = TweenTiming.Default;
		engine.Options.OverwriteSettings = TweenOverwrite.Default;

		// Unity integration for static plugins
		TweenStaticUnitySupport.Register();

		// Default Plugins
		engine.Options.EnablePlugin(TweenStaticAccessorPlugin.Loader);
		engine.Options.EnablePlugin(TweenStaticArithmeticPlugin.Loader);

		// Plugins for special use cases
		engine.Options.EnablePlugin(TweenSlerp.Loader);
		//engine.Options.SetPluginEnabled(TweenRigidbody.Load);
		//engine.Options.SetPluginEnabled(TweenMaterial.Load);

		return engine;
	};

	/// <summary>
	/// The factory method used to create the tween engine.
	/// </summary>
	/// <remarks>
	/// Set this property with your custom factory to use a custom engine.
	/// </remarks>
	public static Func<ITweenEngine> EngineFactory {
		get {
			return _engineFactory;
		}
		set {
			if (_engine != null) {
				Engine.Options.Log(
					TweenLogLevel.Warning, 
					"Setting EngineFactory when Engine has already been created has no effect."
				);
			}
			_engineFactory = value;
		}
	}
	private static Func<ITweenEngine> _engineFactory = DefaultEngineFactory;

	/// <summary>
	/// The tween engine used.
	/// </summary>
	public static ITweenEngine Engine {
		get {
			if (_engine == null) {
				_engine = EngineFactory();
			}
			return _engine;
		}
	}
	private static ITweenEngine _engine;

	/// <summary>
	/// Shorthand for <see cref="ITweenEngine.Options"/>.
	/// </summary>
	public static TweenOptions Options {
		get {
			return Engine.Options;
		}
	}

	// -------- Main Methods --------

	/// <summary>
	/// Create a new tween group with a default target.
	/// </summary>
	/// <param name='target'>
	/// Default target used for all tweens created on the group.
	/// </param>
	/// <param name='template'>
	/// Optional template providing options for this group.
	/// </param>
	public static TweenGroup<TTarget> On<TTarget>(TTarget target, TweenTemplate template = null)
		where TTarget : class
	{
		var parentOptions = Engine.Options;
		if (template != null) parentOptions = template.Options;

		TweenGroup<TTarget> tweenGroup = null;
		if (Engine.Pool != null) {
			tweenGroup = Engine.Pool.GetGroup<TTarget>();
		} else {
			tweenGroup = new TweenGroup<TTarget>();
		}
		tweenGroup.Use(target, parentOptions, Engine);

		return tweenGroup;
	}

	/// <summary>
	/// Create a new tween group without a default target.
	/// </summary>
	/// <remarks>
	/// <para>You'll have to specify a target for each individual tween added
	/// to this group.</para>
	/// <para>Groups returned by this method are not recycled so you can
	/// reuse the group even after all of its tweens have completed.</para>
	/// </remarks>
	/// <param name='template'>
	/// Optional template providing options for this group.
	/// </param>
	/// <seealso cref="Animate.On"/>
	public static TweenGroup<object> Group(TweenTemplate template = null)
	{
		var parentOptions = Engine.Options;
		if (template != null) parentOptions = template.Options;

		TweenGroup<object> tweenGroup = null;
		if (Engine.Pool != null) {
			tweenGroup = Engine.Pool.GetGroup<object>();
		} else {
			tweenGroup = new TweenGroup<object>();
		}
		tweenGroup.Use(null, parentOptions, Engine);
		tweenGroup.Options.Recycle = (parentOptions.Recycle & ~TweenRecycle.Groups);

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
	public static TweenTemplate Template()
	{
		return new TweenTemplate(Engine.Options);
	}

	// -------- Single Tweens --------

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
	/// <typeparam name='TValue'>
	/// The value type of the tween. Has to match the target property
	/// type's value exactly.
	/// </typeparam>
	/// <seealso cref="TweenGroup{TTarget}.To"/>
	public static Tween<TTarget, TValue> To<TTarget, TValue>(
		TTarget target, float duration, string property, TValue toValue
	)
		where TTarget : class
	{
		var tween = Engine.Create(
			TweenMethod.To, target, duration, property, 
			default(TValue), toValue, default(TValue)
		);
		Engine.SinglesGroup.Add(tween);
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
	/// <typeparam name='TValue'>
	/// The value type of the tween. Has to match the target property
	/// type's value exactly.
	/// </typeparam>
	/// <seealso cref="TweenGroup{TTarget}.From"/>
	public static Tween<TTarget, TValue> From<TTarget, TValue>(
		TTarget target, float duration, string property, TValue fromValue
	)
		where TTarget : class
	{
		var tween = Engine.Create(
			TweenMethod.From, target, duration, property, 
			fromValue, default(TValue), default(TValue)
		);
		Engine.SinglesGroup.Add(tween);
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
	/// <typeparam name='TValue'>
	/// The value type of the tween. Has to match the target property
	/// type's value exactly.
	/// </typeparam>
	/// <seealso cref="TweenGroup{TTarget}.FromTo"/>
	public static Tween<TTarget, TValue> FromTo<TTarget, TValue>(
		TTarget target, float duration, string property, TValue fromValue, TValue toValue
	)
		where TTarget : class
	{
		var tween = Engine.Create(
			TweenMethod.FromTo, target, duration, property, 
			fromValue, toValue, default(TValue)
		);
		Engine.SinglesGroup.Add(tween);
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
	/// <typeparam name='TValue'>
	/// The value type of the tween. Has to match the target property
	/// type's value exactly.
	/// </typeparam>
	/// <seealso cref="TweenGroup{TTarget}.By"/>
	public static Tween<TTarget, TValue> By<TTarget, TValue>(
		TTarget target, float duration, string property, TValue byValue
	)
		where TTarget : class
	{
		var tween = Engine.Create(
			TweenMethod.By, target, duration, property, 
			default(TValue), default(TValue), byValue
		);
		Engine.SinglesGroup.Add(tween);
		return tween;
	}

	// -------- Manage Tweens --------

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
		return Engine.Has(target, property);
	}

	/// <summary>
	/// Stop all tweens on a target with an optional property. Stopping
	/// a tween will leave it at its intermediate value.
	/// </summary>
	/// <param name='target'>
	/// The target to stop the tweens on.
	/// </param>
	/// <param name='property'>
	/// Optional property name. If null, all tweens on target are stopped.
	/// </param>
	/// <seealso cref="TweenGroup.Stop"/>
	public static void Stop(object target, string property = null)
	{
		Engine.Stop(target, property);
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
	/// <seealso cref="TweenGroup.Finish"/>
	public static void Finish(object target, string property = null)
	{
		Engine.Finish(target, property);
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
	/// <seealso cref="TweenGroup.Cancel"/>
	public static void Cancel(object target, string property = null)
	{
		Engine.Cancel(target, property);
	}

	// -------- Extensions --------

	/// <summary>
	/// Teach the static accessor plugin to access a property on a type.
	/// </summary>
	/// <remarks>
	/// Use this to extend Animate's ability to access properties on types
	/// without using reflection, by providing two callbacks that get
	/// and set the property on the type.
	/// </remarks>
	public static void EnableAccess<TTarget, TValue>(
		string propertyName, 
		TweenStaticAccessorPlugin.GetAccessor<TTarget, TValue> getter, 
		TweenStaticAccessorPlugin.SetAccessor<TTarget, TValue> setter
	)
		where TTarget : class
	{
		TweenStaticAccessorPlugin.EnableAccess(propertyName, getter, setter);
	}

	/// <summary>
	/// Teach the static arithmetic plugin to calculate with a given type.
	/// </summary>
	/// <remarks>
	/// Use this to extend Animate's ability to calculate with types,
	/// by providing three callbacks that do the necessary calculations 
	/// with a given type.
	/// </remarks>
	public static void EnableArithmetic<TValue>(
		TweenStaticArithmeticPlugin.DiffValue<TValue> diff, 
		TweenStaticArithmeticPlugin.EndValue<TValue> end, 
		TweenStaticArithmeticPlugin.ValueAtPosition<TValue> valueAt
	) {
		TweenStaticArithmeticPlugin.EnableArithmetic(diff, end, valueAt);
	}
}

}
