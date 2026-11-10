namespace Lejuka.Contracts.Game
{
    /// <summary>How dealt cards alternate between the two players.</summary>
    public enum DealMode
    {
        /// <summary>Strict alternation. Favours the first receiver (about 52%).</summary>
        Classic = 0,

        /// <summary>Thue–Morse order. Evens the odds to about 50%.</summary>
        Fair = 1
    }
}
