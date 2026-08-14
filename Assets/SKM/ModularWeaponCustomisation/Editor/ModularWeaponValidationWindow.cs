using System;
using System.Collections.Generic;
using SKM.ModularWeaponCustomisation.Persistence;
using UnityEditor;
using UnityEngine;

namespace SKM.ModularWeaponCustomisation.Editor
{
    /// <summary>Finds actionable authoring errors before entering Play Mode.</summary>
    public sealed class ModularWeaponValidationWindow : EditorWindow
    {
        private enum ValidationSeverity
        {
            Info = 0,
            Warning = 1,
            Error = 2
        }

        private sealed class ValidationMessage
        {
            public ValidationMessage(
                ValidationSeverity severity,
                string message,
                UnityEngine.Object context)
            {
                Severity = severity;
                Message = message;
                Context = context;
                AssetPath = context != null
                    ? AssetDatabase.GetAssetPath(context)
                    : string.Empty;
            }

            public ValidationSeverity Severity { get; }
            public string Message { get; }
            public UnityEngine.Object Context { get; }
            public string AssetPath { get; }
        }

        private readonly List<ValidationMessage> messages =
            new List<ValidationMessage>();
        private Vector2 scrollPosition;
        private bool hasScanned;

        [MenuItem("Tools/SKM/Modular Weapon Customisation/Validator")]
        public static void Open()
        {
            var window = GetWindow<ModularWeaponValidationWindow>();
            window.titleContent = new GUIContent("Weapon Validator");
            window.minSize = new Vector2(520f, 320f);
            window.Show();
        }

        public static void OpenAndScan()
        {
            Open();
            GetWindow<ModularWeaponValidationWindow>().ScanProject();
        }

        /// <summary>Runs the same validation without opening a window.</summary>
        public static bool ValidateProject(out int errors, out int warnings)
        {
            ModularWeaponValidationWindow validator =
                CreateInstance<ModularWeaponValidationWindow>();
            try
            {
                validator.ScanProject();
                validator.CountMessages(out errors, out warnings);
                return errors == 0;
            }
            finally
            {
                DestroyImmediate(validator);
            }
        }

        /// <summary>Entry point for Unity's -executeMethod release checks.</summary>
        public static void ValidateFromCommandLine()
        {
            ModularWeaponValidationWindow validator =
                CreateInstance<ModularWeaponValidationWindow>();
            try
            {
                validator.ScanProject();
                validator.CountMessages(out int errors, out int warnings);
                foreach (ValidationMessage message in validator.messages)
                {
                    string formatted =
                        $"[{message.Severity}] {message.Message} {message.AssetPath}";
                    if (message.Severity == ValidationSeverity.Error)
                    {
                        Debug.LogError(formatted, message.Context);
                    }
                    else if (message.Severity == ValidationSeverity.Warning)
                    {
                        Debug.LogWarning(formatted, message.Context);
                    }
                    else
                    {
                        Debug.Log(formatted, message.Context);
                    }
                }

                if (errors > 0)
                {
                    throw new InvalidOperationException(
                        $"Framework validation failed with {errors} error(s) and " +
                        $"{warnings} warning(s).");
                }

                Debug.Log(
                    $"Framework validation passed with {warnings} warning(s).");
            }
            finally
            {
                DestroyImmediate(validator);
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Modular Weapon Customisation Validator",
                EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Scans definitions and prefabs for configuration errors.",
                EditorStyles.wordWrappedLabel);

            EditorGUILayout.Space();
            if (GUILayout.Button("Scan Project", GUILayout.Height(28f)))
            {
                ScanProject();
            }

            EditorGUILayout.Space();
            if (!hasScanned)
            {
                EditorGUILayout.HelpBox(
                    "Run a scan before packaging or after changing authoring data.",
                    MessageType.Info);
                return;
            }

            DrawSummary();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (ValidationMessage message in messages)
            {
                DrawMessage(message);
            }

            EditorGUILayout.EndScrollView();
        }

