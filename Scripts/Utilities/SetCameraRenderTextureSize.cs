using UnityEngine;
using UnityEngine.Serialization;

namespace Unidice.SDK.Utilities
{
    [ExecuteInEditMode]
    public class SetCameraRenderTextureSize : MonoBehaviour
    {
        public Texture renderTexture;
        public Camera targetCamera;
        [FormerlySerializedAs("overlayCamera")] 
        public Camera baseCamera;


#if UNITY_EDITOR
        public void Update()
        {
            if (targetCamera && renderTexture && baseCamera)
            {
                Resize(targetCamera.pixelWidth, targetCamera.pixelHeight);
            }
        }
#endif

        public void Start() 
        {
            if (targetCamera && renderTexture && baseCamera)
            {
                Resize(targetCamera.pixelWidth, targetCamera.pixelHeight);

                // Call render on base camera or it won't initialize properly
                baseCamera.Render();
            }
        }

        private void Resize(int newRenderTextureWidth, int newRenderTextureHeight)
        {
            baseCamera.targetTexture.Release();
            renderTexture.width = newRenderTextureWidth;
            renderTexture.height = newRenderTextureHeight;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.ClearDirty(renderTexture);
#endif
        }

    }
}