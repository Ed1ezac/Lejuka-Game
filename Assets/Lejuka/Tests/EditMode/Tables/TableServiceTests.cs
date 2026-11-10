using System;
using System.Linq;
using System.Threading;
using Lejuka.Application.Game;
using Lejuka.Application.Players;
using Lejuka.Application.Tables;
using Lejuka.Contracts.Cards;
using Lejuka.Contracts.Common;
using Lejuka.Contracts.Game;
using Lejuka.Contracts.Game.Events;
using Lejuka.Contracts.Players;
using Lejuka.Contracts.Tables;
using Lejuka.Domain.Tables;
using Lejuka.Tests.EditMode.Fakes;
using Lejuka.Tests.EditMode.Support;
using NUnit.Framework;

namespace Lejuka.Tests.EditMode.Tables
{
    /// <summary>End-to-end tests of free play: profile in, match played, profile saved.</summary>
    public sealed class TableServiceTests
    {
        private const int Seed = 31337;
        private const int StartingCredits = 1000;
        private const string ProfileId = "p1";

        // Bronze: 2 games at 10 (escrow 20). Silver: 4 games at 50 (escrow 200), unlocks at 3 match wins.
        private static readonly TableDefinition Bronze = new TableDefinition("bronze", "Bronze", 10, 2, 0);
        private static readonly TableDefinition Silver = new TableDefinition("silver", "Silver", 50, 4, 3);

        private InMemoryProfileRepository _repository;
        private RecordingPresentationGate _gate;
        private ProfileStore _profiles;
        private TableService _service;
        private IPlayerAgent _humanAgent;
        private IPlayerAgent _opponentAgent;

        [SetUp]
        public void SetUp()
        {
            var settings = new FreePlaySettings(DealMode.Classic, StartingCredits);
            _repository = new InMemoryProfileRepository();
            _gate = new RecordingPresentationGate();
            _profiles = new ProfileStore(_repository, settings);
            _service = new TableService(
                new TableCatalog(new[] { Bronze, Silver }),
                _profiles,
                new MatchRunner(GameRunner.CreateDefault(_gate, new SystemRandomSource(Seed)), _gate),
                settings);
            _humanAgent = new RandomAiAgent(new SystemRandomSource(Seed + 1));
            _opponentAgent = new RandomAiAgent(new SystemRandomSource(Seed + 2));
        }

        [Test]
        public void Play_UpdatesCreditsAndStats_AndSavesTheProfile()
        {
            _repository.Seed(new PlayerProfileData(ProfileId, "Player One", 600, 7, 3, 20));

            var result = Play(Silver);

            // The expectation is rebuilt from the published events, not from the returned result.
            var gamesWon = _gate.EventsOf<GameWon>().Count(won => won.Winner == Seat.A);
            var gamesLost = _gate.EventsOf<GameWon>().Count(won => won.Winner == Seat.B);
            Assert.That(gamesWon + gamesLost, Is.EqualTo(Silver.GamesPerMatch));

            var saved = _repository.Find(ProfileId);
            Assert.That(saved.Credits, Is.EqualTo(600 + (Silver.StakePerGame * (gamesWon - gamesLost))));
            Assert.That(saved.MatchesPlayed, Is.EqualTo(8));
            Assert.That(saved.MatchesWon, Is.EqualTo(gamesWon > gamesLost ? 4 : 3));
            Assert.That(saved.GamesWon, Is.EqualTo(20 + gamesWon));
            Assert.That(saved.DisplayName, Is.EqualTo("Player One"));
            Assert.That(_repository.SaveCount, Is.EqualTo(1));

            Assert.That(result.Score.GamesWonA, Is.EqualTo(gamesWon));
            Assert.That(result.Score.GamesWonB, Is.EqualTo(gamesLost));
        }

        [Test]
        public void Play_ManyMatches_KeepsCreditsAndStatsInStepWithTheGamesPlayed()
        {
            const int matches = 12;
            _repository.Seed(new PlayerProfileData(ProfileId, "Player One", 5000, 3, 3, 0));

            var expectedCredits = 5000;
            var expectedMatchesWon = 3;
            var expectedGamesWon = 0;
            for (var i = 0; i < matches; i++)
            {
                var result = Play(Silver);
                var score = result.Score;

                expectedCredits += Silver.StakePerGame * (score.GamesWonA - score.GamesWonB);
                expectedGamesWon += score.GamesWonA;
                if (score.GamesWonA > score.GamesWonB)
                {
                    expectedMatchesWon++;
                }
            }

            var saved = _repository.Find(ProfileId);
            Assert.That(saved.Credits, Is.EqualTo(expectedCredits));
            Assert.That(saved.MatchesPlayed, Is.EqualTo(3 + matches));
            Assert.That(saved.MatchesWon, Is.EqualTo(expectedMatchesWon));
            Assert.That(saved.GamesWon, Is.EqualTo(expectedGamesWon));
            Assert.That(_repository.SaveCount, Is.EqualTo(matches));
            Assert.That(_gate.EventsOf<GameWon>().Count, Is.EqualTo(matches * Silver.GamesPerMatch));
        }

