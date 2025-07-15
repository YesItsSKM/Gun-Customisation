using System;
using System.Collections.Generic;
using UnityEngine;

public class GunSpawnerManager : MonoBehaviour
{
    [Serializable]
    public class GunsToSpawn
    {
        public GunData gunData;
        public Transform spawnTransform;
    }

    public GunsToSpawn[] gunsToSpawn;

    private Dictionary<GunData, GameObject> spawnedGunsMap;

    void Start()
    {
        spawnedGunsMap = new Dictionary<GunData, GameObject>();
        SpawnGuns();
    }

    public void SpawnGuns()
    {
        if (!RunValidationChecks(gunsToSpawn))
            return;

        for (int i = 0; i < gunsToSpawn.Length; i++)
        {
            GunData gunData = gunsToSpawn[i].gunData;
            Transform spawnPoint = gunsToSpawn[i].spawnTransform;

            if (spawnedGunsMap.ContainsKey(gunData) && spawnedGunsMap[gunData] != null)
                continue;

            GameObject newGun = Instantiate(gunData.gunPrefab, spawnPoint.position, spawnPoint.rotation);
            newGun.transform.SetParent(spawnPoint, true);

            spawnedGunsMap[gunData] = newGun;

            Inspectable inspectable = newGun.GetComponent<Inspectable>();
            if (inspectable != null)
            {
                inspectable.SetGunData(gunData);
            }
        }
    }


    /// <summary>
    /// Validates each GunsToSpawn entry; Checks if 'gunData', and/or 'spawnTransform' is NULL.
    /// </summary>
    /// <param name="gunsToSpawn">Array of GunsToSpawn objects to validate</param>
    /// <returns>TRUE if all entries are valid; otherwise, FALSE</returns>

    private bool RunValidationChecks(GunsToSpawn[] gunsToSpawn)
    {
        bool allOk = true;

        if (gunsToSpawn == null || gunsToSpawn.Length == 0)
        {
            Debug.LogError("ERROR: 'gunsToSpawn' array is null or empty.");
            return false;
        }

        for(int i = 0; i < gunsToSpawn.Length; i++)
        {
            if (gunsToSpawn[i] == null)
            {
                Debug.LogError($"ERROR: gunsToSpawn[{i}] is null.");
                allOk = false;
                continue;
            }

            if (gunsToSpawn[i].gunData == null)
            {
                Debug.LogError($"ERROR: gunData in gunsToSpawn[{i}] is null or empty.");
                allOk = false;
            }

            if (gunsToSpawn[i].spawnTransform == null)
            {
                Debug.LogError($"ERROR: spawnTransform in gunsToSpawn[{i}] is null or empty.");
                allOk = false;
            }
        }

        return allOk;
    }
}
