namespace Lejuka.Unity.Agents
{
    /// <summary>What the game is waiting for the human to do.</summary>
    public enum HumanInputKind
    {
        /// <summary>Press Shuffle.</summary>
        Shuffle = 0,

        /// <summary>Choose how many cards to cut.</summary>
        Cut = 1,

        /// <summary>Call the target rank.</summary>
        Rank = 2
    }
}
