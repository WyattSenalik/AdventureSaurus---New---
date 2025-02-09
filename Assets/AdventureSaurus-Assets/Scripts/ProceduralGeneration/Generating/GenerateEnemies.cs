﻿using System.Collections.Generic;
using UnityEngine;

public class GenerateEnemies : MonoBehaviour
{
    private const float _strengthPerDiff = 1f/3f;
    private const float _magicPerDiff = 1f/3f;
    private const float _vitalityPerDiff = 2f/3f;
    private const float _speedPerDiff = 1f/4f;

    // List of potential enemies to spawn. Sorted by difficulty
    [SerializeField] private GameObject[] _enemyPrefabs = null;
    // For reducing the hallway's difficulty
    [SerializeField] private float _hallwayScalar = 0.5f;
    // Chance for a buffed enemy to be a rainbow enemy
    [SerializeField] private int _rainbowChance = 10;

    // Range for max amount of enemies in the room
    [SerializeField] private int _minMaxEnemiesInRoom = 3;
    [SerializeField] private int _maxMaxEnemiesInRoom = 6;

    // Set references
    private void Awake()
    {
        // Check to see if there are enemies to choose from
        if (_enemyPrefabs == null || _enemyPrefabs.Length == 0)
            Debug.Log("No enemies have been specified in GenerateEnemies attached to " + this.name);
        // If we have enemies sort them, since we don't trust us to sort them in editor
        else
            SortEnemies();
    }

    /// <summary>
    /// Spawns enemies in the rooms and saves them as children of the character parent
    /// </summary>
    /// <param name="enemyParent">Parent of all the enemies</param>
    /// <param name="roomParent">Parent of all the rooms</param>
    public void SpawnEnemies(Transform enemyParent, Transform roomParent, int floorBaseDiff)
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
            {
                curRoomScript.RoomDifficulty = Mathf.RoundToInt(floorBaseDiff + curRoomScript.RoomWeight);
            }
            // If its a hallway
            else if (curRoomScript.MyRoomType == RoomType.HALLWAY)
            {
                curRoomScript.RoomDifficulty = Mathf.RoundToInt((floorBaseDiff + curRoomScript.RoomWeight + 1) * _hallwayScalar);
            }
            // If its the end
            else if (curRoomScript.MyRoomType == RoomType.END)
            {
                curRoomScript.RoomDifficulty = Mathf.RoundToInt(floorBaseDiff + curRoomScript.RoomWeight);
            }
            // If its the start or safe room, we don't spawn enemies
            else if (curRoomScript.MyRoomType == RoomType.SAFE || curRoomScript.MyRoomType == RoomType.START)
            {
                curRoomScript.RoomDifficulty = 0;
            }
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

            /// Step 3: Determine the amount of enemies we want in this room
            /// 
            int maxEnemies = Random.Range(_minMaxEnemiesInRoom, _maxMaxEnemiesInRoom);

