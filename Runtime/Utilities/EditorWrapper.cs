#if UNITY_EDITOR
using UnityEditor;
#else
using System;
#endif
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;


namespace Unidice.SDK.Utilities
{
    public static class EditorWrapper
    {
        [Conditional("UNITY_EDITOR")]
        public static void DrawRotation(Vector3 position, Vector3 forward, Quaternion rotation, Color color, int size = 50)
        {
#if UNITY_EDITOR
            rotation.ToAngleAxis(out var angle, out var axis);
            if (angle <= 0) return;
            Handles.color = color;
            Handles.DrawSolidArc(position, axis, forward, angle, size);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void DrawRotation(Vector3 position, Vector3 forward, float angle, Color color, int size = 50)
        {
#if UNITY_EDITOR
            if (float.IsNaN(angle)) return;
            Handles.color = color;
            Handles.DrawSolidArc(position, Vector3.forward, forward, angle, size);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void SetDirty(Object obj)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(obj);
#endif
        }

        private static readonly HashSet<Texture2D> _texturesCheckedThisFrame = new HashSet<Texture2D>();
        public static async UniTask AskMakeReadable(Texture2D item)
        {
            if (_texturesCheckedThisFrame.Contains(item)) return;
            _texturesCheckedThisFrame.Add(item);
            if (item.isReadable) return;
#if UNITY_EDITOR
            var agreed = EditorUtility.DisplayDialog($"Texture not read/write enabled", $"The texture {item.name} is not read/write enabled, but needs to be.\nEnable this now?", "Yes", "No");
            if (!agreed) return;
            var path = AssetDatabase.GetAssetPath(item);
            if (path == null)
            {
                Debug.LogError($"Can't access importer for {item.name}.");
                return;
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.isReadable = true;
            SetDirty(importer);
            importer.SaveAndReimport();
#endif
            await UniTask.NextFrame();
            _texturesCheckedThisFrame.Clear();
        }

        public static IEnumerable<T> GetAllAssets<T>() where T : Object
        {
#if UNITY_EDITOR
            return AssetDatabase.FindAssets($"t:{typeof(T).FullName}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(item => item);
#else
            return Array.Empty<T>();
#endif
        }
    }
}