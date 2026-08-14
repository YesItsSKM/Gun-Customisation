using System;
using System.Collections.Generic;
using UnityEngine;

namespace SKM.ModularWeaponCustomisation.Persistence
{
    /// <summary>Optional JSON reference implementation for loadout persistence.</summary>
    public static class WeaponLoadoutJson
    {
        public static string Serialize(WeaponLoadout loadout, bool prettyPrint = false)
        {
            if (loadout == null)
            {
                throw new ArgumentNullException(nameof(loadout));
            }

            if (string.IsNullOrWhiteSpace(loadout.Weapon.Id))
            {
                throw new InvalidOperationException(
                    "The loadout weapon requires a stable ID before serialization.");
            }

            var selections = new List<AttachmentSelectionRecord>();
            foreach (WeaponAttachmentDefinition attachment in
                     loadout.EnumerateAttachmentsInSlotOrder())
            {
                if (attachment.Slot == null ||
                    string.IsNullOrWhiteSpace(attachment.Slot.Id) ||
                    string.IsNullOrWhiteSpace(attachment.Id))
                {
                    throw new InvalidOperationException(
                        "Every installed attachment and slot requires a stable ID " +
                        "before serialization.");
                }

                selections.Add(
                    new AttachmentSelectionRecord(attachment.Slot.Id, attachment.Id));
            }

            var record = new WeaponLoadoutRecord(loadout.Weapon.Id, selections);
            return JsonUtility.ToJson(record, prettyPrint);
        }

        public static bool TryResolve(
            string json,
            WeaponCatalog catalog,
            out WeaponDefinition weapon,
            out IReadOnlyList<WeaponAttachmentDefinition> attachments,
            out string error)
        {
            weapon = null;
            attachments = Array.Empty<WeaponAttachmentDefinition>();
            error = string.Empty;

            if (catalog == null)
            {
                error = "A WeaponCatalog is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                error = "Loadout JSON is empty.";
                return false;
            }

            WeaponLoadoutRecord record;
            try
            {
                record = JsonUtility.FromJson<WeaponLoadoutRecord>(json);
            }
            catch (ArgumentException exception)
            {
                error = $"Loadout JSON is invalid: {exception.Message}";
                return false;
            }

            if (record == null || record.SchemaVersion != 1)
            {
                error = "Loadout JSON uses an unsupported schema version.";
                return false;
            }

            if (!catalog.TryGetWeapon(record.WeaponId, out weapon))
            {
                error = $"Weapon ID '{record.WeaponId}' is not present in the catalog.";
                return false;
            }

            var resolvedAttachments = new List<WeaponAttachmentDefinition>();
            var occupiedSlots = new HashSet<AttachmentSlotDefinition>();

            foreach (AttachmentSelectionRecord selection in record.Attachments)
            {
                if (selection == null || string.IsNullOrWhiteSpace(selection.AttachmentId))
                {
                    error = "Loadout JSON contains an empty attachment selection.";
                    return false;
                }

                if (!catalog.TryGetAttachment(selection.AttachmentId, out var attachment))
                {
                    error = $"Attachment ID '{selection.AttachmentId}' is not present in the catalog.";
                    return false;
                }

                if (attachment.Slot == null ||
                    !string.Equals(
                        selection.SlotId,
                        attachment.Slot.Id,
                        StringComparison.Ordinal))
                {
                    error = $"Attachment '{selection.AttachmentId}' does not match saved slot " +
                            $"'{selection.SlotId}'.";
                    return false;
                }

                AttachmentInstallFailure failure =
                    WeaponCompatibility.Evaluate(weapon, attachment);
                if (failure != AttachmentInstallFailure.None)
                {
                    error = $"Attachment '{attachment.Id}' is incompatible: {failure}.";
                    return false;
                }

                if (!occupiedSlots.Add(attachment.Slot))
                {
                    error = $"Loadout JSON selects more than one attachment for slot " +
                            $"'{attachment.Slot.Id}'.";
                    return false;
                }

                resolvedAttachments.Add(attachment);
            }

            attachments = resolvedAttachments;
            return true;
        }

        public static bool TryApply(
            string json,
            WeaponRuntime runtime,
            WeaponCatalog catalog,
            out string error)
        {
            error = string.Empty;
            if (runtime == null || !runtime.IsInitialized)
            {
                error = "An initialized WeaponRuntime is required.";
                return false;
            }

            if (!TryResolve(json, catalog, out WeaponDefinition weapon, out var attachments, out error))
            {
                return false;
            }

            if (runtime.Definition != weapon)
            {
                error = $"Saved weapon '{weapon.Id}' does not match runtime weapon " +
                        $"'{runtime.Definition.Id}'.";
                return false;
            }

            foreach (WeaponAttachmentDefinition attachment in attachments)
            {
                if (!runtime.TryGetSocket(attachment.Slot, out _))
                {
                    error = $"Runtime weapon has no socket for slot '{attachment.Slot.Id}'.";
                    return false;
                }
            }

            foreach (AttachmentSlotDefinition slot in runtime.Definition.SupportedSlots)
            {
                if (slot != null &&
                    !runtime.TryRemove(slot, out _, out AttachmentInstallFailure removeFailure))
                {
                    error = $"Could not clear slot '{slot.Id}': {removeFailure}.";
                    return false;
                }
            }

            foreach (WeaponAttachmentDefinition attachment in attachments)
            {
                if (!runtime.TryInstall(attachment, out AttachmentInstallFailure installFailure))
                {
                    error = $"Could not install attachment '{attachment.Id}': {installFailure}.";
                    return false;
                }
            }

            return true;
        }
    }
}
