using System;

namespace Sttz.Tweener {

	/// <summary>
	/// EventArgs used by <see cref="ITweenOptions"/> events.
	/// </summary>
	public class TweenEventArgs : EventArgs
	{
		/// <summary>
		/// ITween instance that triggered the event.
		/// </summary>
		public ITween Tween { get; protected set; }

		/// <summary>
		/// Type of the event.
		/// </summary>
		public TweenEvent Event { get; protected set; }

		/// <summary>
		/// Reason the tween was completed (if <c>Event</c> is 
		/// <c>EventType.Complete</c>).
		/// </summary>
		public TweenCompletedBy CompletedBy { get; protected set; }

		/// <summary>
		/// Error description (if <c>Event</c> is <c>EventType.Error</c>).
		/// </summary>
		public string Error { get; protected set; }

		// Constructor
		public TweenEventArgs(
			ITween tween, 
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