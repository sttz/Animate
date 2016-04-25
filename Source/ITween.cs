using System;
using UnityEngine;

using Sttz.Tweener.Core;
namespace Sttz.Tweener {

	///////////////////
	// Tween

	/// <summary>
	/// Create individually configurable tweens.
	/// </summary>
	/// <remarks>
	/// Note that tweens created using the methods on <c>Tween</c> will not
	/// be registered automatically, you will have to add them to a group
	/// using its <see cref="ITweenGroup.Add"/> method.
	/// </remarks>
	public static class Tween
	{
		///////////////////
		// To

		// TODO: Possible to bring this method back with proper typing?
		/// <summary>
		/// Create a new To tween that will inherit the group's
		/// target and duration.
		/// </summary>
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
		/// <seealso cref="Animate.To<TValue>"/>
		/// <seealso cref="ITweenGroup.To<TValue>"/>
		/*public static Tween<TTarget, TValue> To<TTarget, TValue>(
			string property, TValue toValue, 
			params TweenPluginInfo[] plugins
		) 
			where TTarget : class
		{
			return Tween<TTarget, TValue>.Create(TweenMethod.To, 
				null, float.NaN, property, 
				default(TValue), toValue, default(TValue), 
				plugins
			);
		}*/

		/// <summary>
		/// Create a new To tween that will inherit the group's target.
		/// </summary>
		/// <param name='target'>
		/// The tween's target object which contains the property to tween.
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
		/// <seealso cref="Animate.To<TValue>"/>
		/// <seealso cref="ITweenGroup.To<TValue>"/>
		public static Tween<TTarget, TValue> To<TTarget, TValue>(
			TTarget target, string property, TValue toValue, 
			params TweenPluginInfo[] plugins
		)
			where TTarget : class
		{
			return Tween<TTarget, TValue>.Create(TweenMethod.To, 
				target, float.NaN, property, 
				default(TValue), toValue, default(TValue), 
				plugins
			);
		}

		/// <summary>
		/// Create a new To tween.
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
		/// <seealso cref="Animate.To<TValue>"/>
		/// <seealso cref="ITweenGroup.To<TValue>"/>
		public static Tween<TTarget, TValue> To<TTarget, TValue>(
			TTarget target, float duration, string property, TValue toValue, 
			params TweenPluginInfo[] plugins
		)
			where TTarget : class
		{
			return Tween<TTarget, TValue>.Create(TweenMethod.To, 
				target, duration, property, 
				default(TValue), toValue, default(TValue), 
				plugins
			);
		}

		///////////////////
		// From

		/// <summary>
		/// Create a new From tween that will inherit the group's 
		/// target and duration.
		/// </summary>
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
		/// <seealso cref="Animate.From<TValue>"/>
		/// <seealso cref="ITweenGroup.From<TValue>"/>
		/*public static Tween<TValue> From<TValue>(
			string property, TValue fromValue, 
			params TweenPluginInfo[] plugins
		) {
			return Tween<TValue>.Create(TweenMethod.From, 
				null, float.NaN, property, 
				fromValue, default(TValue), default(TValue), 
				plugins
			);
		}*/

		/// <summary>
		/// Create a new From tween that will inherit the group's target.
		/// </summary>
		/// <param name='target'>
		/// The tween's target object which contains the property to tween.
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
		/// <seealso cref="Animate.From<TValue>"/>
		/// <seealso cref="ITweenGroup.From<TValue>"/>
		public static Tween<TTarget, TValue> From<TTarget, TValue>(
			TTarget target, string property, TValue fromValue, 
			params TweenPluginInfo[] plugins
		)
			where TTarget : class
		{
			return Tween<TTarget, TValue>.Create(TweenMethod.From, 
				target, float.NaN, property, 
				fromValue, default(TValue), default(TValue), 
				plugins
			);
		}

		/// <summary>
		/// Create a new From tween on the group.
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
		/// <seealso cref="Animate.From<TValue>"/>
		/// <seealso cref="ITweenGroup.From<TValue>"/>
		public static Tween<TTarget, TValue> From<TTarget, TValue>(
			TTarget target, float duration, string property, TValue fromValue, 
			params TweenPluginInfo[] plugins
		)
			where TTarget : class
		{
			return Tween<TTarget, TValue>.Create(TweenMethod.From, 
				target, duration, property, 
				fromValue, default(TValue), default(TValue), 
				plugins
			);
		}

		///////////////////
		// FromTo

