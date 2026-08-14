using System.Collections.Generic;
using UnityEngine;

namespace SKM.ModularWeaponCustomisation.Persistence
{
    /// <summary>Resolves stable persistence IDs to authored definition assets.</summary>
    [CreateAssetMenu(
        fileName = "WeaponCatalog_New",
        menuName = "SKM/Modular Weapon Customisation/Weapon Catalog")]
    public sealed class WeaponCatalog : ScriptableObject
    {
        [SerializeField] private List<WeaponDefinition> weapons =
            new List<WeaponDefinition>();
        [SerializeField] private List<WeaponAttachmentDefinition> attachments =
            new List<WeaponAttachmentDefinition>();

        private Dictionary<string, WeaponDefinition> weaponLookup;
        private Dictionary<string, WeaponAttachmentDefinition> attachmentLookup;
        private HashSet<string> ambiguousWeaponIds;
        private HashSet<string> ambiguousAttachmentIds;

        public IReadOnlyList<WeaponDefinition> Weapons => weapons;
        public IReadOnlyList<WeaponAttachmentDefinition> Attachments => attachments;

        private void OnEnable()
        {
            RebuildLookups();
        }

        private void OnValidate()
        {
            RebuildLookups();
        }

        public bool TryGetWeapon(string id, out WeaponDefinition weapon)
        {
            EnsureLookups();
            if (string.IsNullOrWhiteSpace(id))
            {
                weapon = null;
                return false;
            }

            return weaponLookup.TryGetValue(id, out weapon);
        }

        public bool TryGetAttachment(
            string id,
            out WeaponAttachmentDefinition attachment)
        {
            EnsureLookups();
            if (string.IsNullOrWhiteSpace(id))
            {
                attachment = null;
                return false;
            }

            return attachmentLookup.TryGetValue(id, out attachment);
        }

        public void RebuildLookups()
        {
            weaponLookup = new Dictionary<string, WeaponDefinition>(
                System.StringComparer.Ordinal);
            attachmentLookup = new Dictionary<string, WeaponAttachmentDefinition>(
                System.StringComparer.Ordinal);
            ambiguousWeaponIds = new HashSet<string>(System.StringComparer.Ordinal);
            ambiguousAttachmentIds = new HashSet<string>(System.StringComparer.Ordinal);

            foreach (WeaponDefinition weapon in weapons)
            {
                if (weapon == null || string.IsNullOrWhiteSpace(weapon.Id) ||
                    ambiguousWeaponIds.Contains(weapon.Id))
                {
                    continue;
                }

                if (weaponLookup.ContainsKey(weapon.Id))
                {
                    weaponLookup.Remove(weapon.Id);
                    ambiguousWeaponIds.Add(weapon.Id);
                    continue;
                }

                weaponLookup.Add(weapon.Id, weapon);
            }

            foreach (WeaponAttachmentDefinition attachment in attachments)
            {
                if (attachment == null || string.IsNullOrWhiteSpace(attachment.Id) ||
                    ambiguousAttachmentIds.Contains(attachment.Id))
                {
                    continue;
                }

                if (attachmentLookup.ContainsKey(attachment.Id))
                {
                    attachmentLookup.Remove(attachment.Id);
                    ambiguousAttachmentIds.Add(attachment.Id);
                    continue;
                }

                attachmentLookup.Add(attachment.Id, attachment);
            }
        }

        private void EnsureLookups()
        {
            if (weaponLookup == null || attachmentLookup == null)
            {
                RebuildLookups();
            }
        }
    }
}
