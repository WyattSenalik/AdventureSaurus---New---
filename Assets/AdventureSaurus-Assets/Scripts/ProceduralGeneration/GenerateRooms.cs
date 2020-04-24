using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomSide {TOP, RIGHT, BOT, LEFT};

public class GenerateRooms : MonoBehaviour
{
    // Prefabs
    // The room prefab to spawn
    [SerializeField] private GameObject _roomPrefab = null;
    // The bleed light prefab to spawn between rooms
    [SerializeField] private GameObject _bleedLightPrefab = null;

    // Spawning Rooms variables
    // The farthest position down-left a room tile can exist
    [SerializeField] private Vector2Int _lowerLeftBound = new Vector2Int(int.MinValue, int.MinValue);
    // The farthest position up-right a room tile can exist
    [SerializeField] private Vector2Int _upperRightBound = new Vector2Int(int.MaxValue, int.MaxValue);
    // The smallest size a room can be
    [SerializeField] private Vector2Int _minRoomSize = new Vector2Int(6, 6);
    // The largest size a room can be
    [SerializeField] private Vector2Int _maxRoomSize = new Vector2Int(8, 8);
    // For these hallway sizes, width of the hallway is the amount of wall tiles it will get rid of to connect to a room
    // the length of the hallwa is the distance between the current room and the next room
    // The amount of walls this hallway breaks down to connect to a room
    [SerializeField] private Vector2Int _hallwayWidthRange = new Vector2Int(5, 5);
    // The amount of tiles between the prev room and the next room
    [SerializeField] private Vector2Int _hallwayLengthRange = new Vector2Int(6, 6);
    // The distance from the edge of the room a hallway must be
    [SerializeField] private int _hallwayOffsetFromEdge = 2;
    // The amount of rooms to spawn
    private int _amountRoomsToSpawn;
    public int AmountRoomsToSpawn
    {
        set { _amountRoomsToSpawn = value; }
    }
    // The distance two rooms must be apart in any one direction
    [SerializeField] private int _minSpaceBetweenRooms = 1;
    // The chance to not update what the previous room is, so that another room attached to the prevRoom
    [SerializeField] private int _chanceToSpawnBranch = 3;

    // The parent of all rooms and hallways
    private Transform _roomParent;
    // The parent of all bleed lights
    private Transform _bleedLightsParent;

