using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Lejuka.Contracts.Common;
using Lejuka.Contracts.Game;
using Lejuka.Contracts.Players;
using Lejuka.Domain.Players;

namespace Lejuka.Domain.Game
{
    /// <summary>
    /// A fixed series of games between seats A and B. Games are played one at a time,
    /// every game is always played, and the stakes are settled once at the end.
    /// </summary>
    public sealed class Match
    {
        private readonly RoleRotation _roleRotation;
        private readonly IDealOrder _dealOrder;
        private readonly List<GameResult> _results;
        private readonly ReadOnlyCollection<GameResult> _resultsView;
        private GameRound _currentGame;
        private int _gamesWonA;
        private int _gamesWonB;

        /// <summary>Creates a match, rejecting an invalid game count, stake, deal mode or first dealer.</summary>
        public Match(MatchSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            MatchRules.EnsureValidGameCount(settings.GameCount, nameof(settings));
            MatchRules.EnsureValidStake(settings.StakePerGame, nameof(settings));

            _dealOrder = DealOrderFactory.Create(settings.DealMode);
            _roleRotation = new RoleRotation(settings.FirstDealer);
            _results = new List<GameResult>(settings.GameCount);
            _resultsView = _results.AsReadOnly();

            GameCount = settings.GameCount;
            StakePerGame = new Credits(settings.StakePerGame);
        }

        /// <summary>Number of games in the match.</summary>
        public int GameCount { get; }

        /// <summary>Credits each game is worth.</summary>
        public Credits StakePerGame { get; }

        /// <summary>Results of the games recorded so far, in play order.</summary>
        public IReadOnlyList<GameResult> Results => _resultsView;

        /// <summary>True once every game has been played and recorded.</summary>
        public bool IsComplete => _results.Count == GameCount;

        /// <summary>The score so far.</summary>
        public MatchScore Score => new MatchScore(_gamesWonA, _gamesWonB);

        /// <summary>Creates the next game with its roles. The previous game must be recorded first.</summary>
        public GameRound StartNextGame()
        {
            if (IsComplete)
            {
                throw new DomainRuleException("Every game of the match has already been played.");
            }

            if (_currentGame != null)
            {
                throw new DomainRuleException("The current game must be recorded before the next one starts.");
            }

            var gameIndex = _results.Count;
            _currentGame = new GameRound(gameIndex, _roleRotation.RolesFor(gameIndex), _dealOrder);
            return _currentGame;
        }

        /// <summary>Records the result of the current game once it has completed.</summary>
        public GameResult RecordResult(GameRound game)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            if (!ReferenceEquals(game, _currentGame))
            {
                throw new DomainRuleException("Only the match's current game can be recorded.");
            }

            if (game.Stage != GameStage.Completed || !game.Winner.HasValue)
            {
                throw new DomainRuleException("A game can only be recorded once it has completed.");
            }

            var winner = game.Winner.Value;
            var result = new GameResult(game.Index, game.Dealer, game.Cutter, winner, game.DealtCards.Count);
            _results.Add(result);
            if (winner == Seat.A)
            {
                _gamesWonA++;
            }
            else
            {
                _gamesWonB++;
            }

            _currentGame = null;
            return result;
        }

        /// <summary>
        /// Works out the net transfer: the stake times the difference in games won, paid by the loser.
        /// </summary>
        public Settlement CalculateSettlement()
        {
            if (!IsComplete)
            {
                throw new DomainRuleException("A match can only be settled once every game has been played.");
            }

            var score = Score;
            var winner = score.Leader;
            if (!winner.HasValue)
            {
                return new Settlement(score, Credits.Zero, null, null);
            }

            var loser = winner.Value.Opponent();
            var margin = score.WinsFor(winner.Value) - score.WinsFor(loser);
            return new Settlement(score, StakePerGame * margin, loser, winner.Value);
        }
    }
}
