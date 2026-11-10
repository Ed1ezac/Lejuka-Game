using System;

namespace Lejuka.Contracts.Common
{
    /// <summary>
    /// Thrown when a game rule is violated, e.g. acting in the wrong stage or spending reserved credits.
    /// Bad arguments use <see cref="ArgumentOutOfRangeException"/> instead.
    /// </summary>
    public sealed class DomainRuleException : Exception
    {
        /// <summary>Creates the exception with a message naming the broken rule.</summary>
        public DomainRuleException(string message)
            : base(message)
        {
        }
    }
}