    /// <summary>
    /// Spawns and places normal rooms in valid locations such that no rooms will overlap
    /// </summary>
    /// <returns>Transform roomParent</returns>
    public Transform SpawnHallwaysAndRooms()
    {
        // Make sure there is enough space to spawn the rooms
        int maxRoomWidth = _maxRoomSize.x + _minSpaceBetweenRooms * 2;
        int maxRoomHeight = _maxRoomSize.y + _minSpaceBetweenRooms * 2;
        int xDistAvail = _upperRightBound.x - _lowerLeftBound.x;
        int yDistAvail = _upperRightBound.y - _lowerLeftBound.y;
        int maxRoomCoverArea = maxRoomWidth * maxRoomHeight * _amountRoomsToSpawn;
        int givenCoverArea = xDistAvail * yDistAvail;
        if (maxRoomCoverArea > givenCoverArea)
        {
            Debug.Log("That many rooms may not fit. It has the potential to cover " + maxRoomCoverArea
                + " square tiles. You only have " + givenCoverArea + " square tiles.");
            //return;
        }

        // Just in case, to prevent an infinite loop and unity freezing
        // We define a limit to how many times we iterate to find a good spot for the room
        int breakOutLimit = _amountRoomsToSpawn * 100;

        // Create the rooms parent and center it
        _roomParent = (new GameObject(ProceduralGenerationController.ROOM_PARENT_NAME)).transform;
        _roomParent.position = Vector3.zero;

        // Create the lights parent and center it
        _bleedLightsParent = (new GameObject(ProceduralGenerationController.BLEEDLIGHT_PARENT_NAME)).transform;
        _bleedLightsParent.position = Vector3.zero;

        // Create the first room
        // Define its dimensions
        Vector2Int fRoomSize = Vector2Int.zero;
        fRoomSize.x = (Random.Range(_minRoomSize.x/2, _maxRoomSize.x/2 + 1) * 2) + 1;
        fRoomSize.y = (Random.Range(_minRoomSize.y/2, _maxRoomSize.y/2 + 1) * 2) + 1;
        // Determine a position for it (in the middle)
        Vector2Int fRoomPos = Vector2Int.zero;
        fRoomPos.x = (_lowerLeftBound.x + _upperRightBound.x) / 2;
        fRoomPos.y = (_lowerLeftBound.y + _upperRightBound.y) / 2;
        // Create the first room
        Transform fRoomTrans = Instantiate(_roomPrefab, new Vector3(fRoomPos.x, fRoomPos.y, 0), Quaternion.identity, _roomParent).transform;
        fRoomTrans.localScale = new Vector3(fRoomSize.x, fRoomSize.y, fRoomTrans.localScale.z);
        fRoomTrans.name = "Room 0";

        // Get its Room script and set its weight to 0, as well as make it not a hallway
        Room fRoomScriptRef = fRoomTrans.GetComponent<Room>();
        fRoomScriptRef.RoomWeight = 0;
        fRoomScriptRef.MyRoomType = RoomType.START;

        // Set the last room info to be that of the first room
        Vector2 prevRoomSize = fRoomSize;
        Vector2 prevRoomPos = fRoomPos;
        Transform prevRoomTrans = fRoomTrans;
        // Iterate to create the correct number of rooms
        int roomsSpawned = 1; // The counter
        int timesLooped = 0; // In case it gets stuck in an infinite loop
        while (roomsSpawned < _amountRoomsToSpawn)
        {
            // Determine a side to spawn the hallway on
            RoomSide hallwaySide = RoomSide.TOP + Random.Range(0, 4);
            // Choose the size of the hallway
            int hallwayWidth = Random.Range(_hallwayWidthRange.x, _hallwayWidthRange.y + 1);
            int hallwayLength = Random.Range(_hallwayLengthRange.x, _hallwayLengthRange.y + 1);
            Vector2Int hallwayScale = Vector2Int.one;
            // Choose the size of the new room
            Vector2Int newRoomSize = Vector2Int.zero;
            newRoomSize.x = (Random.Range(_minRoomSize.x / 2, _maxRoomSize.x / 2 + 1) * 2) + 1; ;
            newRoomSize.y = (Random.Range(_minRoomSize.y / 2, _maxRoomSize.y / 2 + 1) * 2) + 1; ;


            // The positions of the hallway and room
            Vector2 hallwayPos = Vector2.zero;
            Vector2 newRoomPos = Vector2.zero;
            // The positionf of the point where the room and hallway meet
            Vector2 hallPrevJoinPoint = Vector2.zero;
            Vector2 hallNewJoinPoint = Vector2.zero;
            // The rotation of the bleed light broadcasting from the prev room into the hallway
            float bleedLightRot = 0f;


            // In case the first side choice does not work, we will loop this until we find a side that works
            bool spawnSuccessful = false; // If the most recent spawn was successful
            int amountSidesTested = 0; // The amount of sides of the current room that have been tested
            int prevRoomIndex = _roomParent.childCount - 1; // The index of the previous room
            while (!spawnSuccessful)
            {
                // This information will be used in calculating the positions of the hallway and room
                // Holds the information about half the size of the hallway and room in both the x and y
                Vector2 hallwayRadius = new Vector2(hallwayWidth / 2f, hallwayLength / 2f);
                Vector2 prevRoomRadius = new Vector2(prevRoomSize.x / 2f, prevRoomSize.y / 2f);
                Vector2 newRoomRadius = new Vector2(newRoomSize.x / 2f, newRoomSize.y / 2f);
                int offsetPrev = 0; // The offest where to spawn the hallway in relation to the previous room
                int offsetNew = 0; // The offest where to spawn the hallway in relation to the new room
                // We now have to do things based on which side the hallway will spawn
                switch (hallwaySide)
                {
                    case RoomSide.TOP:
                        // Calculate offset with the previous room (TOP and BOT) are the same calculation
                        offsetPrev = Mathf.RoundToInt(Random.Range(-prevRoomRadius.x + hallwayRadius.x + _hallwayOffsetFromEdge, 
                            prevRoomRadius.x - hallwayRadius.x - _hallwayOffsetFromEdge));
                        // Calculate the hallway's position
                        hallwayPos = prevRoomPos + new Vector2(offsetPrev, prevRoomRadius.y + hallwayRadius.y);
                        // Calculate offeset with the new room (TOP and BOT) are same calculation
                        offsetNew = Mathf.RoundToInt(Random.Range(-newRoomRadius.x + hallwayRadius.x + _hallwayOffsetFromEdge, 
                            newRoomRadius.x - hallwayRadius.x - _hallwayOffsetFromEdge));
                        // Calculate the room's position
                        newRoomPos = hallwayPos + new Vector2(-offsetNew, newRoomRadius.y + hallwayRadius.y);
                        // Set the hallway's scale
                        hallwayScale = new Vector2Int(hallwayWidth, hallwayLength);

                        // Calculate the join location of the hallway and the previous room
                        hallPrevJoinPoint = hallwayPos + new Vector2(0, -hallwayRadius.y);
                        // Calculate the join location of the hallway and the new room
                        hallNewJoinPoint = hallwayPos + new Vector2(0, hallwayRadius.y);
                        // The angle of the bleed light for TOP (up)
                        bleedLightRot = 0f;
                        break;
                    case RoomSide.BOT:
                        // Calculate offset with the previous room (TOP and BOT) are the same calculation
                        offsetPrev = Mathf.RoundToInt(Random.Range(-prevRoomRadius.x + hallwayRadius.x + _hallwayOffsetFromEdge, prevRoomRadius.x - hallwayRadius.x - _hallwayOffsetFromEdge));
                        // Calculate the hallway's position
                        hallwayPos = prevRoomPos + new Vector2(offsetPrev, -prevRoomRadius.y - hallwayRadius.y);
                        // Calculate offeset with the new room (TOP and BOT) are same calculation
                        offsetNew = Mathf.RoundToInt(Random.Range(-newRoomRadius.x + hallwayRadius.x + _hallwayOffsetFromEdge, newRoomRadius.x - hallwayRadius.x - _hallwayOffsetFromEdge));
                        // Calculate the room's position
                        newRoomPos = hallwayPos + new Vector2(-offsetNew, -newRoomRadius.y - hallwayRadius.y);
                        // Set the hallway's scale
                        hallwayScale = new Vector2Int(hallwayWidth, hallwayLength);

                        // Calculate the join location of the hallway and the previous room
                        hallPrevJoinPoint = hallwayPos + new Vector2(0, hallwayRadius.y);
                        // Calculate the join location of the hallway and the new room
                        hallNewJoinPoint = hallwayPos + new Vector2(0, -hallwayRadius.y);
                        // The angle of the bleed light for BOT (down)
                        bleedLightRot = 180f;
                        break;
                    case RoomSide.RIGHT:
                        // Calculate offset with the previous room (RIGHT and LEFT) are the same calculaiton
                        offsetPrev = Mathf.RoundToInt(Random.Range(-prevRoomRadius.y + hallwayRadius.x + _hallwayOffsetFromEdge, prevRoomRadius.y - hallwayRadius.x - _hallwayOffsetFromEdge));
                        // Calculate the hallway's position
                        hallwayPos = prevRoomPos + new Vector2(prevRoomRadius.x + hallwayRadius.y, offsetPrev);
                        // Calculate offeset with the new room (RIGHT and LEFT) are same calculation
                        offsetNew = Mathf.RoundToInt(Random.Range(-newRoomRadius.y + hallwayRadius.x + _hallwayOffsetFromEdge, newRoomRadius.y - hallwayRadius.x - _hallwayOffsetFromEdge));
                        // Calculate the room's position
                        newRoomPos = hallwayPos + new Vector2(newRoomRadius.x + hallwayRadius.y, -offsetNew);
                        // Set the hallway's scale
                        hallwayScale = new Vector2Int(hallwayLength, hallwayWidth);

                        // Calculate the join location of the hallway and the previous room
                        hallPrevJoinPoint = hallwayPos + new Vector2(-hallwayRadius.y, 0);
                        // Calculate the join location of the hallway and the new room
                        hallNewJoinPoint = hallwayPos + new Vector2(hallwayRadius.y, 0);
                        // The angle of the bleed light for RIGHT (right)
                        bleedLightRot = 270f;
                        break;
                    case RoomSide.LEFT:
                        // Calculate offset with the previous room (RIGHT and LEFT) are the same calculaiton
                        offsetPrev = Mathf.RoundToInt(Random.Range(-prevRoomRadius.y + hallwayRadius.x + _hallwayOffsetFromEdge, prevRoomRadius.y - hallwayRadius.x - _hallwayOffsetFromEdge));
                        // Calculate the hallway's position
                        hallwayPos = prevRoomPos + new Vector2(-prevRoomRadius.x - hallwayRadius.y, offsetPrev);
                        // Calculate offeset with the new room (RIGHT and LEFT) are same calculation
                        offsetNew = Mathf.RoundToInt(Random.Range(-newRoomRadius.y + hallwayRadius.x + _hallwayOffsetFromEdge, newRoomRadius.y - hallwayRadius.x - _hallwayOffsetFromEdge));
                        // Calculate the room's position
                        newRoomPos = hallwayPos + new Vector2(-newRoomRadius.x - hallwayRadius.y, -offsetNew);
                        // Set the hallway's scale
                        hallwayScale = new Vector2Int(hallwayLength, hallwayWidth);

                        // Calculate the join location of the hallway and the previous room
                        hallPrevJoinPoint = hallwayPos + new Vector2(hallwayRadius.y, 0);
                        // Calculate the join location of the hallway and the new room
                        hallNewJoinPoint = hallwayPos + new Vector2(-hallwayRadius.y, 0);
                        // The angle of the bleed light for LEFT (left)
                        bleedLightRot = 90f;
                        break;
                    default:
                        Debug.Log("TOP, BOT, RIGHT, and LEFT were all not chosen as the direction to spawn the hallway at.");
                        break;
                }

                // If the room is valid, we can spawn it
                if (IsRoomValid(newRoomPos, newRoomSize))
                {
                    // Create the hallway and the room
                    Transform hallwayTrans = Instantiate(_roomPrefab, new Vector3(hallwayPos.x, hallwayPos.y, 0), Quaternion.identity, _roomParent).transform;
                    hallwayTrans.localScale = new Vector3(hallwayScale.x, hallwayScale.y, hallwayTrans.localScale.z);
                    Transform newRoomTrans = Instantiate(_roomPrefab, new Vector3(newRoomPos.x, newRoomPos.y, 0), Quaternion.identity, _roomParent).transform;
                    newRoomTrans.localScale = new Vector3(newRoomSize.x, newRoomSize.y, newRoomTrans.localScale.z);

                    // Make the new room be adjacent to the hallway,
                    // the hallway adjacent to the new room and the previous rooom,
                    // and the previous room adjacent to the hallway
                    // First get references
                    Room newRoomScriptRef = newRoomTrans.GetComponent<Room>();
                    Room hallwayScriptRef = hallwayTrans.GetComponent<Room>();
                    Room prevRoomScriptRef = prevRoomTrans.GetComponent<Room>();
                    // Now add the adj rooms
                    newRoomScriptRef.AdjacentRooms.Add(hallwayScriptRef);
                    hallwayScriptRef.AdjacentRooms.Add(newRoomScriptRef);
                    hallwayScriptRef.AdjacentRooms.Add(prevRoomScriptRef);
                    prevRoomScriptRef.AdjacentRooms.Add(hallwayScriptRef);
                    // Update the weights of each room to be 1 plus their previous room
                    hallwayScriptRef.RoomWeight = prevRoomScriptRef.RoomWeight + 1;
                    newRoomScriptRef.RoomWeight = hallwayScriptRef.RoomWeight + 1;
                    // Set the new hallway to be a hallway and the new room to not be a hallway
                    hallwayScriptRef.MyRoomType = RoomType.HALLWAY;
                    newRoomScriptRef.MyRoomType = RoomType.NORMAL;

                    ///// Spawn the lights at the join locations with the proper angles
                    //// For between the prev room and hallway
                    
                    /// From the prev room to the hallway
                    // Spawn BleedLight prefab
                    GameObject hallFromPrevBroadcastObj = Instantiate(_bleedLightPrefab, hallPrevJoinPoint, 
                                                                      Quaternion.Euler(0, 0, bleedLightRot), _bleedLightsParent);
                    Vector3 tempPos = hallFromPrevBroadcastObj.transform.position;
                    tempPos.z = -1;
                    hallFromPrevBroadcastObj.transform.position = tempPos;
                    // Get a reference to the BleedLight script attached to that object
                    BleedLight hallFromPrevBroadcastBleedLight = hallFromPrevBroadcastObj.GetComponent<BleedLight>();
                    // Add the BleedLight to the appropriate broadcast and receive lists
                    prevRoomScriptRef.BroadcastLights.Add(hallFromPrevBroadcastBleedLight);
                    hallwayScriptRef.ReceiveLights.Add(hallFromPrevBroadcastBleedLight);
                    // Give the BleedLight references to its corresponding Broadcast and Receive Rooms
                    hallFromPrevBroadcastBleedLight.BroadcastRoom = prevRoomScriptRef;
                    hallFromPrevBroadcastBleedLight.ReceiveRoom = hallwayScriptRef;

                    /// From the hallway to the previous room
                    // Spawn BleedLight prefab
                    GameObject hallFromPrevReceiveObj = Instantiate(_bleedLightPrefab, hallPrevJoinPoint,
                                                                    Quaternion.Euler(0, 0, 180 + bleedLightRot), _bleedLightsParent);
                    tempPos = hallFromPrevReceiveObj.transform.position;
                    tempPos.z = -1;
                    hallFromPrevReceiveObj.transform.position = tempPos;
                    // Get a reference to the BleedLight script attached to that object
                    BleedLight hallFromPrevReceiveBleedLight = hallFromPrevReceiveObj.GetComponent<BleedLight>();
                    // Add the BleedLight to the appropriate broadcast and receive lists
                    hallwayScriptRef.BroadcastLights.Add(hallFromPrevReceiveBleedLight);
                    prevRoomScriptRef.ReceiveLights.Add(hallFromPrevReceiveBleedLight);
                    // Give the BleedLight references to its corresponding Broadcast and Receive Rooms
                    hallFromPrevReceiveBleedLight.BroadcastRoom = hallwayScriptRef;
                    hallFromPrevReceiveBleedLight.ReceiveRoom = prevRoomScriptRef;

                    //// For between the new room and hallway
                    /// From the new room to the hallway
                    // Spawn BleedLight prefab
                    GameObject hallFromNewBroadcastObj = Instantiate(_bleedLightPrefab, hallNewJoinPoint,
                                                                     Quaternion.Euler(0, 0, 180 + bleedLightRot), _bleedLightsParent);
                    tempPos = hallFromNewBroadcastObj.transform.position;
                    tempPos.z = -1;
                    hallFromNewBroadcastObj.transform.position = tempPos;
                    // Get a reference to the BleedLight script attached to that object
                    BleedLight hallFromNewBroadcastBleedLight = hallFromNewBroadcastObj.GetComponent<BleedLight>();
                    // Add the BleedLight to the appropriate broadcast and receive lists
                    newRoomScriptRef.BroadcastLights.Add(hallFromNewBroadcastBleedLight);
                    hallwayScriptRef.ReceiveLights.Add(hallFromNewBroadcastBleedLight);
                    // Give the BleedLight references to its corresponding Broadcast and Receive Rooms
                    hallFromNewBroadcastBleedLight.BroadcastRoom = newRoomScriptRef;
                    hallFromNewBroadcastBleedLight.ReceiveRoom = hallwayScriptRef;

                    /// From the hallway to the new room
                    // Spawn BleedLight prefab
                    GameObject hallFromNewReceiveObj = Instantiate(_bleedLightPrefab, hallNewJoinPoint,
                                                                   Quaternion.Euler(0, 0, bleedLightRot), _bleedLightsParent);
                    tempPos = hallFromNewReceiveObj.transform.position;
                    tempPos.z = -1;
                    hallFromNewReceiveObj.transform.position = tempPos;
                    // Get a reference to the BleedLight script attached to that object
                    BleedLight hallFromNewReceiveBleedLight = hallFromNewReceiveObj.GetComponent<BleedLight>();
                    // Add the BleedLight to the appropriate broadcast and receive lists
                    hallwayScriptRef.BroadcastLights.Add(hallFromNewReceiveBleedLight);
                    newRoomScriptRef.ReceiveLights.Add(hallFromNewReceiveBleedLight);
                    // Give the BleedLight references to its corresponding Broadcast and Receive Rooms
                    hallFromNewReceiveBleedLight.BroadcastRoom = hallwayScriptRef;
                    hallFromNewReceiveBleedLight.ReceiveRoom = newRoomScriptRef;


                    // Give the hallway and room a name
                    hallwayTrans.name = "Hallway " + (roomsSpawned - 1).ToString();
                    newRoomTrans.name = "Room " + roomsSpawned.ToString();


                    // Make the chance not to update the prevRoom to instead create a branch
                    // If the room is not a branch
                    if (Random.Range(0, _chanceToSpawnBranch + 12 / _amountRoomsToSpawn ) != 0)
                    {
                        // Make the prevRoom the room
                        prevRoomPos = newRoomPos;
                        prevRoomSize = newRoomSize;
                        prevRoomTrans = newRoomTrans;
                    }
                    // If the room is a branch
                    else
                        newRoomTrans.name += " Branch";


                    // Increment
                    ++roomsSpawned;
                    // Say that the spawn was successful
                    spawnSuccessful = true;
                }
                // If the room isn't valid and we have not yet checked all sides of the prevRoom, pick a new side and test again
                else if (amountSidesTested < 4)
                {
                    // If the room is not valid, we need to choose the next side
                    hallwaySide = RoomSide.TOP + ((int)hallwaySide + 1) % 4;
                    ++amountSidesTested;
                    spawnSuccessful = false;
                }
                // If the room isn't valid and we have already checked all the sides, we want to choose a different room to be the current room
                else if (amountSidesTested >= 4)
                {
                    // We want the normal room that was before the last normal room, this means the room that is 2 behind the current prevRoom
                    // since we spawn an initial room and then hallway, room, hallway, etc.
                    prevRoomIndex -= 2;
                    // If the new index is invalid, we have gone through all rooms possible, 
                    // so it is invalid to spawn any other rooms, so stop creating rooms entirely
                    if (prevRoomIndex < 0)
                    {
                        Debug.Log("Cannot create anymore rooms");
                        return _roomParent;
                    }
                    // Otherwise, the room is valid, so we can set the prevRoom info to that room
                    else
                    {
                        // Set the prevRoom info
                        prevRoomTrans = _roomParent.GetChild(prevRoomIndex);
                        prevRoomPos = prevRoomTrans.position;
                        prevRoomSize = prevRoomTrans.localScale;
                        // Also reset how many iterations of amountSidesTested we had
                        amountSidesTested = 0;
                    }
                }
                else
                {
                    Debug.Log("WARNING - BUG DETECTED. THIS LINE IS NEVER SUPPOSED TO BE EXECUTED.");
                }

                if (++timesLooped > breakOutLimit)
                {
                    Debug.Log("Too many iterations, had to break");
                    break;
                }
            }
        }

        // Return the parent of the rooms
        return _roomParent;
    }

