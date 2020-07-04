# Animate

Animate is a high-performance generic tweening engine written in C#
and optimized to use in the Unity game engine.

### Introduction

What separates Animate from most other C# tweening engines is that it
uses generics for the main tweening operations. This avoids expensive
boxing of value types and casting when accessing the properties.

Animate also recycles its objects and uses values types where possible
to avoid creating work for the garbage collector.

There exist three main tweening modes, that can be used depending on
the target platform:
- Code generation creates accessors and arithmetic operations dynamically
  as needed. This is the most convenient and pretty fast but takes
  more time for warm up and is not available on AOT (IL2CPP) platforms
  or with .Net Standard.
- Reflection dynamically accesses properties and arithmetic operators.
  This is convenient but slow and mostly useful during development.
- Static mode uses pre-defined and user-provided callbacks for accessing
  properties and arithmetics. This is very fast but requires a bit
  of set up per property and arithmetic type.

Animate provides a plugin system to extend the basic behavior. All 
the modes described above are implemented as plugins as well.

### Usage

The quickest way to create a tween using Animate are the 
[Animate.To](xref:Sttz.Tweener.Animate.To*), [Animate.From](xref:Sttz.Tweener.Animate.From*), 
[Animate.FromTo](xref:Sttz.Tweener.Animate.FromTo*) and [Animate.By](xref:Sttz.Tweener.Animate.By*) 
methods. Those methods create a single tween that will inherit the global
options.

The most flexible way to create a tween is the [Animate.On](xref:Sttz.Tweener.Animate.On*)
method. It returns a group with a default target, allows to create
multiple tweens in one go and define options that apply to all of those tweens.

```cs
// Shortest tween invocation, using global options:
Animate.To(transform, 5f, "position", Vector3.one);

// Override options for that tween:
Animate.To(transform, 5f, "position", Vector3.one)
    .Ease(Easing.QuadraticOut)
    .OnComplete((args) => {
        Debug.Log("Tween completed!");
    });

// Create a group, options apply to all tweens in the group:
Animate.On(transform).Over(5f)
    .To("position", Vector3.one)
    .To("rotation", Quaternion.identity)
    .To(3f, "localScale", Vector3.one)
    .Ease(Easing.QuinticInOut);

// Set options for an individual tween in the group:
Animate.On(transform).Over(5f)
    .To("position", Vector3.one)
    .To("rotation", Quaternion.identity, 
        t => t.Ease(Easing.Linear)
    )
    .Ease(Easing.QuinticInOut);
```

### Options

A tween's options can be set on the [TweenOptions](xref:Sttz.Tweener.TweenOptions) class. Options
are stacked, allowing to set an option on a global, template, group or tween's
level. Options in a lower level override their parent's options.

- **Global options**: [Animate.Options](xref:Sttz.Tweener.Animate.Options)
- **Template options**: Create a template using [Animate.Template](xref:Sttz.Tweener.Animate.Template)
  and then use it with [Animate.On](xref:Sttz.Tweener.Animate.On*) and [Animate.Group](xref:Sttz.Tweener.Animate.Group*).
- **Group options**: Groups extend [TweenOptionsContainer.Options](xref:Sttz.Tweener.TweenOptionsContainer.Options).
- **Tween options**: Tweens extend [TweenOptionsContainer.Options](xref:Sttz.Tweener.TweenOptionsContainer.Options).

For Templates, Groups and Tweens, [TweenOptionsFluid](xref:Sttz.Tweener.TweenOptionsFluid) implements
the fluid interface for setting options.

Events are also part of the options stack, meaning that events bubble up the stack
and e.g. listening for global or a group's [TweenOptions.ErrorEvent](xref:Sttz.Tweener.TweenOptions.ErrorEvent)
will trigger for any tween error globally or inside the group.

Events for groups and tweens are reset once the group is recycled or the tween
completes. Therefore, it's typically not necessary to unregister event handlers.

> [!TIP]
> A lot of Animate's options use flags that can be combined to define
> the exact behavior. Most of those options also provide default combinations
> that should cover most use cases. To combine different flags to create
> your own combination, use the binary-or operator, e.g:
> `Animate.Options.OverwriteSettings = 
>     TweenOverwrite.OnStart | TweenOverwrite.Finish | TweenOverwrite.Overlapping;`

### Recycling

Groups and tweens will be recycled by default to reduce heap memory pressure.
This means you can't reuse groups or tweens and should typically just create
new groups or tweens (which will use recycled instances).

