using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unidice.SDK.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

namespace Unidice.SDK.Unidice
{
    [Serializable]
    public class ImageDatabase : IImageDatabase
    {
        private const int DIMENSIONS = 240;

        private static Color32 _clearColor = Color.black;
        private static Color32[] _pixelsClearTexture = new Color32[DIMENSIONS * DIMENSIONS].Select(c => _clearColor).ToArray();
        private static Material _blitBackgroundMaterial;
        private static Material _blitLayerMaterial;
        [SerializeField] private Texture2D[] images;
        private Dictionary<Hash128, int> _indices; // Texture hash => image index
        private List<ImageSequence>[] _usage;
        
        public int Count { get; private set; }
        public bool Busy { get; private set; }

        private readonly List<int> _synchronizingQueue = new List<int>(); // Stores ids for sync requests
        private UnityEvent OnSynchronizeFinished { get; } = new UnityEvent();


        public async UniTask SynchronizeSequence(IEnumerable<ImageSequence> sequences, IProgress<float> progress, CancellationToken cancellationToken)
        {
            // If there is already one queued, don't queue again, but wait until the latest is complete
            if (_synchronizingQueue.Count > 1)
            {
                var last = _synchronizingQueue.Last();
                while (_synchronizingQueue.Contains(last))
                {
                    progress?.Report(0);
                    await OnSynchronizeFinished.GetAsyncEventHandler(cancellationToken).OnInvokeAsync(); // Wait until a sync is complete
                }
                return;
            }

            var syncId = GetNewSyncId();
            _synchronizingQueue.Add(syncId);

            if (_synchronizingQueue.Count > 1) // including new id
            {
                Debug.Log($"Waiting for {_synchronizingQueue.First()} to finish...");
                progress?.Report(0);
                await OnSynchronizeFinished.GetAsyncEventHandler(cancellationToken).OnInvokeAsync(); // After synchronization is complete, run it again
            }
            //var progress = Progress.Create<float>(p => Debug.Log($"Synchronizing... {p:P0}"));
            try
            {
                var array = sequences.ToArray();
                await SynchronizeSequence_Internal(array, progress, cancellationToken);
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while compiling sequences.");
                Debug.LogException(e);
            }

            _synchronizingQueue.Remove(syncId);
            OnSynchronizeFinished.Invoke();
        }

        private int GetNewSyncId()
        {
            int result = 0;
            while (result == 0 || _synchronizingQueue.Contains(result)) result = Random.Range(1, int.MaxValue);
            return result;
        }

        private async UniTask SynchronizeSequence_Internal(IEnumerable<ImageSequence> sequences, IProgress<float> progress, CancellationToken cancellationToken)
        {
            var newSequences = sequences.ToArray();
            var loadedSequences = _usage.Where(u => u != null).SelectMany(u => u).Distinct().ToArray();
            var toUnload = loadedSequences.Except(newSequences).ToArray();
            var toLoad = newSequences.Except(loadedSequences).ToArray();

            var total = toLoad.Length + toUnload.Length;
            var i = 0;
            foreach (var sequence in toLoad)
            {
                await LoadSequence(sequence, Progress.Create<float>(p => progress?.Report((i + p) / total)), cancellationToken, true);
                i++;
            }

            foreach (var sequence in toUnload)
            {
                await UnloadSequence(sequence, Progress.Create<float>(p => progress?.Report((i + p) / total)), cancellationToken);
                i++;
            }
            progress?.Report(1);
        }

        [Obsolete("Use Synchronize sequence instead and pass all sequences in use at once.")]
        public async UniTask LoadSequence(ImageSequence sequence, IProgress<float> progress, CancellationToken cancellationToken)
        {
            await LoadSequence(sequence, progress, cancellationToken, true);
        }

        private async UniTask LoadSequence(ImageSequence sequence, IProgress<float> progress, CancellationToken cancellationToken, bool dummy = false)
        {
            if (!sequence) Debug.LogError("Sequence is null.");
            else if (sequence.animation == null) Debug.LogError("Sequence animation is null.");
            else if (images == null) throw new Exception("Image Database has not been initialized.");

            await UniTask.WaitUntil(() => !Busy, cancellationToken: cancellationToken);

            Busy = true;

            var amount = sequence.animation.Length;

            if (sequence.indices != null)
            {
                // Sequence has already been parsed. This can happen after restarting. Still should get the updated indices, just in case.
            }


            var indices = new int[amount];

            var i = 0;
            foreach (var image in sequence.animation)
            {
                if (!image) Debug.LogError($"Image is null in sequence {sequence.name}.");

                Profiler.BeginSample("Hashing");
                var stack = GetStack(sequence, image);
                if (!CheckReadable(stack)) continue;
                var hash = CalcHash(stack);
                Profiler.EndSample();
                image.anisoLevel = 4;
                if (!_indices.TryGetValue(hash, out var index))
                {
                    var texture = Convert(stack, sequence.size.x, sequence.size.y);
                    texture.name = image.name;

                    index = Array.IndexOf(images, null);
                    images[index] = texture;

                    // Add to device
                    await UniTask.Delay(TimeSpan.FromSeconds(0.01f), cancellationToken: cancellationToken);
                    //Debug.Log($"Loaded {image.name} at index {index} as part of sequence {sequence.name}.".Colored(Color.gray));

                    _indices.Add(hash, index);
                    Count++;
                }
                if (_usage[index] == null) _usage[index] = new List<ImageSequence> { sequence };
                else _usage[index].Add(sequence);

                indices[i++] = index;
                progress?.Report((float)i / amount);
            }

            sequence.indices = indices;

            Busy = false;
        }

