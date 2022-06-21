using Unidice.SDK.Interfaces;
using UnityEngine;

namespace Unidice.SDK.Unidice
{
    public abstract class UnidiceStub : MonoBehaviour
    {
        public abstract IUnidice GetUnidice();
    }
}