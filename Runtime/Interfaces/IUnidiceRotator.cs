using UnityEngine.Events;

namespace Unidice.SDK.Interfaces
{
    public interface IUnidiceRotator
    {
        UnityEvent OnRotated { get; }
        UnityEvent OnRolled { get; }
        UnityEvent OnShake { get; }
        UnityEvent OnStartedRolling { get; }
        bool RollInSecret { get; set; }
    }
}