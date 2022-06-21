using UnityEngine;
using UnityEngine.UI;

namespace Unidice.SDK.Utilities
{
    [AddComponentMenu("")] // Don't show in menu
    public class AnimatedLayoutElement : LayoutElement
    {
        public float smoothTime = 1;
        private RectTransform _rect;
        private RectTransform _source;

        private Vector2 _anchoredPosition;
        private Vector2 _localPosition;

        public bool Initialized { get; private set; }

        public void Initialize(RectTransform source)
        {
            Initialized = true;
            _source = source;
            _rect = GetComponent<RectTransform>();
        }

        public void Update()
        {
            if (!Initialized) return;
            if (_source.anchoredPosition != _rect.anchoredPosition)
            {
                _rect.anchoredPosition = Vector2.SmoothDamp(_rect.anchoredPosition, _source.anchoredPosition, ref _anchoredPosition, smoothTime);
            }
            if (_source.localPosition != _rect.localPosition)
            {
                _rect.localPosition = Vector2.SmoothDamp(_rect.localPosition, _source.localPosition, ref _localPosition, smoothTime);
            }
        }

        protected override void OnDisable()
        {
            if(_source) _source.gameObject.SetActive(false);
        }

        protected override void OnEnable()
        {
            if(_source) _source.gameObject.SetActive(true);
        }
    }
}