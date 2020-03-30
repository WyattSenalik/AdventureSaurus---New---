using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class DeathCheck : MonoBehaviour
{
    // The list of ally character transforms
    private List<Transform> _players;
    // If initialize has been called in this script yet.
    // To prevent checking if non existant players are dead
    private bool _hasInitialized;

    // Called when the component is toggled active
    // Subscribe to events
    private void OnEnable()
    {
        // When the generation is finished, initialize this script
        ProceduralGenerationController.OnFinishGeneration += Initialize;
    }

    // Called when the component is toggled off
    // Unsubscribe from events
    private void OnDisable()
    {
        ProceduralGenerationController.OnFinishGeneration -= Initialize;
    }

    // Called when the gameobject is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        ProceduralGenerationController.OnFinishGeneration -= Initialize;
    }

    // Called 0th, before Start
    private void Awake()
    {
        // Don't check if the players are dead before we know who they are
        _hasInitialized = false;
    }

    /// <summary>
    /// Initializes things for this script.
    /// Called from the FinishGenerating event
    /// </summary>
    /// <param name="charParent">The parent of all the characters</param>
    /// <param name="roomParent">The parent of all the rooms (unused)</param>
    /// <param name="wallParent">The parent of all the walls (unused)</param>
    /// <param name="stairsTrans">The transform of the stairs (unused)</param>
    private void Initialize(Transform charParent, Transform roomParent, Transform wallParent, Transform stairsTrans)
    {
        _players = new List<Transform>();
        // Iterate over the children of the players parent
        foreach (Transform potAlly in charParent)
        {
            // Make sure its an ally, then add it
            MoveAttack mARef = potAlly.GetComponent<MoveAttack>();
            if (mARef != null && mARef.WhatAmI == CharacterType.Ally)
                _players.Add(potAlly);
        }
        // Allow the update function to check if everyone is dead
        _hasInitialized = true;
    }

    // Update is called once per frame   
    private void Update()
    {
        // Make sure the list of allies has been set before iterating over it
        if (_hasInitialized)
        {
            // Assume the player is dead
            bool gameOver = true;
            // Prove it wrong
            foreach (Transform ally in _players)
            {
                if (ally != null)
                {
                    gameOver = false;
                    break;
                }
            }
            // If all the players are null, load the gameover screen
            if (gameOver)
            {
                SceneManager.LoadScene("GameOver");
            }
        }
    }
    
}
