using System.Collections.Generic;
using UnityEngine;

public class GunAttachmentSpawnManager : MonoBehaviour
{
    #region Singleton Declaration
    public static GunAttachmentSpawnManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance == this)
        {
            Destroy(this.gameObject);
        }

        Instance = this;
    }
    #endregion

    [SerializeField] private GunAttachmentDataBank gunAttachmentDataBank;

    private Dictionary<AttachmentType, int> currentlySelectedAttachmentMap = new Dictionary<AttachmentType, int>();

    private RayCastInteractionManager interactionManagerInstance;
    private void Start()
    {
        interactionManagerInstance = RayCastInteractionManager.Instance;
    }

    public GunAttachmentData CycleAttachment(AttachmentType attachmentType, int direction)
    {
        if (gunAttachmentDataBank == null)
            return null;

        if (!gunAttachmentDataBank.AttachmentsLookupMap.TryGetValue(attachmentType, out var listOfAttachments) || listOfAttachments.Count == 0)
            return null;

        if (!currentlySelectedAttachmentMap.ContainsKey(attachmentType))
            currentlySelectedAttachmentMap[attachmentType] = 0;

        int currentAttachmentIndex = currentlySelectedAttachmentMap[attachmentType];
        int newAttachmentIndex = (currentAttachmentIndex + direction + listOfAttachments.Count) % listOfAttachments.Count;

        currentlySelectedAttachmentMap[attachmentType] = newAttachmentIndex;

        var attachment = listOfAttachments[newAttachmentIndex];

        SpawnAttachment(attachment);

        return attachment;
    }


    void SpawnAttachment(GunAttachmentData attachment)
    {
        var spawnPoints = interactionManagerInstance.CurrentInspectableObject.GetComponentsInChildren<AttachmentSpawnPoint>();

        foreach (var spawnPoint in spawnPoints)
        {
            if (attachment.attachmentPrefab == null) continue;

            if (spawnPoint.AttachmentType == attachment.attachmentType)
            {
                Instantiate(attachment.attachmentPrefab, spawnPoint.transform);
            }
        }
    }
}
