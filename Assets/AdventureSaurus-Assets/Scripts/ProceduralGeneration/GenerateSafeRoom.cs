using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GenerateSafeRoom : MonoBehaviour
{
    // The fire pit object to spawn
    [SerializeField] private GameObject _firePitPrefab = null;

    // Called before start
    private void Awake()
    {
        // Validation
        if (_firePitPrefab == null)
            Debug.Log("firePitPrefab was not set correctly in GenerateStairs attached to " + this.name);
    }

    public Transform SpawnSafeRoom(Transform roomParent, Tilemap tilemapRef, TileSet activeTilset)
    {
        // Get the safe room, the middle most room
        int midRoomIndex = roomParent.childCount / 2;
        int curRoomIndex = midRoomIndex - 1;
        short iterateAmount = 1;
        // References to the room being tested
        Transform safeRoom;
        Room safeRoomScriptRef;
        // Until we find a room that isn't a hallway, keep iterating
        do
        {
            curRoomIndex += iterateAmount;
            // If we've tried all the rooms while iterating up, start iterating down
            if (curRoomIndex >= roomParent.childCount)
            {
                curRoomIndex = midRoomIndex;
                iterateAmount = -1;
            }
            // Get the test room and set the safe room script ref
            safeRoom = roomParent.GetChild(curRoomIndex);
            safeRoomScriptRef = safeRoom.GetComponent<Room>();
            // Make sure it it exists
            if (safeRoomScriptRef == null)
            {
                Debug.Log(safeRoom.name + " had no Room script attached to it");
                return null;
            }
        } while (safeRoomScriptRef.MyRoomType != RoomType.NORMAL);
        // We should now have a room that is normal
        // Set it to safe
        safeRoomScriptRef.MyRoomType = RoomType.SAFE;


        // Calculate the middle of that room
        Vector3Int centerOfRoom = new Vector3Int(Mathf.RoundToInt(safeRoom.position.x), Mathf.RoundToInt(safeRoom.position.y), 0);
        // Place the fire tile on that position
        // Set the tile
        tilemapRef.SetTile(centerOfRoom, activeTilset.GetUnlitCampfire());


        // Create the fireplace obj at the middle of the room
        GameObject firePlaceObj = Instantiate(_firePitPrefab, centerOfRoom, Quaternion.identity);
        firePlaceObj.name = "Fireplace";
        // Set the active fire component of the campfire script
        Campfire campfireRef = firePlaceObj.GetComponent<Campfire>();
        if (campfireRef != null)
        {
            campfireRef.LitCampfire = activeTilset.GetLitCampfire();
            campfireRef.TilemapRef = tilemapRef;
        }
        else
            Debug.Log("No Campfire script attached to the spawned campfire");
        // Give the transform of the fireplace
        return firePlaceObj.transform;
    }
}
