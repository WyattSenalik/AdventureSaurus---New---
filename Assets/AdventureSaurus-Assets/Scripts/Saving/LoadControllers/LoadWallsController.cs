using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LoadWallsController
{
    /// <summary>
    /// Creates wall objects based on the stored data
    /// </summary>
    /// <param name="wallsParent">Transform that will serve as the parent for the new walls</param>
    /// <param name="wallPrefab">Prefab of a wall</param>
    public static void LoadWalls(Transform wallsParent, GameObject wallPrefab)
    {
        // Get the amount of walls
        ChildAmountData wallAmData = SaveSystem.LoadWallAmount();
        int amountWalls = wallAmData.GetChildAmount();

        // Load each wall
        for (int i = 0; i < amountWalls; ++i)
        {
            // Get the data for the wall
            WallData wallData = SaveSystem.LoadWall(i);
            // Create the new wall as a child of the wall parent
            GameObject wallObj = Object.Instantiate(wallPrefab, wallsParent);

            // Set its transform components
            wallObj.transform.position = wallData.GetPosition();
        }
    }
}
