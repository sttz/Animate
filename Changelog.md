# Changelog

### 3.1.0 (2025-01-19)
* Add async methods using Awaitable on Unity 2023.1+
* Add TweenGroup overloads with custom target but using the group's duration
* Add built-in accessor for Time.timeScale to the static plugin
* Fix tween not overwritten if overwriting tween is started in the same frame as it ends

### 3.0.5 (2024-05-11)
* Add a few more built-in accessors for the static plugin
* Fix compilation without "com.unity.ugui" package installed

### 3.0.4 (2023-06-18)
* Fix `WaitForTweenDuration` not respecting the tween's timing

### 3.0.2 (2022-04-30)
* Fix tweens failing with Error when target Unity Object is destroyed
  before the tween is initialized, instead of only logging a Debug message

### 3.0.1 (2022-03-15)
* Fix overwriting overlap check with tweens with different lengths
* Fix tweens getting started before their delay
* Fix tweens getting updated before their delay
* Add `package.json` for Unity package manager
* Fix documentation link

### 3.0.0 (2020-07-04)
* Initial release of version 3
