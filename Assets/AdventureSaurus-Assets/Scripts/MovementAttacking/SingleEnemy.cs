using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingleEnemy : MonoBehaviour
{
    // MoveAttackController script for pathing
    private MoveAttackController _mAContRef;
    protected MoveAttackController MAContRef
    {
        get { return _mAContRef; }
    }
    // MoveAttack script attached to this enemy
    private MoveAttack _mARef;
    protected MoveAttack MARef
    {
        get { return _mARef; }
    }
    // The Node this enemy is currently on
    private Node _standingNode;
    protected Node StandingNode
    {
        get { return _standingNode; }
    }
    // The parent of all the characters
    private Transform _charParent;
    protected Transform CharacterParent
    {
        get { return _charParent; }
    }

    // Events
    // When a single enemy finishes their turn
    public delegate void SingleEnemyFinish();
    public static event SingleEnemyFinish OnSingleEnemyFinish;

    // Called before start
    // Set references
    private void Awake()
    {
        // GameController references
        GameObject gameContObj = GameObject.FindWithTag("GameController");
        if (gameContObj == null)
            Debug.Log("Could not find any gameobject with the tag GameController");
        else
        {
            _mAContRef = gameContObj.GetComponent<MoveAttackController>();
            if (_mAContRef == null)
                Debug.Log("There is no MoveAttackController script attached to " + gameContObj.name);
        }

        // Self references
        _mARef = this.GetComponent<MoveAttack>();
        if (_mARef == null)
            Debug.Log("There is no MoveAttack script attached to " + this.name);

        // TEMPORARAY FIX
        _charParent = GameObject.Find("CharacterParent").transform;
    }

    /// <summary>
    /// Takes the turn of the enemy
    /// </summary>
    public void TakeTurn()
    {
        // Get the node this enemy is standing on
        _standingNode = _mAContRef.GetNodeByWorldPosition(this.transform.position);
        // We are going to use the enemy's move tiles, so we need to recalculate those, 
        // since other characters have probably moved on their turn
        MARef.CalcMoveTiles();
        // Find the tile the enemy should move to
        Node nodeToMoveTo = FindTileToMoveTo();
        // Make sure we have a place to move
        if (nodeToMoveTo != null)
        {
            // After the enemy finishes moving, it will call the CharacterFinishedMovingEvent.
            // So we add the BeginAttemptAction function to be called when that event is called.
            // It will remove itself from the event once it is called.
            // The idea is that only one function will be attached to that event at a time (will belong to current character)
            MoveAttack.OnCharacterFinishedMoving += BeginAttemptAction;
            // Start moving the enemy
            MoveToTile(nodeToMoveTo);

            /// We want the enemy's turn to look like Move, Action, End.
            /// But we only call MoveToTile above.
            /// This is because we need the enemy to finish moving before it does its action.
            /// Action is called when the character finishes moving.
            /// Similarly, End is called when the character finishes its action.
        }
        // If we have no where to move, just finish this enemy's turn
        else
        {
            // Call the finish enemy turn event
            if (OnSingleEnemyFinish != null)
                OnSingleEnemyFinish();
        }
    }

    /// <summary>
    /// Tells the character to move
    /// </summary>
    /// <param name="nodeToMoveTo"></param>
    private void MoveToTile(Node nodeToMoveTo)
    {
        // Path to the tile
        _mAContRef.Pathing(_standingNode, nodeToMoveTo, CharacterType.Enemy);
        // Start moving along that path
        _mARef.StartMove();
    }

    /// <summary>
    /// Starts Attempt Action and removes itself from the event
    /// </summary>
    private void BeginAttemptAction()
    {
        // Remove itself from the finished moving event
        MoveAttack.OnCharacterFinishedMoving -= BeginAttemptAction;
        // Add BeginFinishTurn to the finished action event
        MoveAttack.OnCharacterFinishedAction += BeginEndTurn;

        // Actually attempt the action
        AttemptAction();
    }

    /// <summary>
    /// Removes itself from the finished action event and
    /// calls the event for finishing this single enemy's turn
    /// </summary>
    private void BeginEndTurn()
    {
        // Remove itself from the finished
        MoveAttack.OnCharacterFinishedAction -= BeginEndTurn;

        // See if we should be waiting until health is being decreased, or something else is holding up taking the next enemy's turn
        OnSingleEnemyFinish();
    }

    /// <summary>
    /// Finds and returns the tile this enemy should move to.
    /// Specified in the overrides.
    /// </summary>
    /// <returns>Node that this enemy should move to</returns>
    abstract protected Node FindTileToMoveTo();

    /// <summary>
    /// Attempts to do whatever action this enemy does.
    /// Called after the character finishes moving.
    /// Specified in the overrides.
    /// </summary>
    abstract protected void AttemptAction();
}
