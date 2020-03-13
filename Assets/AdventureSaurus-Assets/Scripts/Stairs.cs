using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Stairs : MonoBehaviour
{
    // Name of scene to transition to
    [SerializeField] private string sceneToGoTo = "Title Screen";

    // List of players
    private List<Transform> players;
    public List<Transform> Players
    {
        set { players = value; }
    }
    // Transform of the stairs (to hold position)
    private Transform stair;
    // If a character is currently on the stairs
    private bool touchedOnce;
    // The transform of the character on the stairs
    private Transform whoIsOn;
    // A reference to the prompter script to ask the player if they would like to advance
    private Prompter prompter;


    // Called before start
    private void Awake()
    {
        // Initialize the array of players
        players = new List<Transform>();
        touchedOnce = false;
    }

    /// <summary>
    /// Called from Procedural Generation after everything is created.
    /// Gets the allies from the character parent
    /// </summary>
    /// <param name="charParent">Parent of all characters</param>
    /// <param name="stairsTrans">Transform of the stairs</param>
    /// <param name="prompterRef">Reference to the prompter</param>
    public void Initialize(Transform charParent, Transform stairsTrans, Prompter prompterRef)
    {
        // Set the stairs transform
        stair = stairsTrans;
        // Initialize the array of players
        players = new List<Transform>();
        // Iterate over the characters to get the allies
        foreach (Transform character in charParent)
        {
            MoveAttack mARef = character.GetComponent<MoveAttack>();
            if (mARef != null && mARef.WhatAmI == CharacterType.Ally)
            {
                players.Add(character);
            }
        }
        // Set the prompter
        prompter = prompterRef;
    }

    // Called once per frame
    private void Update()
    {
        touchStairs();
        // Iterate over the players
        foreach (Transform player in players)
        { 
            // If it exists and 
            if (player != null && whoIsOn == player)
            {
                if (player.transform.position != stair.transform.position)
                {
                    touchedOnce = false;
                }
            }
        }
        
    }

   
    private void touchStairs()
    {
        if (touchedOnce == false)
        {
            foreach (Transform player in players)
            {
                if (player != null && player.position == stair.transform.position)
                {
                    prompt();
                    touchedOnce = true;
                    whoIsOn = player;
                    return;
                }
            }
        }
    }
    
    private void prompt()
    {
        
        prompter.PromptGame();
        
    }

    public void yes()
    {

        SceneManager.LoadScene(sceneToGoTo);
        prompter.PromptGame();
    }
    public void no()
    {
        prompter.PromptGame();
    }
    
}
