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
    private Vector2 objectRotation;
    private float rotationSensitivity = 3f;
    #endregion


    void Start()
    {
        currentlyBeingInspected = false;

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        objectRotation.x = initialRotation.x;
        objectRotation.y = initialRotation.y;

        inspectionTransform = GameObject.FindGameObjectWithTag("InspectionTransform").transform;
        depthOfFieldManager = FindAnyObjectByType<DepthOfFieldManager>();
    }

    private void Update()
    {
        if (currentlyBeingInspected)
        {
            if (Input.GetMouseButton(0))
            {
                float deltaX = Input.GetAxis("Mouse X");
                float deltaY = Input.GetAxis("Mouse Y");

                objectRotation.x -= (deltaX * rotationSensitivity);
                objectRotation.y += (deltaY * rotationSensitivity);

                transform.rotation = Quaternion.Euler(objectRotation.y, objectRotation.x, 0f);
            }
        }
    }

    public void StartInspecting()
    {
        currentlyBeingInspected = true;

        StartCoroutine(CR_MoveTo(inspectionTransform.position, inspectionTransform.rotation, lerpDuration));
        StartCoroutine(depthOfFieldManager.CR_BlurBackground(true, lerpDuration));
    }

    public void StopInspecting()
    {
        currentlyBeingInspected = false;

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