        public IEnumerable<Texture2D> GetImages()
        {
            return images.Where(i => i);
        }

        public void Initialize()
        {
            _blitBackgroundMaterial = new Material(Shader.Find("Shader Graphs/UnidiceBlitBackground"));
            _blitLayerMaterial = new Material(Shader.Find("Shader Graphs/UnidiceBlitLayer"));
            images = new Texture2D[100];
            _usage = new List<ImageSequence>[100];
            _indices = new Dictionary<Hash128, int>(100);
        }

        private async UniTask UnloadSequence(ImageSequence sequence, IProgress<float> progress, CancellationToken cancellationToken)
        {
            if (!sequence) Debug.LogError("Sequence is null.");
            else if (sequence.animation == null) Debug.LogError("Sequence animation is null.");
            else if (images == null) throw new Exception("Image Database has not been initialized.");

            if (Busy) await UniTask.WaitUntil(() => !Busy, cancellationToken: cancellationToken);

            Busy = true;

            var indices = sequence.indices;
            sequence.indices = null;

            var i = 0;
            foreach (var index in indices)
            {
                var usedBy = _usage[index];
                usedBy.Remove(sequence);

                if (usedBy.Count == 0)
                {
                    // Delete from device
                    await UniTask.Delay(TimeSpan.FromSeconds(0.01f), cancellationToken: cancellationToken);

                    // No longer in use
                    foreach (var pair in _indices.ToArray())
                    {
                        if (pair.Value == index) _indices.Remove(pair.Key);
                    }

                    Debug.Log($"Unloaded image {images[index].name} at index {index}. (no longer in use)".Colored(Color.gray));
                    images[index] = null;
                    Count--;
                }

                i++;
                progress?.Report((float)i / indices.Length);
            }

            Busy = false;
        }

        private bool CheckReadable(Texture2D[] stack)
        {
            var isReadable = true;
            foreach (var item in stack)
                if (!item.isReadable)
                {
                    Debug.LogError($"Texture {item.name} must be Read/Write enabled!", item);
                    EditorWrapper.AskMakeReadable(item).Forget();
                    isReadable = false;
                }

            return isReadable;
        }

        private Hash128 CalcHash(Texture2D[] stack)
        {
            var hash = new Hash128();
            foreach (var item in stack)
            {
                var inHash = new Hash128();
                HashUtilities.ComputeHash128(item.GetRawTextureData(), ref inHash);
                if (!hash.isValid) hash = inHash;
                else HashUtilities.AppendHash(ref inHash, ref hash);
            }

            return hash;
        }

        private Texture2D[] GetStack(ImageSequence sequence, Texture2D image)
        {
            // Concatenate all backgroundLayers, the image, and then all overlayLayers
            var size = (sequence.backgroundLayers?.Length ?? 0) + 1 + (sequence.overlayLayers?.Length ?? 0);
            var stack = new Texture2D[size];
            var i = 0;
            if (sequence.backgroundLayers != null)
                foreach (var img in sequence.backgroundLayers)
                    stack[i++] = img;

            stack[i++] = image;

            if (sequence.overlayLayers != null)
                foreach (var img in sequence.overlayLayers)
                    stack[i++] = img;

            return stack;
        }

        private static Texture2D Convert(Texture2D[] stack, int newWidth, int newHeight)
        {
            var rt = RenderTexture.GetTemporary(newWidth, newHeight, 16, GraphicsFormat.R8G8B8A8_SRGB, 8);
            rt.filterMode = FilterMode.Bilinear;
            RenderTexture.active = rt;
            newWidth = Mathf.Clamp(newWidth, 0, DIMENSIONS);
            newHeight = Mathf.Clamp(newHeight, 0, DIMENSIONS);
            var nTex = new Texture2D(DIMENSIONS, DIMENSIONS);
            nTex.SetPixels32(_pixelsClearTexture);
            nTex.anisoLevel = 4;
            nTex.filterMode = FilterMode.Bilinear;
            nTex.wrapMode = TextureWrapMode.Clamp;
            var first = true;
            foreach (var source in stack)
                if (first)
                {
                    Graphics.Blit(source, rt, _blitBackgroundMaterial);
                    first = false;
                }
                else
                {
                    Graphics.Blit(source, rt, _blitLayerMaterial);
                }

            var marginX = DIMENSIONS / 2 - newWidth / 2;
            var marginY = DIMENSIONS / 2 - newHeight / 2;
            nTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), marginX, marginY);
            nTex.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            return nTex;
        }

        public Texture2D GetTexture(int index)
        {
            return images[index];
        }
    }
}