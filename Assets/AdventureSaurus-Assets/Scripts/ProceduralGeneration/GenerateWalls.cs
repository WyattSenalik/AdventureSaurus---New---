using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateWalls : MonoBehaviour
{
    [SerializeField] private GameObject wallPrefab = null; // The prefab for the wall
    // Reference to the room parent. Get from ProceduralGenerationController
    private Transform roomParent;
    public Transform RoomParent
    {
        set { roomParent = value; }
    }
    // The parent of all the transforms that will be spawned
    private Transform wallParent;

    /// <summary>
    /// Instantiates a bunch of empty transforms that mark where a wall is
    /// </summary>
    public void SpawnWallTransforms()
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
                lesserEnd = new Vector2Int(lesserStart.x, Mathf.RoundToInt(leftEdge.y + hallwayRadii.y - 0.5f - 1));
                greaterEnd = new Vector2Int(greaterStart.x, Mathf.RoundToInt(rightEdge.y + hallwayRadii.y - 0.5f - 1));
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
                lesserStart = new Vector2Int(Mathf.RoundToInt(botEdge.x + hallwayRadii.x + 0.5f + 1), Mathf.RoundToInt(botEdge.y + 0.5f));
                greaterStart = new Vector2Int(Mathf.RoundToInt(topEdge.x - hallwayRadii.x + 0.5f + 1), Mathf.RoundToInt(topEdge.y - 0.5f));
                // Where to stop iterating at
                lesserEnd = new Vector2Int(Mathf.RoundToInt(botEdge.x + hallwayRadii.x - 0.5f - 1), lesserStart.y);
                greaterEnd = new Vector2Int(Mathf.RoundToInt(topEdge.x + hallwayRadii.x - 0.5f - 1), greaterStart.y);
                // The amount to iterate by
                iterateVect = new Vector2Int(1, 0);
                // The extend amount to reach into the other room and and no walls there
                extendWallVect = new Vector2Int(1, 0);
            }

            // Be careful about infinite loops
            int maxIterate = 10000;
            int carefulCount = 0;
            // Iterate over the positions that we shouldn't have walls and add them to the list for the lesser list
            for (Vector2Int curVect = lesserStart; curVect != lesserEnd; curVect += iterateVect)
            {
                noWallPositions.Add(iterateVect);
                //Debug.Log(curVect);
                if (++carefulCount > maxIterate)
                {
                    Debug.Log("Infinite loop detected");
                    return;
                }
            }
            carefulCount = 0;
            // Iterate over the positions that we shouldn't have walls and add them to the list for the greater list
            for (Vector2Int curVect = greaterStart; curVect != greaterEnd; curVect += iterateVect)
            {
                noWallPositions.Add(iterateVect);
                if (++carefulCount > maxIterate)
                {
                    Debug.Log("Infinite loop detected");
                    return;
                }
            }
        }

        noWallPositions.Clear();
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

        /*
        // Iterate over the rooms adjacent to the current room to add positions to where we do not want walls
        foreach (Room adjRoom in curRoomScript.AdjacentRooms)
        {
            // Determine what side of the cur room this adjacent room is on
            RoomSide curRoomSide = RoomSide.TOP;
            // Get the distances
            float xDist = curHall.position.x - adjRoom.transform.position.x;
            float yDist = curRoom.position.y - adjRoom.transform.position.y;
            // If its closer in the y, that means we are either RIGHT or LEFT
            if (Mathf.Abs(xDist) > Mathf.Abs(yDist))
            {
                // If the xDist is positive, the adjacent room is left of the curRoom
                if (xDist > 0)
                    curRoomSide = RoomSide.LEFT;
                // If its negative, the adjacent room is right of the curRoom
                else
                    curRoomSide = RoomSide.RIGHT;
            }
            // If its closer in the x, that means we are either TOP or BOT
            else
            {
                // If the yDist is positive, the adjacent room is to the bot of the curRoom
                if (yDist > 0)
                    curRoomSide = RoomSide.BOT;
                // If its negative, the adjacent room is to the top of the curRoom
                else
                    curRoomSide = RoomSide.TOP;
            }


            Transform adjRoomTrans = adjRoom.transform; // Quick Ref
            Vector2 iterateStartPoint = Vector2.zero; // The start point to start iterating from
            Vector2 iterateEndPoint = Vector2.zero; // The end point to cease iterating from
            Vector2Int iterateDirection = Vector2Int.zero; // The amount to iterate by each iteration
            // Do some different calculations for where to start iterating from depening on where the curRoomSide is
            //Debug.Log(curRoom.name + " is " + curRoomSide + " " + adjRoom.name);
            switch (curRoomSide)
            {
                case RoomSide.LEFT:
                    // Calculate the start point
                    iterateStartPoint = new Vector2(adjRoomTrans.position.x + (adjRoomTrans.localScale.x - 1) / 2f + 1,
                        adjRoomTrans.position.y - (adjRoomTrans.localScale.y + 1) / 2f - 1);
                    // Calculate the end point (same for LEFT and RIGHT)
                    iterateEndPoint = new Vector2(iterateStartPoint.x, adjRoomTrans.position.y + (adjRoomTrans.localScale.y + 1) / 2f - 1);
                    // Determine the amount to iterate by (same for LEFT and RIGHT)
                    iterateDirection = new Vector2Int(0, 1);
                    break;
                case RoomSide.RIGHT:
                    // Calculate the start point
                    iterateStartPoint = new Vector2(adjRoomTrans.position.x - (adjRoomTrans.localScale.x + 1) / 2f - 1,
                        adjRoomTrans.position.y - (adjRoomTrans.localScale.y + 1) / 2f - 1);
                    // Calculate the end point (same for LEFT and RIGHT)
                    iterateEndPoint = new Vector2(iterateStartPoint.x, adjRoomTrans.position.y + (adjRoomTrans.localScale.y + 1) / 2f - 1);
                    // Determine the amount to iterate by (same for LEFT and RIGHT)
                    iterateDirection = new Vector2Int(0, 1);
                    break;
                case RoomSide.TOP:
                    // Calculate the start point
                    iterateStartPoint = new Vector2(adjRoomTrans.position.x - (adjRoomTrans.localScale.x + 1) / 2f - 1,
                        adjRoomTrans.position.y - (adjRoomTrans.localScale.y + 1) / 2f - 1);
                    // Calculate the end point (same for TOP and BOT)
                    iterateEndPoint = new Vector2(adjRoomTrans.position.x + (adjRoomTrans.localScale.x + 1) / 2f - 1, iterateStartPoint.y);
                    // Determine the amount to iterate by (same for TOP and BOT)
                    iterateDirection = new Vector2Int(1, 0);
                    break;
                case RoomSide.BOT:
                    // Calculate the start point
                    iterateStartPoint = new Vector2(adjRoomTrans.position.x - (adjRoomTrans.localScale.x + 1) / 2f - 1,
                        adjRoomTrans.position.y + (adjRoomTrans.localScale.y - 1) / 2f + 1);
                    // Calculate the end point (same for TOP and BOT)
                    iterateEndPoint = new Vector2(adjRoomTrans.position.x + (adjRoomTrans.localScale.x + 1) / 2f - 1, iterateStartPoint.y);
                    // Determine the amount to iterate by (same for TOP and BOT)
                    iterateDirection = new Vector2Int(1, 0);
                    break;
                default:
                    Debug.Log("TOP, BOT, RIGHT, and LEFT were all not chosen as where the adjRoom was to the curRoom");
                    break;
            }


            // Iterate until we reach the end iteration point, add each of the points along the way
            // These two are for infinite loop detection
            int maxIterate = 10000;
            int carefulCounter = 0;
            // The current point
            Vector2 curIteratePoint = new Vector2(iterateStartPoint.x, iterateStartPoint.y);
            while (curIteratePoint != iterateEndPoint)
            {
                // Add the points where we do not want a wall
                noWallPositions.Add(new Vector2Int(Mathf.RoundToInt(curIteratePoint.x), Mathf.RoundToInt(curIteratePoint.y)));

                // Iterate
                curIteratePoint += iterateDirection;
                // Inifinite loop detection
                if (++carefulCounter > maxIterate)
                {
                    Debug.Log("Infinite loop detected");
                    return;
                }
            }
            // Add the final point
            noWallPositions.Add(new Vector2Int(Mathf.RoundToInt(curIteratePoint.x), Mathf.RoundToInt(curIteratePoint.y)));

        }

        foreach (Vector2 wallPos in noWallPositions)
        {
            Debug.Log("No wall at " + wallPos);
        }

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

        }*/
    }
}
