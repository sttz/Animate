using System;
using Sttz.Tweener.Core;

namespace Sttz.Tweener {

/// <summary>
/// Animate Tweening Engine.
/// </summary>
/// <remarks>
/// version 3.0.2
/// 
/// Animate is a high-performance generic tweening engine written in C#
/// and optimized to use in the Unity game engine.
/// </remarks>
public static class Animate
{
	/// <summary>
	/// Animate version string.
	/// </summary>
	public const string Version = "3.0.2";
	/// <summary>
	/// Animate version number.
	/// </summary>
	public const int VersionNumber = 3012;

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
