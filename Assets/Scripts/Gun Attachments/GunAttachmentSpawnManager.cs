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

        return attachment;
    }
}
