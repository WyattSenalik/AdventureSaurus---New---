using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistantController : MonoBehaviour
{
    // Temporary parent of the allies
    [SerializeField] private Transform tempAllyParent = null;
    // This is the scene that is tested for if it is the newly loaded scene
    [SerializeField] private Scene nextFloorScene;

    // List of allies, we will want to make these persistant throughout scenes
    private List<GameObject> allies;

    // The amount to tone down the difficulty for the next floor (1 sets the base difficulty of the next floor
    // to be the hardest difficulty of the last floor. 0 resets the difficulty back to 0)
    [SerializeField] private float difficultyToner = 0.75f;
    // The difficulty of the next floor
    private int nextFloorDiff;
    // The amount of rooms the next floor will have.
    // Starts out as the beginning number of floors.
    [SerializeField] private int nextFloorRoomAm = 5;
    // The amount of floors until the amount of rooms increases
    [SerializeField] private int floorsUntilRoomAmInc = 3;
    // The max amount of rooms
    [SerializeField] private int maxRooms = 9;
    // Whether the next floor should have a campfire
    private bool shouldHaveCamfire;
    // The amount of floors that must be progressed until a fireplace will spawn
    [SerializeField] private int floorsUntilFire = 2;
    // What floor we are on
    private int nextFloorNum;

    // Called before start
    private void Awake()
    {
        // Make sure we don't destroy this gameObject when we hop between floors
        DontDestroyOnLoad(this.gameObject);
        // Make sure we don't destroy the tempAllyParent unnecessarily
        DontDestroyOnLoad(tempAllyParent.gameObject);
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Initialize the list
        allies = new List<GameObject>();
        // Get the initial allies
        foreach (Transform character in tempAllyParent) {
            // Try to pull a move attack script off the character, they should have one
            MoveAttack charMARef = character.GetComponent<MoveAttack>();
            // See if the character is an ally, if they aren't we just want to test the next character
            if (charMARef == null || charMARef.WhatAmI != CharacterType.Ally)
                continue;
            // Add the ally to the list
            allies.Add(character.gameObject);
        }
        //Debug.Log("Finished initializing allies");
        // Initialize the starting floors stuff
        // Initialize the floor difficulty to 0
        nextFloorDiff = 0;
        shouldHaveCamfire = false;
        nextFloorNum = 1;
        // Start the first floor's generation
        StartGeneration();
    }

    /// <summary>
    /// Adds the given ally to the list and makes them persist between scenes
    /// </summary>
    /// <param name="allyToAdd"></param>
    private void AppendAlly(GameObject allyToAdd)
    {
        allies.Add(allyToAdd);
        DontDestroyOnLoad(allyToAdd);
    }

    /// <summary>
    /// Reinitializes all allies
    /// </summary>
    private void ReinitializeAllies()
    {
        //Debug.Log("ReinitializeAllies");
        // Reinitialize each ally
        foreach (GameObject ally in allies)
        {
            if (ally != null)
                ReinitializeAlly(ally);
        }
    }

    /// <summary>
    /// Reinitializes a single ally.
    /// Calls SetReferences and Initialize (when appropriate) on each script attached to the ally
    /// </summary>
    /// <param name="allyObj">Ally to reinitalize</param>
    private void ReinitializeAlly(GameObject allyObj)
    {
        // MoveAttack
        MoveAttack mARef = allyObj.GetComponent<MoveAttack>();
        if (mARef == null)
            Debug.Log(this.name + " did not have a MoveAttack script attached to it");
        // Set the foreign references and re initialize the MoveAttack script
        mARef.SetReferences();
        mARef.Initialize();

        // Health
        Health hpRef = allyObj.GetComponent<Health>();
        if (hpRef == null)
            Debug.Log(this.name + " did not have a Health script attached to it");
        // Set the foreign references and re initialize the Health script
        hpRef.SetReferences();
        hpRef.Initialize();

        // Stats
        Stats statsRef = allyObj.GetComponent<Stats>();
        if (statsRef == null)
            Debug.Log(this.name + " did not have a Stats script attached to it");
        // Set the foreign references of the Stats script. We do not re initalize here, since we have no need to
        statsRef.SetReferences();

        // Skills
        Skill skillRef = allyObj.GetComponent<Skill>();
        if (skillRef == null)
            Debug.Log(this.name + " did not have a Skill script attached to it");
        // Set the foreign references of the Skill script. We do not re initialize here, since we have no need to
        skillRef.SetReferences();

        // Make it so the ally can move again
        mARef.ResetMyTurn();
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
            Debug.Log("Could not find a gameobject with the tag GenerationController");
        // Get the procedural generation script attached to it
        ProceduralGenerationController genContScript = genCont.GetComponent<ProceduralGenerationController>();
        if (genContScript == null)
            Debug.Log("Could not a ProceduralGenerationController script attached to " + genCont.name);
        // Give the procedural generator the ally temp parent
        genContScript.AllyTempParent = tempAllyParent;
        // Set the difficulty of the floor
        genContScript.CurrentFloorDifficulty = nextFloorDiff;

        // Start the generation
        genContScript.GenerateFloor(shouldHaveCamfire, nextFloorRoomAm);

        // We need to set the allies references again
        ReinitializeAllies();

        //Debug.Log("Finished StartGeneration");
    }

    /// <summary>
    /// Prepares to go down another floor by moving enemies to the tempAllyHolder.
    /// Called from Yes button associated with the stairs "want to move on" prompt
    /// </summary>
    public void PrepareNextFloor()
    {
        // Make sure ally be a child of the tempAllyParent and move them to the origin
        foreach (GameObject allyObj in allies)
        {
            if (allyObj == null)
                continue;
            allyObj.transform.SetParent(tempAllyParent);
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
        nextFloorDiff = Mathf.RoundToInt(genContScript.GetMostDifficultFloor() * difficultyToner);

        // Update what floor this is
        ++nextFloorNum;

        // Test if we should increment the amount of rooms
        if (nextFloorRoomAm < maxRooms && nextFloorNum % floorsUntilRoomAmInc == 0)
            ++nextFloorRoomAm;

        // Test if the current floor should have a campfire
        shouldHaveCamfire = nextFloorNum % floorsUntilFire == 0;
    }
}
