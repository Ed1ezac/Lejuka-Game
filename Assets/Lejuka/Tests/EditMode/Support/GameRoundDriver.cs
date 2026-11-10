using System;
using Lejuka.Application.Game;
using Lejuka.Contracts.Cards;
using Lejuka.Contracts.Game;
using Lejuka.Contracts.Players;
using Lejuka.Domain.Cards;
using Lejuka.Domain.Game;

namespace Lejuka.Tests.EditMode.Support
{
    /// <summary>Plays domain games directly, without the application layer.</summary>
    public static class GameRoundDriver
    {
        /// <summary>
        /// Rebuilds the deck a game holds after it was shuffled with <paramref name="seed"/> and cut,
        /// so a test can work out the expected deal without looking inside the game.
        /// </summary>
        public static Deck ReplicaDeck(int seed, int cutCount)
        {
            var deck = Deck.CreateStandard();
            deck.Shuffle(new SystemRandomSource(seed));
            deck.Cut(cutCount);
            return deck;
        }

        /// <summary>The 0-based position of the first card of the given rank.</summary>
        public static int FirstIndexOf(Deck deck, Rank rank)
        {
            for (var i = 0; i < deck.Cards.Count; i++)
            {
                if (deck.Cards[i].Rank == rank)
                {
                    return i;
                }
            }

            throw new InvalidOperationException($"The deck has no {rank}.");
        }

        /// <summary>Shuffles, cuts, announces and deals until the target appears. Leaves the game in the ceremony.</summary>
        public static void PlayToCeremony(GameRound round, int seed, int cutCount, Rank targetRank)
        {
            round.Shuffle(new SystemRandomSource(seed));
            round.Cut(cutCount);
            round.Announce(targetRank);
            while (round.Stage == GameStage.Dealing)
            {
                round.DealNext();
            }
        }

        /// <summary>
        /// Plays the game to completion so that <paramref name="winner"/> wins,
        /// by announcing a rank whose first card falls to that seat.
        /// </summary>
        public static void PlayToWin(GameRound round, Seat winner, IDealOrder dealOrder, int seed)
        {
            const int cutCount = 26;
            var replica = ReplicaDeck(seed, cutCount);

            for (var rank = Ranks.Lowest; rank <= Ranks.Highest; rank++)
            {
                var index = FirstIndexOf(replica, rank);
                var recipient = dealOrder.IsFirstReceiverTurn(index) ? round.Cutter : round.Dealer;
                if (recipient != winner)
                {
                    continue;
                }

                PlayToCeremony(round, seed, cutCount, rank);
                round.CompleteCeremony();
                return;
            }

            throw new InvalidOperationException($"Seed {seed} offers no rank that lets seat {winner} win.");
        }
    }
}
