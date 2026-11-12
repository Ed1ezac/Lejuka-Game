using UnityEngine;

namespace Lejuka.Unity
{
    /// <summary>
    /// Reports serialized references that were never assigned, so a scene that is missing
    /// an object fails with a clear message instead of a NullReferenceException mid-game.
    /// </summary>
    public static class WiringGuard
    {
        /// <summary>The hint added to every report: the menu item that assigns the references.</summary>
        public const string FixHint = "Run Lejuka > Wire Mockup Scene and read its checklist.";

        /// <summary>Returns true when the reference is assigned; otherwise logs which field is missing.</summary>
        public static bool IsAssigned(Object owner, Object reference, string fieldName)
        {
            if (reference != null)
            {
                return true;
            }

            var ownerName = owner != null ? $"{owner.GetType().Name} on '{owner.name}'" : "A component";
            Debug.LogError($"[Lejuka] {ownerName} has no '{fieldName}' assigned. {FixHint}", owner);
            return false;
        }
    }
}
