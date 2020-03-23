﻿using System.Collections;
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
        // Be careful of the infinite
        int maxRoomIterations = roomParent.childCount + 1;
        int currentRoomIterations = 0;

        // Iterate over each room
        foreach (Transform curRoom in roomParent)
        {
            /// Step 1: Assign a difficulty to the room
            ///
            Room curRoomScript = curRoom.GetComponent<Room>();
            // Make sure it has a room script, it should always
            if (curRoomScript == null)
            {
                Debug.Log("WARNING - BUG DETECTED: " + curRoom.name + " has no Room script attached to it");
                continue;
            }

            // If its a normal room
            if (curRoomScript.MyRoomType == RoomType.NORMAL)
                curRoomScript.RoomDifficulty = floorBaseDiff + curRoomScript.RoomWeight * 2;
            // If its a hallway
            else if (curRoomScript.MyRoomType == RoomType.HALLWAY)
                curRoomScript.RoomDifficulty = Mathf.RoundToInt((floorBaseDiff + (curRoomScript.RoomWeight * 2)) * hallwayScalar);
            // If its the end
            else if (curRoomScript.MyRoomType == RoomType.END)
                curRoomScript.RoomDifficulty = Mathf.RoundToInt((floorBaseDiff + (curRoomScript.RoomWeight * 2)) * endScalar);
            // If its the start or safe room, we don't spawn enemies
            else if (curRoomScript.MyRoomType == RoomType.SAFE || curRoomScript.MyRoomType == RoomType.START)
                curRoomScript.RoomDifficulty = 0;
            // If its an unhandled room type
            else
            {
                curRoomScript.RoomDifficulty = 0;
                Debug.Log("Unkown room type in GenerateEnemies");
            }


            /// Step 2: Get a list of the locations in the room we can spawn the enemy.
            ///
            // Get the bounds of the room
            Vector2Int botLeftFloorSpace = new Vector2Int(Mathf.RoundToInt(curRoom.position.x - (curRoom.localScale.x - 1) / 2f + 1),
                Mathf.RoundToInt(curRoom.position.y - (curRoom.localScale.y - 1) / 2f + 1));
            Vector2Int topRightFloorSpace = new Vector2Int(Mathf.RoundToInt(curRoom.position.x + (curRoom.localScale.x - 1) / 2f - 1),
                Mathf.RoundToInt(curRoom.position.y + (curRoom.localScale.y - 1) / 2f - 1));
            // Create the list of available spots
            List<Vector2Int> availSpawnPositions = new List<Vector2Int>();
            // Iterate over the spots in the room to add them to the available spots list
            // Each row (y)
            for (int row = botLeftFloorSpace.y; row <= topRightFloorSpace.y; ++row)
            {
                // Each col (x)
                for (int col = botLeftFloorSpace.x; col <= topRightFloorSpace.x; ++col)
                {
                    // Add the current position to the list
                    availSpawnPositions.Add(new Vector2Int(col, row));
                }
            }

            /// Step 3: Create the enemies for the room in compliance with the difficulty
            /// This algorithm may be edited by adding more steps between 1 and 2 to restrict the 
            /// difficulty of the enemies spawned and other tweaks
            /// 
            // Being extra careful
            int maxIterations = curRoomScript.RoomDifficulty;
            int currentIterations = 0;
            // We start the current difficulty at 0, since there are no enemies currently in the room
            int currentDifficulty = 0;
            // Create a new enemy until the currentDifficulty is the room's difficulty
            while (currentDifficulty < curRoomScript.RoomDifficulty && availSpawnPositions.Count > 0)
            {
                /// Step 3a: Pick an enemy from the list of enemies
                /// 
                int enemyPrefIndex = Random.Range(0, enemyPrefabs.Length);
                GameObject enemyPrefToSpawn = enemyPrefabs[enemyPrefIndex];

                /// Step 3b: Pick a spot in the room to spawn the enemy
                /// 
                int randPosIndex = Random.Range(0, availSpawnPositions.Count);
                Vector2Int spawnGridPos = availSpawnPositions[randPosIndex];
                // Cast the grid position to a world position
                Vector3 spawnWorldPos = new Vector3(spawnGridPos.x, spawnGridPos.y, 0);
                // Remove the spot we spawned them at from the availSpawnPositions, so that no enemy may be spawned here again
                availSpawnPositions.RemoveAt(randPosIndex);

                /// Step 3c: Spawn the enemy
                /// 
                GameObject spawnedEnemyObj = Instantiate(enemyPrefToSpawn, characterParent);
                spawnedEnemyObj.transform.position = spawnWorldPos;

                /// Step 3d: Increment the current difficulty
                /// 
                EnemyDifficulty enDiffScriptRef = spawnedEnemyObj.GetComponent<EnemyDifficulty>();
                // If the enemy for some reason has no EnemyDifficulty script attached to it
                if (enDiffScriptRef != null)
                {
                    // Incase the enemy difficulty was set wrong
                    if (enDiffScriptRef.Difficulty <= 0)
                        currentDifficulty += 1;
                    else
                        currentDifficulty += enDiffScriptRef.Difficulty;
                }
                else
                    currentDifficulty += 1;

                // Be careful of the inifinite
                if (++currentIterations > maxIterations)
                {
                    Debug.Log("WARNING - POTENTIAL BUG DETECTED - We tried to spawn enemies " + currentIterations + " times");
                    break;
                }
            }


            // Beware the infinite
            if (++currentRoomIterations > maxRoomIterations)
            {
                Debug.Log("WARNING - POTENTIAL BUG DETECTED - We tried to spawn enemies in " + currentRoomIterations + " rooms. "
                    + " There are only " + maxRoomIterations + " rooms");
                break;
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
