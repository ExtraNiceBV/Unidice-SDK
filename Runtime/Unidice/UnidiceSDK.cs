using Unidice.SDK.Interfaces;
using Unidice.SDK.Unidice.Dummy;
using Object = UnityEngine.Object;

namespace Unidice.SDK.Unidice
{
    public static class UnidiceSDK
    {
        public static IUnidice GetUnidice()
        {
            var stub = Object.FindObjectOfType<UnidiceStub>(); // This tries to find the simulator
            if (stub) return stub.GetUnidice();

            // TODO: Apploader should check if unidice is connected and if not, load the simulator.
            return UnidiceWrapper ??= new UnidiceWrapper();
            return UnidiceDummy ??= new UnidiceDummy();
        }

        public static IUnidice UnidiceWrapper { get; private set; }
        public static IUnidice UnidiceDummy { get; private set; }
    }
}