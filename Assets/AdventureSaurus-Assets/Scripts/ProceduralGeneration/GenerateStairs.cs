using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Now you may be wondering, did this really need to be its own script...      Who's to say?

public class GenerateStairs : MonoBehaviour
{
    [SerializeField] private GameObject stairsPrefab = null;

    // Called before start
    private void Awake()
    {
        if (stairsPrefab == null)
            Debug.Log("stairsPrefab was not set correctly in GenerateStairs attached to " + this.name);
    }

    /// <summary>
    /// Creates stairs somewhere in the last room
    /// </summary>
    /// <param name="roomParent">Parent of all the rooms</param>
    /// <returns>The transform of the stairs</returns>
    public Transform SpawnStairs(Transform roomParent)
    {
        // We assume the rooms are sorted by weight, so the last room is the room with the highest weight
        Transform lastRoom = roomParent.GetChild(roomParent.childCount - 1);
        // We also set that room as the last room here
        Room lastRoomScriptRef = lastRoom.GetComponent<Room>();
        lastRoomScriptRef.MyRoomType = RoomType.END;

        // We need to pick a spot in the room to spawn the stairs
        // That spot needs to be in the room, not on the walls
        // Determine the bounds for it
        Vector2Int botLeftPos = new Vector2Int(Mathf.RoundToInt(lastRoom.position.x - (lastRoom.localScale.x - 1) / 2f + 1),
            Mathf.RoundToInt(lastRoom.position.y - (lastRoom.localScale.y - 1) / 2f + 1));
        Vector2Int topRightPos = new Vector2Int(Mathf.RoundToInt(lastRoom.position.x + (lastRoom.localScale.x - 1) / 2f - 1),
            Mathf.RoundToInt(lastRoom.position.y + (lastRoom.localScale.y - 1) / 2f - 1));

        // Pick a random spot
        Vector2Int stairsPos = new Vector2Int(Random.Range(botLeftPos.x, topRightPos.x + 1), Random.Range(botLeftPos.y, topRightPos.y + 1));

        // Spawn the stairs there and return their transform
        GameObject stairsObj = Instantiate(stairsPrefab, new Vector3(stairsPos.x, stairsPos.y, 0), Quaternion.identity);
        stairsObj.name = "Stairs";
        return stairsObj.transform;
    }
}
