using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGeneration : MonoBehaviour
{
    // Spawning Rooms variables
    [SerializeField] private Vector2Int lowerLeftBound = Vector2Int.zero; // The farthest position down-left a room tile can exist
    [SerializeField] private Vector2Int upperRightBound = new Vector2Int(100, 100); // The farthest position up-right a room tile can exist
    [SerializeField] private Vector2Int minRoomSize = new Vector2Int(7, 7); // The smallest size a room can be
    [SerializeField] private Vector2Int maxRoomSize = new Vector2Int(20, 20); // The largest size a room can be
    [SerializeField] private int amountRoomsToSpawn = 5; // The amount of rooms to spawn
    [SerializeField] private int minSpaceBetweenRooms = 8; // The distance two rooms must be apart in any one direction
    [SerializeField] private GameObject roomPrefab = null; // The room prefab to spawn

    private Transform roomParent;

    // Start is called before the first frame update
    void Start()
    {
        // Make sure there is enough space to spawn the rooms
        int maxRoomWidth = maxRoomSize.x + minSpaceBetweenRooms * 2;
        int maxRoomHeight = maxRoomSize.y + minSpaceBetweenRooms * 2;
        int xDistAvail = upperRightBound.x - lowerLeftBound.x;
        int yDistAvail = upperRightBound.y - lowerLeftBound.y;
        int maxRoomCoverArea = maxRoomWidth * maxRoomHeight * amountRoomsToSpawn;
        int givenCoverArea = xDistAvail * yDistAvail;
        if (maxRoomCoverArea > givenCoverArea)
        {
            Debug.Log("That many rooms may not fit. It has the potential to cover " + maxRoomCoverArea
                + " square tiles. You only have " + givenCoverArea + " square tiles.");
            return;
        }

        SpawnNormalRooms();
    }

    /// <summary>
    /// Spawns and places normal rooms in valid locations such that no rooms will overlap
    /// </summary>
    private void SpawnNormalRooms()
    {
        int breakOutLimit = 10000000;

        // Create the rooms parent and center it
        roomParent = (new GameObject("RoomParent")).transform;
        roomParent.position = Vector3.zero;

        // Iterate to create the correct number of rooms
        for (int curAmountRooms = 0; curAmountRooms < amountRoomsToSpawn; ++curAmountRooms)
        {
            // Get the size of the current room
            int xSize = Random.Range(minRoomSize.x, maxRoomSize.x);
            int ySize = Random.Range(minRoomSize.y, maxRoomSize.y);
            Vector2Int curRoomSize = new Vector2Int(xSize, ySize);
            // Iterate until we find a valid position for the room
            Vector2Int potPos = Vector2Int.zero;
            int counter = 0;
            do
            {
                int xPos = Random.Range(lowerLeftBound.x, upperRightBound.x + 1);
                int yPos = Random.Range(lowerLeftBound.y, upperRightBound.y + 1);
                potPos = new Vector2Int(xPos, yPos);

                if (++counter > breakOutLimit)
                {
                    Debug.Log("Had to break");
                    break;
                }
            } while (!IsRoomValid(potPos, curRoomSize));
            // Once we break from the loop, we know the room is valid, so spawn it
            Transform newRoomTrans = Instantiate(roomPrefab, new Vector3(potPos.x, potPos.y), Quaternion.identity, roomParent).transform;
            newRoomTrans.localScale = new Vector3(curRoomSize.x, curRoomSize.y);
        }
    }

    /// <summary>
    /// Determines if a room made with the given parameters would overlap/be to close to another room.
    /// </summary>
    /// <param name="testPos">The position of the room</param>
    /// <param name="roomSize">The scale of the room</param>
    /// <returns>True if the room is valid</returns>
    private bool IsRoomValid(Vector2Int testPos, Vector2Int roomSize)
    {
        // Calculate the lower left and upper right positions of the room
        Vector2Int roomRadius = new Vector2Int(Mathf.RoundToInt(0.5f * roomSize.x), Mathf.RoundToInt(0.5f * roomSize.y));
        Vector2Int lowerLeftRoomPos = testPos - roomRadius;
        Vector2Int upperRightRoomPos = testPos + roomRadius;
        // Make sure the room is within the bounds specified
        if (lowerLeftRoomPos.x < lowerLeftBound.x || lowerLeftRoomPos.y < lowerLeftBound.y ||
            upperRightRoomPos.x > upperRightBound.x || upperRightRoomPos.y > upperRightBound.y)
        {
            return false;
        }
        // Iterate over each currently existing room
        foreach (Transform roomTrans in roomParent)
        {
            int xDist = Mathf.RoundToInt(Mathf.Abs(testPos.x - roomTrans.position.x));
            int yDist = Mathf.RoundToInt(Mathf.Abs(testPos.y - roomTrans.position.y));

            int xRadiiSum = Mathf.RoundToInt((roomSize.x + roomTrans.localScale.x) / 2f);
            int yRadiiSum = Mathf.RoundToInt((roomSize.y + roomTrans.localScale.y) / 2f);

            // If the current room is too close to the room being tested, return false
            if (xDist - xRadiiSum < minSpaceBetweenRooms && yDist - yRadiiSum < minSpaceBetweenRooms)
            {
                //Debug.Log("New room with size " + roomSize + " at " + testPos + " is too close to "
                //    + roomTrans.name + " at " + roomTrans.position + " with size " + roomTrans.localScale);
                return false;
            }
        }
        // If we made it here, the room is valid
        return true;
    }
}
