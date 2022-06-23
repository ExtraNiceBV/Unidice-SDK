using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Unidice.SDK.System
{
    public abstract class AppLoaderLoadStep : MonoBehaviour
    {
        public abstract UniTask Execute(CancellationToken cancellationToken);
        public abstract string Label { get; }
    }
}