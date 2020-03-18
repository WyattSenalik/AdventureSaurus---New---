using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateEnemies : MonoBehaviour
{
    // List of potential enemies to spawn. Sorted by difficulty
    [SerializeField] private GameObject[] enemyPrefabs = null;
    // For reducing the hallway's difficulty
    [SerializeField] private float hallwayScalar = 0.5f;
    // For increasing the end room's difficulty
    [SerializeField] private float endScalar = 1.2f;

    // Set references
    private void Awake()
    {
        // Check to see if there are enemies to choose from
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            Debug.Log("No enemies have been specified in GenerateEnemies attached to " + this.name);
        // If we have enemies sort them, since we don't trust us to sort them in editor
        else
            SortEnemies();
    }

    /// <summary>
    /// Spawns enemies in the rooms and saves them as children of the character parent
    /// </summary>
    /// <param name="characterParent">Parent of all the characters</param>
    /// <param name="roomParent">Parent of all the rooms</param>
    public void SpawnEnemies(Transform characterParent, Transform roomParent, int floorBaseDiff)
    {
        // Assign a difficulty to each room
        foreach (Transform curRoom in roomParent)
        {
            Room curRoomScript = curRoom.GetComponent<Room>();
            // Make sure it has a room script, it should always
            if (curRoomScript == null)
            {
                Debug.Log("WARNING - BUG DETECTED: " + curRoom.name + " has no Room script attached to it");
                continue;
            }

            // If its a normal room
            if (curRoomScript.MyRoomType == RoomType.NORMAL)
                curRoomScript.RoomDifficulty = floorBaseDiff + curRoomScript.RoomWeight;
            // If its a hallway
            else if (curRoomScript.MyRoomType == RoomType.HALLWAY)
                curRoomScript.RoomDifficulty = Mathf.RoundToInt((floorBaseDiff + curRoomScript.RoomWeight) * hallwayScalar);
            // If its the end
            else if (curRoomScript.MyRoomType == RoomType.END)
                curRoomScript.RoomDifficulty = Mathf.RoundToInt((floorBaseDiff + curRoomScript.RoomWeight) * endScalar);
            // If its the start or safe room, we don't spawn enemies
            else if (curRoomScript.MyRoomType == RoomType.SAFE || curRoomScript.MyRoomType == RoomType.START)
                curRoomScript.RoomDifficulty = 0;
            // If its an unhandled room type
            else
            {
                curRoomScript.RoomDifficulty = 0;
                Debug.Log("Unkown room type in GenerateEnemies");
            }
        }
    }

    /// <summary>
    /// Sorts the enemyPrefabs list.
    /// </summary>
    private void SortEnemies()
    {
        // Iterate over the enemies
        for (int i = 0; i < enemyPrefabs.Length; ++i)
        {
            // Get the current enemies information
            EnemyDifficulty enemyDiffRef0 = enemyPrefabs[i].GetComponent<EnemyDifficulty>();
            int enemyDiff0 = enemyDiffRef0.Difficulty;
            // Assume this enemy is the weakest
            int weakestEnemyIndex = i;
            int weakestEnemyDiff = enemyDiff0;
            // Iterate over the enemies after the current to see if one is weaker
            for (int k = i + 1; k < enemyPrefabs.Length; ++k)
            {
                // Get the next enemies information
                EnemyDifficulty enemyDiffRef1 = enemyPrefabs[k].GetComponent<EnemyDifficulty>();
                int enemyDiff1 = enemyDiffRef1.Difficulty;

                // If we found an enemy weaker than the current enemy, make this enemy the new weakest
                if (enemyDiff1 < weakestEnemyDiff)
                {
                    weakestEnemyDiff = enemyDiff1;
                    weakestEnemyIndex = k;
                }
            }
            // Swap the enemies
            GameObject tempBucket = enemyPrefabs[i];
            enemyPrefabs[i] = enemyPrefabs[weakestEnemyIndex];
            enemyPrefabs[weakestEnemyIndex] = tempBucket;
        }
    }
}
