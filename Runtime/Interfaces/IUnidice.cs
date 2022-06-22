using Unidice.SDK.Unidice;

namespace Unidice.SDK.Interfaces
{
    public interface IUnidice
    {
        /// <summary>
        /// Control the images loaded to the Unidice.
        /// </summary>
        IImageDatabase Images { get; }

        /// <summary>
        /// Control the sides of the Unidice.
        /// </summary>
        IUnidiceSides Sides { get; }

        /// <summary>
        /// Get data from the rotation component of the Unidice.
        /// </summary>
        IUnidiceRotator Rotator { get; }

        /// <summary>
        /// Returns true if the reference is valid.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Move the Unidice in or out of the "secret box" in the simulator. Does nothing with the real Unidice.
        /// </summary>
        void MoveToSecret(bool secret);
    }
}