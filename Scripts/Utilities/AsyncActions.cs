using System.Threading;
using Cysharp.Threading.Tasks;

namespace Unidice.SDK.Utilities
{
    public delegate UniTask AsyncAction(CancellationToken cancellationToken);
    public delegate UniTask AsyncAction<in T>(T arg, CancellationToken cancellationToken);
    public delegate UniTask AsyncAction<in T1, in T2>(T1 arg1, T2 arg2, CancellationToken cancellationToken);
    public delegate UniTask AsyncAction<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3, CancellationToken cancellationToken);
}