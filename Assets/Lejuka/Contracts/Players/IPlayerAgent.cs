using System.Threading;
using System.Threading.Tasks;
using Lejuka.Contracts.Cards;

namespace Lejuka.Contracts.Players
{
    /// <summary>
    /// Supplies one seat's decisions. A human agent waits for UI input; an AI agent answers straight away.
    /// Answers are validated by the domain, so an agent never has to be trusted.
    /// </summary>
    public interface IPlayerAgent
    {
        /// <summary>Completes when the Dealer is ready for the deck to be shuffled.</summary>
        Task RequestShuffleAsync(ShuffleRequest request, CancellationToken cancellationToken);

        /// <summary>Returns the cut count, expected to lie within the request's range.</summary>
        Task<int> ChooseCutAsync(CutRequest request, CancellationToken cancellationToken);

        /// <summary>Returns the rank the Cutter calls as the target.</summary>
        Task<Rank> ChooseTargetRankAsync(TargetRequest request, CancellationToken cancellationToken);
    }
}
