using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Unidice.SDK.Unidice
{
    public interface IImageDatabase
    {
        [Obsolete("Use Synchronize sequence instead and pass all sequences in use at once.")]
        UniTask LoadSequence(ImageSequence sequence, IProgress<float> progress = null, CancellationToken cancellationToken = new CancellationToken());
        
        /// <summary>
        /// Get all images stored on the Unidice.
        /// </summary>
        IEnumerable<Texture2D> GetImages();

        /// <summary>
        /// Synchronize a list of <see cref="ImageSequence"/> with the Unidice. Loads and unloads images to match the list. When called multiple times, will queue up to one request. All further requests will hold until the queued request was processed.
        /// </summary>
        /// <param name="sequences">A list of <see cref="ImageSequence"/> to load.</param>
        /// <param name="progress">Reports progress in percent.</param>
        UniTask SynchronizeImagesSequence(IEnumerable<ImageSequence> sequences, IProgress<float> progress, CancellationToken cancellationToken);

        /// <summary>
        /// Make sure a list of <see cref="ImageSequence"/> are loaded on the Unidice. Loads and unloads images as needed, but doesn't remove unlisted images (as <see cref="SynchronizeImagesSequence"/> would). When called multiple times, will queue up to one request. All further requests will hold until the queued request was processed.
        /// This is useful if you want to make sure certain sequences are loaded, without unloading all the game assets.
        /// </summary>
        /// <param name="sequences">A list of <see cref="ImageSequence"/> to load.</param>
        /// <param name="progress">Reports progress in percent.</param>
        UniTask LoadImagesSequence(IEnumerable<ImageSequence> sequences, IProgress<float> progress, CancellationToken cancellationToken);
    }
}