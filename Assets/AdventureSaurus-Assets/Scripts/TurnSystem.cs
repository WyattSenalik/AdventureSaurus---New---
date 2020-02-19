using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurnState {GAMESTOP, PLAYERTURN, ENEMYTURN }//the states the game can be in


public class TurnSystem : MonoBehaviour
{
    private TurnState state;
    [SerializeField] Transform CharcterTeam = null;
    [SerializeField] GameObject endTurnButtObj = null;
    private EnemyMoveAttackAI enMAAIRef;
    private MoveAttackGUIController mAGUIContRef;
    private bool stop;

    private void Awake()
    {
        GameObject gameController = GameObject.FindWithTag("GameController");
        if (gameController == null)
            Debug.Log("Could not find any GameObject with the tag GameController");
        else
        {
            enMAAIRef = gameController.GetComponent<EnemyMoveAttackAI>();
            if (enMAAIRef == null)
            {
                Debug.Log("Could not find EnemyMoveAttackAI attached to " + gameController.name);
            }
            mAGUIContRef = gameController.GetComponent<MoveAttackGUIController>();
            if (mAGUIContRef == null)
            {
                Debug.Log("Could not find MoveAttackGUIController attached to " + gameController.name);
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        state = TurnState.GAMESTOP;//code between GAMESTOP and PLAYERTURN is for setup


        stop = true;
    }

    /// <summary>
    /// Called from EnemyMoveAttackAI after all enemies have moved
    /// </summary>
    public void StartPlayerTurn()//starts Players Turn, Enables player to control characters
    {
        endTurnButtObj.SetActive(true);
        state = TurnState.PLAYERTURN;
        // Reset everyone's turns
        ResetEveryoneTurns();
        mAGUIContRef.AllowSelect();
    }

    /// <summary>
    /// 
    /// </summary>
    void StartEnemyTurn()//starts Enemies Turn, Starts their AI
    {
        endTurnButtObj.SetActive(false);
        state = TurnState.ENEMYTURN;
        // Reset everyone's turns
        ResetEveryoneTurns();
        mAGUIContRef.DenySelect();
        enMAAIRef.StartTakeTurn(); 
    }
    /// <summary>
    /// Called from 
    /// </summary>
    public void IsPlayerDone()//checks if player has done everything that they can do with thier characters
    {
        bool areDone = true;
        foreach (Transform player in CharcterTeam)
        {
            MoveAttack ma = player.GetComponent<MoveAttack>();
            if(ma == null)
            {
                Debug.Log("There was no move attack attached to " + player.name);
                continue;
            }

            if(ma.WhatAmI == CharacterType.Ally)//checks if its a player character
            {
                if( !(ma.HasMoved && ma.HasAttacked))
                {
                    areDone = false;
                    break;
                }
            }
            
        }
        if(areDone)
        {
            EndPlayerTurn();
        }
    }
    //checks all enemies to see if they are done with their turn
    public void IsEnemyDone()//gets called each time an enemy is done taking actions
    {
        bool areDone = true;//Starts true to assume the player is done but if any have actions left sets it to false
        foreach (Transform player in CharcterTeam)
        {
            MoveAttack ma = player.GetComponent<MoveAttack>();
            if (ma == null)
            {
                Debug.Log("There was no move attack attached to " + player.name);
                continue;
            }

            if (ma.WhatAmI == CharacterType.Enemy)
            {
                if (!(ma.HasMoved && ma.HasAttacked))
                {
                    areDone = false;
                    break;
                }
            }

        }
        if (areDone == true)
        {

            StartPlayerTurn();
        }

    }

    public void SetToGameStop()//sets state to GameStop which will be used for pausing or cutscenes
    {
        state = TurnState.GAMESTOP;
    }

    public void EndPlayerTurn()
    {
        mAGUIContRef.DenySelect();//disables some user's input
        StartEnemyTurn();
    }

    /// <summary>
    /// Resets the turns of all characters, letting them move and attack again
    /// We resets them all because on the player's turn we need them to be able to see the enemies' potential movements
    /// </summary>
    private void ResetEveryoneTurns()
    {
        // Iterate over each character
        foreach (Transform character in CharcterTeam)
        {
            MoveAttack characterMA = character.GetComponent<MoveAttack>();
            if (characterMA == null)
            {
                Debug.Log("There is a non character object in " + CharcterTeam.name);
            }
            else
            {
                characterMA.ResetMyTurn();
            }
        }
    }
    
}
