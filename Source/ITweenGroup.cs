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
	public interface ITweenGroup
	{
		///////////////////
		// Options

		/// <summary>
		/// Access the tween's options. All options can also be set 
		/// using the fluid interface methods on 
		/// <see cref="ITweenOptionsFluid<TContainer>"/>.
		/// </summary>
		ITweenOptions Options { get; }

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
