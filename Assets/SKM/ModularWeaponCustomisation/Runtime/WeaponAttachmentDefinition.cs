using System.Collections.Generic;
using UnityEngine;

namespace SKM.ModularWeaponCustomisation
{
    /// <summary>Immutable authored data describing one installable attachment.</summary>
    [CreateAssetMenu(
        fileName = "WeaponAttachment_New",
        menuName = "SKM/Modular Weapon Customisation/Weapon Attachment")]
    public sealed class WeaponAttachmentDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id = "attachment";
        [SerializeField] private string displayName = "Attachment";

        [Header("Installation")]
        [SerializeField] private GameObject prefab = null;
        [SerializeField] private AttachmentSlotDefinition slot = null;

        [Header("Stats")]
        [SerializeField] private List<WeaponStatModifier> statModifiers =
            new List<WeaponStatModifier>();

        [Header("Compatibility")]
        [Tooltip("When empty, all weapons supporting the slot are allowed.")]
        [SerializeField] private List<WeaponDefinition> compatibleWeapons =
            new List<WeaponDefinition>();
        [SerializeField] private List<WeaponDefinition> incompatibleWeapons =
            new List<WeaponDefinition>();

        public string Id => id;
        public string DisplayName => displayName;
        public GameObject Prefab => prefab;
        public AttachmentSlotDefinition Slot => slot;
        public IReadOnlyList<WeaponStatModifier> StatModifiers => statModifiers;
        public IReadOnlyList<WeaponDefinition> CompatibleWeapons => compatibleWeapons;
        public IReadOnlyList<WeaponDefinition> IncompatibleWeapons => incompatibleWeapons;
        public bool IsValid => !string.IsNullOrWhiteSpace(id) && slot != null;

        public bool SupportsWeapon(WeaponDefinition weapon)
        {
            if (weapon == null || incompatibleWeapons.Contains(weapon))
            {
                return false;
            }

            return compatibleWeapons.Count == 0 || compatibleWeapons.Contains(weapon);
        }

        private void OnValidate()
        {
            id = id?.Trim();
            displayName = displayName?.Trim();
        }
    }
}
