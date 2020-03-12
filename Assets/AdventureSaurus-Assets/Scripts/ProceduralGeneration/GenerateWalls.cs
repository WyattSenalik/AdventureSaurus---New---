using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateWalls : MonoBehaviour
{
    [SerializeField] private GameObject wallPrefab = null; // The prefab for the wall
    // The parent of all the transforms that will be spawned
    private Transform wallParent;

    /// <summary>
    /// Instantiates a bunch of empty transforms that mark where a wall is
    /// </summary>
    /// <param name="roomParent">The parent of all the rooms</param>
    /// <returns>Transform wallParent</returns>
    public Transform SpawnWallTransforms(Transform roomParent)
    {
        // Create the wall parent and center it
        wallParent = (new GameObject("WallParent")).transform;
        wallParent.position = Vector3.zero;

        // This list will hold positions where we do not want walls
        List<Vector2Int> noWallPositions = new List<Vector2Int>();
        // Iterate over the hallways to calculate where there should not be hallways
        for (int i = 1; i < roomParent.childCount; i += 2)
        {
            // Get the transform of the hallway
            Transform curHall = roomParent.GetChild(i);

            // These will be used to iterate over tiles to specify them to not be walls
            Vector2Int lesserStart = Vector2Int.zero;
            Vector2Int greaterStart = Vector2Int.zero;
            Vector2Int iterateVect = Vector2Int.zero;
            Vector2Int lesserEnd = Vector2Int.zero;
            Vector2Int greaterEnd = Vector2Int.zero;
            Vector2Int extendWallVect = Vector2Int.zero;
            // Radii of the hallway
            Vector2 hallwayRadii = new Vector2(curHall.localScale.x / 2f, curHall.localScale.y / 2f);

            // Determine if the hallway is larger either x scale wise or y scale wise
            // If the hallway has a larger width than height
            if (curHall.localScale.x > curHall.localScale.y)
            {
                // Calculate the left and right edges of the hallway
                Vector2 leftEdge = new Vector2(curHall.position.x - hallwayRadii.x, curHall.position.y);
                Vector2 rightEdge = new Vector2(curHall.position.x + hallwayRadii.x, curHall.position.y);
                // Where to start iterating from
                lesserStart = new Vector2Int(Mathf.RoundToInt(leftEdge.x + 0.5f), Mathf.RoundToInt(leftEdge.y - hallwayRadii.y + 0.5f + 1));
                greaterStart = new Vector2Int(Mathf.RoundToInt(rightEdge.x - 0.5f), Mathf.RoundToInt(rightEdge.y - hallwayRadii.y + 0.5f + 1));
                // Where to stop iterating at
                lesserEnd = new Vector2Int(lesserStart.x, Mathf.RoundToInt(leftEdge.y + hallwayRadii.y - 0.5f));
                greaterEnd = new Vector2Int(greaterStart.x, Mathf.RoundToInt(rightEdge.y + hallwayRadii.y - 0.5f));
                // The amount to iterate by
                iterateVect = new Vector2Int(0, 1);
                // The extend amount to reach into the other room and and no walls there
                extendWallVect = new Vector2Int(1, 0);
            }
            // If the hallway has a larger height than width
            else
            {
                // Calculate the left and right edges of the hallway
                Vector2 botEdge = new Vector2(curHall.position.x, curHall.position.y - hallwayRadii.y);
                Vector2 topEdge = new Vector2(curHall.position.x, curHall.position.y + hallwayRadii.y);
                // Where to start iterating from
                lesserStart = new Vector2Int(Mathf.RoundToInt(botEdge.x - hallwayRadii.x + 0.5f + 1), Mathf.RoundToInt(botEdge.y + 0.5f));
                greaterStart = new Vector2Int(Mathf.RoundToInt(topEdge.x - hallwayRadii.x + 0.5f + 1), Mathf.RoundToInt(topEdge.y - 0.5f));
                // Where to stop iterating at
                lesserEnd = new Vector2Int(Mathf.RoundToInt(botEdge.x + hallwayRadii.x - 0.5f), lesserStart.y);
                greaterEnd = new Vector2Int(Mathf.RoundToInt(topEdge.x + hallwayRadii.x - 0.5f), greaterStart.y);
                // The amount to iterate by
                iterateVect = new Vector2Int(1, 0);
                // The extend amount to reach into the other room and and no walls there
                extendWallVect = new Vector2Int(0, 1);
            }

            // Be careful about infinite loops
            int maxIterate = roomParent.childCount * 100000;
            int carefulCount = 0;
            // Iterate over the positions that we shouldn't have walls and add them to the list for the lesser list
            for (Vector2Int curVect = lesserStart; curVect != lesserEnd; curVect += iterateVect)
            {
                noWallPositions.Add(curVect);
                noWallPositions.Add(curVect - extendWallVect);
                // Caution test
                if (++carefulCount > maxIterate)
                {
                    Debug.Log("Infinite loop detected");
                    return wallParent;
                }
            }
            carefulCount = 0;
            // Iterate over the positions that we shouldn't have walls and add them to the list for the greater list
            for (Vector2Int curVect = greaterStart; curVect != greaterEnd; curVect += iterateVect)
            {
                noWallPositions.Add(curVect);
                noWallPositions.Add(curVect + extendWallVect);
                // Caution test
                if (++carefulCount > maxIterate)
                {
                    Debug.Log("Infinite loop detected");
                    return wallParent;
                }
            }
        }

        // Iterate over each room
        foreach (Transform curRoom in roomParent)
        {
            // Actual make the walls for this room
            Transform curRoomTrans = curRoom.transform;
            // Calculate the bottom left corner of the room
            Vector2Int curRoomBotLeft = new Vector2Int(Mathf.RoundToInt(curRoomTrans.position.x - ((curRoomTrans.localScale.x - 1) / 2f)),
                Mathf.RoundToInt(curRoomTrans.position.y - ((curRoomTrans.localScale.y - 1) / 2f)));
            // Calculate the top right corner of the room
            Vector2Int curRoomTopRight = new Vector2Int(Mathf.RoundToInt(curRoomTrans.position.x + ((curRoomTrans.localScale.x - 1) / 2f)),
                Mathf.RoundToInt(curRoomTrans.position.y + ((curRoomTrans.localScale.y - 1) / 2f)));
            // Iterate from left to right
            for (int i = curRoomBotLeft.x; i <= curRoomTopRight.x; ++i)
            {
                // Create a bottom wall, if it isn't a wall we should spawn, spawn it
                Vector2Int botWallPos = new Vector2Int(i, curRoomBotLeft.y);
                if (!noWallPositions.Contains(botWallPos))
                {
                    Transform botWallTrans = Instantiate(wallPrefab, new Vector3(botWallPos.x, botWallPos.y, 0), Quaternion.identity, wallParent).transform;
                    botWallTrans.name = "Wall " + botWallPos.x + " " + botWallPos.y;
                }
                // Create a top wall, if it isn't a wall we should spawn, spawn it
                Vector2Int topWallPos = new Vector2Int(i, curRoomTopRight.y);
                if (!noWallPositions.Contains(topWallPos))
                {
                    Transform topWallTrans = Instantiate(wallPrefab, new Vector3(topWallPos.x, topWallPos.y, 0), Quaternion.identity, wallParent).transform;
                    topWallTrans.name = "Wall " + topWallPos.x + " " + topWallPos.y;
                }
            }
            // Iterate from top to bottom
            for (int i = curRoomBotLeft.y + 1; i <= curRoomTopRight.y - 1; ++i)
            {
                // Create a bottom wall, if it isn't a wall we should spawn, spawn it
                Vector2Int leftWallPos = new Vector2Int(curRoomBotLeft.x, i);
                if (!noWallPositions.Contains(leftWallPos))
                {
                    Transform leftWallTrans = Instantiate(wallPrefab, new Vector3(leftWallPos.x, leftWallPos.y, 0), Quaternion.identity, wallParent).transform;
                    leftWallTrans.name = "Wall " + leftWallPos.x + " " + leftWallPos.y;
                }
                // Create a top wall, if it isn't a wall we should spawn, spawn it
                Vector2Int rightWallPos = new Vector2Int(curRoomTopRight.x, i);
                if (!noWallPositions.Contains(rightWallPos))
                {
                    Transform rightWallTrans = Instantiate(wallPrefab, new Vector3(rightWallPos.x, rightWallPos.y, 0), Quaternion.identity, wallParent).transform;
                    rightWallTrans.name = "Wall " + rightWallPos.x + " " + rightWallPos.y;
                }

            }
        }

        // Give the parent of all the walls
        return wallParent;
    }
}
