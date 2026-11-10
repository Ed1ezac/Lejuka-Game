using Lejuka.Contracts.Tables;
using Lejuka.Domain.Tables;

namespace Lejuka.Application.Tables
{
    /// <summary>A table together with one player's access to it.</summary>
    /// <param name="Table">The table.</param>
    /// <param name="Access">Whether the player has it unlocked, can afford it, and why not if denied.</param>
    public sealed record TableStatus(TableDefinition Table, TableAccessResult Access);
}
