using Unidice.SDK.Interfaces;
using Unidice.SDK.Unidice.Dummy;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unidice.SDK.Unidice
{
    public static class UnidiceSDK
    {
        public static IUnidice GetUnidice()
        {
            var stub = Object.FindObjectOfType<UnidiceStub>();
            if (stub) return stub.GetUnidice();

            Debug.LogWarning("Implement once SDK is available."); // TODO: Apploader should check if unidice is connected and if not, load the simulator.
            return UnidiceDummy;
        }

        public static IUnidice UnidiceDummy { get; } = new UnidiceDummy();
    }
}