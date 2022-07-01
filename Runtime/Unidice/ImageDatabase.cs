using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public const int MAX_IMAGES = 80;
        private static Color32 _clearColor = Color.black;
        private static Color32[] _pixelsClearTexture = new Color32[ImageSequence.IMAGE_PIXEL_SIZE * ImageSequence.IMAGE_PIXEL_SIZE].Select(c => _clearColor).ToArray();
        private static Material _blitBackgroundMaterial;
        private static Material _blitLayerMaterial;
        [SerializeField] private Texture2D[] loadedImages;
        private Dictionary<Texture2D, int> _indices; // Texture hash => image index
        private List<ImageSequence>[] _usage; // for each image index, a list of sequences that use the image
        private Dictionary<Texture2D, Hash128> _hashes; // Texture => image hash
        private Dictionary<Hash128, Texture2D> _images; // Image hash => texture

        public int EncodeQuality { get; set; } = 75;
        public bool Verbose { get; set; } = true;
        public int Count { get; private set; }
        public int BytesInUse { get; private set; }
        public bool Busy { get; private set; }

        private readonly List<int> _operationQueue = new List<int>(); // Stores ids for sync requests
        private UnityEvent OnOperationFinished { get; } = new UnityEvent();


        public async UniTask SynchronizeImagesSequence(IEnumerable<ImageSequence> sequences, IProgress<float> progress, CancellationToken cancellationToken)
        {
            async UniTask Operation(CancellationToken cancellationToken) => await SynchronizeSequence_Internal(sequences.ToArray(), true, progress, cancellationToken);

            await QueueOperation(Operation, progress, cancellationToken);
        }

        public async UniTask LoadImagesSequence(IEnumerable<ImageSequence> sequences, IProgress<float> progress, CancellationToken cancellationToken)
        {
            async UniTask Operation(CancellationToken cancellationToken) => await SynchronizeSequence_Internal(sequences.ToArray(), false, progress, cancellationToken);

            await QueueOperation(Operation, progress, cancellationToken);
        }

        private async UniTask QueueOperation(AsyncAction operation, IProgress<float> progress, CancellationToken cancellationToken)
        {
            // If there is already one queued, don't queue again, but wait until the latest is complete
            if (_operationQueue.Count > 1)
            {
                var last = _operationQueue.Last();
                while (_operationQueue.Contains(last))
                {
                    progress?.Report(0);
                    await OnOperationFinished.GetAsyncEventHandler(cancellationToken)
                        .OnInvokeAsync(); // Wait until a sync is complete
                }

                return;
            }

            var syncId = GetNewSyncId();
            _operationQueue.Add(syncId);

            if (_operationQueue.Count > 1) // including new id
            {
                if (Verbose)
                    Debug.Log($"Waiting for {_operationQueue.First()} to finish...");
                progress?.Report(0);
                await OnOperationFinished.GetAsyncEventHandler(cancellationToken)
                    .OnInvokeAsync(); // After synchronization is complete, run it again
            }

            //var progress = Progress.Create<float>(p => Debug.Log($"Synchronizing... {p:P0}"));
            try
            {
                await operation(cancellationToken);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Debug.LogError($"Exception while running operation.");
            }

            _operationQueue.Remove(syncId);
            OnOperationFinished.Invoke();
        }

        private int GetNewSyncId()
        {
            int result = 0;
            while (result == 0 || _operationQueue.Contains(result)) result = Random.Range(1, int.MaxValue);
            return result;
        }

        private async UniTask SynchronizeSequence_Internal(ImageSequence[] requiredSequences, bool removeUnused, IProgress<float> progress, CancellationToken cancellationToken)
        {
            var toRender = requiredSequences.Where(s => s.Frames == null).ToArray();
            var i = 0;

            // Render the sequences and store the rendered frames in each sequence
            // This is fairly expensive, but okay if called sparingly
            foreach (var sequence in toRender)
            {
                await RenderSequence(sequence, Progress.Create<float>(p => progress?.Report((i + p) / toRender.Length / 3)), cancellationToken);
                i++;
                if (i % 5 == 0) await UniTask.Yield(cancellationToken);
            }

            // Determine what needs to be loaded and unloaded
            // This looks expensive, but with a set of no more than 100 images that's neglectable
            var loadedSequences = _usage.Where(u => u != null).SelectMany(u => u).ToArray();
            var loadedFrames = loadedSequences.SelectMany(s => s.Frames).ToArray();
            var toLoad = requiredSequences.Except(loadedSequences).ToArray();
            var framesToLoad = toLoad.SelectMany(s => s.Frames);
            var requiredFrames = requiredSequences.SelectMany(s => s.Frames).Distinct().ToArray();  // Distinct and Union works, because identical renders use the same texture via hashes
            var unloadableFrames = loadedFrames.Except(requiredFrames).ToList();

            if (requiredFrames.Length > MAX_IMAGES) throw new Exception($"Tried to load more images ({requiredFrames.Length}) than possible ({MAX_IMAGES}).");

            var unload = new HashSet<ImageSequence>();
            if (removeUnused)
            {
                var unusedSequences = loadedSequences.Except(requiredSequences);
                unload.AddRangeArray(unusedSequences);
            }
            else
            {
                var framesThatWillBeUsed = loadedFrames.Union(framesToLoad);
                var neededToUnload = framesThatWillBeUsed.Count() - MAX_IMAGES;
                int count = 0;
                while (count < neededToUnload)
                {
                    if (unloadableFrames.Count == 0) throw new Exception("This should have been caught by the previous exception.");
                    var frame = unloadableFrames[0];
                    var index = _indices[frame];
                    var unloadableSequences = _usage[index];
                    if(Verbose)
                        Debug.Log($"To unload {frame.name}, we have to unload the following sequences:\n{unloadableSequences.Select(s => s.name).Aggregate((a, b) => $"{a}\n{b}")}");
                    unload.AddRangeArray(unloadableSequences);
                    unloadableFrames.Remove(frame);
                    count++;
                }
            }

            var total = toLoad.Length + unload.Count;
            i = 0;

            foreach (var sequence in unload)
            {
                await UnloadSequence(sequence, Progress.Create<float>(p => progress?.Report((i + p) / total *2/3)), cancellationToken);
                i++;
            }
            foreach (var sequence in toLoad)
            {
                await LoadSequence(sequence, Progress.Create<float>(p => progress?.Report((i + p) / total *2/3)), cancellationToken);
                i++;
            }

            progress?.Report(1);
        }

        private async UniTask RenderSequence(ImageSequence sequence, IProgress<float> progress, CancellationToken cancellationToken)
        {
            var i = 0;
            var length = Mathf.Max(1, sequence.Animation.Count);
            if (sequence.Frames != null) return;

            var frames = new Texture2D[length];
            foreach (var image in sequence.Animation)
            {
                if (!image) Debug.LogError($"Image is null in sequence {sequence.name}.");
                Profiler.BeginSample("Render animation sequence frame");

                // Render texture
                var frame = RenderFrame(sequence, image);

                // Compute hash if needed
                if (!_hashes.TryGetValue(frame, out var hash))
                {
                    HashUtilities.ComputeHash128(frame.GetRawTextureData(), ref hash);
                    _hashes.Add(frame, hash);
                }

                // Check if texture has been created before (and use that one)
                if (_images.TryGetValue(hash, out var hashedTexture))
                {
                    frame = hashedTexture;
                }
                else
                {
                    _images.Add(hash, frame);
                }

                frames[i] = frame;
                Profiler.EndSample();

                i++;
                progress.Report(1f * i / length);
                if (i % 5 == 0) await UniTask.Yield(cancellationToken); // Do 5 per frame
            }
            sequence.Frames = frames;
            progress.Report(1);
        }

        private Texture2D RenderFrame(ImageSequence sequence, Texture2D image)
        {
            var stack = GetStack(sequence, image);
            if (!CheckReadable(stack)) return null;
            var texture = Convert(stack, ImageSequence.IMAGE_PIXEL_SIZE, ImageSequence.IMAGE_PIXEL_SIZE);
            texture.name = image.name;
            texture.anisoLevel = 4;
            return texture;
        }

        private async UniTask LoadSequence(ImageSequence sequence, IProgress<float> progress, CancellationToken cancellationToken)
        {
            if (!sequence) Debug.LogError("Sequence is null.");
            else if (sequence.Animation == null) Debug.LogError("Sequence animation is null.");
            else if (loadedImages == null) throw new Exception("Image Database has not been initialized.");

            await UniTask.WaitUntil(() => !Busy, cancellationToken: cancellationToken);

            Busy = true;

            var amount = sequence.Frames.Count;

            if (sequence.Indices != null)
            {
                // Sequence has already been parsed. This can happen after restarting. Still should get the updated indices, just in case.
            }


            var indices = new int[amount];

            var i = 0;
            foreach (var frame in sequence.Frames)
            {
                if (!frame) Debug.LogError($"Frame is null in sequence {sequence.name}.");
            

                if (!_indices.TryGetValue(frame, out var index))
                {
                    index = Array.IndexOf(loadedImages, null);
                    await PushToDevice(index, frame, sequence, cancellationToken);

                    _indices.Add(frame, index);
                    Count++;
                }
                if (_usage[index] == null) _usage[index] = new List<ImageSequence> { sequence };
                else _usage[index].Add(sequence);
            
                indices[i++] = index;
                progress?.Report((float)i / amount);
            }

            sequence.Indices = indices;

            Busy = false;
        }

        private async UniTask PushToDevice(int index, Texture2D texture, ImageSequence sequence, CancellationToken cancellationToken)
        {
            var data = texture.EncodeToJPG(EncodeQuality);

            // Decode for display purposes
            var success = texture.LoadImage(data, false);
            Debug.Assert(success, $"Failed to decode texture {texture.name} again.", texture);

            loadedImages[index] = texture;

            var size = data.Length;
            BytesInUse += size;

            // Add to device
            await UniTask.Delay(TimeSpan.FromSeconds(0.05f), cancellationToken: cancellationToken);

            if(Verbose)
                Debug.Log($"Loaded {texture.name} at index {index} as part of sequence {sequence.name}. Size: {(size/1024f):F2}kb".Colored(Color.gray));
        }

        public IEnumerable<Texture2D> GetImages()
        {
            return loadedImages.Where(i => i);
        }

        public void Initialize()
        {
            _blitBackgroundMaterial = new Material(Shader.Find("Shader Graphs/UnidiceBlitBackground"));
            _blitLayerMaterial = new Material(Shader.Find("Shader Graphs/UnidiceBlitLayer"));
            loadedImages = new Texture2D[MAX_IMAGES];
            _usage = new List<ImageSequence>[MAX_IMAGES];
            _indices = new Dictionary<Texture2D, int>(MAX_IMAGES);
            _hashes = new Dictionary<Texture2D, Hash128>(MAX_IMAGES);
            _images = new Dictionary<Hash128, Texture2D>(MAX_IMAGES);
        }

        private async UniTask UnloadSequence(ImageSequence sequence, IProgress<float> progress, CancellationToken cancellationToken)
        {
            if (!sequence) Debug.LogError("Sequence is null.");
            else if (sequence.Animation == null) Debug.LogError("Sequence animation is null.");
            else if (loadedImages == null) throw new Exception("Image Database has not been initialized.");

            if (Busy) await UniTask.WaitUntil(() => !Busy, cancellationToken: cancellationToken);

            Busy = true;

            var indices = sequence.Indices;
            sequence.Indices = null;

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

                    // Update usage; TODO: Do this smarter later
                    var data = loadedImages[index].EncodeToJPG(EncodeQuality);
                    var size = data.Length;
                    BytesInUse -= size;

                    if (Verbose)
                        Debug.Log($"Unloaded image {loadedImages[index].name} at index {index}. (no longer in use)".Colored(Color.gray));
                    loadedImages[index] = null;
                    Count--;
                }

                i++;
                progress?.Report((float)i / indices.Count);
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

        private Hash128 CalcHash(Texture2D texture)
        {
            var hash = new Hash128();
            HashUtilities.ComputeHash128(texture.GetRawTextureData(), ref hash);
            return hash;
        }

        private Texture2D[] GetStack(ImageSequence sequence, Texture2D image)
        {
            // Concatenate all backgroundLayers, the image, and then all overlayLayers
            var size = (sequence.BackgroundLayers?.Count ?? 0) + 1 + (sequence.OverlayLayers?.Count ?? 0);
            var stack = new Texture2D[size];
            var i = 0;
            if (sequence.BackgroundLayers != null)
                foreach (var img in sequence.BackgroundLayers)
                    stack[i++] = img;

            stack[i++] = image;

            if (sequence.OverlayLayers != null)
                foreach (var img in sequence.OverlayLayers)
                    stack[i++] = img;

            return stack;
        }

        private static Texture2D Convert(Texture2D[] stack, int newWidth, int newHeight)
        {
            var rt = RenderTexture.GetTemporary(newWidth, newHeight, 16, GraphicsFormat.R8G8B8A8_SRGB, 8);
            rt.filterMode = FilterMode.Bilinear;
            RenderTexture.active = rt;
            newWidth = Mathf.Clamp(newWidth, 0, ImageSequence.IMAGE_PIXEL_SIZE);
            newHeight = Mathf.Clamp(newHeight, 0, ImageSequence.IMAGE_PIXEL_SIZE);
            var nTex = new Texture2D(ImageSequence.IMAGE_PIXEL_SIZE, ImageSequence.IMAGE_PIXEL_SIZE);
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

            var marginX = ImageSequence.IMAGE_PIXEL_SIZE / 2 - newWidth / 2;
            var marginY = ImageSequence.IMAGE_PIXEL_SIZE / 2 - newHeight / 2;
            nTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), marginX, marginY);
            nTex.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            return nTex;
        }

        public Texture2D GetTexture(int index)
        {
            return loadedImages[index];
        }
    }
}