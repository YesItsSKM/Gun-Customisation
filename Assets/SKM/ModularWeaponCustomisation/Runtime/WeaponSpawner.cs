using System;
using UnityEngine;

namespace SKM.ModularWeaponCustomisation
{
    /// <summary>
    /// Maintains one current weapon instance. A replacement is fully initialized
    /// before the previous instance is released.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WeaponSpawner : MonoBehaviour
    {
        [SerializeField] private Transform spawnParent = null;
        [SerializeField] private WeaponDefinition startingWeapon = null;
        [SerializeField] private bool spawnOnStart = false;

        public event EventHandler<WeaponSpawnChangedEventArgs> CurrentWeaponChanged;

        public WeaponRuntime CurrentWeapon { get; private set; }
        public Transform SpawnParent => spawnParent != null ? spawnParent : transform;

        private void Start()
        {
            if (spawnOnStart && startingWeapon != null &&
                !TrySpawn(startingWeapon, out _, out WeaponSpawnFailure failure))
            {
                Debug.LogError(
                    $"Could not spawn starting weapon '{startingWeapon.DisplayName}': " +
                    $"{failure}.",
                    this);
            }
        }

        /// <summary>
        /// Creates and initializes a weapon. The current instance is unchanged when
        /// validation or initialization fails.
        /// </summary>
        public bool TrySpawn(
            WeaponDefinition definition,
            out WeaponRuntime spawnedWeapon,
            out WeaponSpawnFailure failure)
        {
            spawnedWeapon = null;

            if (definition == null)
            {
                failure = WeaponSpawnFailure.MissingWeaponDefinition;
                return false;
            }

            if (definition.Prefab == null)
            {
                failure = WeaponSpawnFailure.MissingPrefab;
                return false;
            }

            GameObject spawnedObject = Instantiate(
                definition.Prefab,
                SpawnParent,
                false);
            spawnedObject.name = definition.DisplayName;
            spawnedWeapon = spawnedObject.GetComponent<WeaponRuntime>();

            if (spawnedWeapon == null)
            {
                DestroySpawnedObject(spawnedObject);
                failure = WeaponSpawnFailure.MissingRuntime;
                return false;
            }

            if ((!spawnedWeapon.IsInitialized ||
                 spawnedWeapon.Definition != definition) &&
                !spawnedWeapon.Initialize(definition))
            {
                DestroySpawnedObject(spawnedObject);
                spawnedWeapon = null;
                failure = WeaponSpawnFailure.InitializationFailed;
                return false;
            }

            WeaponRuntime previousWeapon = CurrentWeapon;
            CurrentWeapon = spawnedWeapon;
            failure = WeaponSpawnFailure.None;

            CurrentWeaponChanged?.Invoke(
                this,
                new WeaponSpawnChangedEventArgs(previousWeapon, CurrentWeapon));

            if (previousWeapon != null && previousWeapon != CurrentWeapon)
            {
                DestroySpawnedObject(previousWeapon.gameObject);
            }

            return true;
        }

        /// <summary>Clears and destroys the current instance, if one exists.</summary>
        public bool Clear()
        {
            if (CurrentWeapon == null)
            {
                return false;
            }

            WeaponRuntime previousWeapon = CurrentWeapon;
            CurrentWeapon = null;
            CurrentWeaponChanged?.Invoke(
                this,
                new WeaponSpawnChangedEventArgs(previousWeapon, null));
            DestroySpawnedObject(previousWeapon.gameObject);
            return true;
        }

        private static void DestroySpawnedObject(GameObject spawnedObject)
        {
            if (spawnedObject == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(spawnedObject);
            }
            else
            {
                DestroyImmediate(spawnedObject);
            }
        }
    }
}