Note that groups from [Animate.Group](xref:Sttz.Tweener.Animate.Group*) and templates are not 
recycled ([TweenRecycle.Groups](xref:Sttz.Tweener.TweenRecycle.Groups) is unset on 
[TweenOptions.Recycle](xref:Sttz.Tweener.TweenOptions.Recycle)) to allow creating and reusing groups with
custom settings.

If you want to hold on to a tween or group, there are two main options:
- Increase the group's or tween's [TweenOptionsContainer.RetainCount](xref:Sttz.Tweener.TweenOptionsContainer.RetainCount)
  until you don't need it anymore and then decrease it again. This will
  prevent it from being recycled for that duration.
- Set [TweenOptions.Recycle](xref:Sttz.Tweener.TweenOptions.Recycle). This allows to disable recycling for 
  groups and/or tweens on any level (global, group, tween).
- Set [ITweenEngine.Pool](xref:Sttz.Tweener.Core.ITweenEngine.Pool) to `null` to disable recycling completely.

> [!WARNING]
> You need to increase the retain count when you e.g. wait for a tween to
> complete by checking it repeatedly. If not retained, the tween will be 
> recycled immediately on completion and the check will return invalid information.

### Default Plugins

Animate provides three sets of default plugins, each with different convenience,
speed and compatibility tradeoff:
- **Static**: The static plugins use no reflection, are performant but require
  some setup per tweened property or tween type.
- **Reflection**: The reflection plugin is relatively slow but works on 
  AOT/IL2CPP platforms.
- **Codegen**: The codegen reflection plugin is fast but only works on non-AOT
  platforms and not on .Net Standard.

By default, reflection is disabled and only the static plugin used. Animate 
comes with static support for many Unity properties and types built-in but
will have to enable support for custom properties, types or ones not covered
by the default support.

To enable reflection, set the `ANIMATE_REFLECTION` compilation define (.e.g. 
in Unity's player settings). This will enable codegen where possible and fall
back to plain reflection on other platforms.

To extend support of the static plugin, use the [Animate.EnableAccess](xref:Sttz.Tweener.Animate.EnableAccess*)
and [Animate.EnableArithmetic](xref:Sttz.Tweener.Animate.EnableArithmetic*) methods to enable access to a
property or tweening a type respectively.

```cs
class Example {
	public float field;
}

// Enable tweening "field" on the class "Example"
Animate.EnableAccess("field",
	(Example t) => t.field,
	(t, v) => t.field = v);

// Add support for tweening long
Animate.EnableArithmetic<long>(
    (start, end) => end - start,
    (start, diff) => start + diff,
    (start, end, diff, position) => start + (long)(diff * (double)position)
);
```

### Plugins

There are two main ways to use plugins, enabling them for automatic loading
on any options level or requiring them on a single tween.

The first allows the plugin to check if it's needed and load itself automatically
if that's the case. If an automatic plugin does not load, it fails silently.

Loading a plugin on a single tween, however, requires the plugin to be used
and an error will be raised if the plugin fails to load.

```cs
// Enable a plugin globally
Animate.Options.EnablePlugin(TweenRigidbody.Load);

// Enable a plugin for a group
Animate.On(transform).Over(5f)
	.EnablePlugin(TweenRigidbody.Load);
	.To("position", Vector3.one);

// Requiring a plugin for a tween (will raise an error if it fails)
Animate.To(transform, 5f, "position", Vector3.one)
	.PluginRigidbody();
```

The order in which plugins are enabled matters, if multiple plugins
could be used. In case of conflict, the plugin loaded last will be
used.

Plugins can also be re-enabled or re-disabled on the different options levels,
i.e. a plugin can be enabled globally but disabled for a specific group.

**Bundled Plugins:**
* [TweenFollow](xref:Sttz.Tweener.TweenFollow): Tween position relative to another moving Transform
* [TweenMaterial](xref:Sttz.Tweener.TweenMaterial): Tween properties of materials
* [TweenRigidbody](xref:Sttz.Tweener.TweenRigidbody): Properly tween Rigidbodies to avoid physics instabilities
* [TweenSlerp](xref:Sttz.Tweener.TweenSlerp): Tween rotations using Slerp (shortest distance)
* [TweenStruct](xref:Sttz.Tweener.TweenStruct): Tween members of structs (uses codegen)
