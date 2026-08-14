using UnityEngine;

namespace SKM.ModularWeaponCustomisation
{
    /// <summary>
    /// Identifies an attachment category shared by weapon definitions, attachment
    /// definitions, and visual sockets.
    /// </summary>
    [CreateAssetMenu(
        fileName = "AttachmentSlot_New",
        menuName = "SKM/Modular Weapon Customisation/Attachment Slot")]
    public sealed class AttachmentSlotDefinition : ScriptableObject
    {
        [SerializeField] private string id = "attachment-slot";
        [SerializeField] private string displayName = "Attachment Slot";

        /// <summary>A stable identifier used by persistence and validation.</summary>
        public string Id => id;

        /// <summary>The user-facing slot name.</summary>
        public string DisplayName => displayName;

        /// <summary>Returns whether the definition contains the minimum authoring data.</summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(id);

        private void OnValidate()
        {
            id = id?.Trim();
            displayName = displayName?.Trim();
        }
    }
}
