using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProceduralGenerationController : MonoBehaviour
{
    // Reference to the Transform that will temporarily hold all allies
    private Transform allyTempParent = null;
    public Transform AllyTempParent
    {
        set { allyTempParent = value; }
        get { return allyTempParent; }
    }

    // In the case we don't want to generate
    [SerializeField] private bool shouldGenerate = true;
    [SerializeField] private Transform roomParent;
    [SerializeField] private Transform wallParent;
    [SerializeField] private Transform stairsTrans;
    private Tilemap tilemapRef;
    private Transform fireTrans;
    [SerializeField] private Transform charParent;

    // References for generation
    private GenerateRooms genRoomsRef;
    private GenerateWalls wallsGenRef;
    private GenerateStairs stairsGenRef;
    private GenerateTiles tilesGenRef;
    private GenerateSafeRoom safeRoomGenRef;
    private PlaceAllies placeAlliesRef;

    // References for initializing things
    private EnemyMoveAttackAI enMAAIRef;
    private MoveAttackController mAContRef;
    private TurnSystem turnSysRef;
    private Pause pauseRef;
    private PauseMenuController pauseMenuContRef;
    private CamFollow camFollowRef;
    private CharDetailedMenuController charDetMenuContRef;
    private Prompter promptRef;
    private Stairs stairsScriptRef;
    private MapCam mapCamRef;
    private DeathCheck deathCheckRef;

    // Set references
    private void Awake()
    {
        // Validation on Serialize Fields
        //if (allyTempParent == null)
            //Debug.Log("allyTempParent was not initialized correctly in ProceduralGenerationContoller attached to " + this.name);

        // Validation on other Procedural Gen Scripts
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
        safeRoomGenRef = procGenContObj.GetComponent<GenerateSafeRoom>();
        if (safeRoomGenRef == null)
            Debug.Log("There was no GenerateSafeRoom attached to " + procGenContObj.name);
        placeAlliesRef = procGenContObj.GetComponent<PlaceAllies>();
        if (placeAlliesRef == null)
            Debug.Log("There was no PlaceAllies attached to " + procGenContObj.name);

        // Validation on initialization scripts
        GameObject gameContObj = GameObject.FindWithTag("GameController");
        if (gameContObj == null)
            Debug.Log("Could not find any object with the tag GameController");
        enMAAIRef = gameContObj.GetComponent<EnemyMoveAttackAI>();
        if (enMAAIRef == null)
            Debug.Log("There was no EnemyMoveAttackAI attached to " + gameContObj.name);
        mAContRef = gameContObj.GetComponent<MoveAttackController>();
        if (mAContRef == null)
            Debug.Log("There was no MoveAttackController attached to " + gameContObj.name);
        turnSysRef = gameContObj.GetComponent<TurnSystem>();
        if (turnSysRef == null)
            Debug.Log("There was no TurnSystem attached to " + gameContObj.name);
        pauseRef = gameContObj.GetComponent<Pause>();
        if (pauseRef == null)
            Debug.Log("There was no Pause attached to " + gameContObj.name);
        pauseMenuContRef = gameContObj.GetComponent<PauseMenuController>();
        if (pauseMenuContRef == null)
            Debug.Log("There was no PauseMenuController attached to " + gameContObj.name);
        charDetMenuContRef = gameContObj.GetComponent<CharDetailedMenuController>();
        if (charDetMenuContRef == null)
            Debug.Log("There was no CharDetailedMenuController attached to " + gameContObj.name);
        promptRef = gameContObj.GetComponent<Prompter>();
        if (promptRef == null)
            Debug.Log("There was no Prompter attached to " + gameContObj.name);
        stairsScriptRef = gameContObj.GetComponent<Stairs>();
        if (stairsScriptRef == null)
            Debug.Log("There was no Stairs attached to " + gameContObj.name);
        deathCheckRef = gameContObj.GetComponent<DeathCheck>();
        if (deathCheckRef == null)
            Debug.Log("There was no DeathCheck attached to " + gameContObj.name);

        GameObject cameraObj = GameObject.FindWithTag("MainCamera");
        if (cameraObj == null)
            Debug.Log("Could not find any object with the tag MainCamera");
        camFollowRef = cameraObj.GetComponent<CamFollow>();
        if (camFollowRef == null)
            Debug.Log("There was no CamFollow attached to " + cameraObj.name);

        GameObject mapCamObj = GameObject.FindWithTag("MapCam");
        if (mapCamObj == null)
            Debug.Log("Could not find any object with the tag MapCam");
        mapCamRef = mapCamObj.GetComponent<MapCam>();
        if (mapCamRef == null)
            Debug.Log("There was no MapCam attached to " + mapCamObj.name);

    }

    /// <summary>
    /// Generates the floor using the various generation scripts.
    /// Called from persistant controller when the time is right
    /// </summary>
    public void GenerateFloor()
    {
        if (genRoomsRef != null && wallsGenRef != null && stairsGenRef != null && tilesGenRef != null
            && safeRoomGenRef != null && placeAlliesRef != null)
        {
            if (shouldGenerate)
            {
                // Spawn the rooms, hallways, and lights
                // Also get a reference to the parent of the rooms
                roomParent = genRoomsRef.SpawnHallwaysAndRooms();
                // Sort all the lights in the room so that they correspond to the correct adjacent room
                foreach (Transform curRoomTrans in roomParent)
                {
                    Room curRoomScript = curRoomTrans.GetComponent<Room>();
                    curRoomScript.SortAdjacentRooms();
                }

                // Spawn the wall transforms
                wallParent = wallsGenRef.SpawnWallTransforms(roomParent);

                // Sort those rooms quick, we have to do this after walls, since walls assumes the format is room, hallway, room, hallway, etc.
                // Everything after this assumes the rooms are sorted by weight
                if (!SortRooms(roomParent))
                    Debug.Log("Failed to sort");

                // Spawn the stairs in the last room
                stairsTrans = stairsGenRef.SpawnStairs(roomParent);

                // Create the tiles
                tilemapRef = tilesGenRef.SpawnTileMap(roomParent, wallParent, stairsTrans);

                // Make the safe room
                fireTrans = safeRoomGenRef.SpawnSafeRoom(roomParent, tilemapRef);

                // Create the Character parent
                charParent = new GameObject("CharacterParent").transform;
                charParent.position = Vector3.zero;
            }

            // Being extra careful
            int maxIterations = 100;
            int counter = 0;
            // Place the allies in the character parent
            while (allyTempParent.childCount != 0)
            {
                allyTempParent.GetChild(0).SetParent(charParent);
                // Avoid infinite loop
                if (++counter > maxIterations)
                {
                    Debug.Log("Forced a break");
                    break;
                }
            }
            // Place the allies randomly in the room only if we are supposed to generate
            if (shouldGenerate)
                placeAlliesRef.PutAlliesInStartRoom(roomParent, charParent);
            // Initialize scripts
            InitializeScripts();
        }
    }

    /// <summary>
    /// Initializes all the scripts that need to be initialized
    /// </summary>
    private void InitializeScripts()
    {
        // Initialize EnemyMoveAttackAI
        enMAAIRef.Initialize(charParent);
        // Initialize MoveAttackController
        mAContRef.Initialize(roomParent, wallParent, charParent);
        // Initialize TurnSystem
        turnSysRef.Initialize(charParent);
        // Initialize Pause
        pauseRef.Initialize(charParent);
        // Initalize PauseMenuController
        pauseMenuContRef.Initialize(charParent);
        // Initialize Prompt
        promptRef.Initialize(charParent);
        // Initialize CamFollow
        camFollowRef.Initialize(charParent);
        // Initialize CharDetailedMenuController
        charDetMenuContRef.Initialize(pauseMenuContRef.AlliesStats);
        // Initialize Stairs script
        stairsScriptRef.Initialize(charParent, stairsTrans, promptRef);
        // Initialize MapCam script
        mapCamRef.Initialize(mAContRef.GridTopLeft, mAContRef.GridBotRight);
        // Iniitlaize DeathCheck script
        deathCheckRef.Initialize(charParent);
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
