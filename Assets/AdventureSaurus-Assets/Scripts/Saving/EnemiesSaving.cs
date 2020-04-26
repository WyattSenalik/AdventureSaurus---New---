using UnityEngine;

public class EnemiesSaving : MonoBehaviour
{
    // All the enemy prefabs
    [SerializeField] private GameObject[] _allEnemyPrefabs = null;


    // Called before Start
    private void Awake()
    {
        // Create the Hashtable for Enemies
        if (_allEnemyPrefabs != null)
            SaveEnemiesController.BuildHashtable(_allEnemyPrefabs);
    }
}
