using System.Threading;
using System.Threading.Tasks;

namespace Lejuka.Contracts.Players
{
    /// <summary>Storage for player profiles.</summary>
    public interface IProfileRepository
    {
        /// <summary>Loads a profile, or returns <c>null</c> when none is stored under <paramref name="id"/>.</summary>
        Task<PlayerProfileData> LoadAsync(string id, CancellationToken cancellationToken);

        /// <summary>Stores the profile under its own id, replacing any previous version.</summary>
        Task SaveAsync(PlayerProfileData profile, CancellationToken cancellationToken);
    }
}
