using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SKM.ModularWeaponCustomisation
{
    /// <summary>
    /// Holds the mutable, per-weapon attachment state. Authored definitions remain
    /// immutable and can safely be shared by any number of weapon instances.
    /// </summary>
    public sealed class WeaponLoadout
    {
        private readonly Dictionary<AttachmentSlotDefinition, WeaponAttachmentDefinition>
            installedAttachments;
        private readonly ReadOnlyDictionary<AttachmentSlotDefinition, WeaponAttachmentDefinition>
            readOnlyInstalledAttachments;

        public WeaponLoadout(WeaponDefinition weapon)
        {
            Weapon = weapon != null
                ? weapon
                : throw new ArgumentNullException(nameof(weapon));

            installedAttachments =
                new Dictionary<AttachmentSlotDefinition, WeaponAttachmentDefinition>();
            readOnlyInstalledAttachments =
                new ReadOnlyDictionary<AttachmentSlotDefinition, WeaponAttachmentDefinition>(
                    installedAttachments);
        }

        public event EventHandler<WeaponLoadoutChangedEventArgs> Changed;

        public WeaponDefinition Weapon { get; }

        public IReadOnlyDictionary<AttachmentSlotDefinition, WeaponAttachmentDefinition>
            InstalledAttachments => readOnlyInstalledAttachments;

        public bool TryInstall(
            WeaponAttachmentDefinition attachment,
            out AttachmentInstallFailure failure)
        {
            failure = WeaponCompatibility.Evaluate(Weapon, attachment);
            if (failure != AttachmentInstallFailure.None)
            {
                return false;
            }

            AttachmentSlotDefinition slot = attachment.Slot;
            installedAttachments.TryGetValue(slot, out WeaponAttachmentDefinition previous);

            if (previous == attachment)
            {
                return true;
            }

            installedAttachments[slot] = attachment;
            Changed?.Invoke(
                this,
                new WeaponLoadoutChangedEventArgs(slot, previous, attachment));
            return true;
        }

        public bool TryRemove(
            AttachmentSlotDefinition slot,
            out WeaponAttachmentDefinition removedAttachment,
            out AttachmentInstallFailure failure)
        {
            removedAttachment = null;

            if (slot == null)
            {
                failure = AttachmentInstallFailure.MissingSlotDefinition;
                return false;
            }

            if (!Weapon.SupportsSlot(slot))
            {
                failure = AttachmentInstallFailure.UnsupportedSlot;
                return false;
            }

            failure = AttachmentInstallFailure.None;
            if (!installedAttachments.TryGetValue(slot, out removedAttachment))
            {
                return true;
            }

            installedAttachments.Remove(slot);
            Changed?.Invoke(
                this,
                new WeaponLoadoutChangedEventArgs(slot, removedAttachment, null));
            return true;
        }

        public bool TryGetAttachment(
            AttachmentSlotDefinition slot,
            out WeaponAttachmentDefinition attachment)
        {
            if (slot == null)
            {
                attachment = null;
                return false;
            }

            return installedAttachments.TryGetValue(slot, out attachment);
        }

        public IEnumerable<WeaponAttachmentDefinition> EnumerateAttachmentsInSlotOrder()
        {
            foreach (AttachmentSlotDefinition slot in Weapon.SupportedSlots)
            {
                if (slot != null && installedAttachments.TryGetValue(slot, out var attachment))
                {
                    yield return attachment;
                }
            }
        }
    }
}
