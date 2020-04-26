using UnityEngine;

public class AlliesSaving : MonoBehaviour
{
    // All the enemy prefabs
    [SerializeField] private GameObject[] _allAllyPrefabs = null;


    // Called before Start
    private void Awake()
    {
        // Create the Hashtable for Allies
        if (_allAllyPrefabs != null)
            SaveAlliesController.BuildHashtable(_allAllyPrefabs);
    }
}
