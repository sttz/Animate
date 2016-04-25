using System;
using UnityEngine;

using Sttz.Tweener.Core;
namespace Sttz.Tweener {

	/// <summary>
	/// A container for a group of tweens.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A group can provide defaults for all tween options as well as
	/// the tween target and allows to control just that group of tweens.
	/// </para>
	/// <para>
	/// All the events of the group will be cleared once it's being recycled,
	/// you normally don't have to unregister event handlers you register.
	/// </para>
	/// </remarks>
	public interface ITweenGroup : ITweenOptionsFluid<ITweenGroup>
	{
		///////////////////
		// Add

		/// <summary>
		/// Add a custom tween instance.
		/// </summary>
		/// <remarks>
		/// In contrast to the fluid tween factories on <c>ITweenGroup</c>
		/// (e.g. To/From/FromTo/By) which do not allow to set tween-specific
		/// options, adding a tween created using the methods on <see cref="Tween"/>
		/// allows to override the group's options for an individual tween.
		/// </remarks>
		/// <example>
		/// <code>
		/// Animate.On(target)
		///     .Ease(Easing.QuadraticOut)
		///     // The tween created on the group inherits its easing
		///     .To(2f, "firstProperty", targetValue)
		///     // The tween added to the group can override the easing
		///     .Add(Tween.To(5f, "secondProperty", targetValue)
		///         .Ease(Easing.Linear)
		///     );
		/// </code>
		/// </example>
		/// <param name='tween'>
		/// <c>ITween</c> instance to add. Use the methods on <see cref="Tween"/> instead
		/// of instantiating <c>Tween&lt;TValue&gt;</c> directly.
		/// </param>
		ITweenGroup Add(ITween tween);

		///////////////////
		// To

		/// <summary>
		/// Create a new To tween on the group, inheriting the group's
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
		/// <seealso cref="Tween.To<TValue>"/>
		ITweenGroup To<TValue>(string property, TValue toValue, params TweenPluginInfo[] plugins);

		/// <summary>
		/// Create a new To tween on the group, inheriting the group's target.
		/// </summary>
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
		/// <seealso cref="Tween.To<TValue>"/>
		ITweenGroup To<TValue>(float duration, string property, TValue toValue, params TweenPluginInfo[] plugins);

		/// <summary>
		/// Create a new To tween on the group.
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
		/// <seealso cref="Tween.To<TValue>"/>
		ITweenGroup To<TValue>(object target, float duration, string property, TValue toValue, params TweenPluginInfo[] plugins);

		///////////////////
		// From

		/// <summary>
		/// Create a new From tween on the group, inheriting the group's 
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
		/// <seealso cref="Tween.From<TValue>"/>
		ITweenGroup From<TValue>(string property, TValue fromValue, params TweenPluginInfo[] plugins);

		/// <summary>
		/// Create a new From tween on the group, inheriting the group's target.
		/// </summary>
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
		/// <seealso cref="Tween.From<TValue>"/>
		ITweenGroup From<TValue>(float duration, string property, TValue fromValue, params TweenPluginInfo[] plugins);

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
		/// <seealso cref="Tween.From<TValue>"/>
		ITweenGroup From<TValue>(object target, float duration, string property, TValue fromValue, params TweenPluginInfo[] plugins);

		///////////////////
		// FromTo

		/// <summary>
		/// Create a new FromTo tween on the group, inheriting the group's 
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
		/// <seealso cref="Tween.FromTo<TValue>"/>
		ITweenGroup FromTo<TValue>(string property, TValue fromValue, TValue toValue, params TweenPluginInfo[] plugins);

		/// <summary>
		/// Create a new FromTo tween on the group, inheriting the group's target.
		/// </summary>
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
		/// <seealso cref="Tween.FromTo<TValue>"/>
		ITweenGroup FromTo<TValue>(float duration, string property, TValue fromValue, TValue toValue, params TweenPluginInfo[] plugins);

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
		/// <seealso cref="Tween.FromTo<TValue>"/>
		ITweenGroup FromTo<TValue>(object target, float duration, string property, TValue fromValue, TValue toValue, params TweenPluginInfo[] plugins);

		///////////////////
		// By

