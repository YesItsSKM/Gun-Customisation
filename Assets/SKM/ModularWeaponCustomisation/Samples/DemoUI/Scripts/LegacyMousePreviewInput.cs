using SKM.ModularWeaponCustomisation.Preview;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SKM.ModularWeaponCustomisation.Samples.DemoUI
{
    /// <summary>
    /// Optional old Input Manager adapter used by the generated demo. Projects
    /// using the Input System package should drive WeaponPreviewController directly.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LegacyMousePreviewInput : MonoBehaviour
    {
        [SerializeField] private Camera interactionCamera = null;
        [SerializeField] private WeaponPreviewController previewController = null;
        [SerializeField] private LayerMask interactionLayers = ~0;
        [SerializeField, Min(0f)] private float maximumDistance = 100f;
        [SerializeField, Min(0f)] private float rotationSensitivity = 3f;
        [SerializeField, Min(0f)] private float zoomSensitivity = 0.12f;

        private float zoom;

        private void Awake()
        {
            if (interactionCamera == null)
            {
                interactionCamera = GetComponent<Camera>();
            }

            if (interactionCamera == null)
            {
                interactionCamera = Camera.main;
            }

            if (previewController != null)
            {
                zoom = previewController.Zoom01;
            }
        }

        private void Update()
        {
            if (previewController == null || interactionCamera == null)
            {
                return;
            }

            if (!previewController.IsPreviewing)
            {
                TrySelectWeapon();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                previewController.TryEndPreview();
                return;
            }

            bool pointerOverUi =
                EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject();
            if (Input.GetMouseButton(0) && !pointerOverUi)
            {
                previewController.Rotate(
                    new Vector2(
                        Input.GetAxis("Mouse X") * rotationSensitivity,
                        -Input.GetAxis("Mouse Y") * rotationSensitivity));
            }

            float scroll = Input.mouseScrollDelta.y;
            if (!Mathf.Approximately(scroll, 0f) && !pointerOverUi)
            {
                zoom = Mathf.Clamp01(zoom + scroll * zoomSensitivity);
                previewController.SetZoom(zoom);
            }
        }

        private void TrySelectWeapon()
        {
            if (!Input.GetMouseButtonDown(0) ||
                (EventSystem.current != null &&
                 EventSystem.current.IsPointerOverGameObject()))
            {
                return;
            }

            Ray ray = interactionCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    maximumDistance,
                    interactionLayers))
            {
                return;
            }

            WeaponRuntime weapon = hit.collider.GetComponentInParent<WeaponRuntime>();
            if (weapon != null && previewController.TryBeginPreview(weapon))
            {
                zoom = previewController.Zoom01;
            }
        }
    }
}
