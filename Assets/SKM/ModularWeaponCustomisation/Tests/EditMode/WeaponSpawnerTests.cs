using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace SKM.ModularWeaponCustomisation.Tests
{
    public sealed class WeaponSpawnerTests
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
        public void Spawn_InitializesReplacementBeforeReleasingPreviousWeapon()
        {
            WeaponDefinition firstDefinition = CreateWeapon("first");
            WeaponDefinition secondDefinition = CreateWeapon("second");
            GameObject spawnerObject = CreateGameObject("Spawner", active: true);
            WeaponSpawner spawner = spawnerObject.AddComponent<WeaponSpawner>();
            WeaponRuntime previousReported = null;
            WeaponRuntime currentReported = null;
            spawner.CurrentWeaponChanged += (_, eventArgs) =>
            {
                previousReported = eventArgs.PreviousWeapon;
                currentReported = eventArgs.CurrentWeapon;
            };

            Assert.That(
                spawner.TrySpawn(
                    firstDefinition,
                    out WeaponRuntime first,
                    out WeaponSpawnFailure firstFailure),
                Is.True);
            Assert.That(firstFailure, Is.EqualTo(WeaponSpawnFailure.None));
            Assert.That(first.IsInitialized, Is.True);
            Assert.That(spawner.CurrentWeapon, Is.SameAs(first));

            Assert.That(
                spawner.TrySpawn(
                    secondDefinition,
                    out WeaponRuntime second,
                    out WeaponSpawnFailure secondFailure),
                Is.True);

            Assert.That(secondFailure, Is.EqualTo(WeaponSpawnFailure.None));
            Assert.That(previousReported, Is.SameAs(first));
            Assert.That(currentReported, Is.SameAs(second));
            Assert.That(first == null, Is.True);
            Assert.That(second.Definition, Is.SameAs(secondDefinition));
            Assert.That(spawner.CurrentWeapon, Is.SameAs(second));
        }

        [Test]
        public void SpawnFailure_DoesNotReplaceCurrentWeapon()
        {
            WeaponDefinition validDefinition = CreateWeapon("valid");
            WeaponDefinition invalidDefinition = CreateDefinition(
                "invalid",
                CreateGameObject("No Runtime", active: false));
            WeaponSpawner spawner =
                CreateGameObject("Spawner", active: true).AddComponent<WeaponSpawner>();
            Assert.That(
                spawner.TrySpawn(validDefinition, out WeaponRuntime current, out _),
                Is.True);

            bool spawned = spawner.TrySpawn(
                invalidDefinition,
                out WeaponRuntime rejected,
                out WeaponSpawnFailure failure);

            Assert.That(spawned, Is.False);
            Assert.That(rejected, Is.Null);
            Assert.That(failure, Is.EqualTo(WeaponSpawnFailure.MissingRuntime));
            Assert.That(spawner.CurrentWeapon, Is.SameAs(current));
        }

        private WeaponDefinition CreateWeapon(string id)
        {
            GameObject prefab = CreateGameObject($"{id} Prefab", active: false);
            WeaponRuntime runtime = prefab.AddComponent<WeaponRuntime>();
            WeaponDefinition definition = CreateDefinition(id, prefab);
            var runtimeSerialized = new SerializedObject(runtime);
            runtimeSerialized.FindProperty("definition").objectReferenceValue = definition;
            runtimeSerialized.FindProperty("initializeOnAwake").boolValue = false;
            runtimeSerialized.ApplyModifiedPropertiesWithoutUndo();
            return definition;
        }

        private WeaponDefinition CreateDefinition(string id, GameObject prefab)
        {
            WeaponDefinition definition = Create<WeaponDefinition>();
            var serialized = new SerializedObject(definition);
            serialized.FindProperty("id").stringValue = id;
            serialized.FindProperty("displayName").stringValue = id;
            serialized.FindProperty("prefab").objectReferenceValue = prefab;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return definition;
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
