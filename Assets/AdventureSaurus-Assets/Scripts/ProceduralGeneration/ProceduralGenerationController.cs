using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGenerationController : MonoBehaviour
{
    // References
    private GenerateRooms genRoomsRef = null;
    private GenerateWalls wallsGenRef = null;

    private void Awake()
    {
        GameObject procGenContObj = this.gameObject;

        genRoomsRef = procGenContObj.GetComponent<GenerateRooms>();
        if (genRoomsRef == null)
            Debug.Log("There was no GenerateRooms attached to " + procGenContObj.name);
        wallsGenRef = procGenContObj.GetComponent<GenerateWalls>();
        if (wallsGenRef == null)
            Debug.Log("There was no GenerateWalls attached to " + procGenContObj.name);
    }

    // Start is called before the first frame update
    private void Start()
    {
        if (genRoomsRef != null && wallsGenRef != null)
        {
            // Spawn the rooms, hallways, and lights
            genRoomsRef.SpawnHallwaysAndRooms();
            // Give the walls generator the parent of the rooms
            wallsGenRef.RoomParent = genRoomsRef.RoomParent;
            // Spawn the wall transforms
            wallsGenRef.SpawnWallTransforms();
        }
    }
}
