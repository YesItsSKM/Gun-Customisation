namespace SKM.ModularWeaponCustomisation
{
    /// <summary>Central compatibility rules shared by runtime and editor tooling.</summary>
    public static class WeaponCompatibility
    {
        public static AttachmentInstallFailure Evaluate(
            WeaponDefinition weapon,
            WeaponAttachmentDefinition attachment)
        {
            if (weapon == null)
            {
                return AttachmentInstallFailure.MissingWeaponDefinition;
            }

            if (attachment == null)
            {
                return AttachmentInstallFailure.MissingAttachmentDefinition;
            }

            if (attachment.Slot == null)
            {
                return AttachmentInstallFailure.MissingSlotDefinition;
            }

            if (!weapon.SupportsSlot(attachment.Slot))
            {
                return AttachmentInstallFailure.UnsupportedSlot;
            }

            if (!attachment.SupportsWeapon(weapon))
            {
                return AttachmentInstallFailure.IncompatibleWeapon;
            }

            return AttachmentInstallFailure.None;
        }
    }
}
