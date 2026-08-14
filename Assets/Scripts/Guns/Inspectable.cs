using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Inspectable : MonoBehaviour
{
    [Header("Inspection")]
    [SerializeField] private Transform inspectionTransform;
    [SerializeField] private DepthOfFieldManager depthOfFieldManager;
    [SerializeField, Min(0f)] private float transitionDuration = 0.3f;
    [SerializeField, Min(0f)] private float rotationSensitivity = 3f;
    [SerializeField] private Vector2 pitchLimits = new Vector2(-80f, 80f);

    private GunData gunData;
    private bool currentlyBeingInspected;
    private bool isTransitioning;
    private Transform initialParent;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private Vector2 objectRotation;
    private Coroutine movementTransition;

    private void Start()
    {
        currentlyBeingInspected = false;
        isTransitioning = false;

        initialParent = transform.parent;
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;

        ResolveSceneReferences();
    }

    private void Update()
    {
        if (!currentlyBeingInspected || isTransitioning || !Input.GetMouseButton(0))
        {
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        float deltaX = Input.GetAxis("Mouse X");
        float deltaY = Input.GetAxis("Mouse Y");

        objectRotation.x = Mathf.Clamp(
            objectRotation.x + (deltaY * rotationSensitivity),
            Mathf.Min(pitchLimits.x, pitchLimits.y),
            Mathf.Max(pitchLimits.x, pitchLimits.y));
        objectRotation.y -= deltaX * rotationSensitivity;

        transform.rotation = Quaternion.Euler(objectRotation.x, objectRotation.y, 0f);
    }

    private void OnDisable()
    {
        if (!currentlyBeingInspected)
        {
            return;
        }

        if (movementTransition != null)
        {
            StopCoroutine(movementTransition);
            movementTransition = null;
        }

        currentlyBeingInspected = false;
        isTransitioning = false;
        RestoreInitialLocalPose();
        depthOfFieldManager?.SetBackgroundBlur(false, 0f);
        GunEvents.FireGunInspectionEndedEvent();

        RayCastInteractionManager manager = RayCastInteractionManager.Instance;
        if (manager != null && manager.CurrentInspectableObject == this)
        {
            manager.ResetInspectionManager();
        }
    }

    public void SetGunData(GunData data)
    {
        gunData = data;
    }

    public GunData GetGunData() => gunData;

    public void StartInspecting(float duration = 0.3f)
    {
        TryStartInspecting(duration);
    }

    public bool TryStartInspecting(float duration = -1f)
    {
        if (currentlyBeingInspected || isTransitioning)
        {
            return false;
        }

        ResolveSceneReferences();
        if (inspectionTransform == null)
        {
            Debug.LogError("Inspectable requires an inspection transform.", this);
            return false;
        }

        if (gunData == null)
        {
            Debug.LogError("Inspectable cannot begin inspection until GunData has been assigned.", this);
            return false;
        }

        currentlyBeingInspected = true;
        GunEvents.FireGunInspectionEvent(gunData);

        float resolvedDuration = ResolveTransitionDuration(duration);
        depthOfFieldManager?.SetBackgroundBlur(true, resolvedDuration);
        StartMovementTransition(
            inspectionTransform.position,
            inspectionTransform.rotation,
            resolvedDuration,
            SetRotationFromCurrentTransform);

        return true;
    }

    public void StopInspecting(float duration = 0.3f)
    {
        TryStopInspecting(duration);
    }

    public bool TryStopInspecting(float duration = -1f, Action onCompleted = null)
    {
        if (!currentlyBeingInspected)
        {
            return false;
        }

        currentlyBeingInspected = false;
        GunEvents.FireGunInspectionEndedEvent();

        float resolvedDuration = ResolveTransitionDuration(duration);
        depthOfFieldManager?.SetBackgroundBlur(false, resolvedDuration);

        GetInitialWorldPose(out Vector3 targetPosition, out Quaternion targetRotation);
        StartMovementTransition(targetPosition, targetRotation, resolvedDuration, () =>
        {
            RestoreInitialLocalPose();
            onCompleted?.Invoke();
        });

        return true;
    }

    private void ResolveSceneReferences()
    {
        if (inspectionTransform == null)
        {
            GameObject inspectionObject = GameObject.FindGameObjectWithTag("InspectionTransform");
            if (inspectionObject != null)
            {
                inspectionTransform = inspectionObject.transform;
            }
        }

        if (depthOfFieldManager == null)
        {
            depthOfFieldManager = FindAnyObjectByType<DepthOfFieldManager>();
        }
    }

    private float ResolveTransitionDuration(float requestedDuration)
    {
        return requestedDuration >= 0f ? requestedDuration : transitionDuration;
    }

    private void StartMovementTransition(
        Vector3 targetPosition,
        Quaternion targetRotation,
        float duration,
        Action onCompleted)
    {
        if (movementTransition != null)
        {
            StopCoroutine(movementTransition);
        }

        movementTransition = StartCoroutine(
            MoveTo(targetPosition, targetRotation, duration, onCompleted));
    }

    private IEnumerator MoveTo(
        Vector3 targetPosition,
        Quaternion targetRotation,
        float duration,
        Action onCompleted)
    {
        isTransitioning = true;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        if (duration > 0f)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                float progress = elapsedTime / duration;
                transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, progress);

                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        transform.SetPositionAndRotation(targetPosition, targetRotation);
        isTransitioning = false;
        movementTransition = null;
        onCompleted?.Invoke();
    }

    private void SetRotationFromCurrentTransform()
    {
        Vector3 eulerAngles = transform.rotation.eulerAngles;
        objectRotation = new Vector2(NormalizeAngle(eulerAngles.x), eulerAngles.y);
    }

    private void GetInitialWorldPose(out Vector3 position, out Quaternion rotation)
    {
        if (initialParent == null)
        {
            position = initialLocalPosition;
            rotation = initialLocalRotation;
            return;
        }

        position = initialParent.TransformPoint(initialLocalPosition);
        rotation = initialParent.rotation * initialLocalRotation;
    }

    private void RestoreInitialLocalPose()
    {
        if (transform.parent == initialParent)
        {
            transform.localPosition = initialLocalPosition;
            transform.localRotation = initialLocalRotation;
        }
    }

    private static float NormalizeAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }
}
