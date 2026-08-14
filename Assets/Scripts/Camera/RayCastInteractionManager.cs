using UnityEngine;
using UnityEngine.EventSystems;

public class RayCastInteractionManager : MonoBehaviour
{
    public static RayCastInteractionManager Instance { get; private set; }

    [Header("Interaction")]
    [SerializeField] private Camera interactionCamera;
    [SerializeField] private LayerMask interactionLayers = ~0;
    [SerializeField, Min(0f)] private float maximumInteractionDistance = 100f;

    private Inspectable currentInspectableObject;
    public Inspectable CurrentInspectableObject => currentInspectableObject;

    private bool isInspecting;
    public bool IsInspecting => isInspecting;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Only one RayCastInteractionManager can be active at a time.", this);
            Destroy(this);
            return;
        }

        Instance = this;
        ResetInspectionManager();

        if (interactionCamera == null)
        {
            interactionCamera = GetComponent<Camera>();
        }

        if (interactionCamera == null)
        {
            interactionCamera = Camera.main;
        }

        if (interactionCamera == null)
        {
            Debug.LogError("Raycast interaction requires an interaction camera.", this);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (currentInspectableObject == null && isInspecting)
        {
            ResetInspectionManager();
        }

        if (!isInspecting && Input.GetMouseButtonDown(0))
        {
            TryBeginInspection();
        }

        if (isInspecting && Input.GetKeyDown(KeyCode.Escape))
        {
            EndInspection();
        }
    }

    public void CancelInspection(float transitionDuration = 0f)
    {
        EndInspection(transitionDuration);
    }

    public void ResetInspectionManager()
    {
        isInspecting = false;
        currentInspectableObject = null;
    }

    private void TryBeginInspection()
    {
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (interactionCamera == null || !interactionCamera.isActiveAndEnabled)
        {
            return;
        }

        Ray ray = interactionCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hitInfo, maximumInteractionDistance, interactionLayers))
        {
            return;
        }

        Inspectable inspectable = hitInfo.collider.GetComponentInParent<Inspectable>();
        if (inspectable == null || !inspectable.TryStartInspecting())
        {
            return;
        }

        currentInspectableObject = inspectable;
        isInspecting = true;
    }

    private void EndInspection(float transitionDuration = -1f)
    {
        if (currentInspectableObject == null)
        {
            ResetInspectionManager();
            return;
        }

        Inspectable inspectable = currentInspectableObject;
        bool stopping = inspectable.TryStopInspecting(transitionDuration, ResetInspectionManager);
        if (!stopping)
        {
            ResetInspectionManager();
        }
    }
}
