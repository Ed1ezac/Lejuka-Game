namespace Lejuka.Contracts.Game.Events
{
    /// <summary>A game has entered a new stage.</summary>
    /// <param name="GameIndex">0-based index of the game within the match.</param>
    /// <param name="Stage">The stage just entered.</param>
    public sealed record StageChanged(int GameIndex, GameStage Stage) : IGameEvent;
}
