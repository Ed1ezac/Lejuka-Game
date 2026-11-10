using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Lejuka.Contracts.Cards;
using Lejuka.Contracts.Common;
using Lejuka.Contracts.Game;
using Lejuka.Contracts.Players;
using Lejuka.Domain.Cards;

namespace Lejuka.Domain.Game
{
    /// <summary>
    /// One game of a match: shuffle and cut, announce the target, deal until the target appears, ceremony.
    /// Each action is only legal in its own stage and throws <see cref="DomainRuleException"/> otherwise.
    /// </summary>
    /// <remarks>
    /// Named GameRound rather than Game because a type that shares its namespace's name
    /// cannot be referenced by its short name from sibling namespaces.
    /// </remarks>
    public sealed class GameRound
    {
        private readonly Deck _deck = Deck.CreateStandard();
        private readonly IDealOrder _dealOrder;
        private readonly List<DealResult> _dealtCards = new List<DealResult>();
        private readonly ReadOnlyCollection<DealResult> _dealtCardsView;
        private bool _isShuffled;

        /// <summary>Creates a game with a fresh deck, ready to be shuffled.</summary>
        public GameRound(int index, GameRoles roles, IDealOrder dealOrder)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Game index cannot be negative.");
            }

            if (roles == null)
            {
                throw new ArgumentNullException(nameof(roles));
            }

            if (roles.Dealer == roles.Cutter)
            {
                throw new ArgumentException("Dealer and Cutter must be different seats.", nameof(roles));
            }

            _dealOrder = dealOrder ?? throw new ArgumentNullException(nameof(dealOrder));
            _dealtCardsView = _dealtCards.AsReadOnly();

            Index = index;
            Dealer = roles.Dealer;
            Cutter = roles.Cutter;
            Stage = GameStage.ShuffleAndCut;
        }

        /// <summary>0-based index of this game within its match.</summary>
        public int Index { get; }

        /// <summary>The seat that shuffles and deals.</summary>
        public Seat Dealer { get; }

        /// <summary>The seat that cuts, calls the target and receives the first card.</summary>
        public Seat Cutter { get; }

        /// <summary>The current stage.</summary>
        public GameStage Stage { get; private set; }

        /// <summary>The announced target rank, or <c>null</c> before the announcement.</summary>
        public Rank? TargetRank { get; private set; }

        /// <summary>The winning seat, or <c>null</c> until the target card has been dealt.</summary>
        public Seat? Winner { get; private set; }

        /// <summary>Cards still in the deck.</summary>
        public int DeckCount => _deck.Count;

        /// <summary>The cards dealt so far, in deal order.</summary>
        public IReadOnlyList<DealResult> DealtCards => _dealtCardsView;

        /// <summary>The Dealer shuffles the deck. Allowed once, before the cut.</summary>
        public void Shuffle(IRandomSource random)
        {
            EnsureStage(GameStage.ShuffleAndCut, nameof(Shuffle));
            if (_isShuffled)
            {
                throw new DomainRuleException("The deck has already been shuffled.");
            }

            _deck.Shuffle(random);
            _isShuffled = true;
        }

        /// <summary>The Cutter cuts the shuffled deck, which ends the shuffle-and-cut stage.</summary>
        public void Cut(int cutCount)
        {
            EnsureStage(GameStage.ShuffleAndCut, nameof(Cut));
            if (!_isShuffled)
            {
                throw new DomainRuleException("The deck must be shuffled before it is cut.");
            }

            _deck.Cut(cutCount);
            Stage = GameStage.AnnounceTarget;
        }

        /// <summary>The Cutter calls the target rank, which starts the deal.</summary>
        public void Announce(Rank targetRank)
        {
            EnsureStage(GameStage.AnnounceTarget, nameof(Announce));
            if (!Ranks.IsDefined(targetRank))
            {
                throw new ArgumentOutOfRangeException(nameof(targetRank), targetRank, "Unknown rank.");
            }

            TargetRank = targetRank;
            Stage = GameStage.Dealing;
        }

        /// <summary>
        /// Deals the top card. The first card goes to the Cutter; the deal order decides the rest.
        /// Dealing the target card sets the <see cref="Winner"/> and moves the game to the ceremony.
        /// </summary>
        public DealResult DealNext()
        {
            EnsureStage(GameStage.Dealing, nameof(DealNext));
            if (_deck.Count == 0)
            {
                // Every rank is in a full deck four times, so this means the deck was corrupted.
                throw new InvalidOperationException("The deck ran out before the target rank appeared.");
            }

            var card = _deck.Draw();
            var dealIndex = _dealtCards.Count;
            var recipient = _dealOrder.IsFirstReceiverTurn(dealIndex) ? Cutter : Dealer;
            var result = new DealResult(card, recipient, dealIndex, card.Rank == TargetRank);
            _dealtCards.Add(result);

            if (result.IsTarget)
            {
                Winner = recipient;
                Stage = GameStage.Ceremony;
            }

            return result;
        }

        /// <summary>Ends the ceremony and completes the game.</summary>
        public void CompleteCeremony()
        {
            EnsureStage(GameStage.Ceremony, nameof(CompleteCeremony));
            Stage = GameStage.Completed;
        }

        private void EnsureStage(GameStage expected, string action)
        {
            if (Stage != expected)
            {
                throw new DomainRuleException(
                    $"{action} is only allowed in stage {expected}, but the game is in stage {Stage}.");
            }
        }
    }
}
