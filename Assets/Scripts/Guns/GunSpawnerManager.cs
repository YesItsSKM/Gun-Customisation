using System.Collections.Generic;
using UnityEngine;

public class GunSpawnerManager : MonoBehaviour
{
    public GunData[] gunsToSpawn;
    public Transform[] gunsSpawnTransform;

    private Dictionary<GunData, GameObject> spawnedGunsMap;

    void Start()
    {
        spawnedGunsMap = new Dictionary<GunData, GameObject>();
        SpawnGuns();
    }

    void SpawnGuns()
    {
        if (!RunValidationChecks(gunsToSpawn, gunsSpawnTransform))
            return;

        for (int i = 0; i < gunsToSpawn.Length; i++)
        {
            if (spawnedGunsMap.ContainsKey(gunsToSpawn[i])
                && spawnedGunsMap[gunsToSpawn[i]] != null) continue;

            GameObject newGun = Instantiate(gunsToSpawn[i].gunPrefab,
                                gunsSpawnTransform[i].transform.position, 
                                gunsSpawnTransform[i].transform.rotation);

            newGun.transform.SetParent(gunsSpawnTransform[i].transform, true);

            spawnedGunsMap[gunsToSpawn[i]] = newGun;
        }
    }


    /// <summary>
    /// Return FALSE if 'gunsToSpawn[]' and 'gunsSpawnTransform[]' is NULL or has mismatch in number of elements.
    /// </summary>
    /// <param name="gunsToSpawn"></param>
    /// <param name="gunsSpawnTransform"></param>
    /// <returns></returns>
    private bool RunValidationChecks(GunData[] gunsToSpawn, Transform[] gunsSpawnTransform)
    {
        bool allOk = true;

        if (gunsToSpawn == null)
        {
            Debug.LogError("ERROR: No Guns To Spawn!");
            allOk = false;
        }

        if (gunsSpawnTransform == null)
        {
            Debug.LogError("ERROR: Gun Spawnpoints NOT specified!");
            allOk = false;
        }

        if (gunsToSpawn.Length != gunsSpawnTransform.Length)
        {
            Debug.LogError("ERROR: Mismatch in 'gunsToSpawn' and 'gunsSpawnTransform' array lengths");
            allOk = false;
        }

        return allOk;
    }
}
