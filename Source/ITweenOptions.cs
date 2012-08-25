using System;
using UnityEngine;

using Sttz.Tweener.Core;
namespace Sttz.Tweener {

	/// <summary>
	/// Tween options.
	/// </summary>
	/// <remarks>
	/// Options in Animate can be set in different scopes. If an option
	/// is not set in a scope, it will inherit the settings from its parent
	/// scope. This allows to define global defaults and override them with
	/// various granularity in templates, groups or individual tweens.
	/// 
	/// <para>
	/// The available scopes are:
	/// <list type="bullet">
	/// <item>Global defaults at <see cref="P:Animate.Options"/></item>
	/// <item>Template created using <see cref="M:Animate.Template"/> (optional)</item>
	/// <item>Tween Groups created using <see cref="M:Animate.On"/>
	/// or <see cref="M:Animate.Group"/></item>
	/// <item>Tweens created using the methods on <see cref="Animate"/> 
	/// or <see cref="Tween"/></item>
	/// </list>
	/// </para>
	/// </remarks>
	public interface ITweenOptions
	{
		/// <summary>
		/// Recycling of tweens and groups.
		/// </summary>
		TweenRecycle Recycle { get; set; }

		/// <summary>
		/// Duration of the tween in seconds.
		/// </summary>
		float Duration { get; set; }
		/// <summary>
		/// Easing to apply to the tween. See <see cref="Easing"/> for
		/// possible values. <see cref="Easing.Linear"/> disables easing.
		/// </summary>
		EasingMethod Easing { get; set; }
		/// <summary>
		/// Timing options of the tween. This sets if the tween is
		/// updated during <c>Update</c>, <c>FixedUpdate</c> or <c>LateUpdate</c>
		/// and if it uses real time or not.
		/// </summary>
		/// <seealso cref="TweenTiming"/>
		TweenTiming TweenTiming { get; set; }
		/// <summary>
		/// Delay before the tween starts (in seconds).
		/// </summary>
		float StartDelay { get; set; }

		/// <summary>
		/// How conflicting tweens are resolved. See <see cref="TweenOverwrite"/>
		/// for all available options.
		/// </summary>
		TweenOverwrite OverwriteSettings { get; set; }

		/// <summary>
		/// Event triggered when the tween initializes (usually in the frame
		/// after it was created or when calling <see cref="M:ITween.Validate"/>.)
		/// </summary>
		event EventHandler<TweenEventArgs> InitializeEvent;
		/// <summary>
		/// Event triggered when the tween starts (e.g. after the start delay).
		/// </summary>
		event EventHandler<TweenEventArgs> StartEvent;
		/// <summary>
		/// Event triggered every time the tween updates.
		/// </summary>
		event EventHandler<TweenEventArgs> UpdateEvent;
		/// <summary>
		/// Event triggered when the tween completes (either normally or
		/// by being stopped or overwritten, check 
		/// <see cref="TweenEventArgs.CompletedBy"/> if you need to know
		/// what completed the tween.
		/// </summary>
		event EventHandler<TweenEventArgs> CompleteEvent;
		/// <summary>
		/// Triggered when an error occurs. <see cref="TweenEventArgs.Error"/>
		/// and <see cref="ITween.Error"/> contain a description of the error.
		/// </summary>
		event EventHandler<TweenEventArgs> ErrorEvent;

		/// <summary>
		/// Default plugin used if no other plugin is used or automatically
		/// activated.
		/// </summary>
		TweenPluginInfo DefaultPlugin { get; set; }

		/// <summary>
		/// Enable a plugin's automatic activation. The plugin will try
		/// to autodetect when it's needed and activate automatically.
		/// Not all plugins might support automatic activation. Usually
		/// plugins provide a <c>Automatic</c> method if they do.
		/// </summary>
		/// <param name='plugin'>
		/// Plugin to enable automatic activation for. Use the plugin's
		/// <c>Automatic</c> method, e.g. <see cref="M:TweenSctruct.Automatic"/>.
		/// </param>
		/// <param name='enable'>
		/// Set to false to disable a plugin's automatic activation.
		/// </param>
		void SetAutomatic(TweenPluginInfo plugin, bool enable = true);

		/// <summary>
		/// Log level of the current scope. Only messages with equal
		/// or higher level will be logged to the console.
		/// </summary>
		TweenLogLevel LogLevel { get; set; }

		/// <summary>
		/// Internal methods.
		/// </summary>
		ITweenOptionsInternal Internal { get; }
	}

