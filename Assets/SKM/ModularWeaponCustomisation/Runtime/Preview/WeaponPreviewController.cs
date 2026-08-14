using System;
using UnityEngine;

namespace SKM.ModularWeaponCustomisation.Preview
{
    /// <summary>
    /// Runs an input-independent weapon inspection transition. UI and input code
    /// drive rotation and zoom through the public methods.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WeaponPreviewController : MonoBehaviour
    {
        [Header("Placement")]
        [SerializeField] private Transform previewAnchor = null;
        [Tooltip("Optional camera/view transform used to define zoom-toward-viewer.")]
        [SerializeField] private Transform zoomReference = null;
        [SerializeField] private Vector2 zoomDistanceLimits =
            new Vector2(0f, 0.35f);
        [SerializeField, Range(0f, 1f)] private float initialZoom = 0.5f;

        [Header("Rotation")]
        [SerializeField] private Vector2 pitchLimits = new Vector2(-70f, 70f);

        [Header("Transition")]
        [SerializeField, Min(0f)] private float transitionDuration = 0.25f;
        [SerializeField] private AnimationCurve transitionCurve =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private bool useUnscaledTime = true;

        private WeaponRuntime currentWeapon;
        private Transform target;
        private Transform originalParent;
        private bool originalHadParent;
        private int originalSiblingIndex;
        private Vector3 originalLocalPosition;
        private Quaternion originalLocalRotation;
        private Vector3 originalLocalScale;
        private Vector3 originalWorldPosition;
        private Quaternion originalWorldRotation;
        private Vector3 originalWorldScale;
        private Vector3 transitionStartPosition;
        private Quaternion transitionStartRotation;
        private float transitionElapsed;
        private float yaw;
        private float pitch;
        private float zoom01;

        public event Action<WeaponRuntime> PreviewStarted;
        public event Action<WeaponRuntime> PreviewEnded;
        public event Action<WeaponPreviewState> StateChanged;

        public Transform PreviewAnchor => previewAnchor;
        public WeaponRuntime CurrentWeapon => currentWeapon;
        public WeaponPreviewState State { get; private set; } =
            WeaponPreviewState.Idle;
        public bool IsPreviewing => State != WeaponPreviewState.Idle;
        public float Zoom01 => zoom01;
        public Vector2 OrbitAngles => new Vector2(yaw, pitch);

        private void LateUpdate()
        {
            if (State == WeaponPreviewState.Idle)
            {
                return;
            }

            if (target == null)
            {
                ClearSession(notifyEnded: true);
                return;
            }

            switch (State)
            {
                case WeaponPreviewState.Entering:
                    UpdateEntering();
                    break;
                case WeaponPreviewState.Active:
                    ApplyPreviewPose();
                    break;
                case WeaponPreviewState.Exiting:
                    UpdateExiting();
                    break;
            }
        }

        private void OnDisable()
        {
            if (State != WeaponPreviewState.Idle)
            {
                RestoreImmediately();
            }
        }

        private void OnValidate()
        {
            transitionDuration = Mathf.Max(0f, transitionDuration);
            pitchLimits = Ordered(pitchLimits);
            zoomDistanceLimits = Ordered(zoomDistanceLimits);
            zoomDistanceLimits.x = Mathf.Max(0f, zoomDistanceLimits.x);
            zoomDistanceLimits.y = Mathf.Max(
                zoomDistanceLimits.x,
                zoomDistanceLimits.y);
            initialZoom = Mathf.Clamp01(initialZoom);
        }

        /// <summary>Begins previewing a weapon when the controller is idle.</summary>
        public bool TryBeginPreview(WeaponRuntime weapon)
        {
            if (weapon == null || previewAnchor == null ||
                State != WeaponPreviewState.Idle)
            {
                return false;
            }

            currentWeapon = weapon;
            target = weapon.transform;
            CaptureOriginalPose();

            transitionStartPosition = target.position;
            transitionStartRotation = target.rotation;
            transitionElapsed = 0f;
            yaw = 0f;
            pitch = 0f;
            zoom01 = initialZoom;

            target.SetParent(null, true);
            SetState(WeaponPreviewState.Entering);
            PreviewStarted?.Invoke(currentWeapon);

            if (transitionDuration <= Mathf.Epsilon)
            {
                CompleteEntering();
            }

            return true;
        }

        /// <summary>Starts returning the current weapon to its captured pose.</summary>
        public bool TryEndPreview()
        {
            if (State == WeaponPreviewState.Idle ||
                State == WeaponPreviewState.Exiting || target == null)
            {
                return false;
            }

            transitionStartPosition = target.position;
            transitionStartRotation = target.rotation;
            transitionElapsed = 0f;
            SetState(WeaponPreviewState.Exiting);

            if (transitionDuration <= Mathf.Epsilon)
            {
                CompleteExiting();
            }

            return true;
        }

        /// <summary>Adds orbit angles in degrees while entering or active.</summary>
        public bool Rotate(Vector2 deltaDegrees)
        {
            if (State != WeaponPreviewState.Entering &&
                State != WeaponPreviewState.Active)
            {
                return false;
            }

            yaw = WrapAngle(yaw + deltaDegrees.x);
            pitch = Mathf.Clamp(pitch + deltaDegrees.y, pitchLimits.x, pitchLimits.y);
            return true;
        }

        /// <summary>Sets absolute orbit angles in degrees.</summary>
        public bool SetOrbit(float yawDegrees, float pitchDegrees)
        {
            if (State != WeaponPreviewState.Entering &&
                State != WeaponPreviewState.Active)
            {
                return false;
            }

            yaw = WrapAngle(yawDegrees);
            pitch = Mathf.Clamp(pitchDegrees, pitchLimits.x, pitchLimits.y);
            return true;
        }

        /// <summary>Sets normalized zoom without choosing a mouse or controller binding.</summary>
        public bool SetZoom(float normalizedZoom)
        {
            if (State != WeaponPreviewState.Entering &&
                State != WeaponPreviewState.Active)
            {
                return false;
            }

            zoom01 = Mathf.Clamp01(normalizedZoom);
            return true;
        }

        /// <summary>Restores the default orbit and authored initial zoom.</summary>
        public bool ResetView()
        {
            if (State != WeaponPreviewState.Entering &&
                State != WeaponPreviewState.Active)
            {
                return false;
            }

            yaw = 0f;
            pitch = 0f;
            zoom01 = initialZoom;
            return true;
        }

        /// <summary>Immediately restores the weapon, including its original parent.</summary>
        public bool RestoreImmediately()
        {
            if (State == WeaponPreviewState.Idle)
            {
                return false;
            }

            if (target != null)
            {
                RestoreOriginalLocalPose();
            }

            ClearSession(notifyEnded: true);
            return true;
        }

        private void UpdateEntering()
        {
            float progress = AdvanceTransition();
            GetPreviewPose(out Vector3 destinationPosition, out Quaternion destinationRotation);
            float easedProgress = EvaluateTransition(progress);
            target.SetPositionAndRotation(
                Vector3.LerpUnclamped(
                    transitionStartPosition,
                    destinationPosition,
                    easedProgress),
                Quaternion.SlerpUnclamped(
                    transitionStartRotation,
                    destinationRotation,
                    easedProgress));

            if (progress >= 1f)
            {
                CompleteEntering();
            }
        }

        private void CompleteEntering()
        {
            ApplyPreviewPose();
            SetState(WeaponPreviewState.Active);
        }

        private void UpdateExiting()
        {
            float progress = AdvanceTransition();
            GetRestoreWorldPose(out Vector3 destinationPosition, out Quaternion destinationRotation);
            float easedProgress = EvaluateTransition(progress);
            target.SetPositionAndRotation(
                Vector3.LerpUnclamped(
                    transitionStartPosition,
                    destinationPosition,
                    easedProgress),
                Quaternion.SlerpUnclamped(
                    transitionStartRotation,
                    destinationRotation,
                    easedProgress));

            if (progress >= 1f)
            {
                CompleteExiting();
            }
        }

        private void CompleteExiting()
        {
            RestoreOriginalLocalPose();
            ClearSession(notifyEnded: true);
        }

        private float AdvanceTransition()
        {
            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            transitionElapsed += Mathf.Max(0f, deltaTime);
            return transitionDuration <= Mathf.Epsilon
                ? 1f
                : Mathf.Clamp01(transitionElapsed / transitionDuration);
        }

        private float EvaluateTransition(float progress)
        {
            return transitionCurve != null
                ? transitionCurve.Evaluate(progress)
                : progress;
        }

        private void ApplyPreviewPose()
        {
            GetPreviewPose(out Vector3 position, out Quaternion rotation);
            target.SetPositionAndRotation(position, rotation);
        }

        private void GetPreviewPose(out Vector3 position, out Quaternion rotation)
        {
            float zoomDistance = Mathf.Lerp(
                zoomDistanceLimits.x,
                zoomDistanceLimits.y,
                zoom01);
            Vector3 towardViewer = -previewAnchor.forward;
            if (zoomReference != null)
            {
                Vector3 toReference =
                    zoomReference.position - previewAnchor.position;
                if (toReference.sqrMagnitude > 0.000001f)
                {
                    towardViewer = toReference.normalized;
                }
            }

            position = previewAnchor.position + towardViewer * zoomDistance;
            rotation = previewAnchor.rotation * Quaternion.Euler(pitch, yaw, 0f);
        }

        private void CaptureOriginalPose()
        {
            originalParent = target.parent;
            originalHadParent = originalParent != null;
            originalSiblingIndex = target.GetSiblingIndex();
            originalLocalPosition = target.localPosition;
            originalLocalRotation = target.localRotation;
            originalLocalScale = target.localScale;
            originalWorldPosition = target.position;
            originalWorldRotation = target.rotation;
            originalWorldScale = target.lossyScale;
        }

        private void GetRestoreWorldPose(
            out Vector3 position,
            out Quaternion rotation)
        {
            if (originalParent != null)
            {
                position = originalParent.TransformPoint(originalLocalPosition);
                rotation = originalParent.rotation * originalLocalRotation;
                return;
            }

            position = originalWorldPosition;
            rotation = originalWorldRotation;
        }

        private void RestoreOriginalLocalPose()
        {
            if (originalHadParent && originalParent == null)
            {
                target.SetParent(null, true);
                target.SetPositionAndRotation(originalWorldPosition, originalWorldRotation);
                target.localScale = originalWorldScale;
                return;
            }

            target.SetParent(originalParent, false);
            target.SetSiblingIndex(originalSiblingIndex);
            target.localPosition = originalLocalPosition;
            target.localRotation = originalLocalRotation;
            target.localScale = originalLocalScale;
        }

        private void ClearSession(bool notifyEnded)
        {
            WeaponRuntime endedWeapon = currentWeapon;
            target = null;
            currentWeapon = null;
            originalParent = null;
            originalHadParent = false;
            SetState(WeaponPreviewState.Idle);

            if (notifyEnded)
            {
                PreviewEnded?.Invoke(endedWeapon);
            }
        }

        private void SetState(WeaponPreviewState value)
        {
            if (State == value)
            {
                return;
            }

            State = value;
            StateChanged?.Invoke(State);
        }

        private static Vector2 Ordered(Vector2 range)
        {
            return range.x <= range.y
                ? range
                : new Vector2(range.y, range.x);
        }

        private static float WrapAngle(float degrees)
        {
            return Mathf.Repeat(degrees + 180f, 360f) - 180f;
        }
    }
}
