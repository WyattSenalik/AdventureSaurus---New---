using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class DeathCheck : MonoBehaviour
{
    // The list of ally character transforms
    private List<Transform> players;
    // If initialize has been called in this script yet.
    // To prevent checking if non existant players are dead
    private bool hasInitialized;

    // Called 0th, before Start
    private void Awake()
    {
        // Don't check if the players are dead before we know who they are
        hasInitialized = false;
    }

    /// <summary>
    /// Sets the players list to hold the transforms of the ally characters.
    /// Called from ProceduralGenerationController
    /// </summary>
    /// <param name="characterParent">The parent of all the character</param>
    public void Initialize(Transform characterParent)
    {
        players = new List<Transform>();
        // Iterate over the children of the players parent
        foreach (Transform potAlly in characterParent)
        {
            // Make sure its an ally, then add it
            MoveAttack mARef = potAlly.GetComponent<MoveAttack>();
            if (mARef != null && mARef.WhatAmI == CharacterType.Ally)
                players.Add(potAlly);
        }
        // Allow the update function to check if everyone is dead
        hasInitialized = true;
    }

    // Update is called once per frame   
    private void Update()
    {
        // Make sure the list of allies has been set before iterating over it
        if (hasInitialized)
        {
            // Assume the player is dead
            bool gameOver = true;
            // Prove it wrong
            foreach (Transform ally in players)
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
