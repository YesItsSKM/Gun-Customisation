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

    public IReadOnlyDictionary<AttachmentType, List<GunAttachmentData>> AttachmentsLookupMap => attachmentsLookupMap;

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
}
