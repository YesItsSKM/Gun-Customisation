using System.Collections;
using UnityEngine;

public class Inspectable : MonoBehaviour
{
    #region Private Variables
    private bool currentlyBeingInspected;
    private float lerpDuration = 0.3f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private Transform inspectionTransform;

    private DepthOfFieldManager depthOfFieldManager;
    #endregion


    void Start()
    {
        currentlyBeingInspected = false;

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        inspectionTransform = GameObject.FindGameObjectWithTag("InspectionTransform").transform;
        depthOfFieldManager = FindAnyObjectByType<DepthOfFieldManager>();
    }

    public void StartInspecting()
    {
        currentlyBeingInspected = true;
        Debug.Log("Started inspecting...");

        StartCoroutine(CR_MoveTo(inspectionTransform.position, inspectionTransform.rotation, lerpDuration));
        StartCoroutine(depthOfFieldManager.CR_BlurBackground(true, lerpDuration));
    }

    public void StopInspecting()
    {
        currentlyBeingInspected = false;
        Debug.Log("Stopped inspecting.");

        StartCoroutine(depthOfFieldManager.CR_BlurBackground(false, lerpDuration));
        StartCoroutine(CR_MoveTo(initialPosition, initialRotation, lerpDuration));
    }

    IEnumerator CR_MoveTo(Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, elapsedTime/duration);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, elapsedTime/duration);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;
    }

}
