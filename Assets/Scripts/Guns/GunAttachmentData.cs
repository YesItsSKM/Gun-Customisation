using UnityEngine;

[CreateAssetMenu(fileName = "GunAttachmentData_NewGun", menuName = "Guns/Attachment Data")]
public class GunAttachmentData : ScriptableObject
{
    public string attachmentName = string.Empty;

    public enum AttachmentType 
    { 
        Scope,
        Barrel,
        Magazine,
        Grip,
        Stock
    };

    public AttachmentType attachmentType = AttachmentType.Scope;

    public float accuracyModifier = 0f;
    public float rangeModifier = 0f;
    public float fireRateModifier = 0f;
    public float recoilModifier = 0f;
    public float reloadTimeModifier = 0f;
    public ushort ammoModifier = 0;
}
