using UnityEngine;
using UnityEngine.EventSystems;

namespace Unidice.SDK.System
{
    /// <summary>
    /// Attach this component to a camera to automatically connect it to the simulator screen.
    /// </summary>
    [AddComponentMenu("Simulator/App Raycaster Target")]
    public class AppRaycasterTarget : MonoBehaviour
    {
#pragma warning disable CS0109 // complaining about new being not needed
        public new Camera[] cameras;
        public EventSystem eventSystem;
#pragma warning restore CS0109
    }
}