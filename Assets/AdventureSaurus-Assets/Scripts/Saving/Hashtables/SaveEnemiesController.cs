using System.Collections;
using UnityEngine;

public static class SaveEnemiesController
{
    // The hashtable for converting from int to Prefab
    private static Hashtable _prefHash;


    //// Getters
    /// <summary>
    /// Get a Prefab by its key
    /// </summary>
    /// <param name="key">int key for the Prefab</param>
    /// <returns>GameObject. Prefab of an Enemy</returns>
    public static GameObject GetEnemyPrefab(int key) { return _prefHash[key] as GameObject; }

    /// <summary>
    /// Create the hashtable for looking up the prefab of an enemy
    /// </summary>
    /// <param name="allEnemyPrefabs">The prefabs to set in the hashtable</param>
    public static void BuildHashtable(GameObject[] allEnemyPrefabs)
    {
        _prefHash = SavePrefabsController.BuildHashtable(allEnemyPrefabs);
    }
}
