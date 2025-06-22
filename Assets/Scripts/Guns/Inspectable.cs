using System.Collections;
using UnityEngine;

public class Inspectable : MonoBehaviour
{
    bool currentlyBeingInspected;
    Vector3 initialPosition;
    Quaternion initialRotation;

    Transform inspectionTransform;
    float lerpDuration = 0.3f;

    void Start()
    {
        currentlyBeingInspected = false;

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        inspectionTransform = GameObject.FindGameObjectWithTag("InspectionTransform").transform;
    }

    public void StartInspecting()
    {
        currentlyBeingInspected = true;
        Debug.Log("Started inspecting...");

        StartCoroutine(MoveTo(inspectionTransform.position, inspectionTransform.rotation, lerpDuration));
    }

    public void StopInspecting()
    {
        currentlyBeingInspected = false;
        Debug.Log("Stopped inspecting.");

        StartCoroutine(MoveTo(initialPosition, initialRotation, lerpDuration));
    }

    IEnumerator MoveTo(Vector3 targetPosition, Quaternion targetRotation, float duration)
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
