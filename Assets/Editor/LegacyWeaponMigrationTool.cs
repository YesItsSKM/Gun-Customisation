using System;
using System.Collections.Generic;
using System.Text;
using SKM.ModularWeaponCustomisation.Persistence;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SKM.ModularWeaponCustomisation.ProjectMigration
{
    /// <summary>
    /// Project-local, non-destructive bridge from the prototype data to the new
    /// framework. Generated assets remain outside the commercial package root.
    /// </summary>
    public static class LegacyWeaponMigrationTool
    {
        private const string GeneratedParent = "Assets/Generated";

        [MenuItem(
            "Tools/SKM/Modular Weapon Customisation/Project Migration/" +
            "Generate Legacy Migration Assets")]
        public static void Generate()
        {
            bool shouldContinue = EditorUtility.DisplayDialog(
                "Generate migration assets",
                "This creates new definitions and prefab variants under " +
                "'Assets/Generated'. It does not edit source GunData, attachment " +
                "data, or prefabs. Generated variants still reference the legacy " +
                "art and must not be included in a commercial package until its " +
                "redistribution rights are verified.",
                "Generate",
                "Cancel");
            if (!shouldContinue)
            {
                return;
            }

            try
            {
                string outputRoot = CreateOutputFolders(out MigrationFolders folders);
                Dictionary<string, WeaponStatDefinition> stats =
                    CreateStatDefinitions(folders.Stats);
                Dictionary<AttachmentType, AttachmentSlotDefinition> slots =
                    CreateSlotDefinitions(folders.Slots);
                List<WeaponAttachmentDefinition> attachments =
                    ConvertAttachments(folders.Attachments, slots, stats);
                List<WeaponDefinition> weapons =
                    ConvertWeapons(folders, slots, stats);
                WeaponCatalog catalog = CreateCatalog(
                    outputRoot,
                    weapons,
                    attachments);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Selection.activeObject = catalog;
                EditorGUIUtility.PingObject(catalog);
                Debug.Log(
                    $"Legacy migration created {weapons.Count} weapon(s), " +
                    $"{attachments.Count} attachment(s), and a catalog under " +
                    $"'{outputRoot}'. Position every placeholder socket and run " +
                    "the framework validator before using the variants.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    "Migration stopped",
                    "Migration left any assets already created in their new output " +
                    $"folder so the source remains untouched. {exception.Message}",
                    "Close");
            }
        }

        private static string CreateOutputFolders(out MigrationFolders folders)
        {
            EnsureFolder("Assets", "Generated");
            string baseName =
                $"LegacyWeaponMigration_{DateTime.Now:yyyyMMdd_HHmmss}";
            string name = baseName;
            int suffix = 2;
            while (AssetDatabase.IsValidFolder($"{GeneratedParent}/{name}"))
            {
                name = $"{baseName}_{suffix++}";
            }

            string guid = AssetDatabase.CreateFolder(GeneratedParent, name);
            string root = AssetDatabase.GUIDToAssetPath(guid);
            folders = new MigrationFolders(
                root,
                CreateFolder(root, "Stats"),
                CreateFolder(root, "Slots"),
                CreateFolder(root, "Attachments"),
                CreateFolder(root, "Weapons"),
                CreateFolder(root, "Prefabs"));
            return root;
        }

        private static Dictionary<string, WeaponStatDefinition>
            CreateStatDefinitions(string folder)
        {
            var definitions = new Dictionary<string, WeaponStatDefinition>(
                StringComparer.Ordinal)
            {
                ["accuracy"] = CreateStat(
                    folder,
                    "Accuracy",
                    "accuracy",
                    "Accuracy",
                    "%",
                    0f,
                    100f,
                    true),
                ["range"] = CreateStat(
                    folder,
                    "Range",
                    "range",
                    "Range",
                    "m",
                    0f,
                    2000f,
                    true),
                ["fire-rate"] = CreateStat(
                    folder,
                    "FireRate",
                    "fire-rate",
                    "Fire Rate",
                    "RPM",
                    0f,
                    1000f,
                    true),
                ["recoil"] = CreateStat(
                    folder,
                    "Recoil",
                    "recoil",
                    "Recoil",
                    "%",
                    0f,
                    100f,
                    false),
                ["reload-time"] = CreateStat(
                    folder,
                    "ReloadTime",
                    "reload-time",
                    "Reload Time",
                    "s",
                    0f,
                    5f,
                    false),
                ["ammo-capacity"] = CreateStat(
                    folder,
                    "AmmoCapacity",
                    "ammo-capacity",
                    "Ammo Capacity",
                    "rounds",
                    0f,
                    100f,
                    true)
            };

            return definitions;
        }

        private static WeaponStatDefinition CreateStat(
            string folder,
            string fileName,
            string id,
            string displayName,
            string unit,
            float minimum,
            float maximum,
            bool higherIsBetter)
        {
            WeaponStatDefinition definition =
                ScriptableObject.CreateInstance<WeaponStatDefinition>();
            var serialized = new SerializedObject(definition);
            serialized.FindProperty("id").stringValue = id;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("unit").stringValue = unit;
            serialized.FindProperty("minimumValue").floatValue = minimum;
            serialized.FindProperty("maximumValue").floatValue = maximum;
            serialized.FindProperty("higherIsBetter").boolValue = higherIsBetter;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(
                definition,
                $"{folder}/{fileName}.asset");
            return definition;
        }

        private static Dictionary<AttachmentType, AttachmentSlotDefinition>
            CreateSlotDefinitions(string folder)
        {
            var definitions =
                new Dictionary<AttachmentType, AttachmentSlotDefinition>();
            foreach (AttachmentType legacyType in
                     (AttachmentType[])Enum.GetValues(typeof(AttachmentType)))
            {
                if (legacyType == AttachmentType.None)
                {
                    continue;
                }

                AttachmentSlotDefinition definition =
                    ScriptableObject.CreateInstance<AttachmentSlotDefinition>();
                var serialized = new SerializedObject(definition);
                serialized.FindProperty("id").stringValue =
                    ToStableId(legacyType.ToString());
                serialized.FindProperty("displayName").stringValue =
                    SplitPascalCase(legacyType.ToString());
                serialized.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.CreateAsset(
                    definition,
                    $"{folder}/{legacyType}.asset");
                definitions.Add(legacyType, definition);
            }

            return definitions;
        }

        private static List<WeaponAttachmentDefinition> ConvertAttachments(
            string folder,
            IReadOnlyDictionary<AttachmentType, AttachmentSlotDefinition> slots,
            IReadOnlyDictionary<string, WeaponStatDefinition> stats)
        {
            List<GunAttachmentData> legacyAttachments =
                LoadAssets<GunAttachmentData>();
            var converted = new List<WeaponAttachmentDefinition>();
            var usedIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (GunAttachmentData legacy in legacyAttachments)
            {
                if (legacy == null || legacy.attachmentType == AttachmentType.None ||
                    !slots.TryGetValue(
                        legacy.attachmentType,
                        out AttachmentSlotDefinition slot))
                {
                    continue;
                }

                string displayName = string.IsNullOrWhiteSpace(legacy.attachmentName)
                    ? legacy.name
                    : legacy.attachmentName.Trim();
                string id = MakeUniqueId(ToStableId(displayName), usedIds);
                WeaponAttachmentDefinition definition =
                    ScriptableObject.CreateInstance<WeaponAttachmentDefinition>();
                var serialized = new SerializedObject(definition);
                serialized.FindProperty("id").stringValue = id;
                serialized.FindProperty("displayName").stringValue = displayName;
                serialized.FindProperty("prefab").objectReferenceValue =
                    legacy.attachmentPrefab;
                serialized.FindProperty("slot").objectReferenceValue = slot;

                var modifiers = new List<LegacyModifier>
                {
                    new LegacyModifier(stats["accuracy"], legacy.accuracyModifier),
                    new LegacyModifier(stats["range"], legacy.rangeModifier),
                    new LegacyModifier(stats["fire-rate"], legacy.fireRateModifier),
                    new LegacyModifier(stats["recoil"], legacy.recoilModifier),
                    new LegacyModifier(stats["reload-time"], legacy.reloadTimeModifier),
                    new LegacyModifier(stats["ammo-capacity"], legacy.ammoModifier)
                };
                modifiers.RemoveAll(modifier =>
                    Mathf.Approximately(modifier.Value, 0f));
                WriteModifiers(
                    serialized.FindProperty("statModifiers"),
                    modifiers);
                serialized.ApplyModifiedPropertiesWithoutUndo();

                string path = AssetDatabase.GenerateUniqueAssetPath(
                    $"{folder}/{SanitizeFileName(displayName)}.asset");
                AssetDatabase.CreateAsset(definition, path);
                converted.Add(definition);
            }

            return converted;
        }

        private static List<WeaponDefinition> ConvertWeapons(
            MigrationFolders folders,
            IReadOnlyDictionary<AttachmentType, AttachmentSlotDefinition> slots,
            IReadOnlyDictionary<string, WeaponStatDefinition> stats)
        {
            List<GunData> legacyWeapons = LoadAssets<GunData>();
            var converted = new List<WeaponDefinition>();
            var usedIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (GunData legacy in legacyWeapons)
            {
                if (legacy == null || legacy.gunPrefab == null)
                {
                    Debug.LogWarning(
                        $"Skipped legacy gun '{legacy?.name ?? "null"}' because it " +
                        "does not reference a prefab.",
                        legacy);
                    continue;
                }

                string displayName = string.IsNullOrWhiteSpace(legacy.gunName)
                    ? legacy.name
                    : legacy.gunName.Trim().Replace('_', ' ');
                string id = MakeUniqueId(ToStableId(displayName), usedIds);
                WeaponDefinition definition =
                    ScriptableObject.CreateInstance<WeaponDefinition>();
                var serialized = new SerializedObject(definition);
                serialized.FindProperty("id").stringValue = id;
                serialized.FindProperty("displayName").stringValue = displayName;
                WriteBaseStats(serialized.FindProperty("baseStats"), legacy, stats);
                WriteSupportedSlots(
                    serialized.FindProperty("supportedSlots"),
                    slots);
                serialized.ApplyModifiedPropertiesWithoutUndo();

                string definitionPath = AssetDatabase.GenerateUniqueAssetPath(
                    $"{folders.Weapons}/{SanitizeFileName(displayName)}.asset");
                AssetDatabase.CreateAsset(definition, definitionPath);

                GameObject variant = CreateConfiguredVariant(
                    legacy.gunPrefab,
                    definition,
                    slots,
                    folders.Prefabs,
                    displayName);
                var savedDefinition = new SerializedObject(definition);
                savedDefinition.FindProperty("prefab").objectReferenceValue = variant;
                savedDefinition.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(definition);
                converted.Add(definition);
            }

            return converted;
        }

        private static GameObject CreateConfiguredVariant(
            GameObject sourcePrefab,
            WeaponDefinition definition,
            IReadOnlyDictionary<AttachmentType, AttachmentSlotDefinition> slots,
            string folder,
            string displayName)
        {
            string sourcePath = AssetDatabase.GetAssetPath(sourcePrefab);
            if (string.IsNullOrEmpty(sourcePath))
            {
                throw new InvalidOperationException(
                    $"Could not resolve prefab '{sourcePrefab.name}'.");
            }

            GameObject instance = PrefabUtility.LoadPrefabContents(sourcePath);
            try
            {
                instance.name = $"{displayName} - Framework";
                WeaponRuntime runtime = instance.GetComponent<WeaponRuntime>();
                if (runtime == null)
                {
                    runtime = instance.AddComponent<WeaponRuntime>();
                }

                var runtimeSerialized = new SerializedObject(runtime);
                runtimeSerialized.FindProperty("definition").objectReferenceValue =
                    definition;
                runtimeSerialized.ApplyModifiedPropertiesWithoutUndo();

                var configuredSlots =
                    new HashSet<AttachmentSlotDefinition>();
                AttachmentSpawnPoint[] legacyPoints =
                    instance.GetComponentsInChildren<AttachmentSpawnPoint>(true);
                foreach (AttachmentSpawnPoint legacyPoint in legacyPoints)
                {
                    if (legacyPoint.AttachmentType == AttachmentType.None ||
                        !slots.TryGetValue(
                            legacyPoint.AttachmentType,
                            out AttachmentSlotDefinition slot))
                    {
                        continue;
                    }

                    AttachmentSocket socket =
                        legacyPoint.GetComponent<AttachmentSocket>();
                    if (socket == null)
                    {
                        socket = legacyPoint.gameObject.AddComponent<AttachmentSocket>();
                    }

                    AssignSocket(socket, slot);
                    configuredSlots.Add(slot);
                }

                foreach (KeyValuePair<AttachmentType, AttachmentSlotDefinition> pair
                         in slots)
                {
                    if (configuredSlots.Contains(pair.Value))
                    {
                        continue;
                    }

                    var placeholder =
                        new GameObject($"POSITION ME - Socket - {pair.Key}");
                    placeholder.transform.SetParent(instance.transform, false);
                    AttachmentSocket socket =
                        placeholder.AddComponent<AttachmentSocket>();
                    AssignSocket(socket, pair.Value);
                }

                string prefabPath = AssetDatabase.GenerateUniqueAssetPath(
                    $"{folder}/{SanitizeFileName(displayName)}.prefab");
                GameObject saved =
                    PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                if (saved == null)
                {
                    throw new InvalidOperationException(
                        $"Unity did not save a variant for '{displayName}'.");
                }

                return saved;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(instance);
            }
        }

        private static void AssignSocket(
            AttachmentSocket socket,
            AttachmentSlotDefinition slot)
        {
            var serialized = new SerializedObject(socket);
            serialized.FindProperty("slot").objectReferenceValue = slot;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static WeaponCatalog CreateCatalog(
            string outputRoot,
            IReadOnlyList<WeaponDefinition> weapons,
            IReadOnlyList<WeaponAttachmentDefinition> attachments)
        {
            WeaponCatalog catalog =
                ScriptableObject.CreateInstance<WeaponCatalog>();
            var serialized = new SerializedObject(catalog);
            WriteObjectReferences(serialized.FindProperty("weapons"), weapons);
            WriteObjectReferences(
                serialized.FindProperty("attachments"),
                attachments);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(catalog, $"{outputRoot}/WeaponCatalog.asset");
            catalog.RebuildLookups();
            return catalog;
        }

        private static void WriteBaseStats(
            SerializedProperty property,
            GunData legacy,
            IReadOnlyDictionary<string, WeaponStatDefinition> stats)
        {
            var values = new[]
            {
                new LegacyStat(stats["accuracy"], legacy.accuracy),
                new LegacyStat(stats["range"], legacy.range),
                new LegacyStat(stats["fire-rate"], legacy.fireRate),
                new LegacyStat(stats["recoil"], legacy.recoil),
                new LegacyStat(stats["reload-time"], legacy.reloadTime),
                new LegacyStat(stats["ammo-capacity"], legacy.ammo)
            };

            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                SerializedProperty entry = property.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("definition").objectReferenceValue =
                    values[i].Definition;
                entry.FindPropertyRelative("value").floatValue = values[i].Value;
            }
        }

        private static void WriteSupportedSlots(
            SerializedProperty property,
            IReadOnlyDictionary<AttachmentType, AttachmentSlotDefinition> slots)
        {
            property.arraySize = slots.Count;
            int index = 0;
            foreach (AttachmentType legacyType in
                     (AttachmentType[])Enum.GetValues(typeof(AttachmentType)))
            {
                if (slots.TryGetValue(
                        legacyType,
                        out AttachmentSlotDefinition slot))
                {
                    property.GetArrayElementAtIndex(index++).objectReferenceValue =
                        slot;
                }
            }
        }

        private static void WriteModifiers(
            SerializedProperty property,
            IReadOnlyList<LegacyModifier> modifiers)
        {
            property.arraySize = modifiers.Count;
            for (int i = 0; i < modifiers.Count; i++)
            {
                SerializedProperty entry = property.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("definition").objectReferenceValue =
                    modifiers[i].Definition;
                entry.FindPropertyRelative("operation").enumValueIndex =
                    (int)WeaponStatModifierOperation.Add;
                entry.FindPropertyRelative("value").floatValue =
                    modifiers[i].Value;
                entry.FindPropertyRelative("priority").intValue = 0;
            }
        }

        private static void WriteObjectReferences<T>(
            SerializedProperty property,
            IReadOnlyList<T> values)
            where T : Object
        {
            property.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static List<T> LoadAssets<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            var results = new List<T>(guids.Length);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    results.Add(asset);
                }
            }

            results.Sort((left, right) =>
                string.Compare(
                    AssetDatabase.GetAssetPath(left),
                    AssetDatabase.GetAssetPath(right),
                    StringComparison.Ordinal));
            return results;
        }

        private static void EnsureFolder(string parent, string name)
        {
            string path = $"{parent}/{name}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static string CreateFolder(string parent, string name)
        {
            string guid = AssetDatabase.CreateFolder(parent, name);
            return AssetDatabase.GUIDToAssetPath(guid);
        }

        private static string MakeUniqueId(
            string requested,
            ISet<string> usedIds)
        {
            string baseId = string.IsNullOrWhiteSpace(requested)
                ? "unnamed"
                : requested;
            string result = baseId;
            int suffix = 2;
            while (!usedIds.Add(result))
            {
                result = $"{baseId}-{suffix++}";
            }

            return result;
        }

        private static string ToStableId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "unnamed";
            }

            var result = new StringBuilder(value.Length);
            bool previousSeparator = false;
            foreach (char character in value.Trim().ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(character))
                {
                    result.Append(character);
                    previousSeparator = false;
                }
                else if (!previousSeparator && result.Length > 0)
                {
                    result.Append('-');
                    previousSeparator = true;
                }
            }

            return result.ToString().TrimEnd('-');
        }

        private static string SplitPascalCase(string value)
        {
            var result = new StringBuilder(value.Length + 4);
            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];
                if (i > 0 && char.IsUpper(character))
                {
                    result.Append(' ');
                }

                result.Append(character);
            }

            return result.ToString();
        }

        private static string SanitizeFileName(string value)
        {
            char[] invalid = System.IO.Path.GetInvalidFileNameChars();
            var result = new StringBuilder(value.Length);
            foreach (char character in value)
            {
                result.Append(Array.IndexOf(invalid, character) >= 0
                    ? '_'
                    : character);
            }

            string sanitized = result.ToString().Trim();
            return string.IsNullOrEmpty(sanitized) ? "Unnamed" : sanitized;
        }

        private readonly struct MigrationFolders
        {
            public MigrationFolders(
                string root,
                string stats,
                string slots,
                string attachments,
                string weapons,
                string prefabs)
            {
                Root = root;
                Stats = stats;
                Slots = slots;
                Attachments = attachments;
                Weapons = weapons;
                Prefabs = prefabs;
            }

            public string Root { get; }
            public string Stats { get; }
            public string Slots { get; }
            public string Attachments { get; }
            public string Weapons { get; }
            public string Prefabs { get; }
        }

        private readonly struct LegacyStat
        {
            public LegacyStat(WeaponStatDefinition definition, float value)
            {
                Definition = definition;
                Value = value;
            }

            public WeaponStatDefinition Definition { get; }
            public float Value { get; }
        }

        private readonly struct LegacyModifier
        {
            public LegacyModifier(WeaponStatDefinition definition, float value)
            {
                Definition = definition;
                Value = value;
            }

            public WeaponStatDefinition Definition { get; }
            public float Value { get; }
        }
    }
}