	/// <summary>
	/// Fluid interface to set <see cref="ITweenOptions"/> settings.
	/// </summary>
	/// <remarks>
	/// Groups and tweens implement a fluid interface to set <c>ITweenOption</c>
	/// settings in addition to the properties on <c>ITweenOption</c> itself.
	/// This allows to set any number of option in a single instruction:
	/// <code>Animate.Group().Over(5f).Delay(3f).Ease(Easing.Linear);</code>
	/// </remarks>
	public interface ITweenOptionsFluid<TContainer>
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
		// Properties

		/// <summary>
		/// Duration of the tween (in seconds).
		/// </summary>
		TContainer Over(float duration);
		/// <summary>
		/// Easing to apply to the tween. See <see cref="Easing"/> for
		/// possible values. <see cref="Easing.Linear"/> disables easing.
		/// </summary>
		TContainer Ease(EasingMethod easing);
		/// <summary>
		/// Timing options of the tween. This sets if the tween is
		/// updated during <c>Update</c>, <c>FixedUpdate</c> or <c>LateUpdate</c>
		/// and if it uses real time or not.
		/// </summary>
		/// <seealso cref="TweenTiming"/>
		TContainer Timing(TweenTiming timing);
		/// <summary>
		/// Delay before the tween starts (in seconds).
		/// </summary>
		TContainer Delay(float seconds);
		/// <summary>
		/// How conflicting tweens are resolved. See <see cref="TweenOverwrite"/>
		/// for all available options.
		/// </summary>
		TContainer Overwrite(TweenOverwrite settings);
		/// <summary>
		/// Enable or disable recycling of tweens or groups.
		/// </summary>
		TContainer Recycle(TweenRecycle recycle);

		///////////////////
		// Events

		/// <summary>
		/// Event triggered when the tween initializes (usually in the frame
		/// after it was created or when calling <see cref="M:ITween.Validate"/>.)
		/// </summary>
		TContainer OnInitialize(EventHandler<TweenEventArgs> handler);
		/// <summary>
		/// Event triggered when the tween starts (e.g. after the start delay).
		/// </summary>
		TContainer OnStart(EventHandler<TweenEventArgs> handler);
		/// <summary>
		/// Event triggered every time the tween updates.
		/// </summary>
		TContainer OnUpdate(EventHandler<TweenEventArgs> handler);
		/// <summary>
		/// Event triggered when the tween completes (either normally or
		/// by being stopped or overwritten, check 
		/// <see cref="TweenEventArgs.CompletedBy"/> if you need to know
		/// what completed the tween.
		/// </summary>
		TContainer OnComplete(EventHandler<TweenEventArgs> handler);
		/// <summary>
		/// Triggered when an error occurs. <see cref="TweenEventArgs.Error"/>
		/// and <see cref="ITween.Error"/> contain a description of the error.
		/// </summary>
		TContainer OnError(EventHandler<TweenEventArgs> handler);

		///////////////////
		// Plugins

		/// <summary>
		/// Enable a plugin's automatic activation. The plugin will try
		/// to autodetect when it's needed and activate automatically.
		/// Not all plugins might support automatic activation. Usually
		/// plugins provide a <c>Automatic</c> method if they do.
		/// </summary>
		/// <param name='plugin'>
		/// Plugin to enable automatic activation for. Use the plugin's
		/// <c>Automatic</c> method, e.g. <see cref="M:TweenSctruct.Automatic"/>.
		/// </param>
		/// <param name='enable'>
		/// Set to false to disable a plugin's automatic activation.
		/// </param>
		TContainer Automate(TweenPluginInfo plugin, bool enable = true);

		///////////////////
		// Coroutines

		/// <summary>
		/// Return a <c>WaitForSeconds</c> instruction that can be used
		/// in Unity coroutines to wait for the duration of the tween
		/// (delay + duration).
		/// </summary>
		/// <see cref="M:ITween.WaitForEndOfTween"/>
		WaitForSeconds WaitForTweenDuration();

		///////////////////
		// Debugging

		/// <summary>
		/// Log level of the current scope. Only messages with equal
		/// or higher level will be logged to the console.
		/// </param>
		TContainer LogLevel(TweenLogLevel level);
	}

	/// <summary>
	/// Set of tween options that can be used as a group's defaults.
	/// </summary>
	/// <remarks>
	/// You can create a tween template using <see cref="Animate.Template"/>
	/// and then use it when creating groups with <see cref="Animate.On"/>
	/// or <see cref="Animate.Group"/>. The template will override the global
	/// options but the group's and tween's options can still override the
	/// template's.
	/// </remarks>
	public interface ITweenTemplate : ITweenOptionsFluid<ITweenTemplate> { }

}