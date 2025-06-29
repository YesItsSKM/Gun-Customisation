using System.Collections;
using System.Linq;
using UnityEngine;

public class SwitchCameras : MonoBehaviour
{
    Camera[] cameras;
    int currentCameraIndex = 0;

    RayCastInteractionManager rayCastInteractionManager;

    void Start()
    {
        cameras = FindObjectsOfType<Camera>().OrderBy(cam => cam.name).ToArray();

        rayCastInteractionManager = FindAnyObjectByType<RayCastInteractionManager>();

        if (cameras.Length == 0)
            Debug.LogError("No cameras found in the scene.");

        ActivateCamera(index: 0);       // security camera
    }

    void Update()
    {
        if (cameras != null && !rayCastInteractionManager.IsInspecting && Input.GetKeyDown(KeyCode.C))
        {
            currentCameraIndex = (currentCameraIndex + 1) % cameras.Length;

            if (rayCastInteractionManager.IsInspecting)
            {
                rayCastInteractionManager.CurrentInspectableObject.StopInspecting(lerpDuration: 0f);
                rayCastInteractionManager.ResetInspectionManager();
            }

            ActivateCamera(currentCameraIndex);
        }
    }

    void ActivateCamera(int index = 0)
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].gameObject.SetActive(i == index);
        }
    }
}
