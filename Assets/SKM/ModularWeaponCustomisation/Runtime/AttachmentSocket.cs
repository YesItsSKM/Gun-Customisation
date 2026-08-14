using UnityEngine;

namespace SKM.ModularWeaponCustomisation
{
    /// <summary>Marks where one attachment category is rendered on a weapon prefab.</summary>
    [DisallowMultipleComponent]
    public sealed class AttachmentSocket : MonoBehaviour
    {
        [SerializeField] private AttachmentSlotDefinition slot = null;
        [SerializeField] private Transform visualRoot = null;

        public AttachmentSlotDefinition Slot => slot;
        public Transform VisualRoot => visualRoot != null ? visualRoot : transform;
    }
}
