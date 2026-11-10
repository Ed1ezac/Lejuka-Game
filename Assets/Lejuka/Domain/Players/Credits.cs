using System;
using Lejuka.Contracts.Common;

namespace Lejuka.Domain.Players
{
    /// <summary>
    /// A non-negative amount of credits. Arithmetic is checked, so an overflow throws instead of wrapping.
    /// </summary>
    public readonly struct Credits : IEquatable<Credits>, IComparable<Credits>
    {
        /// <summary>Creates an amount. Negative values are rejected.</summary>
        public Credits(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Credits cannot be negative.");
            }

            Amount = amount;
        }

        /// <summary>No credits.</summary>
        public static Credits Zero => default;

        /// <summary>The amount as a plain number.</summary>
        public int Amount { get; }

        /// <summary>Adds two amounts.</summary>
        public static Credits operator +(Credits left, Credits right)
        {
            return new Credits(checked(left.Amount + right.Amount));
        }

        /// <summary>Subtracts two amounts. Throws when the result would be negative.</summary>
        public static Credits operator -(Credits left, Credits right)
        {
            if (right.Amount > left.Amount)
            {
                throw new DomainRuleException($"Cannot take {right} from {left}: credits cannot be negative.");
            }

            return new Credits(left.Amount - right.Amount);
        }

        /// <summary>Multiplies an amount by a non-negative count.</summary>
        public static Credits operator *(Credits credits, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count cannot be negative.");
            }

            return new Credits(checked(credits.Amount * count));
        }

        /// <summary>True when both amounts are the same.</summary>
        public static bool operator ==(Credits left, Credits right)
        {
            return left.Amount == right.Amount;
        }

        /// <summary>True when the amounts differ.</summary>
        public static bool operator !=(Credits left, Credits right)
        {
            return left.Amount != right.Amount;
        }

        /// <summary>True when the left amount is smaller.</summary>
        public static bool operator <(Credits left, Credits right)
        {
            return left.Amount < right.Amount;
        }

        /// <summary>True when the left amount is larger.</summary>
        public static bool operator >(Credits left, Credits right)
        {
            return left.Amount > right.Amount;
        }

        /// <summary>True when the left amount is smaller or the same.</summary>
        public static bool operator <=(Credits left, Credits right)
        {
            return left.Amount <= right.Amount;
        }

        /// <summary>True when the left amount is larger or the same.</summary>
        public static bool operator >=(Credits left, Credits right)
        {
            return left.Amount >= right.Amount;
        }

        /// <inheritdoc />
        public int CompareTo(Credits other)
        {
            return Amount.CompareTo(other.Amount);
        }

        /// <inheritdoc />
        public bool Equals(Credits other)
        {
            return Amount == other.Amount;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Credits other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Amount;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Amount.ToString();
        }
    }
}
