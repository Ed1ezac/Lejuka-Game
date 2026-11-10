using Lejuka.Contracts.Players;

namespace Lejuka.Domain.Game
{
    /// <summary>The recorded outcome of one completed game.</summary>
    /// <param name="GameIndex">0-based index of the game within the match.</param>
    /// <param name="Dealer">The seat that dealt.</param>
    /// <param name="Cutter">The seat that cut and received the first card.</param>
    /// <param name="Winner">The seat that received the target card.</param>
    /// <param name="CardsDealt">How many cards were dealt, including the target card.</param>
    public sealed record GameResult(int GameIndex, Seat Dealer, Seat Cutter, Seat Winner, int CardsDealt);
}
