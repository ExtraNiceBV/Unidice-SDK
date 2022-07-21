using Unidice.SDK.Interfaces;
using UnityEngine;

namespace Unidice.SDK.Unidice
{
    public class UnidiceWrapper : IUnidice
    {
        public UnidiceWrapper()
        {
            Debug.LogWarning("Running with the real Unidice wrapper!");
            var images = new ImageDatabase();
            images.Initialize();
            Images = images;
        }

        public IImageDatabase Images { get; private set; }

        public IUnidiceSides Sides { get; private set; } // TODO: Implement all the bits

        public IUnidiceRotator Rotator { get; private set; }

        public bool IsValid => true;

        public void MoveToSecret(bool secret) { }
    }
}