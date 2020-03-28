﻿using UnityEngine;
using UnityEngine.UI;

public enum TurnState {GAMESTOP, PLAYERTURN, ENEMYTURN }//the states the game can be in

public class TurnSystem : MonoBehaviour
{
    // Reference to the endturn button so that we can turn it off when needed
    [SerializeField] Button _endTurnButt = null;
    // The current state of the game
    private TurnState _state;
    public TurnState State {
        get { return _state; }
    }
    // The parent of all the characters on the floor
    private Transform _characterTeam;

    // Eventsa
    // For when it is the enemy's turn
    public delegate void BeginEnemyTurn();
    public static event BeginEnemyTurn OnBeginEnemyTurn;
    // For when it is the player's turn
    public delegate void BeginPlayerTurn();
    public static event BeginPlayerTurn OnBeginPlayerTurn;

    // Called when gameobject is toggled on
    // Subscribe to events
    private void OnEnable()
    {
        // When the enemy's turn ends, start the player's turn
        EnemyTurnController.OnEndEnemyTurn += StartPlayerTurn;
        //EnemyMoveAttackAI.OnEnemyTurnEnd += StartPlayerTurn;
    }

    // Called when gameobject is toggled off
    // Unsubscribe to events
    private void OnDisable()
    {
        EnemyTurnController.OnEndEnemyTurn -= StartPlayerTurn;
        //EnemyMoveAttackAI.OnEnemyTurnEnd -= StartPlayerTurn;
    }

    // Start is called before the first frame update
    private void Start()
    {
        _state = TurnState.GAMESTOP;//code between GAMESTOP and PLAYERTURN is for setup
    }

    /// <summary>
    /// Called from Procedural Generation after everything is created
    /// </summary>
    /// <param name="charParent">Transform that is the parent of all characters</param>
    public void Initialize(Transform charParent)
    {
        _characterTeam = charParent;
    }

    /// <summary>
    /// Called by the OnEnemyTurnEnd event from EnemyMoveAttackAI after all enemies have moved.
    /// Starts Players Turn, Enables player to control characters
    /// </summary>
    public void StartPlayerTurn()
    {
        _endTurnButt.interactable = true;
        _state = TurnState.PLAYERTURN;
        // We reset the turns of all characters
        foreach(Transform potAlly in _characterTeam)
        {
            MoveAttack potAllyMA = potAlly.GetComponent<MoveAttack>();
            if (potAllyMA == null)
            {
                Debug.Log("There is a non character object in " + _characterTeam.name);
            }
            else
            {
                potAllyMA.ResetMyTurn();
            }
        }

        // Call the event that the player's turn has begun
        if (OnBeginPlayerTurn != null)
            OnBeginPlayerTurn();
    }

    /// <summary>
    /// Starts Enemies Turn, Starts their AI
    /// </summary>
    private void StartEnemyTurn()
    {
        _endTurnButt.interactable = false;
        foreach (Transform potEnemy in _characterTeam)
        {
            MoveAttack potEnemyMA = potEnemy.GetComponent<MoveAttack>();
            if (potEnemyMA == null)
            {
                Debug.Log("There is a non character object in " + _characterTeam.name);
            }
            else
            {
                if (potEnemyMA.WhatAmI == CharacterType.Enemy)
                {
                    potEnemyMA.ResetMyTurn();
                }
            }
        }
        _state = TurnState.ENEMYTURN;

        // Start the enemy's turn
        if (OnBeginEnemyTurn != null)
            OnBeginEnemyTurn();
    }
    /// <summary>
    /// Called from 
    /// </summary>
    public void IsPlayerDone()//checks if player has done everything that they can do with thier characters
    {
        bool areDone = true;
        foreach (Transform player in _characterTeam)
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
        foreach (Transform player in _characterTeam)
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
        _state = TurnState.GAMESTOP;
    }

    public void EndPlayerTurn()
    {
        StartEnemyTurn();
    }
    
}
