using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProceduralGenerationController : MonoBehaviour
{
    // Reference to the Transform that will temporarily hold all allies
    private Transform _allyTempParent = null;
    public Transform AllyTempParent
    {
        set { _allyTempParent = value; }
        get { return _allyTempParent; }
    }

    // The current floor's base difficulty.
    // TODO set from persistant controller
    private int _curFloorDiff;
    public int CurrentFloorDifficulty
    {
        set { _curFloorDiff = value; }
    }

    // In the case we don't want to generate
    [SerializeField] private bool _shouldGenerate = true;
    [SerializeField] private Transform _roomParent;
    [SerializeField] private Transform _wallParent;
    [SerializeField] private Transform _stairsTrans;
    private Tilemap _tilemapRef;
    private Transform _fireTrans;
    [SerializeField] private Transform _charParent;

    // References for generation
    private GenerateRooms _genRoomsRef;
    private GenerateWalls _wallsGenRef;
    private GenerateStairs _stairsGenRef;
    private GenerateTiles _tilesGenRef;
    private GenerateSafeRoom _safeRoomGenRef;
    private GenerateEnemies _enemiesGenRef;
    private PlaceAllies _placeAlliesRef;

    // Events
    // When we finsh generating the floor
    // For when we need to initialize scripts after generating
    public delegate void FinishGeneration(Transform charParent, Transform roomParent, Transform wallParent, Transform stairsParent);
    public static event FinishGeneration OnFinishGeneration;
    public delegate void FinishGenerationNoParam();
    public static event FinishGenerationNoParam OnFinishGenerationNoParam;


    // Set references
    private void Awake()
    {
        // Validation on Serialize Fields
        //if (allyTempParent == null)
            //Debug.Log("allyTempParent was not initialized correctly in ProceduralGenerationContoller attached to " + this.name);

        // Validation on other Procedural Gen Scripts
        GameObject procGenContObj = this.gameObject;
        _genRoomsRef = procGenContObj.GetComponent<GenerateRooms>();
        if (_genRoomsRef == null)
            Debug.Log("There was no GenerateRooms attached to " + procGenContObj.name);
        _wallsGenRef = procGenContObj.GetComponent<GenerateWalls>();
        if (_wallsGenRef == null)
            Debug.Log("There was no GenerateWalls attached to " + procGenContObj.name);
        _stairsGenRef = procGenContObj.GetComponent<GenerateStairs>();
        if (_stairsGenRef == null)
            Debug.Log("There was no GenerateStairs attached to " + procGenContObj.name);
        _tilesGenRef = procGenContObj.GetComponent<GenerateTiles>();
        if (_tilesGenRef == null)
            Debug.Log("There was no GenerateTiles attached to " + procGenContObj.name);
        _safeRoomGenRef = procGenContObj.GetComponent<GenerateSafeRoom>();
        if (_safeRoomGenRef == null)
            Debug.Log("There was no GenerateSafeRoom attached to " + procGenContObj.name);
        _placeAlliesRef = procGenContObj.GetComponent<PlaceAllies>();
        if (_placeAlliesRef == null)
            Debug.Log("There was no PlaceAllies attached to " + procGenContObj.name);
        _enemiesGenRef = procGenContObj.GetComponent<GenerateEnemies>();
        if (_enemiesGenRef == null)
            Debug.Log("There was no GenerateEnemies attached to " + procGenContObj.name);
    }

    /// <summary>
    /// Generates the floor using the various generation scripts.
    /// Called from persistant controller when the time is right
    /// </summary>
    /// <param name="spawnFire">If we should put a safe room on this floor</param>
    /// <param name="amountRooms">The amount of rooms to spawn</param>
    public void GenerateFloor(bool spawnFire, int amountRooms)
    {
        //Debug.Log("GenerateFloor");
        if (_genRoomsRef != null && _wallsGenRef != null && _stairsGenRef != null && _tilesGenRef != null
            && _safeRoomGenRef != null && _placeAlliesRef != null)
        {
            if (_shouldGenerate)
            {
                //Debug.Log("SpawnHallwaysAndRooms");
                // Spawn the rooms, hallways, and lights
                // Also get a reference to the parent of the rooms
                _genRoomsRef.AmountRoomsToSpawn = amountRooms;
                _roomParent = _genRoomsRef.SpawnHallwaysAndRooms();
                // Sort all the lights in the room so that they correspond to the correct adjacent room
                foreach (Transform curRoomTrans in _roomParent)
                {
                    Room curRoomScript = curRoomTrans.GetComponent<Room>();
                    curRoomScript.SortAdjacentRooms();
                }

                //Debug.Log("SpawnWallTransforms");
                // Spawn the wall transforms
                _wallParent = _wallsGenRef.SpawnWallTransforms(_roomParent);

                //Debug.Log("SortRooms");
                // Sort those rooms quick, we have to do this after walls, since walls assumes the format is room, hallway, room, hallway, etc.
                // Everything after this assumes the rooms are sorted by weight
                if (!SortRooms(_roomParent))
                    Debug.Log("Failed to sort");

                //Debug.Log("SpawnStairs");
                // Spawn the stairs in the last room
                _stairsTrans = _stairsGenRef.SpawnStairs(_roomParent);

                //Debug.Log("SpawnTileMap");
                // Create the tiles
                _tilemapRef = _tilesGenRef.SpawnTileMap(_roomParent, _wallParent, _stairsTrans);

                //Debug.Log("SpawnSafeRoom");
                // Make the safe room if we should have one
                if (spawnFire)
                {
                    _fireTrans = _safeRoomGenRef.SpawnSafeRoom(_roomParent, _tilemapRef);
                }

                // Create the Character parent
                _charParent = new GameObject("CharacterParent").transform;
                _charParent.position = Vector3.zero;

                //Debug.Log("PlaceAllies");
                // Place the allies randomly in the start room
                _placeAlliesRef.PutAlliesInStartRoom(_roomParent, _allyTempParent);

                //Debug.Log("SpawnEnemies");
                // Spawn the enmies
                _enemiesGenRef.SpawnEnemies(_charParent, _roomParent, _curFloorDiff);
            }

            // Being extra careful
            int maxIterations = 100;
            int counter = 0;
            // Place the allies in the character parent
            while (_allyTempParent.childCount != 0)
            {
                _allyTempParent.GetChild(0).SetParent(_charParent);
                // Avoid infinite loop
                if (++counter > maxIterations)
                {
                    Debug.Log("Forced a break");
                    break;
                }
            }

            // Call the OnFinishGeneration event
            if (OnFinishGeneration != null)
                OnFinishGeneration(_charParent, _roomParent, _wallParent, _stairsTrans);
            if (OnFinishGenerationNoParam != null)
                OnFinishGenerationNoParam();
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

    /// <summary>
    /// Returns the difficulty of the most difficult floor.
    /// Called from Persistant Controller when preparing the next floor.
    /// </summary>
    /// <returns>int difficulty of the Floor with the hardest difficulty</returns>
    public int GetMostDifficultFloor()
    {
        Transform hardRoomTrans = _roomParent.GetChild(_roomParent.childCount - 1);
        Room hardRoomScriptRef = hardRoomTrans.GetComponent<Room>();
        return hardRoomScriptRef.RoomDifficulty;
    }
}
