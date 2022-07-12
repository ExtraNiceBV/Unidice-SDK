using Unidice.SDK.Interfaces;
using UnityEngine.Events;

namespace Unidice.SDK.Unidice.Dummy
{
    public class UnidiceRotatorDummy : IUnidiceRotator
    {
        public UnityEvent OnRotated { get; } = new UnityEvent();

        public UnityEvent OnRolled { get; } = new UnityEvent();

        public UnityEvent OnShake { get; } = new UnityEvent();

        public UnityEvent OnStartedRolling { get; } = new UnityEvent();

        public bool RollInSecret { get; set; }
    }
}