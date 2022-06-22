using System;
using UnityEngine;

namespace Unidice.SDK.Unidice
{
    [CreateAssetMenu(menuName = "Unidice/Image Sequence")]
    public class ImageSequence : ScriptableObject
    {
        public enum LoopMode
        {
            Once,
            Loop,
            PingPong,
            Random
        }

        public Texture2D[] backgroundLayers;
        public Texture2D[] animation;
        public Texture2D[] overlayLayers;
        public float fps = 12;
        public LoopMode loop;
        public Vector2Int size = new Vector2Int(240, 240);
        [NonSerialized] public int[] indices;
    }
}