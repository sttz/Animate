using System;

using Sttz.Tweener.Core;
namespace Sttz.Tweener {

	///////////////////
	// Options

	/// <summary>
	/// The the tweening method.
	/// </summary>
	public enum TweenMethod
	{
		/// <summary>
		/// Tween from the property's current value to the given value.
		/// </summary>
		To,
		/// <summary>
		/// Tween from the given value to the property's current value.
		/// </summary>
		From,
		/// <summary>
		/// Tween from the first to the second given value.
		/// </summary>
		FromTo,
		/// <summary>
		/// Tween from the property's current value by the given value.
		/// </summary>
		By
	}

	/// <summary>
	/// Tween overwriting automatically stops existing tweens and avoids
	/// conflicting tweens to be created.
	/// </summary>
	/// <remarks>
	/// The <c>TweenOverwrite</c> enumeration holds different flag options
	/// that can be combined to define the exact overwrite behavior.
	/// E.g. <c>TweenOverwrite.Default</c> is actually 
	/// <c>(OnStart | Stop | Overlapping)</c>.
	/// <para>
	/// Some fields in TweenOverwrite are mutually exclusive:
	/// <list type="bullet">
	/// <item><c>OnInitialize</c> and <c>OnStart</c></item>
	/// <item><c>Stop</c>, <c>Finish</c> and <c>Cancel</c></item>
	/// <item><c>All</c> and <c>Overlapping</c></item>
	/// </list>
	/// </para>
	/// </remarks>
	[Flags]
	public enum TweenOverwrite
	{
		/// <summary>
		/// Undefined overwriting. The parent scope's overwrite setting
		/// will be used in this case.
		/// </summary>
		Undefined = 0,

		/// <summary>
		/// Turn off overwriting. You'll have to manually call <see cref="Animate.Stop"/>,
		/// <see cref="Animate.Finish"/> or <see cref="Animate.Cancel"/> to
		/// avoid conflicting tweens.
		/// </summary>
		None = -1,

		/// <summary>
		/// Overwrite during initialization. This is usually during the next frame
		/// after the tween was created or when <see cref="ITween.Validate"/> is
		/// called.
		/// </summary>
		OnInitialize = 1,
		/// <summary>
		/// Overwrite when starting the tween. The tween is started before it 
		/// begins animating.
		/// </summary>
		OnStart = 2,

		/// <summary>
		/// Stop tweens that are being overwritten, leaving them at their
		/// current value.
		/// </summary>
		Stop = 4,
		/// <summary>
		/// Finish tweens that are being overwritten, setting them to their
		/// target value.
		/// </summary>
		Finish = 8,
		/// <summary>
		/// Cancel tweens that are being overwritten, setting them to their
		/// initial value.
		/// </summary>
		Cancel = 16,

		/// <summary>
		/// Overwrite all tweens with the same target and property.
		/// </summary>
		All = 32,
		/// <summary>
		/// Overwrite tweens with the same target and property that are
		/// actually overlapping the current tween. E.g. a tween with a 
		/// start delay that doesn't interfere with the current tween is
		/// not overwritten.
		/// </summary>
		Overlapping = 64,

		/// <summary>
		/// Default overwrite preset. Overwrite on start, stopping
		/// all overlapping tweens.
		/// </summary>
		Default = OnStart | Stop | Overlapping,
		/// <summary>
		/// Immediate overwrite preset. Overwrite on initialize, stopping
		/// all tweens with the same target and property.
		/// </summary>
		Immediate = OnInitialize | Stop | All
	}

	/// <summary>
	/// Tween timing defines when a tween is udpated and what time
	/// it's being calcualted with.
	/// </summary>
	[Flags]
	public enum TweenTiming
	{
		/// <summary>
		/// Undefined timing. The parent scope's timing is used in this case.
		/// </summary>
		Undefined = 0,

		/// <summary>
		/// Update the tween during Unity's update loop (best for visuals).
		/// </summary>
		/// <remarks>
		/// Only one of <see cref="Update"/>, <see cref="FixedUpdate"/> or
		/// <see cref="LateUpdate"/> can be set at a time.
		/// </remarks>
		Update = 2,
		/// <summary>
		/// Update the tween during Unity's fixed update loop 
		/// (best when animating properties interacting with physics).
		/// </summary>
		/// <remarks>
		/// Only one of <see cref="Update"/>, <see cref="FixedUpdate"/> or
		/// <see cref="LateUpdate"/> can be set at a time.
		/// </remarks>
		FixedUpdate = 4,
		/// <summary>
		/// Update the tween during Unity's late update loop
		/// (best for overwriting other animations).
		/// </summary>
		/// <remarks>
		/// Only one of <see cref="Update"/>, <see cref="FixedUpdate"/> or
		/// <see cref="LateUpdate"/> can be set at a time.
		/// </remarks>
		LateUpdate = 8,

