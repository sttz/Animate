using System;
using Sttz.Tweener.Core;

namespace Sttz.Tweener {

/// <summary>
/// EventArgs used by <see cref="TweenOptions"/> events.
/// </summary>
public class TweenEventArgs : EventArgs
{
	/// <summary>
	/// Tween instance that triggered the event.
	/// </summary>
	public Tween Tween { get; protected set; }

	/// <summary>
	/// Type of the event.
	/// </summary>
	public TweenEvent Event { get; protected set; }

	/// <summary>
	/// Reason the tween was completed (if <see cref="Event"/> is 
	/// <see cref="TweenEvent.Complete"/>).
	/// </summary>
	public TweenCompletedBy CompletedBy { get; protected set; }

	/// <summary>
	/// Error description (if <see cref="Event"/> is <see cref="TweenEvent.Error"/>).
	/// </summary>
	public string Error { get; protected set; }

	// Constructor
	public TweenEventArgs(
		Tween tween, 
		TweenEvent eventType, 
		string errorDescription = null,
		TweenCompletedBy completedBy = TweenCompletedBy.Undefined
	) {
		Tween = tween;
		Event = eventType;
		CompletedBy = completedBy;
		Error = errorDescription;
	}
}

}
