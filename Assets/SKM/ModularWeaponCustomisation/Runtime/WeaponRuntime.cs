using System;
using System.Collections.Generic;
using UnityEngine;

namespace SKM.ModularWeaponCustomisation
{
    /// <summary>
    /// Owns one weapon instance's loadout, attachment visuals, and effective stats.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WeaponRuntime : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition definition;
        [SerializeField] private bool initializeOnAwake = true;

        private readonly Dictionary<AttachmentSlotDefinition, AttachmentSocket> socketMap =
            new Dictionary<AttachmentSlotDefinition, AttachmentSocket>();
        private readonly Dictionary<AttachmentSlotDefinition, GameObject> visualInstances =
            new Dictionary<AttachmentSlotDefinition, GameObject>();

        public event EventHandler<WeaponLoadoutChangedEventArgs> LoadoutChanged;
        public event Action<WeaponStatsSnapshot> StatsChanged;

        public WeaponDefinition Definition => definition;
        public WeaponLoadout Loadout { get; private set; }
        public WeaponStatsSnapshot EffectiveStats { get; private set; } =
            WeaponStatsSnapshot.Empty;
        public bool IsInitialized { get; private set; }

        private void Awake()
        {
            if (initializeOnAwake)
            {
                Initialize();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromLoadout();
        }

        /// <summary>Initializes or safely reinitializes this weapon instance.</summary>
        public bool Initialize(WeaponDefinition overrideDefinition = null)
        {
            if (overrideDefinition != null)
            {
                definition = overrideDefinition;
            }

            if (definition == null)
            {
                Debug.LogError("WeaponRuntime requires a WeaponDefinition.", this);
                return false;
            }

            UnsubscribeFromLoadout();
            DestroyTrackedVisuals();
            CacheSockets();

            Loadout = new WeaponLoadout(definition);
            Loadout.Changed += HandleLoadoutChanged;
            IsInitialized = true;

            RecalculateStats();
            ApplyStartingLoadout();
            return true;
        }

        public bool TryInstall(
            WeaponAttachmentDefinition attachment,
            out AttachmentInstallFailure failure)
        {
            if (!IsInitialized || Loadout == null)
            {
                failure = AttachmentInstallFailure.RuntimeNotInitialized;
                return false;
            }

            failure = WeaponCompatibility.Evaluate(definition, attachment);
            if (failure != AttachmentInstallFailure.None)
            {
                return false;
            }

            if (!socketMap.ContainsKey(attachment.Slot))
            {
                failure = AttachmentInstallFailure.MissingSocket;
                return false;
            }

            return Loadout.TryInstall(attachment, out failure);
        }

        public bool TryRemove(
            AttachmentSlotDefinition slot,
            out WeaponAttachmentDefinition removedAttachment,
            out AttachmentInstallFailure failure)
        {
            if (!IsInitialized || Loadout == null)
            {
                removedAttachment = null;
                failure = AttachmentInstallFailure.RuntimeNotInitialized;
                return false;
            }

            return Loadout.TryRemove(slot, out removedAttachment, out failure);
        }

        public bool TryGetSocket(
            AttachmentSlotDefinition slot,
            out AttachmentSocket socket)
        {
            if (slot == null)
            {
                socket = null;
                return false;
            }

            return socketMap.TryGetValue(slot, out socket);
        }

        private void CacheSockets()
        {
            socketMap.Clear();
            AttachmentSocket[] discoveredSockets =
                GetComponentsInChildren<AttachmentSocket>(true);

            foreach (AttachmentSocket socket in discoveredSockets)
            {
                if (socket == null || socket.Slot == null)
                {
                    continue;
                }

                if (socketMap.ContainsKey(socket.Slot))
                {
                    Debug.LogError(
                        $"Weapon '{name}' contains more than one socket for slot " +
                        $"'{socket.Slot.DisplayName}'.",
                        socket);
                    continue;
                }

                socketMap.Add(socket.Slot, socket);
            }
        }

        private void ApplyStartingLoadout()
        {
            foreach (WeaponAttachmentDefinition attachment in definition.StartingAttachments)
            {
                if (attachment == null)
                {
                    continue;
                }

                if (!TryInstall(attachment, out AttachmentInstallFailure failure))
                {
                    Debug.LogError(
                        $"Could not install starting attachment '{attachment.DisplayName}' " +
                        $"on weapon '{definition.DisplayName}': {failure}.",
                        this);
                }
            }
        }

        private void HandleLoadoutChanged(
            object sender,
            WeaponLoadoutChangedEventArgs eventArgs)
        {
            RefreshVisual(eventArgs.Slot, eventArgs.CurrentAttachment);
            RecalculateStats();
            LoadoutChanged?.Invoke(this, eventArgs);
        }

        private void RefreshVisual(
            AttachmentSlotDefinition slot,
            WeaponAttachmentDefinition attachment)
        {
            if (visualInstances.TryGetValue(slot, out GameObject previousVisual))
            {
                if (previousVisual != null)
                {
                    DestroyVisual(previousVisual);
                }

                visualInstances.Remove(slot);
            }

            if (attachment == null || attachment.Prefab == null ||
                !socketMap.TryGetValue(slot, out AttachmentSocket socket))
            {
                return;
            }

            GameObject visual = Instantiate(attachment.Prefab, socket.VisualRoot, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visualInstances.Add(slot, visual);
        }

        private void RecalculateStats()
        {
            var modifiers = new List<WeaponStatModifier>();

            if (Loadout != null)
            {
                foreach (WeaponAttachmentDefinition attachment in
                         Loadout.EnumerateAttachmentsInSlotOrder())
                {
                    modifiers.AddRange(attachment.StatModifiers);
                }
            }

            EffectiveStats = WeaponStatsCalculator.Calculate(definition.BaseStats, modifiers);
            StatsChanged?.Invoke(EffectiveStats);
        }

        private void DestroyTrackedVisuals()
        {
            foreach (GameObject visual in visualInstances.Values)
            {
                if (visual != null)
                {
                    visual.transform.SetParent(null, true);
                    DestroyVisual(visual);
                }
            }

            visualInstances.Clear();
        }

        private static void DestroyVisual(GameObject visual)
        {
            if (Application.isPlaying)
            {
                Destroy(visual);
            }
            else
            {
                DestroyImmediate(visual);
            }
        }

        private void UnsubscribeFromLoadout()
        {
            if (Loadout != null)
            {
                Loadout.Changed -= HandleLoadoutChanged;
            }

            IsInitialized = false;
        }
    }
}
