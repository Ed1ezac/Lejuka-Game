namespace Lejuka.Contracts.Game
{
    /// <summary>The stages a single game moves through, in order.</summary>
    public enum GameStage
    {
        /// <summary>The Dealer shuffles and the Cutter cuts.</summary>
        ShuffleAndCut = 0,

        /// <summary>The Cutter calls the target rank.</summary>
        AnnounceTarget = 1,

        /// <summary>Cards are dealt until the target rank appears.</summary>
        Dealing = 2,

        /// <summary>The winner is known and is being announced.</summary>
        Ceremony = 3,

        /// <summary>The game is over and can be recorded.</summary>
        Completed = 4
    }
}
