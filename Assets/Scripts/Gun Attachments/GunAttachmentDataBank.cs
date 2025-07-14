using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AttachmentBankGroup
{
    public AttachmentType AttachmentType;
    public List<GunAttachmentData> attachments;
}

[CreateAssetMenu(fileName = "GunAttachmentData_Bank", menuName = "Guns/Attachment Data Bank")]
public class GunAttachmentDataBank : ScriptableObject
{
    [SerializeField] private List<AttachmentBankGroup> attachments;

    private Dictionary<AttachmentType, List<GunAttachmentData>> attachmentsLookupMap;

    private void OnEnable()
    {       
        if (attachmentsLookupMap == null)
            attachmentsLookupMap = new Dictionary<AttachmentType, List<GunAttachmentData>>();

        foreach (var attachment in attachments)
        {
            if (!attachmentsLookupMap.ContainsKey(attachment.AttachmentType))
                attachmentsLookupMap[attachment.AttachmentType] = attachment.attachments;
        }
    }

    public List<GunAttachmentData> GetAllGunAttachmentDataOfType(AttachmentType attachmentType)
    {
        // TryGetValue with the key 'attachmentType'; if a list of attachment exist, return those;
        // else return an empty new list
        return attachmentsLookupMap.TryGetValue(attachmentType, out List<GunAttachmentData> result) ?
            result : new List<GunAttachmentData>();
    }
}
