﻿using System;
using UnityEngine;

namespace Unidice.SDK.Unidice
{
    [CreateAssetMenu(menuName = "Unidice/Image Sequence")]
    public class ImageSequence : ScriptableObject
    {
        public const int IMAGE_PIXEL_SIZE = 240;
    
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
        [NonSerialized] public int[] indices;
        public int Frames => Mathf.Max(1, animation.Length);
    }
}