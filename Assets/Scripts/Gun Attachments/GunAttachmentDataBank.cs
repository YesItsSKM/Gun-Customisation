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
        attachmentsLookupMap =
            new Dictionary<AttachmentType, List<GunAttachmentData>>();

        if (attachments == null)
        {
            return;
        }

        foreach (AttachmentBankGroup attachment in attachments)
        {
            if (attachment == null ||
                attachmentsLookupMap.ContainsKey(attachment.AttachmentType))
            {
                continue;
            }

            attachmentsLookupMap[attachment.AttachmentType] =
                attachment.attachments ?? new List<GunAttachmentData>();
        }
    }
}
