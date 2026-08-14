using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace SKM.ModularWeaponCustomisation.Tests
{
    public sealed class WeaponCoreTests
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
        public void StatsCalculator_AppliesAddThenMultiplyAndClamps()
        {
            WeaponStatDefinition damage = CreateStat("damage", 0f, 200f);
            var baseStats = new[]
            {
                new WeaponStatValue(damage, 100f)
            };
            var modifiers = new[]
            {
                new WeaponStatModifier(damage, WeaponStatModifierOperation.Add, 10f),
                new WeaponStatModifier(damage, WeaponStatModifierOperation.Multiply, 2f)
            };

            WeaponStatsSnapshot result =
                WeaponStatsCalculator.Calculate(baseStats, modifiers);

            Assert.That(result.GetValueOrDefault(damage), Is.EqualTo(200f));
        }

        [Test]
        public void StatsCalculator_HighestPriorityOverrideWins()
        {
            WeaponStatDefinition recoil = CreateStat("recoil", 0f, 100f);
            var modifiers = new[]
            {
                new WeaponStatModifier(
                    recoil,
                    WeaponStatModifierOperation.Override,
                    70f,
                    priority: 1),
                new WeaponStatModifier(
                    recoil,
                    WeaponStatModifierOperation.Override,
                    25f,
                    priority: 5),
                new WeaponStatModifier(
                    recoil,
                    WeaponStatModifierOperation.Override,
                    90f,
                    priority: 2)
            };

            WeaponStatsSnapshot result =
                WeaponStatsCalculator.Calculate(null, modifiers);

            Assert.That(result.GetValueOrDefault(recoil), Is.EqualTo(25f));
        }

        [Test]
        public void StatsCalculator_RejectsNonFiniteCalculatedResult()
        {
            WeaponStatDefinition heat = CreateStat(
                "heat",
                -float.MaxValue,
                float.MaxValue);
            var baseStats = new[]
            {
                new WeaponStatValue(heat, float.MaxValue)
            };
            var modifiers = new[]
            {
                new WeaponStatModifier(
                    heat,
                    WeaponStatModifierOperation.Multiply,
                    2f)
            };

            Assert.Throws<System.ArgumentException>(
                () => WeaponStatsCalculator.Calculate(baseStats, modifiers));
        }

        [Test]
        public void Loadouts_AreIndependentAndReplacementReportsPreviousAttachment()
        {
            AttachmentSlotDefinition opticSlot = CreateSlot("optic");
            WeaponDefinition weapon = CreateWeapon("rifle", opticSlot);
            WeaponAttachmentDefinition redDot =
                CreateAttachment("red-dot", opticSlot);
            WeaponAttachmentDefinition scope =
                CreateAttachment("scope", opticSlot);

            var firstLoadout = new WeaponLoadout(weapon);
            var secondLoadout = new WeaponLoadout(weapon);
            WeaponLoadoutChangedEventArgs lastChange = null;
            firstLoadout.Changed += (_, eventArgs) => lastChange = eventArgs;

            Assert.That(
                firstLoadout.TryInstall(redDot, out AttachmentInstallFailure firstFailure),
                Is.True);
            Assert.That(firstFailure, Is.EqualTo(AttachmentInstallFailure.None));
            Assert.That(firstLoadout.TryInstall(scope, out _), Is.True);

            Assert.That(lastChange, Is.Not.Null);
            Assert.That(lastChange.PreviousAttachment, Is.SameAs(redDot));
            Assert.That(lastChange.CurrentAttachment, Is.SameAs(scope));
            Assert.That(firstLoadout.TryGetAttachment(opticSlot, out var installed), Is.True);
            Assert.That(installed, Is.SameAs(scope));
            Assert.That(secondLoadout.TryGetAttachment(opticSlot, out _), Is.False);
        }

        [Test]
        public void Loadout_RejectsAttachmentForUnsupportedSlot()
        {
            AttachmentSlotDefinition opticSlot = CreateSlot("optic");
            AttachmentSlotDefinition muzzleSlot = CreateSlot("muzzle");
            WeaponDefinition weapon = CreateWeapon("pistol", opticSlot);
            WeaponAttachmentDefinition suppressor =
                CreateAttachment("suppressor", muzzleSlot);
            var loadout = new WeaponLoadout(weapon);

            bool installed = loadout.TryInstall(suppressor, out var failure);

            Assert.That(installed, Is.False);
            Assert.That(failure, Is.EqualTo(AttachmentInstallFailure.UnsupportedSlot));
            Assert.That(loadout.InstalledAttachments, Is.Empty);
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

        private AttachmentSlotDefinition CreateSlot(string id)
        {
            AttachmentSlotDefinition slot = Create<AttachmentSlotDefinition>();
            var serialized = new SerializedObject(slot);
            serialized.FindProperty("id").stringValue = id;
            serialized.ApplyModifiedPropertiesWithoutUndo();
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
            WeaponAttachmentDefinition attachment = Create<WeaponAttachmentDefinition>();
            var serialized = new SerializedObject(attachment);
            serialized.FindProperty("id").stringValue = id;
            serialized.FindProperty("slot").objectReferenceValue = slot;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return attachment;
        }

        private T Create<T>() where T : ScriptableObject
        {
            T instance = ScriptableObject.CreateInstance<T>();
            createdObjects.Add(instance);
            return instance;
        }
    }
}
