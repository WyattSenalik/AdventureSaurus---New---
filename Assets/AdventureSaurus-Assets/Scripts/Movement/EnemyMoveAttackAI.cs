﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMoveAttackAI : MonoBehaviour
{
    [SerializeField] private Transform charParent = null;   // The parent of all the characters
    [SerializeField] private int aggroRange = 16;    // Range the enemies will aggro
    private TurnSystem turnSysRef;  // Reference to the TurnSystem script
    private MoveAttackController mAContRef; // Reference to the MoveAttackController script
    private CamFollow camFollowRef; // Reference to the CamFollow script
    private List<MoveAttack> alliesMA;  // List of all allies' MoveAttack scripts
    private List<MoveAttack> enemiesMA; // List of all enemies' MoveAttack scripts
    public List<MoveAttack> EnemiesList
    {
        get { return enemiesMA; }
    }
    private int enemyIndex; // The current enemy in enemiesMA that should be moved
    private Vector2Int curAttackNodePos;    // Where the character should attack
    private MoveAttack currentEnemy;    // The currenet enemy reference
    public MoveAttack CurrentEnemy
    {
        get { return currentEnemy; }
    }
    private bool curEnemyActive;    // If the current enemy is an active enemy that will be moving
    private bool duringEnemyTurn;  // If an enemy is still taking its turn
    //public string enemyName;
    //public bool isMoving;

    // Set References
    private void Awake()
    {
        GameObject gameController = GameObject.FindWithTag("GameController");
        // Make sure a GameController exists
        if (gameController == null)
            Debug.Log("Could not find any GameObject with the tag GameController");
        else
        {
            mAContRef = gameController.GetComponent<MoveAttackController>();
            // Make sure we verify we actually found a MoveAttackController
            if (mAContRef == null)
                Debug.Log("There was no MoveAttackController attached to " + gameController.name);

            turnSysRef = gameController.GetComponent<TurnSystem>();
            // Make sure we verify we actually found a TurnSystem
            if (turnSysRef == null)
                Debug.Log("There was no TurnSystem attached to " + gameController.name);
        }

        GameObject mainCamObj = GameObject.FindWithTag("MainCamera");
        if (mainCamObj == null)
            Debug.Log("Could not find any GameObject  with the tag MainCamera");
        else
        {
            camFollowRef = mainCamObj.GetComponent<CamFollow>();
            // Make sure we verify we actually found a MoveAttackController
            if (camFollowRef == null)
                Debug.Log("There was no CamFollow attached to " + mainCamObj.name);
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        enemiesMA = new List<MoveAttack>();
        Initialize();
    }

    /// <summary>
    /// For setting variables and list each time we start a turn
    /// </summary>
    private void Initialize()
    {
        alliesMA = new List<MoveAttack>();
        enemyIndex = 0;
        // Iterate over each character to find all the allies
        foreach (Transform character in charParent)
        {
            MoveAttack ma = character.GetComponent<MoveAttack>();
            if (ma == null)
            {
                //Debug.Log("There was no MoveAttack attached to " + character.name);
                continue;
            }


            if (ma.WhatAmI == CharacterType.Ally)
            {
                alliesMA.Add(ma);
            }
        }
        // Bring out your dead *rings bell* Bring out your dead
        // We have to go over the enemies list and remove any dead enemies
        for (int i = 0; i < enemiesMA.Count; ++i)
        {
            // If we find a dead enemy, get rid of them
            if (enemiesMA[i] == null)
            {
                enemiesMA.RemoveAt(i);
                // We also need to decrement i so that the list does not stop prematurely since the list now shrank by 1
                --i;
            }
        }
        duringEnemyTurn = false;
    }

    /// <summary>
    /// Finds all the enmies and starts to make the first enemy move
    /// </summary>
    public void StartTakeTurn()
    {
        Initialize();    // Refind all the allies, set enemyIndex to 0, and remove any dead enemies from the list
        StartNextEnemy();    // Start from the first enemy
    }

    /// <summary>
    /// Has the current enemy move and then increments it so that the next time this is called, the next enemy will move
    /// </summary>
    public IEnumerator NextEnemy()
    {
        // So that the this enemy can't start until the previous one is done
        while (duringEnemyTurn)
        {
            yield return null;
        }
        duringEnemyTurn = true;

        if (enemyIndex < enemiesMA.Count)
        {
            // Try to get the current enemy we should move
            currentEnemy = enemiesMA[enemyIndex];
            // If the enemy does not exist, do not try to move it
            if (currentEnemy != null)
            {
                // See if the current enemy will be active
                curEnemyActive = false;
                Node enemyNode = mAContRef.GetNodeByWorldPosition(currentEnemy.transform.position);
                for (int i = 0; i < alliesMA.Count; ++i)
                {
                    MoveAttack ally = alliesMA[i];
                    // Make sure the enemy exists
                    if (ally == null)
                    {
                        alliesMA.RemoveAt(i);
                        --i;
                        continue;
                    }

                    Node allyNode = mAContRef.GetNodeByWorldPosition(ally.transform.position);
                    if (Mathf.Abs(enemyNode.position.x - allyNode.position.x) + Mathf.Abs(enemyNode.position.y - allyNode.position.y) <= aggroRange)
                    {
                        curEnemyActive = true;
                        break;
                    }
                }

                if (curEnemyActive)
                {
                    //Debug.Log("Following " + currentEnemy.name);
                    camFollowRef.FollowEnemy(currentEnemy.transform);
                    // Wait until the camera is on the enemy we are about to take the turn of
                    while (!camFollowRef.AmFollowing)
                    {
                        yield return null;
                    }
                    //Debug.Log("Finished Following " + currentEnemy.name);
                }
            }
            // Have the current enemy take their turn
            TakeSingleTurn();
  
            // Increment what enemy we are on for the next time
            ++enemyIndex;
            //Debug.Log(currentEnemy.name + " Incremented Enemy");
        }
        else
        {
            turnSysRef.StartPlayerTurn();
        }

        duringEnemyTurn = false;
        yield return null;
    }

    /// <summary>
    /// Moves the current enemy and makes them attack
    /// </summary>
    private void TakeSingleTurn()
    {
        // Try to get the current enemy we should move
        currentEnemy = enemiesMA[enemyIndex];
        // If the enemy does not exist, do not try to move it
        if (currentEnemy == null)
        {
            Debug.Log("We done bois");
            return;
        }
        else
        {
            Debug.Log("Moving " + currentEnemy.name);
        }

        Node startNode = mAContRef.GetNodeByWorldPosition(currentEnemy.transform.position);
        // Reset this enemies movement
        currentEnemy.CalcMoveTiles();
        currentEnemy.CalcAttackTiles();

        // Get the node this character should try to attack and the node this character should move to
        curAttackNodePos = FindDesiredAttackNode();
        Node desiredNode = FindDesiredMovementNode();
        // If the node returns null, it means we cannot do anything with this enemy
        if (desiredNode == null)
        {
            // Debug.Log(currentEnemy.name + " Attack Node: " + curAttackNodePos);
            Debug.Log("Desired node is null");
            currentEnemy.HasMoved = true;
            AttemptAttack();
            return;
        }
        // See if they are trying to move where a character already is
        else if (desiredNode.occupying != CharacterType.None)
        {
            // Debug.Log(currentEnemy.name + " Start Node: " + startNode.position + ". End Node: " + desiredNode.position);
            // Debug.Log(currentEnemy.name + " Attack Node: " + curAttackNodePos);
            Debug.Log("Wrong move pal");
            currentEnemy.HasMoved = true;
            AttemptAttack();
            return;
        }
        // Debug.Log(currentEnemy.name + " Start Node: " + startNode.position + ". End Node: " + desiredNode.position);
        // Debug.Log(currentEnemy.name + " Attack Node: " + curAttackNodePos);

        /*
        if (FindAllyOutOfRange() != null)
        {
            isMoving = true;
            enemyName = currentEnemy.name;

   
        }
        */
        // Calculate the pathing

        // If they successfully pathed
        if (mAContRef.Pathing(startNode, desiredNode, currentEnemy.WhatAmI))
        {
            // Start moving the character
            currentEnemy.StartMove();
        }
        // If they didn't just attempt an attack
        else
        {
            mAContRef.ResetPathing();
            currentEnemy.HasMoved = true;
            AttemptAttack();
        }

        // Attacking will be called after the enemy has moved
    }

    /// <summary>
    /// Finds if there is an enemy in range and gives its gridPositon, if there's not, it returns an invalid gridPosition
    /// </summary>
    /// <returns>Returns a Vector2Int on the grid if it finds an enemy, otherwise returns a Vector2Int off the grid</returns>
    private Vector2Int FindDesiredAttackNode()
    {
        // If there is an "ally" character in range, we want that node
        // Test if the ally is in range
        // Iterate over each ally
        for (int i = 0; i < alliesMA.Count; ++i)
        {
            MoveAttack ally = alliesMA[i];
            // Make sure the enemy exists
            if (ally == null)
            {
                alliesMA.RemoveAt(i);
                --i;
                continue;
            }
            // Get that ally's node
            Node allyNode = mAContRef.GetNodeByWorldPosition(ally.transform.position);
            // If that ally's node is in this enemy's move tiles, they are in range, so that is the node we want to reach
            if (currentEnemy.AttackTiles.Contains(allyNode))
            {
                //Debug.Log("Found ally to attack at " + allyGridPos);
                return allyNode.position;
            }
        }
        //Debug.Log("No ally in range");
        // If there is no ally in range, we don't want to attack anything with a character on it, so we return a position not on the grid
        return new Vector2Int(mAContRef.GridTopLeft.x - 1, 0);
    }

    /// <summary>
    /// Called from MoveAttack by enemies. Attempts to have the enemy attack
    /// </summary>
    public void AttemptAttack()
    {
        // If the enemy does not exist, do not try to attack something
        if (currentEnemy == null)
        {
            Debug.Log("No enemy is current");
            return;
        }
        // Otherwise attack it
        currentEnemy.StartAttack(curAttackNodePos);
    }

    /// <summary>
    /// Finds the node that the enemy "wants" to move to
    /// </summary>
    /// <returns>A Node that is the Node the enemy has chosen to move to</returns>
    private Node FindDesiredMovementNode()
    {
        // Determine if we found a node with an ally on it, by just checking if the position is on the grid
        Node nodeToAttack = mAContRef.GetNodeAtPosition(curAttackNodePos);
        Node currentEnemyNode = mAContRef.GetNodeByWorldPosition(currentEnemy.transform.position);
        // If we found an ally in range to attack
        if (nodeToAttack != null && nodeToAttack.occupying == CharacterType.Ally)
        {
            // Check to make sure the enemy has a place to stand to attack and that the enemy can reach that node
            //
            // Get the nodes the enemy can attack the ally from
            List<Node> allyAttackNodes = mAContRef.GetNodesDistFromNode(nodeToAttack, currentEnemy.AttackRange);
            // Debug.Log(currentEnemy.name + " ally attack nodes in FindDesiredMovementode: " + allyAttackNodes);
            // Iterate over each of the nodes the enemy could potentially stand at to attack
            for (int j = 0; j < allyAttackNodes.Count; ++j)
            {
                // Debug.Log(currentEnemy.name + " single node at j in FindDesiredMovementode: " + allyAttackNodes[j]);
                // See if the node exists and there is no character there
                if (allyAttackNodes[j] != null && allyAttackNodes[j].occupying == CharacterType.None)
                {
                    // Debug.Log(currentEnemy.name + " single node occupying at j in FindDesiredMovementode: " + allyAttackNodes[j].occupying);
                    // See if there is a path to there for enemies
                    if (mAContRef.Pathing(currentEnemyNode, allyAttackNodes[j], CharacterType.Enemy))
                    {
                        // Debug.Log("Found node to attack" + nodeToAttack.position);
                        return allyAttackNodes[j];
                    }
                }
            }
            // If we reach this point, something went wrong
            // Debug.Log("nodeToAttack in FindDesiredMovementNode wasn't null, but was invalid");
            return null;
        }
        // If there is no ally in range to attack
        else
        {
            Debug.Log(currentEnemy.name + " can't attack an ally this turn");
            // If we have no ally to attack, we need to find the closest ally to me and move as close to them as possible
            Node closestAllyNode = FindAllyOutOfRange();
            // If there are no closest allies, we just should move in place
            if (closestAllyNode == null)
            {
                return mAContRef.GetNodeByWorldPosition(currentEnemy.transform.position);
            }

            // We want to get a list of the nodes that we can attack from arround the ally
            List<Node> potAttackNodes = mAContRef.GetNodesDistFromNode(closestAllyNode, currentEnemy.AttackRange);
            // We remove the nodes that already have something occupying them
            for (int i = 0; i < potAttackNodes.Count; ++i)
            {
                if (potAttackNodes[i].occupying != CharacterType.None)
                {
                    potAttackNodes.RemoveAt(i);
                    --i;
                }
            }
            // Determine the closest node out of those nodes
            if (potAttackNodes.Count > 0) {
                Node closestAttackNode = potAttackNodes[0];
                int closestDist = Mathf.Abs(closestAttackNode.position.x - currentEnemyNode)
                for (int i = 1; i < potAttackNodes.Count; ++i)
                {
                        if ()
                        /// WORKING HERE
                }
            }

            // Find the path to that ally, we don't care about if we can actually move there in this case
            Node startNode = mAContRef.GetNodeByWorldPosition(currentEnemy.transform.position);
            if (mAContRef.Pathing(startNode, closestAllyNode, CharacterType.Enemy, false))
            {
                //Debug.Log("Pathing successful for " + currentEnemy.name);
            }
            else
            {
                //Debug.Log("Pathing failed for " + currentEnemy.name);
            }
            // We need to iterate over the new path to see if the enemy would end up on another enemy
            int testRange = currentEnemy.MoveRange;
            Node currentNode = startNode;
            //Debug.Log(startNode.position);
            // We test if the node is invalid. A valid node is either null or has no one occupying it. If the current node has itself as where to go, 
            // we also want to stop iterating because that node is the end of the path
            while (!(currentNode == null || currentNode.occupying == CharacterType.None || currentNode.whereToGo == currentNode))
            {
                currentNode = startNode;
                // Use the new pathing and the current enemies movement range to determine where we should move
                for (int i = 0; i < testRange; ++i)
                {
                    if (currentNode != null)
                    {
                        //Debug.Log("Current nodes position: " + currentNode.position + ". Current node occupying: " + currentNode.occupying + ". Current node where to go postion: " + currentNode.whereToGo);
                        currentNode = currentNode.whereToGo;
                    }
                }
                --testRange;
                // We now test in the while statement if the current node is still invalid
                // If it comes back that the node is still invalid --testRange makes it so we only iterate over 1 less
            }
            // This can return the startNode
            return currentNode;
        }
    }

    /// <summary>
    /// Finds the closest ally to the current enemy
    /// </summary>
    /// <returns>Returns the node that the closest ally is on</returns>
    private Node FindAllyOutOfRange()
    {
        // Get the node the current enemy is at
        Node startNode = mAContRef.GetNodeByWorldPosition(currentEnemy.transform.position);

        // Find the allies, and see if they are more than maxDepth grid units away anyway
        // If they are, we just return null
        Node closestAllyNode = null; // The node of the closest ally
        // The closest allies distance from the currentEnemy
        int closestAllyDist = (mAContRef.GridTopLeft.y - mAContRef.GridBotRight.y) + (mAContRef.GridBotRight.x - mAContRef.GridTopLeft.x) + 2;
        for (int i = 0; i < alliesMA.Count; ++i)
        {
            MoveAttack curAlly = alliesMA[i];
            // Make sure the current ally exists
            if (curAlly == null)
            {
                continue;
            }

            Node curAllyNode = mAContRef.GetNodeByWorldPosition(curAlly.transform.position);
            int curAllyDist = Mathf.Abs(startNode.position.x - curAllyNode.position.x) + Mathf.Abs(startNode.position.y - curAllyNode.position.y);
            // Quick check if ally is close to this enemy and is closer than the current closest ally
            if (curAllyDist <= aggroRange &&  curAllyDist < closestAllyDist)
            {
                // Check to make sure the enemy has a place to stand to attack and that the enemy can reach that node
                //
                // Get the nodes the enemy can attack the ally from
                List<Node> allyAttackNodes = mAContRef.GetNodesDistFromNode(curAllyNode, currentEnemy.AttackRange);
                // Iterate over each of the nodes the enemy could potentially stand at to attack
                for (int j = 0; j < allyAttackNodes.Count; ++j)
                {
                    // See if the node exists and there is no character there
                    if (allyAttackNodes[j] != null && allyAttackNodes[j].occupying == CharacterType.None)
                    {
                        // See if there is a path to there for enemies
                        if (mAContRef.Pathing(startNode, allyAttackNodes[j], CharacterType.Enemy))
                        {
                            mAContRef.ResetPathing();
                            closestAllyNode = curAllyNode;
                            closestAllyDist = curAllyDist;
                        }
                    }
                }
            }
        }
        // Returns null if the closestAlly node was not found
        return closestAllyNode;
    }

    /// <summary>
    /// Starts a coroutine of nextEnemy
    /// </summary>
    public void StartNextEnemy()
    {
        StartCoroutine(NextEnemy());
    }

    /// <summary>
    /// Called from Room when an ally enters a new room. Sets active the enemies in that room and adds them to the enemies list
    /// </summary>
    /// <param name="enemiesToAdd">The enemies to add to the enemies list</param>
    public void ActivateRoom(List<MoveAttack> enemiesToAdd)
    {
        // Add the enemies to the list
        foreach (MoveAttack enemy in enemiesToAdd)
        {
            //Debug.Log("Activating " + enemy.name);
            if (enemy == null)
                continue;
            enemy.gameObject.SetActive(true);
            // Make sure its not already there
            if (!enemiesMA.Contains(enemy))
                enemiesMA.Add(enemy);
        }
    }

    /// <summary>
    /// Called from Room when the last ally leaves the room. Removes the enemies in that room from the active enemies list
    /// </summary>
    /// <param name="enemiesToRemove">The enemies to remove from the enemies list</param>
    public void DeactivateRoom(List<MoveAttack> enemiesToRemove)
    {
        // Remove the enemies to the list
        foreach (MoveAttack enemy in enemiesToRemove)
        {
            if (enemy == null)
                continue;
            enemy.gameObject.SetActive(false);
            enemiesMA.Add(enemy);
        }
    }
}