        private void ScanProject()
        {
            messages.Clear();

            List<AttachmentSlotDefinition> slots =
                LoadAllAssets<AttachmentSlotDefinition>();
            List<WeaponStatDefinition> stats =
                LoadAllAssets<WeaponStatDefinition>();
            List<WeaponAttachmentDefinition> attachments =
                LoadAllAssets<WeaponAttachmentDefinition>();
            List<WeaponDefinition> weapons =
                LoadAllAssets<WeaponDefinition>();
            List<WeaponCatalog> catalogs =
                LoadAllAssets<WeaponCatalog>();

            ValidateStableIds(slots, slot => slot.Id, "attachment slot");
            ValidateStableIds(stats, stat => stat.Id, "weapon stat");
            ValidateStableIds(attachments, attachment => attachment.Id, "attachment");
            ValidateStableIds(weapons, weapon => weapon.Id, "weapon");

            foreach (AttachmentSlotDefinition slot in slots)
            {
                if (!slot.IsValid)
                {
                    AddError("Attachment slot requires a non-empty stable ID.", slot);
                }
            }

            foreach (WeaponStatDefinition stat in stats)
            {
                if (!stat.IsValid)
                {
                    AddError(
                        "Weapon stat requires a stable ID and a maximum greater than its minimum.",
                        stat);
                }
            }

            foreach (WeaponAttachmentDefinition attachment in attachments)
            {
                ValidateAttachment(attachment);
            }

            foreach (WeaponDefinition weapon in weapons)
            {
                ValidateWeapon(weapon);
            }

            foreach (WeaponCatalog catalog in catalogs)
            {
                ValidateCatalog(catalog);
            }

            messages.Sort(CompareMessages);
            hasScanned = true;
            Repaint();
        }

        private void ValidateAttachment(WeaponAttachmentDefinition attachment)
        {
            if (!attachment.IsValid)
            {
                AddError("Attachment requires a stable ID and slot definition.", attachment);
            }

            if (attachment.Prefab == null)
            {
                AddWarning("Attachment has no visual prefab.", attachment);
            }
            else if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                         attachment.Prefab) > 0)
            {
                AddError("Attachment prefab contains a missing script.", attachment.Prefab);
            }

            foreach (WeaponStatModifier modifier in attachment.StatModifiers)
            {
                if (modifier.Definition == null)
                {
                    AddError("Attachment contains a modifier with no stat definition.", attachment);
                    continue;
                }

                if (float.IsNaN(modifier.Value) || float.IsInfinity(modifier.Value))
                {
                    AddError("Attachment contains a non-finite stat modifier.", attachment);
                }
            }

