using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGenerationController : MonoBehaviour
{
    // References
    private GenerateRooms genRoomsRef = null;
    private GenerateWalls wallsGenRef = null;
    private GenerateStairs stairsGenRef = null;
    private GenerateTiles tilesGenRef = null;

    // Set references
    private void Awake()
    {
        GameObject procGenContObj = this.gameObject;

        genRoomsRef = procGenContObj.GetComponent<GenerateRooms>();
        if (genRoomsRef == null)
            Debug.Log("There was no GenerateRooms attached to " + procGenContObj.name);
        wallsGenRef = procGenContObj.GetComponent<GenerateWalls>();
        if (wallsGenRef == null)
            Debug.Log("There was no GenerateWalls attached to " + procGenContObj.name);
        stairsGenRef = procGenContObj.GetComponent<GenerateStairs>();
        if (stairsGenRef == null)
            Debug.Log("There was no GenerateStairs attached to " + procGenContObj.name);
        tilesGenRef = procGenContObj.GetComponent<GenerateTiles>();
        if (tilesGenRef == null)
            Debug.Log("There was no GenerateTiles attached to " + procGenContObj.name);
    }

    // Start is called before the first frame update
    private void Start()
    {
        if (genRoomsRef != null && wallsGenRef != null)
        {
            // Spawn the rooms, hallways, and lights
            // Also get a reference to the parent of the rooms
            Transform roomParent = genRoomsRef.SpawnHallwaysAndRooms();
            // Spawn the wall transforms
            Transform wallParent = wallsGenRef.SpawnWallTransforms(roomParent);
            // Sort those rooms quick, we have to do this after walls, since walls assumes the format is room, hallway, room, hallway, etc.
            if (!SortRooms(roomParent))
                Debug.Log("Failed to sort");
            // Spawn the stairs in the last room
            Transform stairsTrans = stairsGenRef.SpawnStairs(roomParent);
            // Create the tiles
            tilesGenRef.SpawnTileMap(roomParent, wallParent, stairsTrans);
        }
    }

    /// <summary>
    /// Sorts the children (rooms) of a Transform (the room parent) by their room weight
    /// </summary>
    /// <param name="roomsParent">The parent of the rooms</param>
    /// <returns>Returns whether the sorting was successful or not</returns>
    private bool SortRooms(Transform roomParent)
    {
        int maxIterations = roomParent.childCount * roomParent.childCount * 100;
        int infiniteLoopCounter = 0;
        // Iterate over the children of roomParent
        for (int i = 0; i < roomParent.childCount; ++i)
        {
            // Get the information about the current room we are iterating on
            Transform roomTrans = roomParent.GetChild(i);
            Room roomScriptRef = roomTrans.GetComponent<Room>();
            // Make sure it exists
            if (roomScriptRef == null)
            {
                Debug.Log(roomTrans.name + " had no Room script attached");
                return false;
            }

            // Set a reference to the heaviest room of this iteration
            Transform heaviestRoom = roomTrans;
            int heaviestWeight = roomScriptRef.RoomWeight;
            // Iterate over the other children of roomParent, to test if one is heavier
            for (int j = i + 1; j < roomParent.childCount; ++j)
            {
                // Get the information about the other room
                Transform otherRoomTrans = roomParent.GetChild(j);
                Room otherRoomScriptRef = otherRoomTrans.GetComponent<Room>();
                // Make sure it exists
                if (otherRoomScriptRef == null)
                {
                    Debug.Log(otherRoomTrans.name + " had no Room script attached");
                    return false;
                }

                // If the other room is heavier, make it the new heaviest room
                if (otherRoomScriptRef.RoomWeight > heaviestWeight)
                {
                    heaviestRoom = otherRoomTrans;
                    heaviestWeight = otherRoomScriptRef.RoomWeight;
                }

                // Caution infinite loop checker
                if (++infiniteLoopCounter > maxIterations)
                {
                    Debug.Log("Inifnite loop detected in SortRooms");
                    return false;
                }
            }
            // Set the heaviest room to be the first sibling, so that when we are done, the last child will be the heaviest (end room)
            // and the first will be the lightest (start room)
            heaviestRoom.SetAsFirstSibling();

            // Caution infinite loop checker
            if (++infiniteLoopCounter > maxIterations)
            {
                Debug.Log("Inifnite loop detected in SortRooms");
                return false;
            }
        }
        return true;
    }
}
