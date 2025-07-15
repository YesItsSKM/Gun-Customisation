using UnityEngine;

public class AttachmentUIManager : MonoBehaviour
{
    #region Singleton Declaration

    public static AttachmentUIManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    #endregion

    public GameObject attachmentUI;

    private void Start()
    {
        HideAttachmentUI();
    }

    private void OnEnable()
    {
        GunEvents.OnGunInspected += ShowAttachmentUI;
        GunEvents.OnGunInspectionEnded += HideAttachmentUI;
    }

    private void OnDisable()
    {
        GunEvents.OnGunInspected -= ShowAttachmentUI;
        GunEvents.OnGunInspectionEnded -= HideAttachmentUI;
    }

    void ShowAttachmentUI(GunData gunData)
    {
        attachmentUI.gameObject.SetActive(true);
    }

    void HideAttachmentUI()
    {
        attachmentUI.gameObject.SetActive(false);
    }
}
