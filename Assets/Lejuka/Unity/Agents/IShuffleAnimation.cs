using System.Threading;
using System.Threading.Tasks;

namespace Lejuka.Unity.Agents
{
    /// <summary>Something that can show the deck being shuffled.</summary>
    public interface IShuffleAnimation
    {
        /// <summary>Plays the shuffle and completes when it has finished.</summary>
        Task ShuffleAsync(CancellationToken cancellationToken);
    }
}
