using System;
using System.Collections.Generic;
using System.Text;
using SKM.ModularWeaponCustomisation.Persistence;
using UnityEngine;
using UnityEngine.UI;

namespace SKM.ModularWeaponCustomisation.Samples.DemoUI
{
    /// <summary>
    /// Small uGUI reference panel. It demonstrates event-driven attachment
    /// selection without placing UI responsibilities in the runtime core.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WeaponCustomisationPanel : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private WeaponRuntime weapon = null;
        [SerializeField] private WeaponSpawner spawner = null;
        [SerializeField] private WeaponCatalog catalog = null;

        [Header("Controls")]
        [SerializeField] private Dropdown weaponDropdown = null;
        [SerializeField] private Dropdown slotDropdown = null;
        [SerializeField] private RectTransform attachmentListContent = null;
        [Tooltip("Keep this scene button inactive; a copy is created per attachment.")]
        [SerializeField] private Button attachmentButtonTemplate = null;
        [SerializeField] private Button removeAttachmentButton = null;

        [Header("Labels")]
        [SerializeField] private Text weaponNameLabel = null;
        [SerializeField] private Text installedAttachmentLabel = null;
        [SerializeField] private Text statsLabel = null;
        [SerializeField] private Text statusLabel = null;

        private readonly List<AttachmentSlotDefinition> displayedSlots =
            new List<AttachmentSlotDefinition>();
        private readonly List<WeaponDefinition> displayedWeapons =
            new List<WeaponDefinition>();
        private readonly List<Button> generatedButtons = new List<Button>();
        private bool isSubscribed;
        private bool isSpawnerSubscribed;

        private void OnEnable()
        {
            SubscribeToSpawner();
            if (weapon == null && spawner != null)
            {
                weapon = spawner.CurrentWeapon;
            }

            Subscribe();
            ConnectControls();
            RefreshAll();
        }

        private void OnDisable()
        {
            DisconnectControls();
            Unsubscribe();
            UnsubscribeFromSpawner();
            ClearGeneratedButtons();
        }

        /// <summary>Rebinds the sample UI to another initialized weapon.</summary>
        public void Bind(WeaponRuntime targetWeapon, WeaponCatalog targetCatalog)
        {
            Unsubscribe();
            weapon = targetWeapon;
            catalog = targetCatalog;

            if (isActiveAndEnabled)
            {
                Subscribe();
                RefreshAll();
            }
        }

        public void RefreshAll()
        {
            SetText(
                weaponNameLabel,
                weapon != null && weapon.Definition != null
                    ? weapon.Definition.DisplayName
                    : "No weapon");
            PopulateWeapons();
            PopulateSlots();
            RefreshSelectedSlot();
            RefreshStats();
        }

        private void Subscribe()
        {
            if (isSubscribed || weapon == null)
            {
                return;
            }

            weapon.LoadoutChanged += HandleLoadoutChanged;
            weapon.StatsChanged += HandleStatsChanged;
            isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!isSubscribed)
            {
                return;
            }

            if (weapon != null)
            {
                weapon.LoadoutChanged -= HandleLoadoutChanged;
                weapon.StatsChanged -= HandleStatsChanged;
            }

