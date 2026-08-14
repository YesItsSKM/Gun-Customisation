using System;

namespace SKM.ModularWeaponCustomisation
{
    /// <summary>Describes one successful loadout mutation.</summary>
    public sealed class WeaponLoadoutChangedEventArgs : EventArgs
    {
        public WeaponLoadoutChangedEventArgs(
            AttachmentSlotDefinition slot,
            WeaponAttachmentDefinition previousAttachment,
            WeaponAttachmentDefinition currentAttachment)
        {
            Slot = slot;
            PreviousAttachment = previousAttachment;
            CurrentAttachment = currentAttachment;
        }

        public AttachmentSlotDefinition Slot { get; }
        public WeaponAttachmentDefinition PreviousAttachment { get; }
        public WeaponAttachmentDefinition CurrentAttachment { get; }
    }
}
