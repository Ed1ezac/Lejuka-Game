using Lejuka.Contracts.Cards;
using Lejuka.Contracts.Players;

namespace Lejuka.Contracts.Game.Events
{
    /// <summary>A card has been dealt to a player.</summary>
    /// <param name="GameIndex">0-based index of the game within the match.</param>
    /// <param name="Card">The card dealt.</param>
    /// <param name="Recipient">The seat that received it.</param>
    /// <param name="DealIndex">0-based position of the card in this game's deal.</param>
    /// <param name="IsTarget">True when the card matches the target rank and so ends the deal.</param>
    public sealed record CardDealt(
        int GameIndex,
        CardData Card,
        Seat Recipient,
        int DealIndex,
        bool IsTarget) : IGameEvent;
}
