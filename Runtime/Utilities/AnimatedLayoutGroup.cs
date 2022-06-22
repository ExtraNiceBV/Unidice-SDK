using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unidice.SDK.Utilities
{
    /// <summary>
    /// Used to smoothly move the items inside a layout group. See <see cref="Cyberrun.Utilities.AnimatedLayoutElement"/>, which is used for the actual moving.
    /// </summary>
    public class AnimatedLayoutGroup : UIBehaviour
    {
        [SerializeField] private float smoothTime = 1;
        private readonly Dictionary<RectTransform, RectTransform> _layoutElements = new Dictionary<RectTransform, RectTransform>();
        private bool _ongoingOperation;
        private bool _dirty;
        private readonly List<RectTransform> _destroyedTempList = new List<RectTransform>();

        private void InitializeChild(RectTransform child)
        {
            _ongoingOperation = true;
            var copy = new GameObject(child.name + " match", typeof(RectTransform));
            if(!child.gameObject.activeSelf) copy.SetActive(false);
            copy.hideFlags = HideFlags.HideAndDontSave;

            var layoutElement = child.gameObject.GetComponent<AnimatedLayoutElement>() ?? child.gameObject.AddComponent<AnimatedLayoutElement>();
            layoutElement.ignoreLayout = true;
            layoutElement.smoothTime = smoothTime;
            var rectCopy = copy.GetComponent<RectTransform>();
            _layoutElements.Add(child, rectCopy);

            rectCopy.SetParent(transform, false); // Careful: This triggers OnTransformChildrenChanged
            rectCopy.anchorMin = child.anchorMin;
            rectCopy.anchorMax = child.anchorMax;
            rectCopy.pivot = child.pivot;
            rectCopy.anchoredPosition = child.anchoredPosition;
            rectCopy.sizeDelta = child.sizeDelta;

            layoutElement.Initialize(rectCopy);
            _ongoingOperation = false;
        }

        protected void OnTransformChildrenChanged()
        {
            if (_ongoingOperation) return;
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
            if (Application.isPlaying && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) return; // Catch stopping in editor
#endif

            if (!_dirty)
            {
                Invoker.InvokeAfter(CheckForChanges, 1);
                _dirty = true;
            }
        }
        private void CheckForChanges()
        {
            // Add new children
            foreach (RectTransform child in transform)
            {
                var layoutElement = child.GetComponent<AnimatedLayoutElement>();
                if (layoutElement && layoutElement.Initialized) continue;
                if (_layoutElements.ContainsValue(child)) continue;
                //if (_layoutElements.ContainsKey(child)) continue;

                InitializeChild(child);
            }

            // Delete broken couples
            foreach (var element in _layoutElements)
            {
                if (!element.Key && element.Value)
                {
                    Destroy(element.Value.gameObject);
                    _destroyedTempList.Add(element.Key);
                }
                else if (!element.Value && element.Key)
                {
                    Destroy(element.Key.gameObject);
                    _destroyedTempList.Add(element.Key);
                }
            }

            foreach (var key in _destroyedTempList)
            {
                _layoutElements.Remove(key);
            }
            _destroyedTempList.Clear();

            // Update order
            _ongoingOperation = true;
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i) as RectTransform;
                if (child)
                {
                    if (_layoutElements.ContainsKey(child))
                    {
                        var element = _layoutElements[child];
                        element.transform.SetSiblingIndex(i);
                    }
                }
            }

            _ongoingOperation = false;
            _dirty = false;
        }
    }
}
