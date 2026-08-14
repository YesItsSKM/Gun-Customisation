using System.Linq;
using UnityEngine;
using TMPro;

public class SwitchCameras : MonoBehaviour
{
    [SerializeField] private Camera[] cameras;
    [SerializeField, Min(0)] private int initialCameraIndex = 1;
    [SerializeField] private TextMeshProUGUI activeCamText;

    private int currentCameraIndex;
    private RayCastInteractionManager rayCastInteractionManager;

    private void Start()
    {
        if (cameras == null || cameras.Length == 0)
        {
            cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None)
                .OrderBy(cameraToSort => cameraToSort.name)
                .ToArray();
        }

        cameras = cameras
            .Where(cameraToKeep => cameraToKeep != null)
            .Distinct()
            .ToArray();

        rayCastInteractionManager = RayCastInteractionManager.Instance;

        if (cameras.Length == 0)
        {
            Debug.LogError("No cameras found in the scene.");
            enabled = false;
            return;
        }

        currentCameraIndex = Mathf.Clamp(initialCameraIndex, 0, cameras.Length - 1);
        ActivateCamera(currentCameraIndex);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.C))
        {
            return;
        }

        if (rayCastInteractionManager == null)
        {
            rayCastInteractionManager = RayCastInteractionManager.Instance;
        }

        if (rayCastInteractionManager != null && rayCastInteractionManager.IsInspecting)
        {
            return;
        }

        currentCameraIndex = (currentCameraIndex + 1) % cameras.Length;
        ActivateCamera(currentCameraIndex);
    }

    private void ActivateCamera(int index)
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            bool isActiveCamera = i == index;
            cameras[i].enabled = isActiveCamera;

            if (cameras[i].TryGetComponent(out AudioListener audioListener))
            {
                audioListener.enabled = isActiveCamera;
            }
        }

        if (activeCamText != null)
        {
            activeCamText.text = $"CAM-{currentCameraIndex + 1}";
        }
    }
}