    /// <summary>
    /// Determines if a room made with the given parameters would overlap/be to close to another room.
    /// </summary>
    /// <param name="testPos">The position of the room</param>
    /// <param name="roomSize">The scale of the room</param>
    /// <returns>True if the room is valid</returns>
    private bool IsRoomValid(Vector2 testPos, Vector2Int roomSize)
    {
        // Calculate the lower left and upper right positions of the room
        Vector2 roomRadius = new Vector2(0.5f * roomSize.x, 0.5f * roomSize.y);
        Vector2 lowerLeftRoomPos = testPos - roomRadius;
        Vector2 upperRightRoomPos = testPos + roomRadius;
        // Make sure the room is within the bounds specified
        if (lowerLeftRoomPos.x < _lowerLeftBound.x || lowerLeftRoomPos.y < _lowerLeftBound.y ||
            upperRightRoomPos.x > _upperRightBound.x || upperRightRoomPos.y > _upperRightBound.y)
        {
            return false;
        }
        // Iterate over each currently existing room
        foreach (Transform roomTrans in _roomParent)
        {
            float xDist = Mathf.Abs(testPos.x - roomTrans.position.x);
            float yDist = Mathf.Abs(testPos.y - roomTrans.position.y);

            float xRadiiSum = (roomSize.x + roomTrans.localScale.x) / 2f;
            float yRadiiSum = (roomSize.y + roomTrans.localScale.y) / 2f;

            // If the current room is too close to the room being tested, return false
            if (xDist - xRadiiSum < _minSpaceBetweenRooms && yDist - yRadiiSum < _minSpaceBetweenRooms)
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
