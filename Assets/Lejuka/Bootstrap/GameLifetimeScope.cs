using System;
using System.IO;
using Lejuka.Application.Game;
using Lejuka.Application.Tables;
using Lejuka.Contracts.Common;
using Lejuka.Contracts.Game;
using Lejuka.Contracts.Players;
using Lejuka.Domain.Tables;
using Lejuka.Unity;
using Lejuka.Unity.Agents;
using Lejuka.Unity.Config;
using Lejuka.Unity.Persistence;
using Lejuka.Unity.Presentation;
using Lejuka.Unity.Presenters;
using Lejuka.Unity.Views;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Lejuka.Bootstrap
{
    /// <summary>
    /// The composition root of the mockup scene. It is the only place that knows every layer:
    /// it builds the core services, plugs the Unity adapters into them and starts <see cref="MockupEntryPoint"/>.
    /// </summary>
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [Header("Configs")]
        [SerializeField] private LejukaGameConfig _gameConfig;
        [SerializeField] private TableCatalogConfig _tableCatalog;

        [Header("Scene")]
        [SerializeField] private TableView _tableView;
        [SerializeField] private HudPresenter _hud;
        [SerializeField] private ActionPanelPresenter _actionPanel;
        [SerializeField] private CeremonyPresenter _ceremony;

        /// <inheritdoc />
        protected override void Configure(IContainerBuilder builder)
        {
            EnsureWired();

            RegisterSettings(builder);
            RegisterSceneComponents(builder);

            // One random source for the whole game: the shuffle and the AI both draw from it.
            var random = new SeededRandomSource(new SeedProvider(_gameConfig.FixedSeed));
            builder.RegisterInstance(random).As<IRandomSource>().AsSelf();

            RegisterAgents(builder, random);
            RegisterCore(builder);

            builder.RegisterEntryPoint<MockupEntryPoint>();
        }

        private void RegisterSettings(IContainerBuilder builder)
        {
            builder.RegisterInstance(_gameConfig.ToFreePlaySettings());
            builder.RegisterInstance(_tableCatalog.ToCatalog());
            builder.RegisterInstance(new PresentationTimings(_gameConfig.DealTimePerCard));
            builder.RegisterInstance(new MatchViewState());
            builder.RegisterInstance<IProfileRepository>(CreateProfileRepository());
        }

        private void RegisterSceneComponents(IContainerBuilder builder)
        {
            builder.RegisterComponent(_tableView);
            builder.RegisterComponent(_hud);
            builder.RegisterComponent(_actionPanel);
            builder.RegisterComponent(_ceremony);
        }

        private void RegisterAgents(IContainerBuilder builder, IRandomSource random)
        {
            var humanAgent = new HumanAgent();
            builder.RegisterInstance<IHumanInputSource>(humanAgent);

            var agents = new SeatAgentFactory(
                humanAgent,
                random,
                _tableView,
                _gameConfig.AiDelayMin,
                _gameConfig.AiDelayMax);

            builder.RegisterInstance(new TableSeats(CreateSeat(Seat.A, agents), CreateSeat(Seat.B, agents)));
        }

        // The core types are built with explicit factories rather than reflection: the wiring is checked
        // by the compiler, and nothing depends on constructors surviving IL2CPP code stripping.
        private static void RegisterCore(IContainerBuilder builder)
        {
            builder.Register<IPresentationGate>(
                resolver => new UnityPresentationGate(
                    resolver.Resolve<TableView>(),
                    resolver.Resolve<HudPresenter>(),
                    resolver.Resolve<CeremonyPresenter>(),
                    resolver.Resolve<MatchViewState>(),
                    resolver.Resolve<PresentationTimings>()),
                Lifetime.Singleton);

            builder.Register<GameRunner>(
                resolver => GameRunner.CreateDefault(
                    resolver.Resolve<IPresentationGate>(),
                    resolver.Resolve<IRandomSource>()),
                Lifetime.Singleton);

            builder.Register<MatchRunner>(
                resolver => new MatchRunner(
                    resolver.Resolve<GameRunner>(),
                    resolver.Resolve<IPresentationGate>()),
                Lifetime.Singleton);

            builder.Register<ProfileStore>(
                resolver => new ProfileStore(
                    resolver.Resolve<IProfileRepository>(),
                    resolver.Resolve<FreePlaySettings>()),
                Lifetime.Singleton);

            builder.Register<TableService>(
                resolver => new TableService(
                    resolver.Resolve<TableCatalog>(),
                    resolver.Resolve<ProfileStore>(),
                    resolver.Resolve<MatchRunner>(),
                    resolver.Resolve<FreePlaySettings>()),
                Lifetime.Singleton);
        }

        private SeatBinding CreateSeat(Seat seat, SeatAgentFactory agents)
        {
            var kind = _gameConfig.KindOf(seat);
            return new SeatBinding(_gameConfig.NameOf(seat), kind, agents.Create(kind));
        }

        private IProfileRepository CreateProfileRepository()
        {
            // Written in full: inside a Lejuka.* namespace a bare "Application" means Lejuka.Application.
            var path = Path.Combine(UnityEngine.Application.persistentDataPath, JsonProfileRepository.DefaultFileName);
            var repository = new JsonProfileRepository(path);

#if UNITY_EDITOR
            if (_gameConfig.ResetProfileOnPlay)
            {
                repository.Delete();
                Debug.Log($"[Lejuka] Reset Profile On Play is on: deleted '{path}'.");
            }
#endif

            return repository;
        }

        private void EnsureWired()
        {
            // Not short-circuited: every missing reference should be reported, not just the first.
            var isWired = WiringGuard.IsAssigned(this, _gameConfig, nameof(_gameConfig))
                          & WiringGuard.IsAssigned(this, _tableCatalog, nameof(_tableCatalog))
                          & WiringGuard.IsAssigned(this, _tableView, nameof(_tableView))
                          & WiringGuard.IsAssigned(this, _hud, nameof(_hud))
                          & WiringGuard.IsAssigned(this, _actionPanel, nameof(_actionPanel))
                          & WiringGuard.IsAssigned(this, _ceremony, nameof(_ceremony));

            if (!isWired)
            {
                throw new InvalidOperationException(
                    "GameLifetimeScope is missing references (see the errors above), so the game cannot start. "
                    + WiringGuard.FixHint);
            }
        }
    }
}
