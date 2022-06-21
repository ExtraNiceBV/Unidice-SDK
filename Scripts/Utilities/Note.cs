using UnityEngine;

namespace Unidice.SDK.Utilities
{
    public class Note : MonoBehaviour
    {
        [SerializeField, TextArea(0, 10)] private string text;
    }
}