namespace Lejuka.Domain.Game
{
    /// <summary>Decides which player receives each dealt card.</summary>
    public interface IDealOrder
    {
        /// <summary>
        /// True when the card at the 0-based <paramref name="dealIndex"/> goes to the first receiver,
        /// false when it goes to the other player.
        /// </summary>
        bool IsFirstReceiverTurn(int dealIndex);
    }
}
