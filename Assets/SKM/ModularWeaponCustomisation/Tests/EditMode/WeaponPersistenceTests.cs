using System.Collections.Generic;
using NUnit.Framework;
using SKM.ModularWeaponCustomisation.Persistence;
using UnityEditor;
using UnityEngine;

namespace SKM.ModularWeaponCustomisation.Tests
{
    public sealed class WeaponPersistenceTests
    {
        private readonly List<Object> createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object createdObject in createdObjects)
            {
                if (createdObject != null)
                {
                    Object.DestroyImmediate(createdObject);
                }
            }

            createdObjects.Clear();
        }

        [Test]
        public void Json_RoundTripResolvesStableIdsToCatalogAssets()
        {
            AttachmentSlotDefinition opticSlot = CreateSlot("optic");
            WeaponDefinition rifle = CreateWeapon("rifle", opticSlot);
            WeaponAttachmentDefinition scope =
                CreateAttachment("scope", opticSlot);
            WeaponCatalog catalog = CreateCatalog(rifle, scope);
            var loadout = new WeaponLoadout(rifle);
            Assert.That(loadout.TryInstall(scope, out _), Is.True);

            string json = WeaponLoadoutJson.Serialize(loadout);
            bool resolved = WeaponLoadoutJson.TryResolve(
                json,
                catalog,
                out WeaponDefinition resolvedWeapon,
                out var resolvedAttachments,
                out string error);

            Assert.That(resolved, Is.True, error);
            Assert.That(resolvedWeapon, Is.SameAs(rifle));
            Assert.That(resolvedAttachments, Has.Count.EqualTo(1));
            Assert.That(resolvedAttachments[0], Is.SameAs(scope));
        }

        [Test]
        public void Json_RejectsAttachmentSavedAgainstDifferentSlot()
        {
            AttachmentSlotDefinition opticSlot = CreateSlot("optic");
            WeaponDefinition rifle = CreateWeapon("rifle", opticSlot);
            WeaponAttachmentDefinition scope =
                CreateAttachment("scope", opticSlot);
            WeaponCatalog catalog = CreateCatalog(rifle, scope);
            const string json =
                "{\"schemaVersion\":1,\"weaponId\":\"rifle\"," +
                "\"attachments\":[{\"slotId\":\"muzzle\"," +
                "\"attachmentId\":\"scope\"}]}";

            bool resolved = WeaponLoadoutJson.TryResolve(
                json,
                catalog,
                out _,
                out _,
                out string error);

            Assert.That(resolved, Is.False);
            StringAssert.Contains("does not match saved slot", error);
        }

        [Test]
        public void Json_RejectsUnknownSchemaWithoutMutatingCatalog()
        {
            AttachmentSlotDefinition opticSlot = CreateSlot("optic");
            WeaponDefinition rifle = CreateWeapon("rifle", opticSlot);
            WeaponCatalog catalog = CreateCatalog(rifle);
            const string json =
                "{\"schemaVersion\":99,\"weaponId\":\"rifle\",\"attachments\":[]}";

            bool resolved = WeaponLoadoutJson.TryResolve(
                json,
                catalog,
                out _,
                out _,
                out string error);

            Assert.That(resolved, Is.False);
            StringAssert.Contains("unsupported schema version", error);
            Assert.That(catalog.TryGetWeapon("rifle", out var found), Is.True);
            Assert.That(found, Is.SameAs(rifle));
        }

        [Test]
        public void Catalog_DoesNotResolveAmbiguousStableId()
        {
            WeaponDefinition first = CreateWeapon("duplicate");
            WeaponDefinition second = CreateWeapon("duplicate");
            WeaponCatalog catalog = CreateCatalog(
                new[] { first, second },
                new WeaponAttachmentDefinition[0]);

            bool found = catalog.TryGetWeapon("duplicate", out var weapon);

            Assert.That(found, Is.False);
            Assert.That(weapon, Is.Null);
        }

        private WeaponCatalog CreateCatalog(
            WeaponDefinition weapon,
            params WeaponAttachmentDefinition[] attachments)
        {
            return CreateCatalog(new[] { weapon }, attachments);
        }

        private WeaponCatalog CreateCatalog(
            WeaponDefinition[] weaponDefinitions,
            WeaponAttachmentDefinition[] attachments)
        {
            WeaponCatalog catalog = Create<WeaponCatalog>();
            var serialized = new SerializedObject(catalog);
            SerializedProperty weapons = serialized.FindProperty("weapons");
            weapons.arraySize = weaponDefinitions.Length;
            for (int i = 0; i < weaponDefinitions.Length; i++)
            {
                weapons.GetArrayElementAtIndex(i).objectReferenceValue =
                    weaponDefinitions[i];
            }

            SerializedProperty attachmentList = serialized.FindProperty("attachments");
            attachmentList.arraySize = attachments.Length;
            for (int i = 0; i < attachments.Length; i++)
            {
                attachmentList.GetArrayElementAtIndex(i).objectReferenceValue = attachments[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            catalog.RebuildLookups();
            return catalog;
        }

        private AttachmentSlotDefinition CreateSlot(string id)
        {
            AttachmentSlotDefinition slot = Create<AttachmentSlotDefinition>();
            SetString(slot, "id", id);
            return slot;
        }

        private WeaponDefinition CreateWeapon(
            string id,
            params AttachmentSlotDefinition[] slots)
        {
            WeaponDefinition weapon = Create<WeaponDefinition>();
            var serialized = new SerializedObject(weapon);
            serialized.FindProperty("id").stringValue = id;

            SerializedProperty supportedSlots = serialized.FindProperty("supportedSlots");
            supportedSlots.arraySize = slots.Length;
            for (int i = 0; i < slots.Length; i++)
            {
                supportedSlots.GetArrayElementAtIndex(i).objectReferenceValue = slots[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return weapon;
        }

        private WeaponAttachmentDefinition CreateAttachment(
            string id,
            AttachmentSlotDefinition slot)
        {
            WeaponAttachmentDefinition attachment =
                Create<WeaponAttachmentDefinition>();
            var serialized = new SerializedObject(attachment);
            serialized.FindProperty("id").stringValue = id;
            serialized.FindProperty("slot").objectReferenceValue = slot;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return attachment;
        }

        private static void SetString(Object target, string propertyName, string value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private T Create<T>() where T : ScriptableObject
        {
            T instance = ScriptableObject.CreateInstance<T>();
            createdObjects.Add(instance);
            return instance;
        }
    }
}