            isSubscribed = false;
        }

        private void SubscribeToSpawner()
        {
            if (isSpawnerSubscribed || spawner == null)
            {
                return;
            }

            spawner.CurrentWeaponChanged += HandleCurrentWeaponChanged;
            isSpawnerSubscribed = true;
        }

        private void UnsubscribeFromSpawner()
        {
            if (!isSpawnerSubscribed)
            {
                return;
            }

            if (spawner != null)
            {
                spawner.CurrentWeaponChanged -= HandleCurrentWeaponChanged;
            }

            isSpawnerSubscribed = false;
        }

        private void ConnectControls()
        {
            if (weaponDropdown != null)
            {
                weaponDropdown.onValueChanged.RemoveListener(HandleWeaponChanged);
                weaponDropdown.onValueChanged.AddListener(HandleWeaponChanged);
            }

            if (slotDropdown != null)
            {
                slotDropdown.onValueChanged.RemoveListener(HandleSlotChanged);
                slotDropdown.onValueChanged.AddListener(HandleSlotChanged);
            }

            if (removeAttachmentButton != null)
            {
                removeAttachmentButton.onClick.RemoveListener(RemoveSelectedAttachment);
                removeAttachmentButton.onClick.AddListener(RemoveSelectedAttachment);
            }
        }

        private void DisconnectControls()
        {
            if (weaponDropdown != null)
            {
                weaponDropdown.onValueChanged.RemoveListener(HandleWeaponChanged);
            }

            if (slotDropdown != null)
            {
                slotDropdown.onValueChanged.RemoveListener(HandleSlotChanged);
            }

            if (removeAttachmentButton != null)
            {
                removeAttachmentButton.onClick.RemoveListener(RemoveSelectedAttachment);
            }
        }

        private void PopulateWeapons()
        {
            displayedWeapons.Clear();
            if (catalog != null)
            {
                foreach (WeaponDefinition definition in catalog.Weapons)
                {
                    if (definition != null)
                    {
                        displayedWeapons.Add(definition);
                    }
                }
            }

            if (weaponDropdown == null)
            {
                return;
            }

            var options = new List<Dropdown.OptionData>(displayedWeapons.Count);
            int selectedIndex = 0;
            for (int i = 0; i < displayedWeapons.Count; i++)
            {
                options.Add(
                    new Dropdown.OptionData(displayedWeapons[i].DisplayName));
                if (weapon != null &&
                    displayedWeapons[i] == weapon.Definition)
                {
                    selectedIndex = i;
                }
            }

            weaponDropdown.ClearOptions();
            weaponDropdown.AddOptions(options);
            weaponDropdown.SetValueWithoutNotify(selectedIndex);
            weaponDropdown.interactable =
                spawner != null && displayedWeapons.Count > 1;
        }

        private void HandleWeaponChanged(int index)
        {
            if (spawner == null || index < 0 || index >= displayedWeapons.Count)
            {
                return;
            }

            WeaponDefinition definition = displayedWeapons[index];
            if (!spawner.TrySpawn(
                    definition,
                    out _,
                    out WeaponSpawnFailure failure))
            {
                SetStatus(
                    $"Could not select {definition.DisplayName}: {failure}.");
                PopulateWeapons();
            }
        }

        private void PopulateSlots()
        {
            displayedSlots.Clear();
            if (weapon != null && weapon.Definition != null)
            {
                foreach (AttachmentSlotDefinition slot in
                         weapon.Definition.SupportedSlots)
                {
                    if (slot != null)
                    {
                        displayedSlots.Add(slot);
                    }
                }
            }

            if (slotDropdown == null)
            {
                return;
            }

            int previousIndex = Mathf.Clamp(
                slotDropdown.value,
                0,
                Mathf.Max(0, displayedSlots.Count - 1));
            var options = new List<Dropdown.OptionData>(displayedSlots.Count);
            foreach (AttachmentSlotDefinition slot in displayedSlots)
            {
                options.Add(new Dropdown.OptionData(slot.DisplayName));
            }

            slotDropdown.ClearOptions();
            slotDropdown.AddOptions(options);
            slotDropdown.SetValueWithoutNotify(previousIndex);
            slotDropdown.interactable = displayedSlots.Count > 1;
        }

        private void HandleSlotChanged(int _)
        {
            RefreshSelectedSlot();
        }

        private void RefreshSelectedSlot()
        {
            ClearGeneratedButtons();
            AttachmentSlotDefinition slot = GetSelectedSlot();
            if (slot == null || weapon == null || weapon.Loadout == null)
            {
                SetText(installedAttachmentLabel, "No attachment slot");
                SetRemoveInteractable(false);
                return;
            }

            bool hasInstalled = weapon.Loadout.TryGetAttachment(
                slot,
                out WeaponAttachmentDefinition installed);
            SetText(
                installedAttachmentLabel,
                hasInstalled
                    ? $"Installed: {installed.DisplayName}"
                    : "Installed: None");
            SetRemoveInteractable(hasInstalled);

            if (catalog == null || attachmentButtonTemplate == null ||
                attachmentListContent == null)
            {
                return;
            }

            foreach (WeaponAttachmentDefinition attachment in catalog.Attachments)
            {
                if (attachment == null || attachment.Slot != slot ||
                    WeaponCompatibility.Evaluate(weapon.Definition, attachment) !=
                    AttachmentInstallFailure.None)
                {
                    continue;
                }

                CreateAttachmentButton(attachment, attachment == installed);
            }
        }

        private void CreateAttachmentButton(
            WeaponAttachmentDefinition attachment,
            bool isInstalled)
        {
            Button button = Instantiate(
                attachmentButtonTemplate,
                attachmentListContent,
                false);
            button.gameObject.SetActive(true);
            button.name = $"Attachment - {attachment.DisplayName}";
            button.interactable = !isInstalled;

            Text label = button.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.text = isInstalled
                    ? $"{attachment.DisplayName} (Installed)"
                    : attachment.DisplayName;
            }

            WeaponAttachmentDefinition capturedAttachment = attachment;
            button.onClick.AddListener(() => Install(capturedAttachment));
            generatedButtons.Add(button);
        }

        private void Install(WeaponAttachmentDefinition attachment)
        {
            if (weapon == null)
            {
                SetStatus("No weapon is bound.");
                return;
            }

            if (weapon.TryInstall(attachment, out AttachmentInstallFailure failure))
            {
                SetStatus($"Installed {attachment.DisplayName}.");
            }
            else
            {
                SetStatus($"Could not install {attachment.DisplayName}: {failure}.");
            }
        }

        private void RemoveSelectedAttachment()
        {
            AttachmentSlotDefinition slot = GetSelectedSlot();
            if (weapon == null || slot == null)
            {
                SetStatus("No attachment slot is selected.");
                return;
            }

            if (weapon.TryRemove(
                    slot,
                    out WeaponAttachmentDefinition removed,
                    out AttachmentInstallFailure failure))
            {
                SetStatus(
                    removed != null
                        ? $"Removed {removed.DisplayName}."
                        : "The selected slot was already empty.");
            }
            else
            {
                SetStatus($"Could not clear {slot.DisplayName}: {failure}.");
            }
        }

        private void RefreshStats()
        {
            if (statsLabel == null)
            {
                return;
            }

            if (weapon == null || weapon.EffectiveStats == null)
            {
                statsLabel.text = string.Empty;
                return;
            }

            var builder = new StringBuilder();
            foreach (WeaponStatValue stat in weapon.EffectiveStats.Entries)
            {
                if (stat.Definition == null)
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(stat.Definition.DisplayName);
                builder.Append(": ");
                builder.Append(stat.Value.ToString("0.##"));
                if (!string.IsNullOrWhiteSpace(stat.Definition.Unit))
                {
                    builder.Append(' ');
                    builder.Append(stat.Definition.Unit);
                }
            }

            statsLabel.text = builder.ToString();
        }

        private void HandleLoadoutChanged(
            object sender,
            WeaponLoadoutChangedEventArgs eventArgs)
        {
            RefreshSelectedSlot();
        }

        private void HandleStatsChanged(WeaponStatsSnapshot snapshot)
        {
            RefreshStats();
        }

        private void HandleCurrentWeaponChanged(
            object sender,
            WeaponSpawnChangedEventArgs eventArgs)
        {
            Bind(eventArgs.CurrentWeapon, catalog);
        }

        private AttachmentSlotDefinition GetSelectedSlot()
        {
            int index = slotDropdown != null ? slotDropdown.value : 0;
            return index >= 0 && index < displayedSlots.Count
                ? displayedSlots[index]
                : null;
        }

        private void ClearGeneratedButtons()
        {
            foreach (Button button in generatedButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }

            generatedButtons.Clear();
        }

        private void SetRemoveInteractable(bool value)
        {
            if (removeAttachmentButton != null)
            {
                removeAttachmentButton.interactable = value;
            }
        }

        private void SetStatus(string value)
        {
            SetText(statusLabel, value);
        }

        private static void SetText(Text label, string value)
        {
            if (label != null)
            {
                label.text = value;
            }
        }
    }
}
