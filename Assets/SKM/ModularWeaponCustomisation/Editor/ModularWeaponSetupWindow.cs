using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SKM.ModularWeaponCustomisation.Editor
{
    /// <summary>Creates connected definition assets and prefab components.</summary>
    public sealed class ModularWeaponSetupWindow : EditorWindow
    {
        [SerializeField] private GameObject weaponPrefab = null;
        [SerializeField] private string weaponId = string.Empty;
        [SerializeField] private string weaponDisplayName = string.Empty;
        [SerializeField] private List<AttachmentSlotDefinition> weaponSlots =
            new List<AttachmentSlotDefinition>();

        [SerializeField] private GameObject attachmentPrefab = null;
        [SerializeField] private AttachmentSlotDefinition attachmentSlot = null;
        [SerializeField] private string attachmentId = string.Empty;
        [SerializeField] private string attachmentDisplayName = string.Empty;

        private Vector2 scrollPosition;
        private string feedback = string.Empty;
        private MessageType feedbackType = MessageType.Info;

        [MenuItem("Tools/SKM/Modular Weapon Customisation/Setup Wizard")]
        public static void Open()
        {
            var window = GetWindow<ModularWeaponSetupWindow>();
            window.titleContent = new GUIContent("Weapon Setup");
            window.minSize = new Vector2(500f, 500f);
            window.Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Modular Weapon Customisation Setup",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This wizard creates definition assets. Weapon setup also adds " +
                "WeaponRuntime and missing socket placeholders to a regular prefab.",
                MessageType.Info);

            DrawWeaponSetup();
            EditorGUILayout.Space(16f);
            DrawAttachmentSetup();

            if (!string.IsNullOrEmpty(feedback))
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(feedback, feedbackType);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawWeaponSetup()
        {
            EditorGUILayout.LabelField("Create Weapon", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            weaponPrefab = (GameObject)EditorGUILayout.ObjectField(
                "Prefab",
                weaponPrefab,
                typeof(GameObject),
                allowSceneObjects: false);
            if (EditorGUI.EndChangeCheck() && weaponPrefab != null)
            {
                if (string.IsNullOrWhiteSpace(weaponDisplayName))
                {
                    weaponDisplayName = weaponPrefab.name;
                }

                if (string.IsNullOrWhiteSpace(weaponId))
                {
                    weaponId = ToStableId(weaponPrefab.name);
                }
            }

            weaponId = EditorGUILayout.TextField("Stable ID", weaponId);
            weaponDisplayName = EditorGUILayout.TextField(
                "Display Name",
                weaponDisplayName);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Supported Slots", EditorStyles.boldLabel);
            int removalIndex = -1;
            for (int i = 0; i < weaponSlots.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                weaponSlots[i] = (AttachmentSlotDefinition)EditorGUILayout.ObjectField(
                    $"Slot {i + 1}",
                    weaponSlots[i],
                    typeof(AttachmentSlotDefinition),
                    allowSceneObjects: false);
                if (GUILayout.Button("Remove", GUILayout.Width(70f)))
                {
                    removalIndex = i;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (removalIndex >= 0)
            {
                weaponSlots.RemoveAt(removalIndex);
            }

            if (GUILayout.Button("Add Slot"))
            {
                weaponSlots.Add(null);
            }

            EditorGUILayout.HelpBox(
                "New socket objects are created at the prefab root origin. Position " +
                "and rotate them at the correct mount points before using the weapon.",
                MessageType.Warning);

            using (new EditorGUI.DisabledScope(weaponPrefab == null))
            {
                if (GUILayout.Button("Create and Configure Weapon", GUILayout.Height(28f)))
                {
                    CreateWeapon();
                }
            }
        }

        private void DrawAttachmentSetup()
        {
            EditorGUILayout.LabelField("Create Attachment", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            attachmentPrefab = (GameObject)EditorGUILayout.ObjectField(
                "Visual Prefab",
                attachmentPrefab,
                typeof(GameObject),
                allowSceneObjects: false);
            if (EditorGUI.EndChangeCheck() && attachmentPrefab != null)
            {
                if (string.IsNullOrWhiteSpace(attachmentDisplayName))
                {
                    attachmentDisplayName = attachmentPrefab.name;
                }

                if (string.IsNullOrWhiteSpace(attachmentId))
                {
                    attachmentId = ToStableId(attachmentPrefab.name);
                }
            }

            attachmentSlot = (AttachmentSlotDefinition)EditorGUILayout.ObjectField(
                "Slot",
                attachmentSlot,
                typeof(AttachmentSlotDefinition),
                allowSceneObjects: false);
            attachmentId = EditorGUILayout.TextField("Stable ID", attachmentId);
            attachmentDisplayName = EditorGUILayout.TextField(
                "Display Name",
                attachmentDisplayName);

            using (new EditorGUI.DisabledScope(attachmentSlot == null))
            {
                if (GUILayout.Button("Create Attachment Definition", GUILayout.Height(28f)))
                {
                    CreateAttachment();
                }
            }
        }

        private void CreateWeapon()
        {
            if (!TryValidateWeaponInput(out string error))
            {
                SetFeedback(error, MessageType.Error);
                return;
            }

            string savePath = EditorUtility.SaveFilePanelInProject(
                "Create Weapon Definition",
                $"{weaponPrefab.name}_WeaponDefinition.asset",
                "asset",
                "Choose where to save the weapon definition.");
            if (string.IsNullOrEmpty(savePath))
            {
                return;
            }

            savePath = AssetDatabase.GenerateUniqueAssetPath(savePath);
            var definition = CreateInstance<WeaponDefinition>();
            WriteWeaponDefinition(definition);
            AssetDatabase.CreateAsset(definition, savePath);

            try
            {
                ConfigureWeaponPrefab(definition);
                AssetDatabase.SaveAssets();
                Selection.activeObject = definition;
                EditorGUIUtility.PingObject(definition);
                SetFeedback(
                    $"Created '{savePath}' and configured '{weaponPrefab.name}'. " +
                    "Position every generated socket, then run the validator.",
                    MessageType.Info);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                SetFeedback(
                    $"The definition was created at '{savePath}', but prefab setup " +
                    $"failed: {exception.Message}",
                    MessageType.Error);
            }
        }

        private void CreateAttachment()
        {
            string stableId = attachmentId?.Trim();
            if (string.IsNullOrWhiteSpace(stableId))
            {
                SetFeedback("Attachment stable ID cannot be empty.", MessageType.Error);
                return;
            }

            if (StableIdExists<WeaponAttachmentDefinition>(
                    stableId,
                    definition => definition.Id))
            {
                SetFeedback(
                    $"Attachment stable ID '{stableId}' is already in use.",
                    MessageType.Error);
                return;
            }

            if (attachmentSlot == null)
            {
                SetFeedback("Choose the attachment slot first.", MessageType.Error);
                return;
            }

            string suggestedName = attachmentPrefab != null
                ? attachmentPrefab.name
                : stableId;
            string savePath = EditorUtility.SaveFilePanelInProject(
                "Create Attachment Definition",
                $"{suggestedName}_AttachmentDefinition.asset",
                "asset",
                "Choose where to save the attachment definition.");
            if (string.IsNullOrEmpty(savePath))
            {
                return;
            }

            savePath = AssetDatabase.GenerateUniqueAssetPath(savePath);
            var definition = CreateInstance<WeaponAttachmentDefinition>();
            var serialized = new SerializedObject(definition);
            serialized.FindProperty("id").stringValue = stableId;
            serialized.FindProperty("displayName").stringValue =
                DisplayNameOrFallback(attachmentDisplayName, suggestedName);
            serialized.FindProperty("prefab").objectReferenceValue = attachmentPrefab;
            serialized.FindProperty("slot").objectReferenceValue = attachmentSlot;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(definition, savePath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = definition;
            EditorGUIUtility.PingObject(definition);
            SetFeedback($"Created '{savePath}'.", MessageType.Info);
        }

        private bool TryValidateWeaponInput(out string error)
        {
            if (weaponPrefab == null)
            {
                error = "Choose a weapon prefab.";
                return false;
            }

            string prefabPath = AssetDatabase.GetAssetPath(weaponPrefab);
            if (string.IsNullOrEmpty(prefabPath) ||
                !prefabPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase) ||
                !PrefabUtility.IsPartOfPrefabAsset(weaponPrefab))
            {
                error = "The weapon must be a prefab asset, not a scene object.";
                return false;
            }

            if (PrefabUtility.GetPrefabAssetType(weaponPrefab) == PrefabAssetType.Model)
            {
                error = "Model prefabs are read-only. Create a regular prefab or prefab " +
                        "variant and use that instead.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(weaponId))
            {
                error = "Weapon stable ID cannot be empty.";
                return false;
            }

            if (StableIdExists<WeaponDefinition>(
                    weaponId.Trim(),
                    definition => definition.Id))
            {
                error = $"Weapon stable ID '{weaponId.Trim()}' is already in use.";
                return false;
            }

            var seenSlots = new HashSet<AttachmentSlotDefinition>();
            foreach (AttachmentSlotDefinition slot in weaponSlots)
            {
                if (slot == null)
                {
                    error = "Remove or assign every empty slot entry.";
                    return false;
                }

                if (!seenSlots.Add(slot))
                {
                    error = $"Slot '{slot.DisplayName}' is listed more than once.";
                    return false;
                }
            }

            WeaponRuntime existingRuntime = weaponPrefab.GetComponent<WeaponRuntime>();
            if (existingRuntime != null && existingRuntime.Definition != null)
            {
                error = $"Prefab already references weapon definition " +
                        $"'{existingRuntime.Definition.name}'. Clear it before creating " +
                        "a replacement definition.";
                return false;
            }

            WeaponRuntime[] allRuntimes =
                weaponPrefab.GetComponentsInChildren<WeaponRuntime>(true);
            if (allRuntimes.Length > 0 && existingRuntime == null)
            {
                error = "The prefab contains WeaponRuntime below its root. Move the " +
                        "component to the root before running the wizard.";
                return false;
            }

            error = string.Empty;
            return true;
        }

        private void WriteWeaponDefinition(WeaponDefinition definition)
        {
            var serialized = new SerializedObject(definition);
            serialized.FindProperty("id").stringValue = weaponId.Trim();
            serialized.FindProperty("displayName").stringValue =
                DisplayNameOrFallback(weaponDisplayName, weaponPrefab.name);
            serialized.FindProperty("prefab").objectReferenceValue = weaponPrefab;

            SerializedProperty slots = serialized.FindProperty("supportedSlots");
            slots.arraySize = weaponSlots.Count;
            for (int i = 0; i < weaponSlots.Count; i++)
            {
                slots.GetArrayElementAtIndex(i).objectReferenceValue = weaponSlots[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private void ConfigureWeaponPrefab(WeaponDefinition definition)
        {
            string prefabPath = AssetDatabase.GetAssetPath(weaponPrefab);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

            try
            {
                WeaponRuntime runtime = prefabRoot.GetComponent<WeaponRuntime>();
                if (runtime == null)
                {
                    runtime = prefabRoot.AddComponent<WeaponRuntime>();
                }

                var runtimeSerialized = new SerializedObject(runtime);
                runtimeSerialized.FindProperty("definition").objectReferenceValue = definition;
                runtimeSerialized.ApplyModifiedPropertiesWithoutUndo();

                AttachmentSocket[] existingSockets =
                    prefabRoot.GetComponentsInChildren<AttachmentSocket>(true);
                var existingSlots = new HashSet<AttachmentSlotDefinition>();
                foreach (AttachmentSocket socket in existingSockets)
                {
                    if (socket.Slot != null)
                    {
                        existingSlots.Add(socket.Slot);
                    }
                }

                foreach (AttachmentSlotDefinition slot in weaponSlots)
                {
                    if (existingSlots.Contains(slot))
                    {
                        continue;
                    }

                    var socketObject = new GameObject($"Socket - {slot.DisplayName}");
                    socketObject.transform.SetParent(prefabRoot.transform, false);
                    AttachmentSocket socket = socketObject.AddComponent<AttachmentSocket>();
                    var socketSerialized = new SerializedObject(socket);
                    socketSerialized.FindProperty("slot").objectReferenceValue = slot;
                    socketSerialized.ApplyModifiedPropertiesWithoutUndo();
                }

                if (PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath) == null)
                {
                    throw new InvalidOperationException("Unity did not save the configured prefab.");
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private void SetFeedback(string message, MessageType type)
        {
            feedback = message;
            feedbackType = type;
            Repaint();
        }

        private static string DisplayNameOrFallback(string value, string fallback)
        {
            string trimmed = value?.Trim();
            return string.IsNullOrWhiteSpace(trimmed) ? fallback : trimmed;
        }

        private static string ToStableId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "weapon";
            }

            var result = new StringBuilder(value.Length);
            bool previousWasSeparator = false;
            foreach (char character in value.Trim().ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(character))
                {
                    result.Append(character);
                    previousWasSeparator = false;
                }
                else if (!previousWasSeparator && result.Length > 0)
                {
                    result.Append('-');
                    previousWasSeparator = true;
                }
            }

            return result.ToString().TrimEnd('-');
        }

        private static bool StableIdExists<T>(
            string id,
            Func<T, string> getId)
            where T : UnityEngine.Object
        {
            foreach (string guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null &&
                    string.Equals(getId(asset), id, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