		/// <summary>
		/// Use Unity's default time (i.e. <c>Time.time</c>). Tweens running with
		/// the default time are affected by <c>Time.timeScale</c>.
		/// </summary>
		/// <remarks>
		/// Only one of <see cref="DefaultTime"/>, <see cref="UnscaledTime"/> 
		/// or <see cref="RealTime"/> can be set at a time.
		/// </remarks>
		DefaultTime = 16,
		/// <summary>
		/// Use Unity's unscaled time (i.e. <c>Time.unscaledTime</c>. Tweens
		/// running with unscaled time are unaffected by <c>Time.timeScale</c>.
		/// </summary>
		/// <remarks>
		/// Only one of <see cref="DefaultTime"/>, <see cref="UnscaledTime"/> 
		/// or <see cref="RealTime"/> can be set at a time.
		/// </remarks>
		UnscaledTime = 32,
		/// <summary>
		/// Use Unity's real time (i.e. <c>Time.realTimeSinceStartup</c>. Tweens
		/// running with real time are unaffected by <c>Time.timeScale</c> or 
		/// Unity being paused in the background.
		/// </summary>
		/// <remarks>
		/// Only one of <see cref="DefaultTime"/>, <see cref="UnscaledTime"/> 
		/// or <see cref="RealTime"/> can be set at a time.
		/// </remarks>
		RealTime = 64,

		/// <summary>
		/// Default preset. Update tweens during the update loop and use 
		/// the default time.
		/// </summary>
		Default = Update | DefaultTime,
		/// <summary>
		/// Physics preset. Update tweens during the fixed update loop and use
		/// the default time.
		/// </summary>
		Physics = FixedUpdate | DefaultTime,
		/// <summary>
		/// Menu preset. Update tweens during the update loop and use unscaled time.
		/// This allows tweening while the game is paused by seetting 
		/// <c>Time.timeScale</c> to <c>0</c>.
		/// </summary>
		Menu = Update | UnscaledTime
	}

	/// <summary>
	/// Recycling of tweens and groups.
	/// </summary>
	[Flags]
	public enum TweenRecycle
	{
		/// <summary>
		/// Undefined recycling behavior. The parent scope's recycling
		/// setting is used in this case.
		/// </summary>
		Undefined = 0,

		/// <summary>
		/// Don't recycle.
		/// </summary>
		None = -1,

		/// <summary>
		/// Recycle tweens.
		/// </summary>
		Tweens = 2,
		/// <summary>
		/// Recycle groups.
		/// </summary>
		Groups = 4,

		/// <summary>
		/// Recycle both tween and groups.
		/// </summary>
		All = Tweens | Groups,
	}

	/// <summary>
	/// Log level. Only messages with an equal or higher log level
	/// than the current log level will be logged to the console.
	/// </summary>
	public enum TweenLogLevel
	{
		/// <summary>
		/// Undefined log level. The parent scope's log level is used
		/// in this case.
		/// </summary>
		Undefined,
		/// <summary>
		/// Log debug messages, warnings and errors.
		/// </summary>
		Debug,
		/// <summary>
		/// Log warnings and errors.
		/// </summary>
		Warning,
		/// <summary>
		/// Only log errors.
		/// </summary>
		Error,
		/// <summary>
		/// Don't log anything (not recommended).
		/// </summary>
		Silent
	}

	///////////////////
	// Tween

