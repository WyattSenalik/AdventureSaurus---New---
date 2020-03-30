using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurnController : MonoBehaviour
{
    // Parent of all the characters
    private Transform _charParent;
    // A list of all the active enemies
    private List<SingleEnemy> _allEnemies;
    // Index of the current enemy
    private int _curEnIndex;

    // Events
    // When the enemy turn is over
    public delegate void EndEnemyTurn();
    public static event EndEnemyTurn OnEndEnemyTurn;

    // Called when the gameobject is set to active
    // Subscribe to events
    private void OnEnable()
    {
        // When the turn system says its the enemy's turn start taking turns
        TurnSystem.OnBeginEnemyTurn += BeginFirstEnemyTurn;
        // When one enemy finishes their turn, start the next enemy
        SingleEnemy.OnSingleEnemyFinish += SingleEnemyTurn;
    }

    // Called when the gameobject is toggled off
    // Unsubscribe from events
    private void OnDisable()
    {
        TurnSystem.OnBeginEnemyTurn -= BeginFirstEnemyTurn;
        SingleEnemy.OnSingleEnemyFinish -= SingleEnemyTurn;
    }

    // Called when the gameobject is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        TurnSystem.OnBeginEnemyTurn -= BeginFirstEnemyTurn;
        SingleEnemy.OnSingleEnemyFinish -= SingleEnemyTurn;
    }

    /// <summary>
    /// Has a single enemy take their turn
    /// </summary>
    private void SingleEnemyTurn()
    {
        // Increment the index, initial index is -1
        ++_curEnIndex;
        // If all enemies have not taken their turn yet
        if (_curEnIndex < _allEnemies.Count)
        {
            // Take the current enemy's turn
            _allEnemies[_curEnIndex].TakeTurn();
        }
        // If all the enemies have taken their turn
        else
        {
            // Call the end enemy turn event
            if (OnEndEnemyTurn != null)
            {
                //Debug.Log("OnEndEnemyTurn");
                OnEndEnemyTurn();
            }
        }
    }

    /// <summary>
    /// Starts the turn of the first enemy.
    /// The next ally's turn will be called from the OnSingleEnemyFinish event
    /// </summary>
    private void BeginFirstEnemyTurn()
    {
        //Debug.Log("BeginFirstEnemyTurn");
        // Get all the active enemies
        _allEnemies = new List<SingleEnemy>();

        // TEMPORARAY FIX
        _charParent = GameObject.Find("CharacterParent").transform;

        // Iterate over each character to find all the enemies
        foreach (Transform character in _charParent)
        {
            if (!character.gameObject.activeInHierarchy)
                continue;

            SingleEnemy singEn = character.GetComponent<SingleEnemy>();
            if (singEn == null)
            {
                continue;
            }

            _allEnemies.Add(singEn);
        }

        // Start the first enemy
        _curEnIndex = -1;
        SingleEnemyTurn();
    }
}
