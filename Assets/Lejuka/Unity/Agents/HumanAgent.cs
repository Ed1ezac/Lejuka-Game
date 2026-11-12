using System;
using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Cards;
using Lejuka.Contracts.Players;
using UnityEngine;

namespace Lejuka.Unity.Agents
{
    /// <summary>
    /// The agent for a person at the table. Each request waits until the UI submits an answer.
    /// It never checks the answer: the domain validates every decision.
    /// </summary>
    public sealed class HumanAgent : IPlayerAgent, IHumanInputSource
    {
        // One answer slot for all three decisions: shuffle ignores the value, cut and rank carry it.
        private TaskCompletionSource<int> _answer;
        private HumanInputRequest _pending;

        /// <inheritdoc />
        public event Action<HumanInputRequest> PendingChanged;

        /// <inheritdoc />
        public HumanInputRequest Pending => _pending;

        /// <inheritdoc />
        public async Task RequestShuffleAsync(ShuffleRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            await Awaitable.MainThreadAsync();
            await WaitForAnswerAsync(HumanInputRequest.ForShuffle(request.Dealer), cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int> ChooseCutAsync(CutRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            await Awaitable.MainThreadAsync();
            return await WaitForAnswerAsync(
                HumanInputRequest.ForCut(request.Cutter, request.MinCut, request.MaxCut),
                cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Rank> ChooseTargetRankAsync(TargetRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            await Awaitable.MainThreadAsync();
            return (Rank)await WaitForAnswerAsync(HumanInputRequest.ForRank(request.Cutter), cancellationToken);
        }

        /// <inheritdoc />
        public void SubmitShuffle()
        {
            Submit(HumanInputKind.Shuffle, 0);
        }

        /// <inheritdoc />
        public void SubmitCut(int cutCount)
        {
            Submit(HumanInputKind.Cut, cutCount);
        }

        /// <inheritdoc />
        public void SubmitRank(Rank rank)
        {
            Submit(HumanInputKind.Rank, (int)rank);
        }

        private async Task<int> WaitForAnswerAsync(HumanInputRequest request, CancellationToken cancellationToken)
        {
            if (_answer != null)
            {
                throw new InvalidOperationException(
                    $"A {_pending.Kind} request is still waiting for input, so a {request.Kind} request cannot start.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var answer = new TaskCompletionSource<int>();
            _answer = answer;
            _pending = request;

            using (cancellationToken.Register(() => Cancel(answer, cancellationToken)))
            {
                PendingChanged?.Invoke(request);
                return await answer.Task;
            }
        }

        private void Submit(HumanInputKind kind, int value)
        {
            var answer = _answer;
            if (answer == null)
            {
                // A second click on a button that has already answered. Nothing is waiting, so nothing to do.
                return;
            }

            if (_pending.Kind != kind)
            {
                throw new InvalidOperationException(
                    $"The game is waiting for a {_pending.Kind} decision, not a {kind} decision.");
            }

            // Cleared before completing: the game may ask its next question from inside TrySetResult.
            ClearPending();
            answer.TrySetResult(value);
        }

        private void Cancel(TaskCompletionSource<int> answer, CancellationToken cancellationToken)
        {
            if (ReferenceEquals(_answer, answer))
            {
                ClearPending();
            }

            answer.TrySetCanceled(cancellationToken);
        }

        private void ClearPending()
        {
            _answer = null;
            _pending = null;
            PendingChanged?.Invoke(null);
        }
    }
}
