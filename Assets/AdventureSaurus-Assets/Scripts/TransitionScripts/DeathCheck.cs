using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class DeathCheck : MonoBehaviour
{
    private List<Transform> players;

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
    }

    // Update is called once per frame   
    private void Update()
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
        if(gameOver)
        {
            SceneManager.LoadScene("GameOver");
        }
    }
    
}
