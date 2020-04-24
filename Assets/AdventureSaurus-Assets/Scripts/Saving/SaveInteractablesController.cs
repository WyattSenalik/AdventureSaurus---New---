using System.Collections;
using UnityEngine;

public static class SaveInteractablesController
{
    // The hashtable for converting from int to Prefab
    private static Hashtable _prefHash;


    //// Getters
    ////// <summary>
    /// Get a Prefab by its key
    /// </summary>
    /// <param name="key">int key for the Prefab</param>
    /// <returns>GameObject. Prefab of an Interactable</returns>
    public static GameObject GetPrefab(int key) { return _prefHash[key] as GameObject; }

    /// <summary>
    /// Create the hashtable for looking up the prefab of an interactable
    /// </summary>
    /// <param name="allInteractablesPrefabs">The prefabs to set in the hashtable</param>
    public static void BuildHashtables(GameObject[] allInteractablesPrefabs)
    {
        _prefHash = new Hashtable(allInteractablesPrefabs.Length);
        // Put each prefab into the hashtables based on their key
        foreach (GameObject singlePref in allInteractablesPrefabs)
        {
            // Try to pull an interactable script off the prefab (it should have one)
            Interactable interScriptRef = singlePref.GetComponent<Interactable>();
            if (interScriptRef != null)
            {
                int currentKey = interScriptRef.GetKey();
                _prefHash.Add(currentKey, singlePref);
            }
            else
                Debug.LogError("No Interactable script was attached to " + interScriptRef.name);
        }
    }
}
