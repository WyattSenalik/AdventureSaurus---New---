using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Stairs : MonoBehaviour
{
    // Name of scene to transition to
    [SerializeField] private string _sceneToGoTo = "Title Screen";
    // The prompt menu for if the player would like to go to the next floor
    [SerializeField] private GameObject _promptMenu = null;

    // List of players
    private List<Transform> _players;
    public List<Transform> Players
    {
        set { _players = value; }
    }
    // Transform of the stairs (to hold position)
    private Transform _stairsTrans;
    // If a character is currently on the stairs
    private bool _touchedOnce;
    // The transform of the character on the stairs
    private Transform _whoIsOn;

    // Events
    public delegate void PromptNextFloor();
    public static event PromptNextFloor OnPromptNextFloor;


    // Called when the component is toggled on
    // Subscribe to events
    private void OnEnable()
    {
        // Initialize this script after generation finishes
        ProceduralGenerationController.OnFinishGenerationNoParam += Initialize;
    }

    // Called when the component is toggled off
    // Unsubscribe from events
    private void OnDisable()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
    }

    // Called when the gameobject is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
    }

    // Called before start
    private void Awake()
    {
        // Initialize the array of players
        _players = new List<Transform>();
        _touchedOnce = false;
    }

    /// <summary>
    /// Initializes things for this script.
    /// Called from the FinishGeneratingNoParam event
    /// </summary>
    private void Initialize()
    {
        // Get the character parent
        Transform charParent = GameObject.Find(ProceduralGenerationController.charParentName).transform;

        // Initialize the array of players
        _players = new List<Transform>();
        // Iterate over the characters to get the allies
        foreach (Transform character in charParent)
        {
            MoveAttack mARef = character.GetComponent<MoveAttack>();
            if (mARef != null && mARef.WhatAmI == CharacterType.Ally)
            {
                _players.Add(character);
            }
        }

        // Set the stairs transform
        _stairsTrans = GameObject.Find(ProceduralGenerationController.stairsName).transform;
    }

    // Called once per frame
    private void Update()
    {
        touchStairs();
        // Iterate over the players
        foreach (Transform player in _players)
        { 
            // If it exists and 
            if (player != null && _whoIsOn == player)
            {
                if (player.transform.position != _stairsTrans.transform.position)
                {
                    _touchedOnce = false;
                }
            }
        }
        
    }

   /// <summary>
   /// Check if someone is one the stairs.
   /// </summary>
    private void touchStairs()
    {
        if (_touchedOnce == false)
        {
            foreach (Transform player in _players)
            {
                if (player != null && player.position == _stairsTrans.transform.position)
                {
                    prompt();
                    _touchedOnce = true;
                    _whoIsOn = player;
                    return;
                }
            }
        }
    }
    
    /// <summary>
    /// Prompt the user if they want to advance to the next floor.
    /// Suspends the game
    /// </summary>
    private void prompt()
    {
        // Show the prompt menu
        _promptMenu.SetActive(true);

        // Call the prompt next floor event
        // to suspend the game
        if (OnPromptNextFloor != null)
            OnPromptNextFloor();
    }

    /// <summary>
    /// Called from the yes button in the prompt.
    /// Changes the scene
    /// </summary>
    public void yes()
    {
        SceneManager.LoadScene(_sceneToGoTo);
    }

    /// <summary>
    /// Called from the no button in the prompt.
    /// Turns off the prompt menu and resumes the game
    /// </summary>
    public void no()
    {
        // Hide the prompt menu
        _promptMenu.SetActive(false);

        // Call the prompt next floor event
        // to resume the game
        if (OnPromptNextFloor != null)
            OnPromptNextFloor();
    }
    
}
