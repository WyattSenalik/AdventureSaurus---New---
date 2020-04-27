using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class PersistantController : MonoBehaviour
{
    // Temporary parent of the allies
    [SerializeField] private Transform _tempAllyParent = null;

    // List of allies, we will want to make these persistant throughout scenes
    private List<GameObject> _allies;

    // The amount to tone down the difficulty for the next floor (1 sets the base difficulty of the next floor
    // to be the hardest difficulty of the last floor. 0 resets the difficulty back to 0)
    [SerializeField] private float _difficultyToner = 0.625f;
    // The difficulty of the next floor
    private int _nextFloorDiff;
    // The amount of rooms the next floor will have.
    // Starts out as the beginning number of floors.
    [SerializeField] private int _nextFloorRoomAm = 5;
    // The amount of floors until the amount of rooms increases
    [SerializeField] private int _floorsUntilRoomAmInc = 3;
    // The max amount of rooms
    [SerializeField] private int _maxRooms = 9;
    // Whether the next floor should have a campfire
    private bool _shouldHaveCamfire;
    // The amount of floors that must be progressed until a fireplace will spawn
    [SerializeField] private int _floorsUntilFire = 2;
    // What floor we are on
    private int _nextFloorNum;
    protected int GetNextFloorNum() { return _nextFloorNum; }

    // What floor is win
    [SerializeField] private int _winFloorNum = int.MaxValue;
    // Scene to load when win
    [SerializeField] private string _winSceneName = "WinScene";

    // What tile set we should use
    protected TileSet _activeTileSet;
    [SerializeField] protected TileSet[] _tileSets = null;

    /// <summary>
    /// Called when the script is set active.
    /// Subscribe to events.
    /// </summary>
    private void OnEnable()
    {
        // When the game is quit, get rid of the persistant objects
        PauseMenuController.OnQuitGame += PrepareForQuit;
    }

    /// <summary>
    /// Called when the script is toggled off.
    /// Unsubscribe from events.
    /// </summary>
    private void OnDisable()
    {
        PauseMenuController.OnQuitGame -= PrepareForQuit;
    }

    /// <summary>
    /// Called when the gameobject is destroyed.
    /// Unsubscribe from ALL events
    /// </summary>
    private void OnDestroy()
    {
        PauseMenuController.OnQuitGame -= PrepareForQuit;
    }

    // Called before start
    private void Awake()
    {
        // Make sure we don't destroy this gameObject when we hop between floors
        DontDestroyOnLoad(this.gameObject);
        // Make sure we don't destroy the tempAllyParent unnecessarily
        DontDestroyOnLoad(_tempAllyParent.gameObject);
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Initialize the list
        _allies = new List<GameObject>();
        // Get the initial allies
        foreach (Transform character in _tempAllyParent) {
            // Try to pull a move attack script off the character, they should have one
            MoveAttack charMARef = character.GetComponent<MoveAttack>();
            // See if the character is an ally, if they aren't we just want to test the next character
            if (charMARef == null || charMARef.WhatAmI != CharacterType.Ally)
                continue;
            // Add the ally to the list
            _allies.Add(character.gameObject);
        }
        //Debug.Log("Finished initializing allies");
        // Initialize the starting floors stuff
        // Initialize the floor difficulty to 0
        _nextFloorDiff = 0;
        _nextFloorNum = 1;
        _shouldHaveCamfire = _nextFloorNum % _floorsUntilFire == 0;
        // Start the first floor's generation
        StartGeneration();
    }

    /// <summary>
    /// Adds the given ally to the list and makes them persist between scenes
    /// </summary>
    /// <param name="allyToAdd"></param>
    private void AppendAlly(GameObject allyToAdd)
    {
        _allies.Add(allyToAdd);
        DontDestroyOnLoad(allyToAdd);
    }

    /// <summary>
    /// Starts the generation of the floor
    /// </summary>
    public void StartGeneration()
    {
        //Debug.Log("StartGeneration");
        // Get the procedural generation controller
        GameObject genCont = GameObject.FindWithTag("GenerationController");
        if (genCont == null)
            Debug.LogError("Could not find a gameobject with the tag GenerationController");

        // Get the procedural generation script attached to it
        ProceduralGenerationController genContScript = genCont.GetComponent<ProceduralGenerationController>();
        if (genContScript == null)
            Debug.LogError("Could not a ProceduralGenerationController script attached to " + genCont.name);

        // Set the active tileset
        GenerateTiles genTilesRef = genContScript.GetComponent<GenerateTiles>();
        if (genTilesRef == null)
            Debug.LogError("Could not a GenerateTiles script attached to " + genCont.name);
        // See if we should swap the tileset
        CheckSwapTileset();
        genTilesRef.SetActiveTileSet(_activeTileSet);

        // Set the difficulty of the floor
        genContScript.CurrentFloorDifficulty = _nextFloorDiff;

        // Start the generation
        genContScript.GenerateFloor(_shouldHaveCamfire, _nextFloorRoomAm, _tempAllyParent);
    }

    /// <summary>
    /// Prepares to go down another floor by moving enemies to the tempAllyHolder.
    /// Called from Yes button associated with the stairs "want to move on" prompt
    /// </summary>
    public void PrepareNextFloor()
    {
        // Make sure ally be a child of the tempAllyParent and move them to the origin
        foreach (GameObject allyObj in _allies)
        {
            if (allyObj == null)
                continue;
            //allyObj.transform.SetParent(_tempAllyParent);
            allyObj.transform.position = Vector3.zero;
        }


        // Calculate the stuff for the next floor
        // First we need the procedural gen controller script
        // Get the procedural generation controller
        GameObject genCont = GameObject.FindWithTag("GenerationController");
        if (genCont == null)
            Debug.Log("Could not find a gameobject with the tag GenerationController");
        // Get the procedural generation script attached to it
        ProceduralGenerationController genContScript = genCont.GetComponent<ProceduralGenerationController>();
        if (genContScript == null)
            Debug.Log("Could not a ProceduralGenerationController script attached to " + genCont.name);

        // Grab the difficulty of the last room of the last floor and base the difficulty for the next floor off it
        // Calculate the next floor's difficulty
        _nextFloorDiff = Mathf.RoundToInt(genContScript.GetMostDifficultFloor() * _difficultyToner);
        Debug.Log("Next floor diff: " + _nextFloorDiff);

        // Update what floor this is
        ++_nextFloorNum;

        // If the next floor is win, load the win scene
        if (_nextFloorNum == _winFloorNum)
        {
            SceneManager.LoadScene(_winSceneName);
        }

        // Test if we should increment the amount of rooms
        if (_nextFloorRoomAm < _maxRooms && _nextFloorNum % _floorsUntilRoomAmInc == 0)
            ++_nextFloorRoomAm;

        // Test if the current floor should have a campfire
        _shouldHaveCamfire = _nextFloorNum % _floorsUntilFire == 0;
    }

    /// <summary>
    /// Prepares to quit the game by destroying the persistant controller and the temp ally parent, so they
    /// do not persist into the main menu scene
    /// </summary>
    public void PrepareForQuit()
    {
        Destroy(_tempAllyParent.gameObject);
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Checks if we should swap the tileset to a different tileset
    /// </summary>
    protected abstract void CheckSwapTileset();
}