            /// Step 4: Create the enemies for the room in compliance with the difficulty
            /// This algorithm may be edited by adding more steps between 1 and 2 to restrict the 
            /// difficulty of the enemies spawned and other tweaks
            /// 
            //curRoomScript.RoomDifficulty *= 100;
            // Being extra careful
            int maxIterations = curRoomScript.RoomDifficulty;
            int currentIterations = 0;
            // We start the current difficulty at 0, since there are no enemies currently in the room
            int currentDifficulty = 0;
            // The amount of enemies we have spawned
            int currentAmEnemies = 0;
            // Create a new enemy until the currentDifficulty is the room's difficulty
            while (currentDifficulty < curRoomScript.RoomDifficulty && availSpawnPositions.Count > 0)
            {
                /// Step 4a: Determine the average difficulty an enemy would have to be to make it
                /// to the difficulty level in only x more enemies
                /// 
                int avgDiff = (curRoomScript.RoomDifficulty - currentDifficulty) / (maxEnemies - currentAmEnemies);

                /// Step 4b: Pick a spot in the room to spawn the enemy
                /// 
                int randPosIndex = Random.Range(0, availSpawnPositions.Count);
                Vector2Int spawnGridPos = availSpawnPositions[randPosIndex];
                // Cast the grid position to a world position
                Vector3 spawnWorldPos = new Vector3(spawnGridPos.x, spawnGridPos.y, 0);
                // Remove the spot we spawned them at from the availSpawnPositions, so that no enemy may be spawned here again
                availSpawnPositions.RemoveAt(randPosIndex);

                /// Steps 4c-e: Get a suitable enemy
                /// 
                // Initialize some stuff we will need after the loop
                // Reference to the GameObject of the enemy spawned
                GameObject spawnedEnemyObj = null;
                // Reference to the EnemyDifficulty script attached to the spawned enemy
                EnemyStats enStatsScriptRef;

                int maxSpawnLength = _enemyPrefabs.Length;
                // We will do c-e until we get an enemy that is not stronger than we need
                bool chooseNew = false;
                do
                {
                    /// Step 4c: Pick an enemy from the list of enemies
                    /// 
                    int enemyPrefIndex = Random.Range(0, maxSpawnLength);
                    GameObject enemyPrefToSpawn = _enemyPrefabs[enemyPrefIndex];


                    /// Step 4d: If that enemy is too strong for the current difficulty we need, then to pick a weaker one.
                    /// We will pick a enemy further left in the list, since they are sorted by difficulty
                    /// 
                    enStatsScriptRef = enemyPrefToSpawn.GetComponent<EnemyStats>();
                    if (enStatsScriptRef.GetDifficulty() > curRoomScript.RoomDifficulty - currentDifficulty)
                    {
                        chooseNew = true;
                        maxSpawnLength = enemyPrefIndex;
                    }
                    else
                    {
                        chooseNew = false;
                        /// Step 4e: Spawn the enemy
                        /// 
                        spawnedEnemyObj = Instantiate(enemyPrefToSpawn, enemyParent);
                        spawnedEnemyObj.transform.position = spawnWorldPos;
                    }
                } while (chooseNew);


                /// Step 4e: If that enemy is weaker than the avg difficulty, then we need to buff it
                /// 
                int extraDiff = 0;
                if (avgDiff > enStatsScriptRef.GetDifficulty())
                {
                    // Get the difference in difficulty we need to compensate for
                    extraDiff = avgDiff - enStatsScriptRef.GetDifficulty();
                    // Get the enemy stats we are going to change
                    EnemyStats enemyStats = spawnedEnemyObj.GetComponent<EnemyStats>();

                    // Get the amounts to buff the enemy by
                    int strBuff = Mathf.RoundToInt(_strengthPerDiff * extraDiff);
                    int mgcBuff = Mathf.RoundToInt(_magicPerDiff * extraDiff);
                    int spdBuff = Mathf.RoundToInt(_speedPerDiff * extraDiff);
                    int hpBuff = Mathf.RoundToInt(_vitalityPerDiff * extraDiff);
                    // Increase the stats
                    enemyStats.BuffEnemy(strBuff, mgcBuff, spdBuff, hpBuff);

                    // Give the enemy buff a chance to be rainbow
                    if (Random.Range(0, _rainbowChance) == 0)
                    {
                        // Let this enemy give more experience
                        enemyStats.AddExpToGive(extraDiff);
                        // Give it the rainbow effect
                        spawnedEnemyObj.AddComponent<Rainbow>();
                    }
                }

                /// Step 4f: Increment the current difficulty
                /// 
                // If the enemy for some reason has no EnemyDifficulty script attached to it
                if (enStatsScriptRef != null)
                {
                    // Incase the enemy difficulty was set wrong
                    if (enStatsScriptRef.GetDifficulty() <= 0)
                        currentDifficulty += 1;
                    else
                        currentDifficulty += enStatsScriptRef.GetDifficulty() + extraDiff;
                }
                else
                    currentDifficulty += 1;
                // Also increment the amount of enemies in the room
                ++currentAmEnemies;

                // Be careful of the inifinite
                if (++currentIterations > maxIterations)
                {
                    Debug.Log("WARNING - POTENTIAL BUG DETECTED - We tried to spawn enemies " + currentIterations + " times");
                    break;
                }

                //Debug.Log("After spawning " + spawnedEnemyObj.name + " in " + curRoomScript.name + " cur diff is now " +
                    //currentDifficulty + " out of " + curRoomScript.RoomDifficulty);
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
        for (int i = 0; i < _enemyPrefabs.Length; ++i)
        {
            // Get the current enemies information
            EnemyStats enemyDiffRef0 = _enemyPrefabs[i].GetComponent<EnemyStats>();
            int enemyDiff0 = enemyDiffRef0.GetDifficulty();
            // Assume this enemy is the weakest
            int weakestEnemyIndex = i;
            int weakestEnemyDiff = enemyDiff0;
            // Iterate over the enemies after the current to see if one is weaker
            for (int k = i + 1; k < _enemyPrefabs.Length; ++k)
            {
                // Get the next enemies information
                EnemyStats enemyDiffRef1 = _enemyPrefabs[k].GetComponent<EnemyStats>();
                int enemyDiff1 = enemyDiffRef1.GetDifficulty();

                // If we found an enemy weaker than the current enemy, make this enemy the new weakest
                if (enemyDiff1 < weakestEnemyDiff)
                {
                    weakestEnemyDiff = enemyDiff1;
                    weakestEnemyIndex = k;
                }
            }
            // Swap the enemies
            GameObject tempBucket = _enemyPrefabs[i];
            _enemyPrefabs[i] = _enemyPrefabs[weakestEnemyIndex];
            _enemyPrefabs[weakestEnemyIndex] = tempBucket;
        }
    }
}