		/// <summary>
		/// Create a new FromTo tween that will inherit the group's 
		/// target and duration.
		/// </summary>
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
		/// <seealso cref="Animate.FromTo<TValue>"/>
		/// <seealso cref="ITweenGroup.FromTo<TValue>"/>
		/*public static Tween<TValue> FromTo<TValue>(
			string property, TValue fromValue, TValue toValue, 
			params TweenPluginInfo[] plugins
		) {
			return Tween<TValue>.Create(TweenMethod.FromTo, 
				null, float.NaN, property, 
				fromValue, toValue, default(TValue), 
				plugins
			);
		}*/

		/// <summary>
		/// Create a new FromTo tween that will inherit the group's target.
		/// </summary>
		/// <param name='target'>
		/// The tween's target object which contains the property to tween.
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
		/// <seealso cref="Animate.FromTo<TValue>"/>
		/// <seealso cref="ITweenGroup.FromTo<TValue>"/>
		public static Tween<TTarget, TValue> FromTo<TTarget, TValue>(
			TTarget target, string property, TValue fromValue, TValue toValue, 
			params TweenPluginInfo[] plugins
		)
			where TTarget : class
		{
			return Tween<TTarget, TValue>.Create(TweenMethod.FromTo, 
				target, float.NaN, property, 
				fromValue, toValue, default(TValue), 
				plugins
			);
		}

		/// <summary>
		/// Create a new FromTo tween on the group.
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
		/// <seealso cref="Animate.FromTo<TValue>"/>
		/// <seealso cref="ITweenGroup.FromTo<TValue>"/>
		public static Tween<TTarget, TValue> FromTo<TTarget, TValue>(
			TTarget target, float duration, string property, TValue fromValue, TValue toValue, 
			params TweenPluginInfo[] plugins
		)
			where TTarget : class
		{
			return Tween<TTarget, TValue>.Create(TweenMethod.FromTo, 
				target, duration, property, 
				fromValue, toValue, default(TValue), 
				plugins
			);
		}

		///////////////////
		// By

		/// <summary>
		/// Create a new By tween that will inherit the group's 
		/// target and duration.
		/// </summary>
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
		/// <seealso cref="Animate.By<TValue>"/>
		/// <seealso cref="ITweenGroup.By<TValue>"/>
		/*public static Tween<TValue> By<TValue>(
			string property, TValue byValue, 
			params TweenPluginInfo[] plugins
		) {
			return Tween<TValue>.Create(TweenMethod.By, 
				null, float.NaN, property, 
				default(TValue), default(TValue), byValue, 
				plugins
			);
		}*/

		/// <summary>
		/// Create a new By tween that will inherit the group's target.
		/// </summary>
		/// <param name='target'>
		/// The tween's target object which contains the property to tween.
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
		/// <seealso cref="Animate.By<TValue>"/>
		/// <seealso cref="ITweenGroup.By<TValue>"/>
		public static Tween<TTarget, TValue> By<TTarget, TValue>(
			TTarget target, string property, TValue byValue, 
			params TweenPluginInfo[] plugins
		)
			where TTarget : class
		{
			return Tween<TTarget, TValue>.Create(TweenMethod.By, 
				target, float.NaN, property, 
				default(TValue), default(TValue), byValue, 
				plugins
			);
		}

		/// <summary>
		/// Create a new By tween on the group.
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
		/// <seealso cref="Animate.By<TValue>"/>
		/// <seealso cref="ITweenGroup.By<TValue>"/>
		public static Tween<TTarget, TValue> By<TTarget, TValue>(
			TTarget target, float duration, string property, TValue byValue, 
			params TweenPluginInfo[] plugins
		)
			where TTarget : class
		{
			return Tween<TTarget, TValue>.Create(TweenMethod.By, 
				target, duration, property, 
				default(TValue), default(TValue), byValue, 
				plugins
			);
		}
	}

	///////////////////
	// ITween

	/// <summary>
	/// Individual tween.
	/// </summary>
	/// <remarks>
	/// <para>
	/// There are several ways to create tweens. Use the methods on 
	/// <see cref="Animate"/> to create single tweens that use the global
	/// default options. Use <see cref="Animate.On"/> or 
	/// <see cref="Animate.Group"/> to create a group and then add tweens
	/// to the group using the methods on <see cref="ITweenGroup"/>, which 
	/// allows to set options on the group which will then apply to all 
	/// tweens in that group.
	/// </para>
	/// <para>
	/// You can also create tweens using the methods on <see cref="Tween"/>.
	/// Note that those tweens are not automtically registered with Animate,
	/// you'll have to add them to a group using <see cref="ITweenGroup.Add"/>.
	/// This allows to set individual tween options for tweens in a group.
	/// </para>
	/// <para>
	/// All the events of the tween will be cleared once it's being recycled,
	/// you normally don't have to unregister event handlers you register.
	/// </para>
	/// </remarks>
	public interface ITween : ITweenOptionsFluid<ITween>
	{
		///////////////////
		// Tween properties

