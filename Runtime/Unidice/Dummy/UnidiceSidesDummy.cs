using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unidice.SDK.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace Unidice.SDK.Unidice.Dummy
{
    public class UnidiceSidesDummy : IUnidiceSides
    {
        public void Initialize(Transform transform, ImageDatabase database) { }

        public void Clear() { }

        public void SetSide(ISide side, ImageSequence sequence) { }

        public void SetAllSides(params ImageSequence[] sequences) { }

        public void EnableTap(ISide side, UnityAction<ISide> tapCallback) { }

        public void DisableTap(ISide side) { }

        public void WaitForTap(ISide side, UnityAction<ISide> tapCallback)
        {
            WaitForTapSequence(side, tapCallback, CancellationToken.None).Forget();
        }

        public void Tap(ISide side) { }

        public ISide WorldSideToLocal(ISide side) => side;

        public async UniTask<bool> WaitForTapSequence(ISide side, CancellationToken cancellationToken)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cancellationToken);
            return true;
        }

        public async UniTask<bool> WaitForTapSequence(ISide side, UnityAction<ISide> tapCallback, CancellationToken cancellationToken)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cancellationToken);
            tapCallback?.Invoke(side);
            return true;
        }

        public SideEvent OnTapEnabled { get; } = new SideEvent();

        public SideEvent OnTapDisabled { get; } = new SideEvent();

        public bool CanTap(ISide side) => true;
        public ImageSequence GetSideSequence(ISide side) => null;
        public void SetBrightness(ISide side, float percentage) { }
        public float GetBrightness(ISide side) => 1;
    }
}