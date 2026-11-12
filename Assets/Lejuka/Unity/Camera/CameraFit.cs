using UnityEngine;

namespace Lejuka.Unity.Camera
{
    /// <summary>
    /// Keeps the whole table in view on any screen shape and stretches the room background to cover the view.
    /// Note: inside Lejuka.Unity.* a bare "Camera" means this namespace, so the Unity type is written in full.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    public sealed class CameraFit : MonoBehaviour
    {
        [Tooltip("Orthographic size on wide screens, where the table height is what has to fit.")]
        [SerializeField, Min(0.01f)] private float _minOrthographicSize = 5f;

        [Tooltip("Half the table width, in world units, that must stay visible on narrow screens.")]
        [SerializeField, Min(0.01f)] private float _requiredHalfWidth = 8.8f;

        [Tooltip("Optional. Scaled so it always covers the whole view.")]
        [SerializeField] private SpriteRenderer _background;

        private UnityEngine.Camera _camera;
        private int _lastWidth;
        private int _lastHeight;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
            Apply();
        }

        private void Update()
        {
            if (Screen.width != _lastWidth || Screen.height != _lastHeight)
            {
                Apply();
            }
        }

        private void Apply()
        {
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;

            var aspect = _camera.aspect;
            if (aspect <= 0f)
            {
                return;
            }

            var size = Mathf.Max(_minOrthographicSize, _requiredHalfWidth / aspect);
            _camera.orthographicSize = size;

            FitBackground(size * 2f * aspect, size * 2f);
        }

        private void FitBackground(float viewWidth, float viewHeight)
        {
            if (_background == null || _background.sprite == null)
            {
                return;
            }

            var spriteSize = _background.sprite.bounds.size;
            if (spriteSize.x <= 0f || spriteSize.y <= 0f)
            {
                return;
            }

            // The larger of the two ratios covers the view in both directions, cropping rather than leaving gaps.
            var scale = Mathf.Max(viewWidth / spriteSize.x, viewHeight / spriteSize.y);

            var parent = _background.transform.parent;
            var parentScale = parent != null ? parent.lossyScale : Vector3.one;
            _background.transform.localScale = new Vector3(
                scale / Mathf.Max(parentScale.x, Mathf.Epsilon),
                scale / Mathf.Max(parentScale.y, Mathf.Epsilon),
                1f);
        }
    }
}
