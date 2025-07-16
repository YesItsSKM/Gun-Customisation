using UnityEngine;

public class RayCastInteractionManager : MonoBehaviour
{
    public static RayCastInteractionManager Instance;

    private Camera _camera;
    private Inspectable currentInspectableObject;
    public Inspectable CurrentInspectableObject => currentInspectableObject;

    private bool isInspecting;
    public bool IsInspecting => isInspecting;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _camera = Camera.main;
        ResetInspectionManager();

        if (_camera == null)
            Debug.LogError($"No Main Camera in the scene.");
    }
    
    
    void Update()
    {
        if (Input.GetMouseButton(0) && !isInspecting && _camera.enabled)
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                GameObject clickedObject = hitInfo.collider.gameObject;

                if (clickedObject != null && clickedObject.CompareTag("Inspectable"))
                {
                    currentInspectableObject = clickedObject.GetComponent<Inspectable>();

                    currentInspectableObject.StartInspecting();
                    isInspecting = true;
                }
            }
        }

        if (isInspecting && Input.GetKeyDown(KeyCode.Escape))
        {
            currentInspectableObject.StopInspecting();
            ResetInspectionManager();
        }
    }

    public void ResetInspectionManager()
    {
        isInspecting = false;
        currentInspectableObject = null;
    }
}