            ValidateCompatibilityLists(attachment);
        }

        private void ValidateCompatibilityLists(
            WeaponAttachmentDefinition attachment)
        {
            var compatibleWeapons = new HashSet<WeaponDefinition>();
            foreach (WeaponDefinition weapon in attachment.CompatibleWeapons)
            {
                if (weapon == null)
                {
                    AddError(
                        "Attachment contains an empty compatible-weapon entry.",
                        attachment);
                    continue;
                }

                if (!compatibleWeapons.Add(weapon))
                {
                    AddError(
                        $"Compatible weapon '{weapon.DisplayName}' is listed more than once.",
                        attachment);
                }
            }

            var incompatibleWeapons = new HashSet<WeaponDefinition>();
            foreach (WeaponDefinition weapon in attachment.IncompatibleWeapons)
            {
                if (weapon == null)
                {
                    AddError(
                        "Attachment contains an empty incompatible-weapon entry.",
                        attachment);
                    continue;
                }

                if (!incompatibleWeapons.Add(weapon))
                {
                    AddError(
                        $"Incompatible weapon '{weapon.DisplayName}' is listed more than once.",
                        attachment);
                }

                if (compatibleWeapons.Contains(weapon))
                {
                    AddError(
                        $"Weapon '{weapon.DisplayName}' appears in both compatibility lists.",
                        attachment);
                }
            }
        }

        private void ValidateWeapon(WeaponDefinition weapon)
        {
            if (!weapon.IsValid)
            {
                AddError("Weapon requires a stable ID and prefab.", weapon);
            }

            ValidateWeaponStats(weapon);
            ValidateWeaponSlots(weapon);
            ValidateStartingAttachments(weapon);
            ValidateWeaponPrefab(weapon);
        }

        private void ValidateWeaponStats(WeaponDefinition weapon)
        {
            var seenStats = new HashSet<WeaponStatDefinition>();
            foreach (WeaponStatValue stat in weapon.BaseStats)
            {
                if (stat.Definition == null)
                {
                    AddError("Weapon contains a base stat with no definition.", weapon);
                    continue;
                }

                if (!seenStats.Add(stat.Definition))
                {
                    AddError(
                        $"Weapon contains duplicate base stat '{stat.Definition.DisplayName}'.",
                        weapon);
                }

                if (float.IsNaN(stat.Value) || float.IsInfinity(stat.Value))
                {
                    AddError("Weapon contains a non-finite base stat value.", weapon);
                }
            }
        }

        private void ValidateWeaponSlots(WeaponDefinition weapon)
        {
            var seenSlots = new HashSet<AttachmentSlotDefinition>();
            foreach (AttachmentSlotDefinition slot in weapon.SupportedSlots)
            {
                if (slot == null)
                {
                    AddError("Weapon contains an empty supported-slot entry.", weapon);
                    continue;
                }

                if (!seenSlots.Add(slot))
                {
                    AddError(
                        $"Weapon contains duplicate slot '{slot.DisplayName}'.",
                        weapon);
                }
            }
        }

        private void ValidateStartingAttachments(WeaponDefinition weapon)
        {
            var occupiedSlots = new HashSet<AttachmentSlotDefinition>();
            foreach (WeaponAttachmentDefinition attachment in weapon.StartingAttachments)
            {
                if (attachment == null)
                {
                    AddError("Weapon contains an empty starting-attachment entry.", weapon);
                    continue;
                }

                AttachmentInstallFailure failure =
                    WeaponCompatibility.Evaluate(weapon, attachment);
                if (failure != AttachmentInstallFailure.None)
                {
                    AddError(
                        $"Starting attachment '{attachment.DisplayName}' is invalid: {failure}.",
                        weapon);
                }

                if (attachment.Slot != null && !occupiedSlots.Add(attachment.Slot))
                {
                    AddError(
                        $"More than one starting attachment uses slot " +
                        $"'{attachment.Slot.DisplayName}'.",
                        weapon);
                }
            }
        }

        private void ValidateWeaponPrefab(WeaponDefinition weapon)
        {
            if (weapon.Prefab == null)
            {
                return;
            }

            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(weapon.Prefab) > 0)
            {
                AddError("Weapon prefab contains a missing script.", weapon.Prefab);
            }

            WeaponRuntime runtime = weapon.Prefab.GetComponent<WeaponRuntime>();
            if (runtime == null)
            {
                AddError("Weapon prefab does not contain WeaponRuntime on its root.", weapon.Prefab);
                return;
            }

            if (runtime.Definition != weapon)
            {
                AddError("Weapon prefab's runtime definition does not match this asset.", weapon);
            }

            AttachmentSocket[] prefabSockets =
                weapon.Prefab.GetComponentsInChildren<AttachmentSocket>(true);
            var socketSlots = new HashSet<AttachmentSlotDefinition>();

            foreach (AttachmentSocket socket in prefabSockets)
            {
                if (socket.Slot == null)
                {
                    AddError("Weapon prefab contains a socket with no slot.", socket);
                    continue;
                }

                if (!socketSlots.Add(socket.Slot))
                {
                    AddError(
                        $"Weapon prefab contains duplicate sockets for " +
                        $"'{socket.Slot.DisplayName}'.",
                        weapon.Prefab);
                }
            }

            foreach (AttachmentSlotDefinition slot in weapon.SupportedSlots)
            {
                if (slot != null && !socketSlots.Contains(slot))
                {
                    AddError(
                        $"Weapon prefab has no socket for supported slot '{slot.DisplayName}'.",
                        weapon.Prefab);
                }
            }
        }

        private void ValidateCatalog(WeaponCatalog catalog)
        {
            var weaponAssets = new HashSet<WeaponDefinition>();
            var weaponIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (WeaponDefinition weapon in catalog.Weapons)
            {
                if (weapon == null)
                {
                    AddError("Weapon catalog contains an empty weapon entry.", catalog);
                    continue;
                }

                if (!weaponAssets.Add(weapon))
                {
                    AddError(
                        $"Weapon catalog contains '{weapon.name}' more than once.",
                        catalog);
                }

                if (!string.IsNullOrWhiteSpace(weapon.Id) && !weaponIds.Add(weapon.Id))
                {
                    AddError(
                        $"Weapon catalog resolves more than one weapon as ID '{weapon.Id}'.",
                        catalog);
                }
            }

            var attachmentAssets = new HashSet<WeaponAttachmentDefinition>();
            var attachmentIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (WeaponAttachmentDefinition attachment in catalog.Attachments)
            {
                if (attachment == null)
                {
                    AddError("Weapon catalog contains an empty attachment entry.", catalog);
                    continue;
                }

                if (!attachmentAssets.Add(attachment))
                {
                    AddError(
                        $"Weapon catalog contains '{attachment.name}' more than once.",
                        catalog);
                }

                if (!string.IsNullOrWhiteSpace(attachment.Id) &&
                    !attachmentIds.Add(attachment.Id))
                {
                    AddError(
                        $"Weapon catalog resolves more than one attachment as ID " +
                        $"'{attachment.Id}'.",
                        catalog);
                }
            }

            if (catalog.Weapons.Count == 0)
            {
                AddWarning("Weapon catalog does not contain any weapons.", catalog);
            }
        }

        private void DrawSummary()
        {
            CountMessages(out int errors, out int warnings);

            MessageType type = errors > 0
                ? MessageType.Error
                : warnings > 0
                    ? MessageType.Warning
                    : MessageType.Info;
            string summary = messages.Count == 0
                ? "Validation passed with no issues."
                : $"Validation found {errors} error(s) and {warnings} warning(s).";

            EditorGUILayout.HelpBox(summary, type);
        }

        private void CountMessages(out int errors, out int warnings)
        {
            errors = 0;
            warnings = 0;
            foreach (ValidationMessage message in messages)
            {
                if (message.Severity == ValidationSeverity.Error)
                {
                    errors++;
                }
                else if (message.Severity == ValidationSeverity.Warning)
                {
                    warnings++;
                }
            }
        }

        private static void DrawMessage(ValidationMessage message)
        {
            MessageType messageType = message.Severity == ValidationSeverity.Error
                ? MessageType.Error
                : message.Severity == ValidationSeverity.Warning
                    ? MessageType.Warning
                    : MessageType.Info;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.HelpBox(message.Message, messageType);

            if (!string.IsNullOrEmpty(message.AssetPath) &&
                GUILayout.Button(message.AssetPath, EditorStyles.linkLabel))
            {
                Selection.activeObject = message.Context;
                EditorGUIUtility.PingObject(message.Context);
            }

            EditorGUILayout.EndVertical();
        }

        private void AddError(string message, UnityEngine.Object context)
        {
            messages.Add(new ValidationMessage(ValidationSeverity.Error, message, context));
        }

        private void AddWarning(string message, UnityEngine.Object context)
        {
            messages.Add(new ValidationMessage(ValidationSeverity.Warning, message, context));
        }

        private void ValidateStableIds<T>(
            IEnumerable<T> assets,
            Func<T, string> getId,
            string assetType)
            where T : UnityEngine.Object
        {
            var assetsById = new Dictionary<string, T>(StringComparer.Ordinal);
            foreach (T asset in assets)
            {
                string id = getId(asset);
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                if (assetsById.TryGetValue(id, out T existing))
                {
                    AddError(
                        $"Duplicate {assetType} ID '{id}'. Also used by " +
                        $"'{existing.name}'.",
                        asset);
                }
                else
                {
                    assetsById.Add(id, asset);
                }
            }
        }

        private static List<T> LoadAllAssets<T>() where T : UnityEngine.Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            var assets = new List<T>(guids.Length);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets;
        }

        private static int CompareMessages(ValidationMessage left, ValidationMessage right)
        {
            int severityComparison = right.Severity.CompareTo(left.Severity);
            return severityComparison != 0
                ? severityComparison
                : string.Compare(left.AssetPath, right.AssetPath, StringComparison.Ordinal);
        }
    }
}
