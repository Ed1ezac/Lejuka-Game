using Lejuka.Contracts.Players;
using Lejuka.Domain.Cards;

namespace Lejuka.Domain.Game
{
    /// <summary>The outcome of dealing one card.</summary>
    /// <param name="Card">The card dealt.</param>
    /// <param name="Recipient">The seat that received it.</param>
    /// <param name="DealIndex">0-based position of the card in the deal.</param>
    /// <param name="IsTarget">True when the card matches the target rank and so wins the game.</param>
    public sealed record DealResult(Card Card, Seat Recipient, int DealIndex, bool IsTarget);
}
