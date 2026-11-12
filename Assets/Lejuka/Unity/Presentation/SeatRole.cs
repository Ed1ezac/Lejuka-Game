namespace Lejuka.Unity.Presentation
{
    /// <summary>The role a seat plays in the current game, as shown on the HUD.</summary>
    public enum SeatRole
    {
        /// <summary>No game is running yet.</summary>
        None = 0,

        /// <summary>Shuffles and deals.</summary>
        Dealer = 1,

        /// <summary>Cuts and calls the target.</summary>
        Cutter = 2
    }
}
