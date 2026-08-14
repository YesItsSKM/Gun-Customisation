using System.Collections.Generic;
using UnityEngine;

namespace SKM.ModularWeaponCustomisation
{
    /// <summary>Immutable authored data describing a configurable weapon.</summary>
    [CreateAssetMenu(
        fileName = "Weapon_New",
        menuName = "SKM/Modular Weapon Customisation/Weapon")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id = "weapon";
        [SerializeField] private string displayName = "Weapon";

        [Header("Presentation")]
        [SerializeField] private GameObject prefab = null;

        [Header("Configuration")]
        [SerializeField] private List<WeaponStatValue> baseStats =
            new List<WeaponStatValue>();
        [SerializeField] private List<AttachmentSlotDefinition> supportedSlots =
            new List<AttachmentSlotDefinition>();
        [SerializeField] private List<WeaponAttachmentDefinition> startingAttachments =
            new List<WeaponAttachmentDefinition>();

        public string Id => id;
        public string DisplayName => displayName;
        public GameObject Prefab => prefab;
        public IReadOnlyList<WeaponStatValue> BaseStats => baseStats;
        public IReadOnlyList<AttachmentSlotDefinition> SupportedSlots => supportedSlots;
        public IReadOnlyList<WeaponAttachmentDefinition> StartingAttachments => startingAttachments;
        public bool IsValid => !string.IsNullOrWhiteSpace(id) && prefab != null;

        public bool SupportsSlot(AttachmentSlotDefinition slot)
        {
            return slot != null && supportedSlots.Contains(slot);
        }

        private void OnValidate()
        {
            id = id?.Trim();
            displayName = displayName?.Trim();
        }
    }
}
