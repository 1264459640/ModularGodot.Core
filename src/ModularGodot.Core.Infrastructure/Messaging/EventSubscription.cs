using System;

namespace ModularGodot.Core.Infrastructure.Messaging
{
    /// <summary>
    /// Represents a single event subscription with metadata and disposal tracking
    /// as part of the enhanced event bus system.
    /// </summary>
    public class EventSubscription : IDisposable
    {
        private bool _disposed = false;

        /// <summary>
        /// Unique identifier for the subscription
        /// </summary>
        public string SubscriptionId { get; }

        /// <summary>
        /// The underlying disposable resource for this subscription
        /// </summary>
        public IDisposable Disposable { get; }

        /// <summary>
        /// Indicates whether this is a one-time subscription that automatically disposes after first event
        /// </summary>
        public bool IsOneTime { get; }

        /// <summary>
        /// The type of event being subscribed to
        /// </summary>
        public Type EventType { get; }

        /// <summary>
        /// Current state of the subscription
        /// </summary>
        public bool IsActive => !_disposed;

        /// <summary>
        /// Creates a new EventSubscription instance
        /// </summary>
        /// <param name="subscriptionId">Unique identifier for the subscription</param>
        /// <param name="disposable">The underlying disposable resource</param>
        /// <param name="isOneTime">Whether this is a one-time subscription</param>
        /// <param name="eventType">The type of event being subscribed to</param>
        public EventSubscription(string subscriptionId, IDisposable disposable, bool isOneTime, Type eventType)
        {
            SubscriptionId = subscriptionId ?? throw new ArgumentNullException(nameof(subscriptionId));
            Disposable = disposable ?? throw new ArgumentNullException(nameof(disposable));
            IsOneTime = isOneTime;
            EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
        }

        /// <summary>
        /// Disposes of the subscription and its underlying resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected dispose method to allow for proper cleanup
        /// </summary>
        /// <param name="disposing">True if called from Dispose(), false if called from finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Dispose of managed resources
                try
                {
                    Disposable?.Dispose();
                }
                catch
                {
                    // If there's an issue disposing the underlying resource, log it but continue
                    // This is to ensure proper state cleanup regardless
                }
            }

            _disposed = true;
        }

        /// <summary>
        /// Called when the subscription has been used once and needs to be marked for disposal
        /// </summary>
        public void MarkForAutoDisposal()
        {
            if (_disposed) return;

            if (IsOneTime)
            {
                Dispose();
            }
        }
    }
}