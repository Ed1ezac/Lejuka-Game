using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Lejuka.Unity.Tweening
{
    /// <summary>
    /// The only tweening entry point in the project, built on Unity's <see cref="Awaitable"/>.
    /// Views call nothing else, so swapping in DOTween later means rewriting this one class.
    /// Every tween runs on the main thread and stops as soon as its token is cancelled.
    /// </summary>
    public static class Tween
    {
        /// <summary>
        /// Calls <paramref name="apply"/> once per frame with the eased progress, ending on exactly 1.
        /// A duration of zero or less applies the end value straight away.
        /// </summary>
        public static async Task RunAsync(
            float duration,
            Action<float> apply,
            CancellationToken cancellationToken,
            Func<float, float> ease = null)
        {
            if (apply == null)
            {
                throw new ArgumentNullException(nameof(apply));
            }

            await Awaitable.MainThreadAsync();
            cancellationToken.ThrowIfCancellationRequested();

            var elapsed = 0f;
            while (elapsed < duration)
            {
                var progress = elapsed / duration;
                apply(ease == null ? progress : ease(progress));

                await Awaitable.NextFrameAsync(cancellationToken);
                elapsed += Time.deltaTime;
            }

            apply(1f);
        }

        /// <summary>Waits for the given time. Zero or less returns straight away.</summary>
        public static async Task DelayAsync(float seconds, CancellationToken cancellationToken)
        {
            await Awaitable.MainThreadAsync();
            cancellationToken.ThrowIfCancellationRequested();

            if (seconds > 0f)
            {
                await Awaitable.WaitForSecondsAsync(seconds, cancellationToken);
            }
        }

        /// <summary>Moves and rotates a transform in world space.</summary>
        public static Task MoveAsync(
            Transform target,
            Vector3 position,
            Quaternion rotation,
            float duration,
            CancellationToken cancellationToken)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            target.GetPositionAndRotation(out var startPosition, out var startRotation);
            return RunAsync(
                duration,
                progress => target.SetPositionAndRotation(
                    Vector3.LerpUnclamped(startPosition, position, progress),
                    Quaternion.SlerpUnclamped(startRotation, rotation, progress)),
                cancellationToken,
                Ease.OutCubic);
        }

        /// <summary>Scales a transform up by <paramref name="peak"/> and back to where it started.</summary>
        public static Task PunchScaleAsync(
            Transform target,
            float peak,
            float duration,
            CancellationToken cancellationToken)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var startScale = target.localScale;
            return RunAsync(
                duration,
                progress => target.localScale = startScale * Mathf.LerpUnclamped(1f, peak, Ease.Bump(progress)),
                cancellationToken);
        }
    }
}
