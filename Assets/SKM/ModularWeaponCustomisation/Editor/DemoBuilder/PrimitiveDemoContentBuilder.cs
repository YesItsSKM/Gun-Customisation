using System;
using System.Collections.Generic;
using SKM.ModularWeaponCustomisation.Persistence;
using SKM.ModularWeaponCustomisation.Preview;
using SKM.ModularWeaponCustomisation.Samples.DemoUI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SKM.ModularWeaponCustomisation.Editor.DemoBuilder
{
    /// <summary>
    /// Creates a redistributable demo using Unity primitives and framework code
    /// only. It intentionally has no dependency on the legacy workshop content.
    /// </summary>
    public static class PrimitiveDemoContentBuilder
    {
        public const string OutputRoot =
            "Assets/SKM/ModularWeaponCustomisation/Samples/GeneratedDemo";

        [MenuItem(
            "Tools/SKM/Modular Weapon Customisation/" +
            "Create Redistributable Primitive Demo")]
        public static void BuildFromMenu()
        {
            if (AssetDatabase.IsValidFolder(OutputRoot))
            {
                Object existing = AssetDatabase.LoadAssetAtPath<Object>(OutputRoot);
                Selection.activeObject = existing;
                EditorGUIUtility.PingObject(existing);
                EditorUtility.DisplayDialog(
                    "Demo already exists",
                    $"'{OutputRoot}' already exists. The builder never overwrites " +
                    "sample work. Delete that generated folder manually only if you " +
                    "intend to rebuild it from scratch.",
                    "Close");
                return;
            }

            bool shouldBuild = EditorUtility.DisplayDialog(
                "Create primitive demo",
                "This creates two configurable weapons, four attachments, data " +
                "assets, materials, UI, and a sample scene using only Unity " +
                "primitives and framework code.",
                "Create Demo",
                "Cancel");
            if (!shouldBuild ||
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            try
            {
                Build();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    "Demo generation stopped",
                    "Any assets already created were preserved for inspection. " +
                    exception.Message,
                    "Close");
            }
        }

        /// <summary>Command-line entry point used by release validation.</summary>
        public static void Build()
        {
            if (AssetDatabase.IsValidFolder(OutputRoot))
            {
                throw new InvalidOperationException(
                    $"Demo output already exists at '{OutputRoot}'.");
            }

            DemoFolders folders = CreateFolders();
            DemoMaterials materials = CreateMaterials(folders.Materials);
            DemoDefinitions definitions =
                CreateDefinitionsAndPrefabs(folders, materials);
            string scenePath = CreateScene(folders.Scenes, materials, definitions);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            SceneAsset sceneAsset =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            Selection.activeObject = sceneAsset;
            EditorGUIUtility.PingObject(sceneAsset);
            Debug.Log(
                $"Created redistributable primitive demo at '{OutputRoot}'. " +
                "Open the generated scene, enter Play Mode, and run the validator.");
        }

        private static DemoFolders CreateFolders()
        {
            string root = CreateFolder(
                "Assets/SKM/ModularWeaponCustomisation/Samples",
                "GeneratedDemo");
            return new DemoFolders(
                root,
                CreateFolder(root, "Definitions"),
                CreateFolder(root, "Materials"),
                CreateFolder(root, "Prefabs"),
                CreateFolder(root, "Scenes"));
        }

        private static DemoMaterials CreateMaterials(string folder)
        {
            Shader shader =
                Shader.Find("Universal Render Pipeline/Lit") ??
                Shader.Find("HDRP/Lit") ??
                Shader.Find("Standard");
            if (shader == null)
            {
                throw new InvalidOperationException(
                    "No supported lit shader was found for demo materials.");
            }

            return new DemoMaterials(
                CreateMaterial(
                    folder,
                    "Weapon Dark",
                    shader,
                    new Color(0.055f, 0.065f, 0.075f),
                    0.75f,
                    0.72f),
                CreateMaterial(
                    folder,
                    "Weapon Mid",
                    shader,
                    new Color(0.18f, 0.21f, 0.23f),
                    0.65f,
                    0.58f),
                CreateMaterial(
                    folder,
                    "Accent Cyan",
                    shader,
                    new Color(0.02f, 0.65f, 0.78f),
                    0.25f,
                    0.7f),
                CreateMaterial(
                    folder,
                    "Accent Amber",
                    shader,
                    new Color(0.95f, 0.39f, 0.07f),
                    0.2f,
                    0.65f),
                CreateMaterial(
                    folder,
                    "Room",
                    shader,
                    new Color(0.018f, 0.022f, 0.027f),
                    0.05f,
                    0.25f));
        }

        private static Material CreateMaterial(
            string folder,
            string name,
            Shader shader,
            Color color,
            float metallic,
            float smoothness)
        {
            var material = new Material(shader)
            {
                name = name,
                color = color
            };
            SetFloatIfPresent(material, "_Metallic", metallic);
            SetFloatIfPresent(material, "_Smoothness", smoothness);
            SetFloatIfPresent(material, "_Glossiness", smoothness);
            AssetDatabase.CreateAsset(
                material,
                $"{folder}/{name}.mat");
            return material;
        }

        private static DemoDefinitions CreateDefinitionsAndPrefabs(
            DemoFolders folders,
            DemoMaterials materials)
        {
            AttachmentSlotDefinition opticSlot = CreateSlot(
                folders.Definitions,
                "Optic",
                "optic",
                "Optic");
            AttachmentSlotDefinition muzzleSlot = CreateSlot(
                folders.Definitions,
                "Muzzle",
                "muzzle",
                "Muzzle");

            WeaponStatDefinition accuracy = CreateStat(
                folders.Definitions,
                "Accuracy",
                "accuracy",
                "Accuracy",
                "%",
                0f,
                100f,
                true);
            WeaponStatDefinition range = CreateStat(
                folders.Definitions,
                "Range",
                "range",
                "Range",
                "m",
                0f,
                1000f,
                true);
            WeaponStatDefinition recoil = CreateStat(
                folders.Definitions,
                "Recoil",
                "recoil",
                "Recoil",
                "%",
                0f,
                100f,
                false);
            WeaponStatDefinition handling = CreateStat(
                folders.Definitions,
                "Handling",
                "handling",
                "Handling",
                "%",
                0f,
                100f,
                true);

            GameObject redDotPrefab = CreateRedDotPrefab(
                folders.Prefabs,
                materials);
            GameObject scopePrefab = CreateScopePrefab(
                folders.Prefabs,
                materials);
            GameObject suppressorPrefab = CreateSuppressorPrefab(
                folders.Prefabs,
                materials);
            GameObject brakePrefab = CreateBrakePrefab(
                folders.Prefabs,
                materials);

            WeaponAttachmentDefinition redDot = CreateAttachment(
                folders.Definitions,
                "Red Dot",
                "red-dot",
                opticSlot,
                redDotPrefab,
                new WeaponStatModifier(
                    accuracy,
                    WeaponStatModifierOperation.Add,
                    7f),
                new WeaponStatModifier(
                    handling,
                    WeaponStatModifierOperation.Add,
                    3f));
            WeaponAttachmentDefinition scope = CreateAttachment(
                folders.Definitions,
                "Scout Scope",
                "scout-scope",
                opticSlot,
                scopePrefab,
                new WeaponStatModifier(
                    accuracy,
                    WeaponStatModifierOperation.Add,
                    14f),
                new WeaponStatModifier(
                    range,
                    WeaponStatModifierOperation.Multiply,
                    1.2f),
                new WeaponStatModifier(
                    handling,
                    WeaponStatModifierOperation.Add,
                    -7f));
            WeaponAttachmentDefinition suppressor = CreateAttachment(
                folders.Definitions,
                "Suppressor",
                "suppressor",
                muzzleSlot,
                suppressorPrefab,
                new WeaponStatModifier(
                    recoil,
                    WeaponStatModifierOperation.Add,
                    -4f),
                new WeaponStatModifier(
                    range,
                    WeaponStatModifierOperation.Multiply,
                    0.94f));
            WeaponAttachmentDefinition brake = CreateAttachment(
                folders.Definitions,
                "Muzzle Brake",
                "muzzle-brake",
                muzzleSlot,
                brakePrefab,
                new WeaponStatModifier(
                    recoil,
                    WeaponStatModifierOperation.Add,
                    -12f),
                new WeaponStatModifier(
                    handling,
                    WeaponStatModifierOperation.Add,
                    -2f));

            WeaponDefinition carbine = CreateWeapon(
                folders.Definitions,
                "Demo Carbine",
                "demo-carbine",
                new[]
                {
                    new WeaponStatValue(accuracy, 70f),
                    new WeaponStatValue(range, 420f),
                    new WeaponStatValue(recoil, 38f),
                    new WeaponStatValue(handling, 72f)
                },
                new[] { opticSlot, muzzleSlot });
            WeaponDefinition compact = CreateWeapon(
                folders.Definitions,
                "Demo Compact",
                "demo-compact",
                new[]
                {
                    new WeaponStatValue(accuracy, 58f),
                    new WeaponStatValue(range, 260f),
                    new WeaponStatValue(recoil, 29f),
                    new WeaponStatValue(handling, 88f)
                },
                new[] { opticSlot, muzzleSlot });

            GameObject carbinePrefab = CreateWeaponPrefab(
                folders.Prefabs,
                "Demo Carbine",
                carbine,
                opticSlot,
                muzzleSlot,
                materials,
                compact: false);
            GameObject compactPrefab = CreateWeaponPrefab(
                folders.Prefabs,
                "Demo Compact",
                compact,
                opticSlot,
                muzzleSlot,
                materials,
                compact: true);
            AssignWeaponPrefab(carbine, carbinePrefab);
            AssignWeaponPrefab(compact, compactPrefab);

            WeaponCatalog catalog =
                ScriptableObject.CreateInstance<WeaponCatalog>();
            var catalogSerialized = new SerializedObject(catalog);
            WriteReferences(
                catalogSerialized.FindProperty("weapons"),
                new[] { carbine, compact });
            WriteReferences(
                catalogSerialized.FindProperty("attachments"),
                new[] { redDot, scope, suppressor, brake });
            catalogSerialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(
                catalog,
                $"{folders.Definitions}/Demo Weapon Catalog.asset");
            catalog.RebuildLookups();

            return new DemoDefinitions(carbine, compact, catalog);
        }

        private static AttachmentSlotDefinition CreateSlot(
            string folder,
            string fileName,
            string id,
            string displayName)
        {
            AttachmentSlotDefinition definition =
                ScriptableObject.CreateInstance<AttachmentSlotDefinition>();
            var serialized = new SerializedObject(definition);
            serialized.FindProperty("id").stringValue = id;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(
                definition,
                $"{folder}/{fileName} Slot.asset");
            return definition;
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
                $"{folder}/{fileName} Stat.asset");
            return definition;
        }

        private static WeaponAttachmentDefinition CreateAttachment(
            string folder,
            string displayName,
            string id,
            AttachmentSlotDefinition slot,
            GameObject prefab,
            params WeaponStatModifier[] modifiers)
        {
            WeaponAttachmentDefinition definition =
                ScriptableObject.CreateInstance<WeaponAttachmentDefinition>();
            var serialized = new SerializedObject(definition);
            serialized.FindProperty("id").stringValue = id;
            serialized.FindProperty("displayName").stringValue = displayName;
            serialized.FindProperty("slot").objectReferenceValue = slot;
            serialized.FindProperty("prefab").objectReferenceValue = prefab;
            SerializedProperty modifierList =
                serialized.FindProperty("statModifiers");
            modifierList.arraySize = modifiers.Length;
            for (int i = 0; i < modifiers.Length; i++)
            {
                SerializedProperty entry =
                    modifierList.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("definition").objectReferenceValue =
                    modifiers[i].Definition;
                entry.FindPropertyRelative("operation").enumValueIndex =
                    (int)modifiers[i].Operation;
                entry.FindPropertyRelative("value").floatValue =
                    modifiers[i].Value;
                entry.FindPropertyRelative("priority").intValue =
                    modifiers[i].Priority;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(
                definition,
                $"{folder}/{displayName} Attachment.asset");
            return definition;
        }

        private static WeaponDefinition CreateWeapon(
            string folder,
            string displayName,
            string id,
            IReadOnlyList<WeaponStatValue> baseStats,
            IReadOnlyList<AttachmentSlotDefinition> slots)
        {
            WeaponDefinition definition =
                ScriptableObject.CreateInstance<WeaponDefinition>();
            var serialized = new SerializedObject(definition);
            serialized.FindProperty("id").stringValue = id;
            serialized.FindProperty("displayName").stringValue = displayName;

            SerializedProperty stats = serialized.FindProperty("baseStats");
            stats.arraySize = baseStats.Count;
            for (int i = 0; i < baseStats.Count; i++)
            {
                SerializedProperty entry = stats.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("definition").objectReferenceValue =
                    baseStats[i].Definition;
                entry.FindPropertyRelative("value").floatValue =
                    baseStats[i].Value;
            }

            WriteReferences(serialized.FindProperty("supportedSlots"), slots);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(
                definition,
                $"{folder}/{displayName} Weapon.asset");
            return definition;
        }

        private static GameObject CreateWeaponPrefab(
            string folder,
            string displayName,
            WeaponDefinition definition,
            AttachmentSlotDefinition opticSlot,
            AttachmentSlotDefinition muzzleSlot,
            DemoMaterials materials,
            bool compact)
        {
            var root = new GameObject(displayName);
            try
            {
                float receiverLength = compact ? 0.95f : 1.3f;
                float barrelLength = compact ? 0.3f : 0.5f;
                CreatePart(
                    PrimitiveType.Cube,
                    "Receiver",
                    root.transform,
                    new Vector3(0f, 0f, 0f),
                    new Vector3(0.32f, 0.26f, receiverLength),
                    Vector3.zero,
                    materials.WeaponDark);
                CreatePart(
                    PrimitiveType.Cube,
                    "Upper",
                    root.transform,
                    new Vector3(0f, 0.18f, 0.05f),
                    new Vector3(0.26f, 0.12f, receiverLength * 0.75f),
                    Vector3.zero,
                    materials.WeaponMid);
                CreatePart(
                    PrimitiveType.Cylinder,
                    "Barrel",
                    root.transform,
                    new Vector3(0f, 0.02f, receiverLength * 0.5f + barrelLength),
                    new Vector3(0.075f, barrelLength, 0.075f),
                    new Vector3(90f, 0f, 0f),
                    materials.WeaponMid);
                CreatePart(
                    PrimitiveType.Cube,
                    "Grip",
                    root.transform,
                    new Vector3(0f, -0.3f, -0.18f),
                    new Vector3(0.2f, 0.48f, 0.22f),
                    new Vector3(12f, 0f, 0f),
                    materials.WeaponDark);

                if (!compact)
                {
                    CreatePart(
                        PrimitiveType.Cube,
                        "Stock",
                        root.transform,
                        new Vector3(0f, -0.02f, -0.9f),
                        new Vector3(0.28f, 0.34f, 0.55f),
                        new Vector3(-8f, 0f, 0f),
                        materials.WeaponDark);
                }
                else
                {
                    CreatePart(
                        PrimitiveType.Cube,
                        "Rear Brace",
                        root.transform,
                        new Vector3(0f, 0.02f, -0.58f),
                        new Vector3(0.16f, 0.16f, 0.28f),
                        Vector3.zero,
                        materials.AccentAmber);
                }

                CreateSocket(
                    root.transform,
                    "Optic Socket",
                    opticSlot,
                    new Vector3(0f, 0.31f, 0.05f));
                CreateSocket(
                    root.transform,
                    "Muzzle Socket",
                    muzzleSlot,
                    new Vector3(
                        0f,
                        0.02f,
                        receiverLength * 0.5f + barrelLength * 2f));

                var collider = root.AddComponent<BoxCollider>();
                collider.center = new Vector3(
                    0f,
                    -0.02f,
                    compact ? 0f : -0.12f);
                collider.size = new Vector3(
                    0.42f,
                    0.72f,
                    compact ? 1.75f : 2.6f);

                WeaponRuntime runtime = root.AddComponent<WeaponRuntime>();
                var serialized = new SerializedObject(runtime);
                serialized.FindProperty("definition").objectReferenceValue =
                    definition;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                string path = $"{folder}/{displayName}.prefab";
                GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, path);
                return saved != null
                    ? saved
                    : throw new InvalidOperationException(
                        $"Could not save '{displayName}' prefab.");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateRedDotPrefab(
            string folder,
            DemoMaterials materials)
        {
            var root = new GameObject("Red Dot");
            try
            {
                CreatePart(
                    PrimitiveType.Cube,
                    "Mount",
                    root.transform,
                    new Vector3(0f, 0.035f, 0f),
                    new Vector3(0.18f, 0.07f, 0.22f),
                    Vector3.zero,
                    materials.WeaponDark);
                CreatePart(
                    PrimitiveType.Cube,
                    "Lens Housing",
                    root.transform,
                    new Vector3(0f, 0.14f, 0f),
                    new Vector3(0.16f, 0.16f, 0.055f),
                    Vector3.zero,
                    materials.AccentCyan);
                return SavePrefab(root, $"{folder}/Red Dot.prefab");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateScopePrefab(
            string folder,
            DemoMaterials materials)
        {
            var root = new GameObject("Scout Scope");
            try
            {
                CreatePart(
                    PrimitiveType.Cube,
                    "Mount",
                    root.transform,
                    new Vector3(0f, 0.045f, 0f),
                    new Vector3(0.2f, 0.09f, 0.3f),
                    Vector3.zero,
                    materials.WeaponDark);
                CreatePart(
                    PrimitiveType.Cylinder,
                    "Tube",
                    root.transform,
                    new Vector3(0f, 0.15f, 0f),
                    new Vector3(0.105f, 0.32f, 0.105f),
                    new Vector3(90f, 0f, 0f),
                    materials.WeaponMid);
                CreatePart(
                    PrimitiveType.Cylinder,
                    "Front Ring",
                    root.transform,
                    new Vector3(0f, 0.15f, 0.31f),
                    new Vector3(0.14f, 0.045f, 0.14f),
                    new Vector3(90f, 0f, 0f),
                    materials.AccentCyan);
                return SavePrefab(root, $"{folder}/Scout Scope.prefab");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateSuppressorPrefab(
            string folder,
            DemoMaterials materials)
        {
            var root = new GameObject("Suppressor");
            try
            {
                CreatePart(
                    PrimitiveType.Cylinder,
                    "Body",
                    root.transform,
                    new Vector3(0f, 0f, 0.34f),
                    new Vector3(0.11f, 0.34f, 0.11f),
                    new Vector3(90f, 0f, 0f),
                    materials.WeaponDark);
                CreatePart(
                    PrimitiveType.Cylinder,
                    "End Ring",
                    root.transform,
                    new Vector3(0f, 0f, 0.67f),
                    new Vector3(0.12f, 0.035f, 0.12f),
                    new Vector3(90f, 0f, 0f),
                    materials.AccentCyan);
                return SavePrefab(root, $"{folder}/Suppressor.prefab");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateBrakePrefab(
            string folder,
            DemoMaterials materials)
        {
            var root = new GameObject("Muzzle Brake");
            try
            {
                CreatePart(
                    PrimitiveType.Cylinder,
                    "Body",
                    root.transform,
                    new Vector3(0f, 0f, 0.15f),
                    new Vector3(0.105f, 0.15f, 0.105f),
                    new Vector3(90f, 0f, 0f),
                    materials.WeaponMid);
                CreatePart(
                    PrimitiveType.Cube,
                    "Port",
                    root.transform,
                    new Vector3(0f, 0f, 0.29f),
                    new Vector3(0.24f, 0.08f, 0.08f),
                    Vector3.zero,
                    materials.AccentAmber);
                return SavePrefab(root, $"{folder}/Muzzle Brake.prefab");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static GameObject SavePrefab(GameObject root, string path)
        {
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, path);
            return saved != null
                ? saved
                : throw new InvalidOperationException(
                    $"Could not save demo prefab '{path}'.");
        }

        private static void CreateSocket(
            Transform parent,
            string name,
            AttachmentSlotDefinition slot,
            Vector3 localPosition)
        {
            var socketObject = new GameObject(name);
            socketObject.transform.SetParent(parent, false);
            socketObject.transform.localPosition = localPosition;
            AttachmentSocket socket =
                socketObject.AddComponent<AttachmentSocket>();
            var serialized = new SerializedObject(socket);
            serialized.FindProperty("slot").objectReferenceValue = slot;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreatePart(
            PrimitiveType type,
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Vector3 localEuler,
            Material material)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localEulerAngles = localEuler;
            part.transform.localScale = localScale;
            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }

            return part;
        }

        private static void AssignWeaponPrefab(
            WeaponDefinition definition,
            GameObject prefab)
        {
            var serialized = new SerializedObject(definition);
            serialized.FindProperty("prefab").objectReferenceValue = prefab;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
        }

        private static string CreateScene(
            string folder,
            DemoMaterials materials,
            DemoDefinitions definitions)
        {
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            scene.name = "Modular Weapon Customisation Demo";
            RenderSettings.ambientLight = new Color(0.08f, 0.1f, 0.13f);

            Camera camera = CreateCamera();
            CreateLighting();
            CreateRoom(materials.Room);

            var systemsObject = new GameObject("Demo Systems");
            var displayPoint = new GameObject("Weapon Display Point");
            displayPoint.transform.SetParent(systemsObject.transform, false);
            displayPoint.transform.SetPositionAndRotation(
                new Vector3(-0.55f, 1.2f, 0f),
                Quaternion.Euler(0f, 90f, 0f));

            WeaponSpawner spawner =
                systemsObject.AddComponent<WeaponSpawner>();
            var spawnerSerialized = new SerializedObject(spawner);
            spawnerSerialized.FindProperty("spawnParent").objectReferenceValue =
                displayPoint.transform;
            spawnerSerialized.FindProperty("startingWeapon").objectReferenceValue =
                definitions.Carbine;
            spawnerSerialized.FindProperty("spawnOnStart").boolValue = true;
            spawnerSerialized.ApplyModifiedPropertiesWithoutUndo();

            var previewAnchorObject = new GameObject("Preview Anchor");
            previewAnchorObject.transform.SetParent(systemsObject.transform, false);
            previewAnchorObject.transform.SetPositionAndRotation(
                new Vector3(-0.55f, 1.2f, -0.2f),
                Quaternion.Euler(0f, 90f, 0f));
            WeaponPreviewController preview =
                systemsObject.AddComponent<WeaponPreviewController>();
            var previewSerialized = new SerializedObject(preview);
            previewSerialized.FindProperty("previewAnchor").objectReferenceValue =
                previewAnchorObject.transform;
            previewSerialized.FindProperty("zoomReference").objectReferenceValue =
                camera.transform;
            previewSerialized.FindProperty("transitionDuration").floatValue = 0.3f;
            previewSerialized.ApplyModifiedPropertiesWithoutUndo();

            LegacyMousePreviewInput input =
                systemsObject.AddComponent<LegacyMousePreviewInput>();
            var inputSerialized = new SerializedObject(input);
            inputSerialized.FindProperty("interactionCamera").objectReferenceValue =
                camera;
            inputSerialized.FindProperty("previewController").objectReferenceValue =
                preview;
            inputSerialized.ApplyModifiedPropertiesWithoutUndo();

            CreateDemoUi(spawner, definitions.Catalog);

            string scenePath =
                $"{folder}/Modular Weapon Customisation Demo.unity";
            if (!EditorSceneManager.SaveScene(scene, scenePath))
            {
                throw new InvalidOperationException(
                    $"Unity did not save the demo scene at '{scenePath}'.");
            }

            return scenePath;
        }

        private static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(-0.55f, 1.4f, -4.2f);
            cameraObject.transform.LookAt(new Vector3(-0.55f, 1.15f, 0f));
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.008f, 0.012f, 0.018f);
            camera.fieldOfView = 45f;
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        private static void CreateLighting()
        {
            var keyObject = new GameObject("Key Light");
            keyObject.transform.SetPositionAndRotation(
                new Vector3(-2.2f, 3.2f, -2f),
                Quaternion.Euler(42f, -24f, 0f));
            Light key = keyObject.AddComponent<Light>();
            key.type = LightType.Spot;
            key.color = new Color(0.55f, 0.82f, 1f);
            key.intensity = 9f;
            key.range = 12f;
            key.spotAngle = 62f;

            var rimObject = new GameObject("Rim Light");
            rimObject.transform.SetPositionAndRotation(
                new Vector3(2f, 2.2f, 1.8f),
                Quaternion.Euler(35f, 205f, 0f));
            Light rim = rimObject.AddComponent<Light>();
            rim.type = LightType.Spot;
            rim.color = new Color(1f, 0.34f, 0.08f);
            rim.intensity = 7f;
            rim.range = 10f;
            rim.spotAngle = 55f;
        }

        private static void CreateRoom(Material roomMaterial)
        {
            CreatePart(
                PrimitiveType.Cube,
                "Floor",
                null,
                new Vector3(0f, -0.05f, 0f),
                new Vector3(8f, 0.1f, 8f),
                Vector3.zero,
                roomMaterial);
            CreatePart(
                PrimitiveType.Cube,
                "Back Wall",
                null,
                new Vector3(0f, 2.5f, 2f),
                new Vector3(8f, 5f, 0.1f),
                Vector3.zero,
                roomMaterial);
            CreatePart(
                PrimitiveType.Cube,
                "Display Plinth",
                null,
                new Vector3(-0.55f, 0.35f, 0.3f),
                new Vector3(2.8f, 0.7f, 1.5f),
                Vector3.zero,
                roomMaterial);
        }

        private static void CreateDemoUi(
            WeaponSpawner spawner,
            WeaponCatalog catalog)
        {
            var canvasObject = new GameObject("Demo UI");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var resources = new DefaultControls.Resources();
            GameObject panelObject = DefaultControls.CreatePanel(resources);
            panelObject.name = "Customisation Panel";
            panelObject.transform.SetParent(canvasObject.transform, false);
            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.69f, 0f);
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panelObject.GetComponent<Image>().color =
                new Color(0.015f, 0.022f, 0.03f, 0.94f);

            Text title = CreateText(
                resources,
                panelRect,
                "MODULAR WEAPON LAB",
                30,
                24f,
                46f);
            title.fontStyle = FontStyle.Bold;
            title.color = new Color(0.25f, 0.9f, 1f);

            Text weaponName = CreateText(
                resources,
                panelRect,
                "No weapon",
                25,
                84f,
                38f);

            Dropdown weaponDropdown = CreateDropdown(
                resources,
                panelRect,
                "Weapon",
                132f);
            Dropdown slotDropdown = CreateDropdown(
                resources,
                panelRect,
                "Slot",
                190f);

            Text installedLabel = CreateText(
                resources,
                panelRect,
                "Installed: None",
                20,
                248f,
                32f);

            GameObject contentObject = new GameObject(
                "Attachment Buttons",
                typeof(RectTransform),
                typeof(VerticalLayoutGroup));
            contentObject.transform.SetParent(panelRect, false);
            RectTransform content = contentObject.GetComponent<RectTransform>();
            PlaceTop(content, 292f, 190f, 28f);
            VerticalLayoutGroup layout =
                contentObject.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            Button template = CreateButton(
                resources,
                content,
                "Attachment Template",
                "Attachment");
            LayoutElement templateLayout =
                template.gameObject.AddComponent<LayoutElement>();
            templateLayout.preferredHeight = 38f;
            template.gameObject.SetActive(false);

            Button removeButton = CreateButton(
                resources,
                panelRect,
                "Remove Attachment",
                "REMOVE ATTACHMENT");
            PlaceTop(
                removeButton.GetComponent<RectTransform>(),
                498f,
                42f,
                28f);

            Text stats = CreateText(
                resources,
                panelRect,
                "Stats",
                20,
                562f,
                180f);
            stats.alignment = TextAnchor.UpperLeft;

            Text status = CreateText(
                resources,
                panelRect,
                string.Empty,
                18,
                760f,
                70f);
            status.color = new Color(0.95f, 0.7f, 0.32f);

            WeaponCustomisationPanel panel =
                panelObject.AddComponent<WeaponCustomisationPanel>();
            var serialized = new SerializedObject(panel);
            serialized.FindProperty("spawner").objectReferenceValue = spawner;
            serialized.FindProperty("catalog").objectReferenceValue = catalog;
            serialized.FindProperty("weaponDropdown").objectReferenceValue =
                weaponDropdown;
            serialized.FindProperty("slotDropdown").objectReferenceValue =
                slotDropdown;
            serialized.FindProperty("attachmentListContent").objectReferenceValue =
                content;
            serialized.FindProperty("attachmentButtonTemplate").objectReferenceValue =
                template;
            serialized.FindProperty("removeAttachmentButton").objectReferenceValue =
                removeButton;
            serialized.FindProperty("weaponNameLabel").objectReferenceValue =
                weaponName;
            serialized.FindProperty("installedAttachmentLabel").objectReferenceValue =
                installedLabel;
            serialized.FindProperty("statsLabel").objectReferenceValue = stats;
            serialized.FindProperty("statusLabel").objectReferenceValue = status;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Text help = CreateText(
                resources,
                canvasObject.GetComponent<RectTransform>(),
                "CLICK WEAPON TO INSPECT  •  DRAG TO ORBIT  •  SCROLL TO ZOOM  •  ESC TO RETURN",
                20,
                995f,
                42f);
            RectTransform helpRect = help.rectTransform;
            helpRect.anchorMin = new Vector2(0.02f, 0f);
            helpRect.anchorMax = new Vector2(0.67f, 0f);
            helpRect.pivot = new Vector2(0f, 0f);
            helpRect.offsetMin = new Vector2(0f, 20f);
            helpRect.offsetMax = new Vector2(0f, 62f);
            help.alignment = TextAnchor.MiddleLeft;
            help.color = new Color(0.55f, 0.75f, 0.82f);

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private static Text CreateText(
            DefaultControls.Resources resources,
            Transform parent,
            string text,
            int fontSize,
            float top,
            float height)
        {
            GameObject textObject = DefaultControls.CreateText(resources);
            textObject.name = text;
            textObject.transform.SetParent(parent, false);
            Text label = textObject.GetComponent<Text>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = Color.white;
            label.alignment = TextAnchor.MiddleLeft;
            PlaceTop(label.rectTransform, top, height, 28f);
            return label;
        }

        private static Dropdown CreateDropdown(
            DefaultControls.Resources resources,
            Transform parent,
            string name,
            float top)
        {
            GameObject dropdownObject =
                DefaultControls.CreateDropdown(resources);
            dropdownObject.name = $"{name} Dropdown";
            dropdownObject.transform.SetParent(parent, false);
            Dropdown dropdown = dropdownObject.GetComponent<Dropdown>();
            PlaceTop(dropdown.GetComponent<RectTransform>(), top, 44f, 28f);
            return dropdown;
        }

        private static Button CreateButton(
            DefaultControls.Resources resources,
            Transform parent,
            string name,
            string label)
        {
            GameObject buttonObject = DefaultControls.CreateButton(resources);
            buttonObject.name = name;
            buttonObject.transform.SetParent(parent, false);
            Text text = buttonObject.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.text = label;
                text.fontStyle = FontStyle.Bold;
            }

            return buttonObject.GetComponent<Button>();
        }

        private static void PlaceTop(
            RectTransform rect,
            float top,
            float height,
            float horizontalMargin)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -top);
            rect.sizeDelta = new Vector2(-horizontalMargin * 2f, height);
        }

        private static void WriteReferences<T>(
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

        private static void SetFloatIfPresent(
            Material material,
            string property,
            float value)
        {
            if (material.HasProperty(property))
            {
                material.SetFloat(property, value);
            }
        }

        private static string CreateFolder(string parent, string name)
        {
            string guid = AssetDatabase.CreateFolder(parent, name);
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException(
                    $"Could not create folder '{parent}/{name}'.");
            }

            return path;
        }

        private readonly struct DemoFolders
        {
            public DemoFolders(
                string root,
                string definitions,
                string materials,
                string prefabs,
                string scenes)
            {
                Root = root;
                Definitions = definitions;
                Materials = materials;
                Prefabs = prefabs;
                Scenes = scenes;
            }

            public string Root { get; }
            public string Definitions { get; }
            public string Materials { get; }
            public string Prefabs { get; }
            public string Scenes { get; }
        }

        private readonly struct DemoMaterials
        {
            public DemoMaterials(
                Material weaponDark,
                Material weaponMid,
                Material accentCyan,
                Material accentAmber,
                Material room)
            {
                WeaponDark = weaponDark;
                WeaponMid = weaponMid;
                AccentCyan = accentCyan;
                AccentAmber = accentAmber;
                Room = room;
            }

            public Material WeaponDark { get; }
            public Material WeaponMid { get; }
            public Material AccentCyan { get; }
            public Material AccentAmber { get; }
            public Material Room { get; }
        }

        private readonly struct DemoDefinitions
        {
            public DemoDefinitions(
                WeaponDefinition carbine,
                WeaponDefinition compact,
                WeaponCatalog catalog)
            {
                Carbine = carbine;
                Compact = compact;
                Catalog = catalog;
            }

            public WeaponDefinition Carbine { get; }
            public WeaponDefinition Compact { get; }
            public WeaponCatalog Catalog { get; }
        }
    }
}
