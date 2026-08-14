using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace SKM.ModularWeaponCustomisation.Tests
{
    public sealed class WeaponRuntimeTests
    {
        private readonly List<Object> createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = createdObjects.Count - 1; i >= 0; i--)
            {
                if (createdObjects[i] != null)
                {
                    Object.DestroyImmediate(createdObjects[i]);
                }
            }

            createdObjects.Clear();
        }

        [Test]
        public void InstallAndRemove_UpdatesVisualAndRestoresBaseStats()
        {
            AttachmentSlotDefinition opticSlot = CreateSlot("optic");
            WeaponStatDefinition handling = CreateStat("handling", 0f, 200f);
            GameObject weaponObject = CreateGameObject("Weapon", active: false);
            WeaponDefinition weapon = CreateWeapon(
                "rifle",
                weaponObject,
                opticSlot,
                handling,
                baseValue: 100f);
            AttachmentSocket socket = CreateSocket(weaponObject, opticSlot);
            GameObject visualPrefab = CreateGameObject("Scope Visual", active: false);
            WeaponAttachmentDefinition scope = CreateAttachment(
                "scope",
                opticSlot,
                visualPrefab,
                new WeaponStatModifier(
                    handling,
                    WeaponStatModifierOperation.Add,
                    25f));
            WeaponRuntime runtime = CreateRuntime(weaponObject, weapon);

            Assert.That(runtime.Initialize(), Is.True);
            Assert.That(runtime.EffectiveStats.GetValueOrDefault(handling), Is.EqualTo(100f));

            Assert.That(runtime.TryInstall(scope, out var installFailure), Is.True);
            Assert.That(installFailure, Is.EqualTo(AttachmentInstallFailure.None));
            Assert.That(runtime.EffectiveStats.GetValueOrDefault(handling), Is.EqualTo(125f));
            Assert.That(socket.VisualRoot.childCount, Is.EqualTo(1));

            Assert.That(
                runtime.TryRemove(
                    opticSlot,
                    out WeaponAttachmentDefinition removed,
                    out var removeFailure),
                Is.True);
            Assert.That(removeFailure, Is.EqualTo(AttachmentInstallFailure.None));
            Assert.That(removed, Is.SameAs(scope));
            Assert.That(runtime.EffectiveStats.GetValueOrDefault(handling), Is.EqualTo(100f));
            Assert.That(socket.VisualRoot.childCount, Is.Zero);
            Assert.That(weapon.BaseStats[0].Value, Is.EqualTo(100f));
            Assert.That(scope.StatModifiers[0].Value, Is.EqualTo(25f));
        }

        [Test]
        public void Install_RejectsCompatibleDefinitionWhenSocketIsMissing()
        {
            AttachmentSlotDefinition muzzleSlot = CreateSlot("muzzle");
            GameObject weaponObject = CreateGameObject("Weapon", active: false);
            WeaponDefinition weapon = CreateWeapon(
                "pistol",
                weaponObject,
                muzzleSlot,
                stat: null,
                baseValue: 0f);
            WeaponAttachmentDefinition suppressor = CreateAttachment(
                "suppressor",
                muzzleSlot,
                prefab: null);
            WeaponRuntime runtime = CreateRuntime(weaponObject, weapon);
            Assert.That(runtime.Initialize(), Is.True);

            bool installed = runtime.TryInstall(suppressor, out var failure);

            Assert.That(installed, Is.False);
            Assert.That(failure, Is.EqualTo(AttachmentInstallFailure.MissingSocket));
            Assert.That(runtime.Loadout.InstalledAttachments, Is.Empty);
        }

        private WeaponRuntime CreateRuntime(
            GameObject weaponObject,
            WeaponDefinition definition)
        {
            WeaponRuntime runtime = weaponObject.AddComponent<WeaponRuntime>();
            var serialized = new SerializedObject(runtime);
            serialized.FindProperty("definition").objectReferenceValue = definition;
            serialized.FindProperty("initializeOnAwake").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return runtime;
        }

        private AttachmentSocket CreateSocket(
            GameObject weaponObject,
            AttachmentSlotDefinition slot)
        {
            GameObject socketObject = CreateGameObject("Socket", active: false);
            socketObject.transform.SetParent(weaponObject.transform, false);
            AttachmentSocket socket = socketObject.AddComponent<AttachmentSocket>();
            var serialized = new SerializedObject(socket);
            serialized.FindProperty("slot").objectReferenceValue = slot;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return socket;
        }

        private WeaponDefinition CreateWeapon(
            string id,
            GameObject prefab,
            AttachmentSlotDefinition slot,
            WeaponStatDefinition stat,
            float baseValue)
        {
            WeaponDefinition weapon = Create<WeaponDefinition>();
            var serialized = new SerializedObject(weapon);
            serialized.FindProperty("id").stringValue = id;
            serialized.FindProperty("prefab").objectReferenceValue = prefab;

            SerializedProperty supportedSlots = serialized.FindProperty("supportedSlots");
            supportedSlots.arraySize = 1;
            supportedSlots.GetArrayElementAtIndex(0).objectReferenceValue = slot;

            if (stat != null)
            {
                SerializedProperty baseStats = serialized.FindProperty("baseStats");
                baseStats.arraySize = 1;
                SerializedProperty entry = baseStats.GetArrayElementAtIndex(0);
                entry.FindPropertyRelative("definition").objectReferenceValue = stat;
                entry.FindPropertyRelative("value").floatValue = baseValue;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return weapon;
        }

        private WeaponAttachmentDefinition CreateAttachment(
            string id,
            AttachmentSlotDefinition slot,
            GameObject prefab,
            params WeaponStatModifier[] modifiers)
        {
            WeaponAttachmentDefinition attachment =
                Create<WeaponAttachmentDefinition>();
            var serialized = new SerializedObject(attachment);
            serialized.FindProperty("id").stringValue = id;
            serialized.FindProperty("slot").objectReferenceValue = slot;
            serialized.FindProperty("prefab").objectReferenceValue = prefab;

            SerializedProperty statModifiers = serialized.FindProperty("statModifiers");
            statModifiers.arraySize = modifiers.Length;
            for (int i = 0; i < modifiers.Length; i++)
            {
                SerializedProperty entry = statModifiers.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("definition").objectReferenceValue =
                    modifiers[i].Definition;
                entry.FindPropertyRelative("operation").enumValueIndex =
                    (int)modifiers[i].Operation;
                entry.FindPropertyRelative("value").floatValue = modifiers[i].Value;
                entry.FindPropertyRelative("priority").intValue = modifiers[i].Priority;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return attachment;
        }

        private AttachmentSlotDefinition CreateSlot(string id)
        {
            AttachmentSlotDefinition slot = Create<AttachmentSlotDefinition>();
            SetString(slot, "id", id);
            return slot;
        }

        private WeaponStatDefinition CreateStat(
            string id,
            float minimum,
            float maximum)
        {
            WeaponStatDefinition stat = Create<WeaponStatDefinition>();
            var serialized = new SerializedObject(stat);
            serialized.FindProperty("id").stringValue = id;
            serialized.FindProperty("minimumValue").floatValue = minimum;
            serialized.FindProperty("maximumValue").floatValue = maximum;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return stat;
        }

        private static void SetString(Object target, string propertyName, string value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private GameObject CreateGameObject(string name, bool active)
        {
            var instance = new GameObject(name);
            instance.SetActive(active);
            createdObjects.Add(instance);
            return instance;
        }

        private T Create<T>() where T : ScriptableObject
        {
            T instance = ScriptableObject.CreateInstance<T>();
            createdObjects.Add(instance);
            return instance;
        }
    }
}