        [Test]
        public void Play_WithScriptedAgents_SettlesAgainstTheScore()
        {
            _humanAgent = new ScriptedAgent(10, Rank.Ace);
            _opponentAgent = new ScriptedAgent(40, Rank.King);

            var result = Play(Bronze);

            var saved = _repository.Find(ProfileId);
            var margin = result.Score.GamesWonA - result.Score.GamesWonB;
            Assert.That(saved.Credits, Is.EqualTo(StartingCredits + (Bronze.StakePerGame * margin)));
            Assert.That(saved.GamesWon, Is.EqualTo(result.Score.GamesWonA));
        }

        [Test]
        public void Play_FirstTimePlayer_StartsWithTheConfiguredCredits()
        {
            var result = Play(Bronze);

            var saved = _repository.Find(ProfileId);
            var margin = result.Score.GamesWonA - result.Score.GamesWonB;
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved.Id, Is.EqualTo(ProfileId));
            Assert.That(saved.Credits, Is.EqualTo(StartingCredits + (Bronze.StakePerGame * margin)));
            Assert.That(saved.MatchesPlayed, Is.EqualTo(1));
        }

        [Test]
        public void Play_UsesTheTablesStakeAndGameCount_WithThePlayerInSeatA()
        {
            var result = Play(Bronze);

            Assert.That(result.Settings.GameCount, Is.EqualTo(Bronze.GamesPerMatch));
            Assert.That(result.Settings.StakePerGame, Is.EqualTo(Bronze.StakePerGame));
            Assert.That(result.Settings.DealMode, Is.EqualTo(DealMode.Classic));
            Assert.That(result.Settings.FirstDealer, Is.EqualTo(Seat.A));
        }

        [Test]
        public void Play_LockedTable_IsDenied_AndNothingIsSaved()
        {
            _repository.Seed(new PlayerProfileData(ProfileId, "Player One", 5000, 2, 2, 5));

            var error = Assert.Throws<DomainRuleException>(() => Play(Silver));

            Assert.That(error.Message, Does.Contain(TableDenialReason.Locked.ToString()));
            Assert.That(_repository.SaveCount, Is.EqualTo(0));
            Assert.That(_gate.Events, Is.Empty);
        }

        [Test]
        public void Play_UnlockedButUnaffordableTable_IsDenied_AndNothingIsSaved()
        {
            _repository.Seed(new PlayerProfileData(ProfileId, "Player One", 199, 3, 3, 9));

            var error = Assert.Throws<DomainRuleException>(() => Play(Silver));

            Assert.That(error.Message, Does.Contain(TableDenialReason.InsufficientCredits.ToString()));
            Assert.That(_repository.SaveCount, Is.EqualTo(0));
            Assert.That(_repository.Find(ProfileId).Credits, Is.EqualTo(199));
        }

        [Test]
        public void Play_UnknownTable_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => _service.PlayAsync(ProfileId, "platinum", _opponentAgent, _humanAgent, CancellationToken.None)
                    .Await());
        }

        [Test]
        public void Play_Cancelled_SavesNothing()
        {
            _repository.Seed(new PlayerProfileData(ProfileId, "Player One", 600, 7, 3, 20));

            using (var cancellation = new CancellationTokenSource())
            {
                _gate.OnPresent = gameEvent =>
                {
                    if (gameEvent is GameWon)
                    {
                        cancellation.Cancel();
                    }
                };

                Assert.Catch<OperationCanceledException>(
                    () => _service.PlayAsync(ProfileId, Silver.Id, _opponentAgent, _humanAgent, cancellation.Token)
                        .Await());
            }

            Assert.That(_repository.SaveCount, Is.EqualTo(0));
            Assert.That(_repository.Find(ProfileId).Credits, Is.EqualTo(600));
        }

        [Test]
        public void GetTables_ReportsUnlockedEligibleAndReasonForEachTable()
        {
            var profile = _profiles.LoadOrCreateAsync(ProfileId, CancellationToken.None).Await();

            var tables = _service.GetTables(profile);

            Assert.That(tables.Select(status => status.Table), Is.EqualTo(new[] { Bronze, Silver }));

            Assert.That(tables[0].Access.IsUnlocked, Is.True);
            Assert.That(tables[0].Access.IsEligible, Is.True);
            Assert.That(tables[0].Access.DenialReason, Is.EqualTo(TableDenialReason.None));

            // A new player can afford Silver but has not won the three matches it needs.
            Assert.That(tables[1].Access.IsUnlocked, Is.False);
            Assert.That(tables[1].Access.IsEligible, Is.True);
            Assert.That(tables[1].Access.CanJoin, Is.False);
            Assert.That(tables[1].Access.DenialReason, Is.EqualTo(TableDenialReason.Locked));
        }

        [Test]
        public void GetTables_UnlockedButUnaffordable_ReportsInsufficientCredits()
        {
            _repository.Seed(new PlayerProfileData(ProfileId, "Player One", 199, 3, 3, 9));
            var profile = _profiles.LoadOrCreateAsync(ProfileId, CancellationToken.None).Await();

            var tables = _service.GetTables(profile);

            Assert.That(tables[0].Access.CanJoin, Is.True);
            Assert.That(tables[1].Access.IsUnlocked, Is.True);
            Assert.That(tables[1].Access.IsEligible, Is.False);
            Assert.That(tables[1].Access.DenialReason, Is.EqualTo(TableDenialReason.InsufficientCredits));
        }

        private MatchResult Play(TableDefinition table)
        {
            return _service.PlayAsync(ProfileId, table.Id, _opponentAgent, _humanAgent, CancellationToken.None)
                .Await();
        }
    }
}
