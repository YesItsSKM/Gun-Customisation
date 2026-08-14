using System.Collections.Generic;
using UnityEngine;

public class GunAttachmentSpawnManager : MonoBehaviour
{
    public static GunAttachmentSpawnManager Instance { get; private set; }

    [SerializeField] private GunAttachmentDataBank gunAttachmentDataBank;

    private readonly Dictionary<AttachmentType, int> currentlySelectedAttachmentMap =
        new Dictionary<AttachmentType, int>();
    private readonly Dictionary<AttachmentSpawnPoint, GameObject> spawnedAttachmentMap =
        new Dictionary<AttachmentSpawnPoint, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Only one GunAttachmentSpawnManager can be active at a time.", this);
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public GunAttachmentData CycleAttachment(AttachmentType attachmentType, int direction)
    {
        if (gunAttachmentDataBank == null)
        {
            return null;
        }

        if (!gunAttachmentDataBank.AttachmentsLookupMap.TryGetValue(
                attachmentType,
                out List<GunAttachmentData> attachments) ||
            attachments.Count == 0)
        {
            return null;
        }

        if (!currentlySelectedAttachmentMap.ContainsKey(attachmentType))
        {
            currentlySelectedAttachmentMap[attachmentType] = 0;
        }

        int currentIndex = currentlySelectedAttachmentMap[attachmentType];
        int newIndex = (currentIndex + direction + attachments.Count) % attachments.Count;
        currentlySelectedAttachmentMap[attachmentType] = newIndex;

        GunAttachmentData attachment = attachments[newIndex];
        TrySetAttachment(attachmentType, attachment);
        return attachment;
    }

    private bool TrySetAttachment(
        AttachmentType requestedType,
        GunAttachmentData attachment)
    {
        if (attachment == null ||
            (attachment.attachmentType != AttachmentType.None &&
             attachment.attachmentType != requestedType))
        {
            return false;
        }

        RayCastInteractionManager interactionManager = RayCastInteractionManager.Instance;
        Inspectable inspectable = interactionManager != null
            ? interactionManager.CurrentInspectableObject
            : null;

        if (inspectable == null)
        {
            return false;
        }

        AttachmentSpawnPoint[] spawnPoints =
            inspectable.GetComponentsInChildren<AttachmentSpawnPoint>(true);

        bool spawned = false;
        foreach (AttachmentSpawnPoint spawnPoint in spawnPoints)
        {
            if (spawnPoint.AttachmentType != requestedType)
            {
                continue;
            }

            if (spawnedAttachmentMap.TryGetValue(
                    spawnPoint,
                    out GameObject previousAttachment))
            {
                if (previousAttachment != null)
                {
                    Destroy(previousAttachment);
                }

                spawnedAttachmentMap.Remove(spawnPoint);
            }

            if (attachment.attachmentPrefab != null)
            {
                GameObject instance = Instantiate(
                    attachment.attachmentPrefab,
                    spawnPoint.transform,
                    false);
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                spawnedAttachmentMap.Add(spawnPoint, instance);
            }

            spawned = true;
        }

        return spawned;
    }
}
