using System;
using System.Collections.Generic;
using UnityEngine;

namespace SKM.ModularWeaponCustomisation.Persistence
{
    /// <summary>Serializable stable-ID representation of one attachment selection.</summary>
    [Serializable]
    public sealed class AttachmentSelectionRecord
    {
        [SerializeField] private string slotId = string.Empty;
        [SerializeField] private string attachmentId = string.Empty;

        private AttachmentSelectionRecord()
        {
        }

        public AttachmentSelectionRecord(string slotId, string attachmentId)
        {
            this.slotId = slotId;
            this.attachmentId = attachmentId;
        }

        public string SlotId => slotId;
        public string AttachmentId => attachmentId;
    }

    /// <summary>Serializable stable-ID representation of a complete weapon loadout.</summary>
    [Serializable]
    public sealed class WeaponLoadoutRecord
    {
        [SerializeField] private int schemaVersion = 1;
        [SerializeField] private string weaponId = string.Empty;
        [SerializeField] private List<AttachmentSelectionRecord> attachments =
            new List<AttachmentSelectionRecord>();

        private WeaponLoadoutRecord()
        {
        }

        public WeaponLoadoutRecord(
            string weaponId,
            List<AttachmentSelectionRecord> attachments)
        {
            this.weaponId = weaponId;
            this.attachments = attachments ?? new List<AttachmentSelectionRecord>();
        }

        public int SchemaVersion => schemaVersion;
        public string WeaponId => weaponId;
        public IReadOnlyList<AttachmentSelectionRecord> Attachments =>
            attachments != null
                ? attachments
                : (IReadOnlyList<AttachmentSelectionRecord>)
                    Array.Empty<AttachmentSelectionRecord>();
    }
}
