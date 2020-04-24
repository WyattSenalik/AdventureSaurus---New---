using System.Collections;
using UnityEngine;

public static class SavePrefabsController
{
    /// <summary>
    /// Create the hashtable for looking up the prefab
    /// </summary>
    /// <param name="allPrefabs">The prefabs that we are setting the hashtable to contain<param>
    /// <returns>Hashtable. Built hashtable that has assigned keys to the prefabs</returns>
    public static Hashtable BuildHashtable(GameObject[] allPrefabs)
    {
        Hashtable prefHash = new Hashtable(allPrefabs.Length);
        // Put each prefab into the hashtables based on their key
        foreach (GameObject singlePref in allPrefabs)
        {
            // Try to pull a PrefabKey script off the prefab (it should have one)
            PrefabKey prefKeyScriptRef = singlePref.GetComponent<PrefabKey>();
            if (prefKeyScriptRef != null)
            {
                int currentKey = prefKeyScriptRef.GetPrefabKey();
                prefHash.Add(currentKey, singlePref);
            }
            else
                Debug.LogError("No Interactable script was attached to " + singlePref.name);
        }

        return prefHash;
    }
}