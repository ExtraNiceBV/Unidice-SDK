using Unidice.SDK.Interfaces;
using UnityEngine;

namespace Unidice.SDK.Unidice.Dummy
{
    public class UnidiceDummy : IUnidice
    {
        public UnidiceDummy()
        {
            Debug.LogWarning("Running with Unidice Dummy!");
            var images = new ImageDatabase();
            images.Initialize();
            Images = images;
        }

        public IImageDatabase Images { get; }

        public IUnidiceSides Sides { get; } = new UnidiceSidesDummy();

        public IUnidiceRotator Rotator { get; } = new UnidiceRotatorDummy();

        public bool IsValid => true;

        public void MoveToSecret(bool secret) { }
    }
}