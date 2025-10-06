using System;
using R3;
using ModularGodot.Core.Contracts.Abstractions.Messaging;

namespace ModularGodot.Core.Infrastructure.Messaging
{
    /// <summary>
    /// Represents a thread-safe topic subject for a specific event type
    /// with proper disposal handling and lock protection as part of the enhanced event bus system.
    /// </summary>
    /// <typeparam name="T">The type of event this subject handles</typeparam>
    public class TopicSubject<T> : IDisposable where T : EventBase
    {
        private Subject<T> _subject;
        private bool _disposed = false;
        private readonly object _lock = new object();

        /// <summary>
        /// Gets whether this TopicSubject has been disposed
        /// </summary>
        public bool Disposed => _disposed;

        /// <summary>
        /// Creates a new TopicSubject for the specified event type
        /// </summary>
        public TopicSubject()
        {
            _subject = new Subject<T>();
        }

        /// <summary>
        /// Publishes an event to all subscribers
        /// </summary>
        /// <param name="event">The event to publish</param>
        public void OnNext(T @event)
        {
            lock (_lock)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(TopicSubject<T>));
                _subject?.OnNext(@event);
            }
        }

        /// <summary>
        /// Gets an observable for this subject that subscribers can use to listen for events
        /// </summary>
        /// <returns>An observable for this subject</returns>
        public Observable<T> GetObservable()
        {
            lock (_lock)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(TopicSubject<T>));
                return _subject.AsObservable();
            }
        }

        /// <summary>
        /// Disposes the subject and all its resources
        /// </summary>
        public void Dispose()
        {
            lock (_lock)
            {
                if (!_disposed)
                {
                    _disposed = true;
                    _subject?.OnCompleted();
                    _subject?.Dispose();
                    _subject = null;
                }
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Ensures the subject is properly disposed when garbage collected
        /// </summary>
        ~TopicSubject()
        {
            Dispose(false);
        }

        /// <summary>
        /// Internal method to dispose the subject without entering the lock,
        /// used for finalization purposes
        /// </summary>
        /// <param name="disposing">True if called from explicit disposal, false if called from finalizer</param>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Only dispose managed resources in the managed disposal context
                    lock (_lock)
                    {
                        _subject?.OnCompleted();
                        _subject?.Dispose();
                        _subject = null;
                    }
                }
                else
                {
                    // When called from finalizer, only deal with resources that don't require managed objects
                    _subject?.OnCompleted();
                    _subject = null;
                }

                _disposed = true;
            }
        }
    }
}