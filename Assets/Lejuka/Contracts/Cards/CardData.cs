namespace Lejuka.Contracts.Cards
{
    /// <summary>A card as it crosses a boundary (events, presentation).</summary>
    /// <param name="Rank">The card's rank.</param>
    /// <param name="Suit">The card's suit.</param>
    public sealed record CardData(Rank Rank, Suit Suit);
}
