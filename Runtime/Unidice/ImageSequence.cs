using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unidice.SDK.Unidice
{
    [CreateAssetMenu(menuName = "Unidice/Image Sequence")]
    public class ImageSequence : ScriptableObject
    {
        public static ImageSequence Create(string name, Texture2D[] animation, Texture2D[] backgroundLayers = null, Texture2D[] overlayLayers = null, LoopMode loop = LoopMode.Loop, float fps = 12, float quality = 0.5f)
        {
            var obj = CreateInstance<ImageSequence>();
            obj.name = name;
            obj.animation = animation;
            obj.backgroundLayers = backgroundLayers ?? Array.Empty<Texture2D>();
            obj.overlayLayers = overlayLayers ?? Array.Empty<Texture2D>();
            obj.loop = loop;
            obj.fps = fps;
            obj.quality = quality;
            return obj;
        }

        public const int IMAGE_PIXEL_SIZE = 240;
        public const int MAX_FPS = 10;
    
        public enum LoopMode
        {
            Once,
            Loop,
            PingPong,
            Random
        }

        [SerializeField] private Texture2D[] backgroundLayers;
        [SerializeField] private Texture2D[] animation;
        [SerializeField] private Texture2D[] overlayLayers;
        [SerializeField] private float fps = 12;
        [SerializeField, Range(0, 1)] private float quality = 0.5f;
        [SerializeField] private LoopMode loop;

        public IReadOnlyList<Texture2D> BackgroundLayers => backgroundLayers;
        public IReadOnlyList<Texture2D> Animation => animation;
        public IReadOnlyList<Texture2D> OverlayLayers => overlayLayers;
        public float FPS => MathF.Min(fps, MAX_FPS);
        public LoopMode Loop => loop;
        [field: NonSerialized] public IReadOnlyList<int> Indices { get; internal set; }
        [field: NonSerialized] public IReadOnlyList<Texture2D> Frames { get; internal set; }

        public int EncodeQuality => (int)(quality * 100);
    }
}