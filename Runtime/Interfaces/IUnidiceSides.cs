using System.Threading;
using Cysharp.Threading.Tasks;
using Unidice.SDK.Unidice;
using UnityEngine.Events;

namespace Unidice.SDK.Interfaces
{
    public interface IUnidiceSides
    {
        /// <summary>
        /// Set all sides to black.
        /// </summary>
        void Clear();
        /// <summary>
        /// Set the given side to play a specific <see cref="ImageSequence"/>.
        /// </summary>
        void SetSide(ISide side, ImageSequence sequence);
        /// <summary>
        /// Set each side to a distinct <see cref="ImageSequence"/>. If you want to change all sides to the same, use <see cref="SetSide"/> with <see cref="SideLocal.All"/>.
        /// </summary>
        /// <param name="sequences"></param>
        void SetAllSides(params ImageSequence[] sequences);
        /// <summary>
        /// Starts listening to taps for the given side. Runs tapCallback with the local side that has been tapped.
        /// </summary>
        void EnableTap(ISide side, UnityAction<ISide> tapCallback);
        /// <summary>
        /// Stops listening to taps for the given side.
        /// </summary>
        void DisableTap(ISide side);
        /// <summary>
        /// Calls EnableTap and when tapped it calls DisableTap. tapCallback is called with the local side that has been tapped once this happens.
        /// </summary>
        public void WaitForTap(ISide side, UnityAction<ISide> tapCallback);
        /// <summary>
        /// Trigger a tap for the given side.
        /// </summary>
        void Tap(ISide side);
        /// <summary>
        /// Convert a <see cref="SideWorld"/> to the die's <see cref="SideLocal"/>.
        /// </summary>
        ISide WorldSideToLocal(ISide side);
        /// <summary>
        /// Await this to wait until the tap has happened. Returns true if it was tapped and false if DisableTap was called.
        /// </summary>
        UniTask<bool> WaitForTapSequence(ISide side, CancellationToken cancellationToken);
        /// <summary>
        /// Await this to wait until the tap has happened. Returns true if it was tapped and false if DisableTap was called.
        /// Runs tapCallback with the local side that has been tapped.
        /// </summary>
        UniTask<bool> WaitForTapSequence(ISide side, UnityAction<ISide> tapCallback, CancellationToken cancellationToken);
        /// <summary>
        /// Triggered when tap gets enabled for a side.
        /// </summary>
        SideEvent OnTapEnabled { get; }
        /// <summary>
        /// Triggered when tap gets disabled for a side.
        /// </summary>
        SideEvent OnTapDisabled { get; }
        /// <summary>
        /// Returns true if the given side is listening to a tap.
        /// </summary>
        bool CanTap(ISide side);
        /// <summary>
        /// Get the <see cref="ImageSequence"/> on the specified side.
        /// </summary>
        ImageSequence GetSideSequence(ISide side);
        /// <summary>
        /// Set the brightness of the specified side to a certain percentage.
        /// </summary>
        void SetBrightness(ISide side, float percentage);
        /// <summary>
        /// Get the brightness of the specified side in percent.
        /// </summary>
        float GetBrightness(ISide side);
    }
}