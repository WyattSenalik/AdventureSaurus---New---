using System.Collections;
using UnityEngine;

public static class SaveAlliesController
{
    // The hashtable for converting from int to Prefab
    private static Hashtable _prefHash;


    //// Getters
    /// <summary>
    /// Get a Prefab by its key
    /// </summary>
    /// <param name="key">int key for the Prefab</param>
    /// <returns>GameObject. Prefab of an Ally</returns>
    public static GameObject GetAllyPrefab(int key) { return _prefHash[key] as GameObject; }

    /// <summary>
    /// Create the hashtable for looking up the prefab of an ally
    /// </summary>
    /// <param name="allAllyPrefabs">The prefabs to set in the hashtable</param>
    public static void BuildHashtable(GameObject[] allAllyPrefabs)
    {
        _prefHash = SavePrefabsController.BuildHashtable(allAllyPrefabs);
    }
}