		/// <summary>
		/// Create a new By tween on the group, inheriting the group's 
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
		/// <seealso cref="Tween.By<TValue>"/>
		ITweenGroup By<TValue>(string property, TValue byValue, params TweenPluginInfo[] plugins);

		/// <summary>
		/// Create a new By tween on the group, inheriting the group's target.
		/// </summary>
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
		/// <seealso cref="Tween.By<TValue>"/>
		ITweenGroup By<TValue>(float duration, string property, TValue byValue, params TweenPluginInfo[] plugins);

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
		/// <seealso cref="Tween.By<TValue>"/>
		ITweenGroup By<TValue>(object target, float duration, string property, TValue byValue, params TweenPluginInfo[] plugins);

		///////////////////
		// Group properties

		/// <summary>
		/// Default target object of the group.
		/// </summary>
		object DefaultTarget { get; }

		/// <summary>
		/// Tpye of the default target object of the group.
		/// </summary>
		/// <remarks>
		/// This is the type of the generic parameter used when creating
		/// the group, not the actual type of the default target object.
		/// </remarks>
		Type DefaultTargetType { get; }

		///////////////////
		// Validate

		/// <summary>
		/// Trigger the validation of all the tweens in the group 
		/// and optionally force them to be rendered.
		/// </summary>
		/// <remarks>
		/// <para>Usually tweens are validated during initialization in the frame
		/// after they were created. You can call this method to validate
		/// all tweens in the group right after creating them. This will make
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
		/// Forces all tweens in the group to render (set its target property 
		/// to the initial value) after they have been validated.
		/// </param>
		bool Validate(bool forceRender = false);

		///////////////////
		// Has

		/// <summary>
		/// Check if the group contains any tweens (waiting or tweening).
		/// </summary>
		bool Has();

		/// <summary>
		/// Check if the group contains specific tweens.
		/// </summary>
		/// <param name='target'>
		/// Target object to look for.
		/// </param>
		/// <param name='property'>
		/// Target property to look for (set to null to look for any property
		/// on the target object).
		/// </param>
		bool Has(object target, string property = null);

		///////////////////
		// Stop, Finish & Cancel

		/// <summary>
		/// Stop all tweens in the group (leaving them at their current value),
		/// optionally limiting the tweens to a target object and property.
		/// </summary>
		/// <param name='target'>
		/// Only stop tweens on the target object (set to null to stop all
		/// tweens on all objects).
		/// </param>
		/// <param name='property'>
		/// Only stop tweens with the target property (set to null to stop all
		/// tweens on the target object).
		/// </param>
		void Stop(object target = null, string property = null);

		/// <summary>
		/// Finish all tweens in the group (setting them to their end value),
		/// optionally limiting the tweens to a target object and property.
		/// </summary>
		/// <param name='target'>
		/// Only finish tweens on the target object (set to null to stop all
		/// tweens on all objects).
		/// </param>
		/// <param name='property'>
		/// Only finish tweens with the target property (set to null to stop all
		/// tweens on the target object).
		/// </param>
		void Finish(object target = null, string property = null);

		/// <summary>
		/// Cancel all tweens in the group (setting them to their start value),
		/// optionally limiting the tweens to a target object and property.
		/// </summary>
		/// <param name='target'>
		/// Only cancel tweens on the target object (set to null to stop all
		/// tweens on all objects).
		/// </param>
		/// <param name='property'>
		/// Only cancel tweens with the target property (set to null to stop all
		/// tweens on the target object).
		/// </param>
		void Cancel(object target = null, string property = null);

		///////////////////
		// Coroutines

		/// <summary>
		/// Create a coroutine that will wait until all tweens in the
		/// group have completed (the group becomes empty).
		/// </summary>
		/// <remarks>
		/// Adding new tweens to the group while not all of its
		/// tweens have completed will prolong the time until the
		/// coroutine returns.
		/// </remarks>
		/// <returns>
		/// A coroutine you can yield in one of your own to wait until
		/// all tweens in the group have completed.
		/// </returns>
		Coroutine WaitForEndOfGroup();

		///////////////////
		// Internal

		// Internal tween group methods.
		ITweenGroupInternal Internal { get; }
	}

}
