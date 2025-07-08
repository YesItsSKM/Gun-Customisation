using System.Linq;
using UnityEngine;
using TMPro;

public class SwitchCameras : MonoBehaviour
{
    Camera[] cameras;
    private int currentCameraIndex = 0;

    RayCastInteractionManager rayCastInteractionManager;

    [SerializeField] private TextMeshProUGUI activeCamText;

    void Start()
    {
        cameras = FindObjectsOfType<Camera>().OrderBy(cam => cam.name).ToArray();

        rayCastInteractionManager = FindAnyObjectByType<RayCastInteractionManager>();

        if (cameras.Length == 0)
            Debug.LogError("No cameras found in the scene.");

        currentCameraIndex = 1;     // security camera
        ActivateCamera(currentCameraIndex);
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
            cameras[i].enabled = (i == index);
        }

        activeCamText.text = $"CAM-{currentCameraIndex + 1}";
    }
}