	/// <summary>
	/// States of a tween.
	/// </summary>
	public enum TweenState
	{
		/// <summary>
		/// The tween has not yet been set up using it's <see cref="M:Tween{T}.Use"/> 
		/// method. You should only see tweens in this state if you create an
		/// instance or load one from the <see cref="TweenPool"/>. Usually,
		/// you want to use the methods on <see cref="Animate"/>, 
		/// <see cref="TweenGroup"/> or <see cref="Tween"/> to create instances.
		/// </summary>
		Unused,
		/// <summary>
		/// A not yet initialized tweens. The tween has been set up with its
		/// parameters but not yet initialized. Tweens are initialized during
		/// the next frame after their creation or by calling 
		/// <see cref="ITween.Validate"/>.
		/// </summary>
		Uninitialized,
		/// <summary>
		/// The tween is waiting to start, e.g. because 
		/// <see cref="ITweenOptions.StartDelay"/> has been set.
		/// </summary>
		Waiting,
		/// <summary>
		/// The tween is currently animating.
		/// </summary>
		Tweening,
		/// <summary>
		/// The tween has completed, either normally or by being stopped,
		/// finished or cancelled.
		/// </summary>
		Complete,
		/// <summary>
		/// The tween has encountered an error. See <see cref="ITween.Error"/>
		/// for the error message.
		/// </summary>
		Error
	}

	/// <summary>
	/// Tween event type.
	/// </summary>
	/// <seealso cref="TweenEventArgs.Event"/>
	public enum TweenEvent
	{
		/// <summary>
		/// Initialization event.
		/// </summary>
		/// <seealso cref="ITweenOptions.InitializeEvent"/>
		Initialize,
		/// <summary>
		/// Start event.
		/// </summary>
		/// /// <seealso cref="ITweenOptions.StartEvent"/>
		Start,
		/// <summary>
		/// Update event.
		/// </summary>
		/// <seealso cref="ITweenOptions.UpdateEvent"/>
		Update,
		/// <summary>
		/// Complete event.
		/// </summary>
		/// <seealso cref="ITweenOptions.CompleteEvent"/>
		Complete,
		/// <summary>
		/// Error event.
		/// </summary>
		/// <seealso cref="ITweenOptions.ErrorEvent"/>
		Error
	}

	/// <summary>
	/// How a tween was completed.
	/// </summary>
	/// <remarks>
	/// <para>This is a flags enumeration and some of its fields can
	/// occur in combination, e.g. if the tween was stopped, finished or
	/// canceled, the value can also contain the <c>Overwrite</c> flag
	/// to indicate the completion was caused by the tween being overwritten.</para>
	/// <para>To properly check for a flag in the <c>CompletedBy</c> value
	/// you need to use binary-and:</para>
	/// <code>
	/// if ((tween.CompletedBy & TweenCompletedBy.Stop) > 0) {
	///     // Tween was stopped (overwritten or not)
	/// }
	/// if ((tween.CompletedBy & TweenCompletedBy.Overwrite) > 0) {
	///     // Tween was overwritten (stopped, finished or cancelled)
	/// }
	/// </code>
	/// </remarks>
	/// <seealso cref="TweenEventArgs.CompletedBy"/>
	/// <seealso cref="ITween.CompletedBy"/>
	[Flags]
	public enum TweenCompletedBy
	{
		/// <summary>
		/// Undefined value.
		/// </summary>
		/// <remarks>
		/// The value has not yet been set, e.g. because the tween
		/// has not been completed yet.
		/// </remarks>
		Undefined = 0,

		/// <summary>
		/// Tween was stopped (left at its current value).
		/// </summary>
		Stop = 1,
		/// <summary>
		/// Tween was finished (set to its end value).
		/// </summary>
		Finish = 2,
		/// <summary>
		/// Tween was canceled (set to its start value).
		/// </summary>
		Cancel = 4,
		/// <summary>
		/// Tween completed regularly by reaching its end.
		/// </summary>
		Complete = 8,

		/// <summary>
		/// Tween was overwritten (always in combination with 
		/// <c>Stop</c>, <c>Finish</c> or <c>Cancel</c>).
		/// </summary>
		Overwrite = 16
	}

	///////////////////
	// Easing

	/// <summary>
	/// Easing type enumeration, e.g. for use in the Unity editor.
	/// </summary>
	/// <seealso cref="Easing.EasingForType"/>
	public enum EasingType
	{
		Linear,
		Quadratic,
		Cubic,
		Quartic,
		Quintic,
		Sinusoidal,
		Exponential,
		Circular,
		Back,
		Bounce,
		Elastic
	}

	/// <summary>
	/// Easing direction enumeration, e.g. for use in the Unity editor.
	/// </summary>
	/// <seealso cref="Easing.EasingForType"/>
	public enum EasingDirection
	{
		In,
		Out,
		InOut
	}

}
