using UnityEngine;


[CreateAssetMenu(fileName = "GunData_NewGun", menuName = "Guns/GunData")]
public class GunData : ScriptableObject
{
    public string gunName = string.Empty;
    public GameObject gunPrefab;

    [Header("Base Stats")]
    public float accuracy = 0f;
    public float range = 0f;
    public float fireRate = 0f;
    public float recoil = 0f;
    public float reloadTime = 0f;
    public ushort ammo = 0;

    public GunAttachmentData[] attachmentData;
}
