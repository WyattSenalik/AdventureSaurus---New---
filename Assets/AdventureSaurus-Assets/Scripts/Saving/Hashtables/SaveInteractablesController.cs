using System.Collections;
using UnityEngine;

public static class SaveInteractablesController
{
    // The hashtable for converting from int to Prefab
    private static Hashtable _prefHash;


    //// Getters
    /// <summary>
    /// Get a Prefab by its key
    /// </summary>
    /// <param name="key">int key for the Prefab</param>
    /// <returns>GameObject. Prefab of an Interactable</returns>
    public static GameObject GetInteractablePrefab(int key) { return _prefHash[key] as GameObject; }

    /// <summary>
    /// Create the hashtable for looking up the prefab of an interactable
    /// </summary>
    /// <param name="allInteractablesPrefabs">The prefabs to set in the hashtable</param>
    public static void BuildHashtable(GameObject[] allInteractablesPrefabs)
    {
        _prefHash = SavePrefabsController.BuildHashtable(allInteractablesPrefabs);
    }
}
