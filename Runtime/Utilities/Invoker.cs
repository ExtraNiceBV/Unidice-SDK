using System;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

namespace Unidice.SDK.Utilities
{
    public static class Invoker
    {
        public static async void Invoke(Action action, float delay, DelayType delayType, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            if (delay <= 0)
                await UniTask.NextFrame(timing);
            else
                await UniTask.Delay(TimeSpan.FromSeconds(delay), delayType, timing);
            if (action.Target is Object target && !target) return; // The reference has become null
            action.Invoke();
        }

        public static async void InvokeWhen(Action action, Func<bool> condition, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            await UniTask.WaitUntil(condition, timing);
            if (action.Target is Object target && !target) return; // The reference has become null
            action.Invoke();
        }

        public static async void InvokeAfter(Action action, int frames, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            if (frames > 0) await UniTask.DelayFrame(frames, timing);

            if (action.Target is Object target && !target) return; // The reference has become null
            action.Invoke();
        }
    }
}