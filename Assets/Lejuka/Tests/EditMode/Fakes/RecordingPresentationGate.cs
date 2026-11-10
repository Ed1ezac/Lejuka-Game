using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Game;

namespace Lejuka.Tests.EditMode.Fakes
{
    /// <summary>An instant gate that keeps every event, so tests can assert on what was published and when.</summary>
    public sealed class RecordingPresentationGate : IPresentationGate
    {
        private readonly List<IGameEvent> _events = new List<IGameEvent>();

        public IReadOnlyList<IGameEvent> Events => _events;

        /// <summary>Called for each event as it is presented, e.g. to inspect state mid-match or to cancel.</summary>
        public Action<IGameEvent> OnPresent { get; set; }

        public IReadOnlyList<T> EventsOf<T>()
            where T : IGameEvent
        {
            return _events.OfType<T>().ToList();
        }

        public Task PresentAsync(IGameEvent gameEvent, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            _events.Add(gameEvent);
            OnPresent?.Invoke(gameEvent);

            return cancellationToken.IsCancellationRequested
                ? Task.FromCanceled(cancellationToken)
                : Task.CompletedTask;
        }
    }
}
