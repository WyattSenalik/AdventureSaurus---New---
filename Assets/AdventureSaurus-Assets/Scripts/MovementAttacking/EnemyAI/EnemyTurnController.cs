using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurnController : MonoBehaviour
{
    // A list of all the active enemies
    private List<SingleEnemy> _allEnemies;
    public int GetAmountEnemies() { return _allEnemies.Count; }
    // Index of the current enemy
    private int _curEnIndex;

    // Events
    // Directly before the enemy turn begins
    public delegate void PreSingleEnemyTurn(Transform curEnemy);
    public static event PreSingleEnemyTurn OnPreSingleEnemyTurn;
    // When the enemy turn is over
    public delegate void EndEnemyTurn();
    public static event EndEnemyTurn OnEndEnemyTurn;

    // Called when the gameobject is set to active
    // Subscribe to events
    private void OnEnable()
    {
        // When one enemy finishes their turn, start the next enemy
        SingleEnemy.OnSingleEnemyFinish += SingleEnemyTurn;
        // When a room is loaded, add the enemies in that room to _allEnemies
        Room.OnRoomActivate += AddEnemies;
        // When the camera has finished panning over an enemy;
        CamFollow.OnFinishEnemyPan += BeginTurnAfterPan;
        // When an enemy dies, cull dead enemies
        EnemyHealth.OnEnemyDeath += CullTheDead;
    }

    // Called when the gameobject is toggled off
    // Unsubscribe from events
    private void OnDisable()
    {
        SingleEnemy.OnSingleEnemyFinish -= SingleEnemyTurn;
        Room.OnRoomActivate -= AddEnemies;
        CamFollow.OnFinishEnemyPan -= BeginTurnAfterPan;
        EnemyHealth.OnEnemyDeath -= CullTheDead;
    }

    // Called when the gameobject is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        SingleEnemy.OnSingleEnemyFinish -= SingleEnemyTurn;
        Room.OnRoomActivate -= AddEnemies;
        CamFollow.OnFinishEnemyPan -= BeginTurnAfterPan;
        EnemyHealth.OnEnemyDeath -= CullTheDead;
    }

    // Called before the first frame
    private void Start()
    {
        // Initialize the list of enemies
        _allEnemies = new List<SingleEnemy>();
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
            // Pan over to the enemy before we take their turn
            if (OnPreSingleEnemyTurn != null)
                OnPreSingleEnemyTurn(_allEnemies[_curEnIndex].transform);
        }
        // If all the enemies have taken their turn
        else
        {
            // Call the end enemy turn event
            if (OnEndEnemyTurn != null)
            {
                OnEndEnemyTurn();
            }
        }
    }

    /// <summary>
    /// After the camera finishes panning over to the character, we start that character's turn
    /// </summary>
    private void BeginTurnAfterPan()
    {
        // Take the turn of the current enemy
        _allEnemies[_curEnIndex].TakeTurn();
    }

    /// <summary>
    /// Starts the turn of the first enemy.
    /// Called from TurnSystem
    /// The next ally's turn will be called from the OnSingleEnemyFinish event
    /// </summary>
    public void BeginFirstEnemyTurn()
    {
        // Get rid of the dead enemies
        CullTheDead();

        // Start the first enemy
        _curEnIndex = -1;
        SingleEnemyTurn();
    }

    /// <summary>
    /// Adds enemies to the _allEnemies list and activates them when added
    /// </summary>
    /// <param name="enemiesToAdd">The enemies to add</param>
    private void AddEnemies(List<MoveAttack> enemiesToAdd)
    {
        // Add each enemy to the list
        foreach (MoveAttack singEnemy in enemiesToAdd)
        {
            SingleEnemy singEnemyScriptRef = singEnemy.GetComponent<SingleEnemy>();
            if (singEnemyScriptRef != null && !_allEnemies.Contains(singEnemyScriptRef))
            {
                // Add the enemy
                _allEnemies.Add(singEnemyScriptRef);
                // Turn them active
                singEnemy.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Gets rid of the null enemies in the list
    /// </summary>
    private void CullTheDead()
    {
        // Iterate over the enemies to find all the nulls
        int curIndex = 0;
        while (curIndex < _allEnemies.Count)
        {
            // If the enemy is null, remove it and do not increment the index
            if (_allEnemies[curIndex] == null || _allEnemies[curIndex].GetComponent<Health>().CurHP == 0)
            {
                _allEnemies.RemoveAt(curIndex);
            }
            // If the enemy exists, go to the next enemy
            else
            {
                ++curIndex;
            }
        }
    }
}
