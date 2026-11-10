using System.Linq;
using System.Threading;
using Lejuka.Application.Game;
using Lejuka.Application.Players;
using Lejuka.Contracts.Game;
using Lejuka.Contracts.Players;
using Lejuka.Domain.Game;
using Lejuka.Domain.Players;
using Lejuka.Tests.EditMode.Fakes;
using Lejuka.Tests.EditMode.Support;
using NUnit.Framework;

namespace Lejuka.Tests.EditMode.Game
{
    /// <summary>
    /// Checks how often the first receiver (the Cutter) wins under each deal mode,
    /// once exactly and once by playing real games.
    /// </summary>
    public sealed class ProbabilityTests
    {
        private const int DeckSize = 52;
        private const int CopiesOfTarget = 4;
        private const int SimulatedGames = 200000;
        private const int Seed = 20260611;

        [Test]
        public void Exact_Classic_FirstReceiverWinsAbout52Percent()
        {
            Assert.That(ExactFirstReceiverWinRate(new ClassicDealOrder()), Is.EqualTo(0.51981).Within(1e-4));
        }

        [Test]
        public void Exact_Fair_FirstReceiverWinsHalfTheTime()
        {
            Assert.That(ExactFirstReceiverWinRate(new FairDealOrder()), Is.EqualTo(0.50000).Within(1e-4));
        }

        [Test]
        public void Exact_PositionProbabilities_SumToOne()
        {
            var total = Enumerable.Range(1, DeckSize - CopiesOfTarget + 1).Sum(ProbabilityFirstTargetAt);

            Assert.That(total, Is.EqualTo(1.0).Within(1e-12));
        }

        [Test]
        public void Simulation_Classic_FirstReceiverWinsAbout52Percent()
        {
            Assert.That(SimulatedFirstReceiverWinRate(DealMode.Classic), Is.EqualTo(0.5198).Within(0.005));
        }

        [Test]
        public void Simulation_Fair_FirstReceiverWinsHalfTheTime()
        {
            Assert.That(SimulatedFirstReceiverWinRate(DealMode.Fair), Is.EqualTo(0.5000).Within(0.005));
        }

        /// <summary>
        /// Sums P(first target at position p) over the positions the deal order gives the first receiver.
        /// </summary>
        private static double ExactFirstReceiverWinRate(IDealOrder dealOrder)
        {
            var winRate = 0.0;
            for (var position = 1; position <= DeckSize - CopiesOfTarget + 1; position++)
            {
                if (dealOrder.IsFirstReceiverTurn(position - 1))
                {
                    winRate += ProbabilityFirstTargetAt(position);
                }
            }

            return winRate;
        }

        /// <summary>
        /// The first target sits at 1-based position p when the other three copies all lie below it:
        /// C(52 − p, 3) of the C(52, 4) equally likely placements of the four copies.
        /// </summary>
        private static double ProbabilityFirstTargetAt(int position)
        {
            return Choose(DeckSize - position, CopiesOfTarget - 1) / Choose(DeckSize, CopiesOfTarget);
        }

        private static double Choose(int n, int k)
        {
            if (k < 0 || k > n)
            {
                return 0.0;
            }

            var result = 1.0;
            for (var i = 1; i <= k; i++)
            {
                result = result * (n - k + i) / i;
            }

            return result;
        }

        /// <summary>
        /// Plays one long match through the real application flow and counts the games the Cutter won.
        /// </summary>
        private static double SimulatedFirstReceiverWinRate(DealMode mode)
        {
            var gate = new InstantPresentationGate();
            var runner = new MatchRunner(GameRunner.CreateDefault(gate, new SystemRandomSource(Seed)), gate);

            // A stake of 1 keeps the escrow for this very long match equal to the number of games.
            var setup = new MatchSetup(
                new MatchSettings(SimulatedGames, 1, mode),
                NewParticipant("a", Seed + 1),
                NewParticipant("b", Seed + 2));

            var result = runner.RunAsync(setup, CancellationToken.None).Await();

            var cutterWins = result.Games.Count(game => game.Winner == game.Cutter);
            return cutterWins / (double)SimulatedGames;
        }

        private static MatchParticipant NewParticipant(string id, int seed)
        {
            return new MatchParticipant(
                new Player(id, id, PlayerKind.Ai),
                new RandomAiAgent(new SystemRandomSource(seed)),
                new Wallet(new Credits(SimulatedGames)));
        }
    }
}
