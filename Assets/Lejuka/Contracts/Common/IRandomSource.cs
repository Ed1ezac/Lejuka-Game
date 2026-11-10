namespace Lejuka.Contracts.Common
{
    /// <summary>
    /// The single source of randomness for the core. Implementations must be seedable so games can be replayed.
    /// </summary>
    public interface IRandomSource
    {
        /// <summary>Returns a uniformly distributed integer in <c>[minInclusive, maxExclusive)</c>.</summary>
        int Next(int minInclusive, int maxExclusive);
    }
}
