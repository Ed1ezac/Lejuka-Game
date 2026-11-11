using UnityEngine;

namespace Lejuka.Unity.Tweening
{
    /// <summary>Easing curves. Each maps a linear progress in [0, 1] to an eased progress.</summary>
    public static class Ease
    {
        /// <summary>No easing.</summary>
        public static float Linear(float t)
        {
            return t;
        }

        /// <summary>Starts fast and settles gently. Good for things arriving somewhere.</summary>
        public static float OutCubic(float t)
        {
            var inverse = 1f - t;
            return 1f - (inverse * inverse * inverse);
        }

        /// <summary>Slow at both ends.</summary>
        public static float InOutSine(float t)
        {
            return 0.5f - (0.5f * Mathf.Cos(Mathf.PI * t));
        }

        /// <summary>Rises to 1 at the halfway point and returns to 0. Used for pulses.</summary>
        public static float Bump(float t)
        {
            return Mathf.Sin(Mathf.PI * t);
        }
    }
}