		/// <summary>
		/// Method of the tween (To, From, FromTo or By).
		/// </summary>
		TweenMethod TweenMethod { get; }

		/// <summary>
		/// Target object the tweened property is located on.
		/// </summary>
		object Target { get; }

		/// <summary>
		/// Name of the target property on the target object.
		/// </summary>
		string Property { get; }

		/// <summary>
		/// Type of the target the value is tweened on.
		/// </summary>
		Type TargetType { get; }
		/// <summary>
		/// Type of tweened value of the tween.
		/// </summary>
		/// <remarks>
		/// A tween's type has to match the type of the target property.
		/// Make sure the value you supply when creating a tween has the
		/// same type as the property that's being tweened.
		/// </remarks>
		Type ValueType { get; }

		///////////////////
		// Tween State

		/// <summary>
		/// Current state of the tween.
		/// </summary>
		TweenState State { get; }

		/// <summary>
		/// When an error occured (<c>State</c> is set to <c>Error</c>), this
		/// property contains an error description.
		/// </summary>
		string Error { get; }
		/// <summary>
		/// Explains how the tween was completed.
		/// </summary>
		/// <remarks>
		/// This field is set as soon as thet tween's state becomes 
		/// <c>TweenState.Complete</c> and the Complete event fires.
		/// </remarks>
		/// <seealso cref="TweenEventArgs.CompletedBy"/>
		TweenCompletedBy CompletedBy { get; }

		/// <summary>
		/// The time the tween was created.
		/// </summary>
		float CreationTime { get; }
		/// <summary>
		/// The time the tween was started.
		/// </summary>
		float StartTime { get; }
		/// <summary>
		/// Time time the tween was started, in unscaled time (unaffected by
		/// <c>Time.timeScale</c>).
		/// </summary>
		float StartTimeUnscaled { get; }
		/// <summary>
		/// Time duration of the tween, in unscaled time (unaffected by
		/// <c>Time.timeScale</c>).
		/// </summary>
		/// <seealso cref="ITweenOptions.Duration"/>
		float DurationUnscaled { get; }

		///////////////////
		// Validate

		/// <summary>
		/// Trigger the validation of the tween and optionally force 
		/// it to be rendered.
		/// </summary>
		/// <remarks>
		/// <para>Usually tweens are validated during initialization in the frame
		/// after they were created. You can call this method to validate
		/// the tween right after creating it. This will make
		/// the stacktrace of validation errors more useful as it will point
		/// to where the tween was created instead of to the where it was 
		/// initialized.</para>
		/// <para>Tweens will first render (set their target property) when
		/// they are started, which is usually during the next frame after they
		/// were created or after their start delay. Especially when doing a From
		/// or FromTo tween you might want the initial value to be set 
		/// immediately to avoid visual glitches. Validate the tween and 
		/// set the <c>forceRender</c> parameter to true in this case.</para>
		/// </remarks>
		/// <param name='forceRender'>
		/// Forces the tween to render (set its target property to the initial 
		/// value) after it has been validated.
		/// </param>
		bool Validate(bool forceRender = false);

		///////////////////
		// Manage Tween

		/// <summary>
		/// Stop the tween (leave it at its current value).
		/// </summary>
		void Stop();

		/// <summary>
		/// Finish the tween (set it to its end value).
		/// </summary>
		void Finish();

		/// <summary>
		/// Cancel the tween (set it to its start value).
		/// </summary>
		void Cancel();

		///////////////////
		// Coroutines

		/// <summary>
		/// Create a coroutine that will wait until the tween has completed.
		/// </summary>
		/// <returns>
		/// A coroutine you can yield in one of your own to wait until
		/// the tween has completed.
		/// </returns>
		/// <seealso cref="ITweenOptionsFluid<TContainer>.WaitForTweenDuration"/>
		Coroutine WaitForEndOfTween();

		///////////////////
		// Internal

		// Internal tween methods
		ITweenInternal Internal { get; }
	}

}
