using UnityEngine;

[CreateAssetMenu(fileName = "GunAttachmentData_NewGun", menuName = "Guns/Attachment Data")]
public class GunAttachmentData : ScriptableObject
{
    public string attachmentName = string.Empty;

    public GameObject attachmentPrefab;

    public AttachmentType attachmentType = AttachmentType.None;

    public float accuracyModifier = 0f;
    public float rangeModifier = 0f;
    public float fireRateModifier = 0f;
    public float recoilModifier = 0f;
    public float reloadTimeModifier = 0f;
    public ushort ammoModifier = 0;
}
