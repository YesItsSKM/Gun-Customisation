namespace SKM.ModularWeaponCustomisation
{
    /// <summary>Explains why an attachment operation was rejected.</summary>
    public enum AttachmentInstallFailure
    {
        None = 0,
        MissingWeaponDefinition = 1,
        MissingAttachmentDefinition = 2,
        MissingSlotDefinition = 3,
        UnsupportedSlot = 4,
        IncompatibleWeapon = 5,
        MissingSocket = 6,
        RuntimeNotInitialized = 7
    }
}
