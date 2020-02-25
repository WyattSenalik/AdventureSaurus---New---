using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMoveAttackAI : MonoBehaviour
{
    [SerializeField] private Transform charParent = null;   // The parent of all the characters
    [SerializeField] private int aggroRange = 8;    // Range the enemies will aggro
    private TurnSystem turnSysRef;  // Reference to the TurnSystem script
    private MoveAttackController mAContRef; // Reference to the MoveAttackController script
    private CamFollow camFollowRef; // Reference to the CamFollow script
    private List<MoveAttack> alliesMA;  // List of all allies' MoveAttack scripts
    private List<MoveAttack> enemiesMA; // List of all enemies' MoveAttack scripts
    private int enemyIndex; // The current enemy in enemiesMA that should be moved
    private Vector2Int curAttackNodePos;    // Where the character should attack
    private MoveAttack currentEnemy;    // The currenet enemy reference
    public MoveAttack CurrentEnemy
    {
        get { return currentEnemy; }
    }
    private bool curEnemyActive;    // If the current enemy is an active enemy that will be moving
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
        alliesMA = new List<MoveAttack>();
        enemiesMA = new List<MoveAttack>();
        enemyIndex = 0;
        // Iterate over each character to find all the enemies and allies
        foreach (Transform character in charParent)
        {
            MoveAttack ma = character.GetComponent<MoveAttack>();
            if (ma == null)
            {
                //Debug.Log("There was no MoveAttack attached to " + character.name);
                continue;
            }

            if (ma.WhatAmI == CharacterType.Enemy)
            {
                enemiesMA.Add(ma);
            }
            else if (ma.WhatAmI == CharacterType.Ally)
            {
                alliesMA.Add(ma);
            }
        }
    }

    /// <summary>
    /// Finds all the enmies and starts to make the first enemy move
    /// </summary>
    public void StartTakeTurn()
    {
        Start();    // Refind all the allies and enemies as well set enemyIndex to 0
        StartNextEnemy();    // Start from the first enemy
    }

    /// <summary>
    /// Has the current enemy move and then increments it so that the next time this is called, the next enemy will move
    /// </summary>
    public IEnumerator NextEnemy()
    {
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
                    camFollowRef.FollowEnemy(currentEnemy.transform);
                    // Wait until the camera is on the enemy we are about to take the turn of
                    while (!camFollowRef.AmFollowing)
                    {
                        yield return null;
                    }
                }
            }

            // Have the current enemy take their turn
            TakeSingleTurn();
  
            // Increment what enemy we are on for the next time
            ++enemyIndex;
        }
        else
        {
            turnSysRef.StartPlayerTurn();
        }

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
            return;
        }

        // Reset this enemies movement
        currentEnemy.CalcMoveTiles();
        currentEnemy.CalcAttackTiles();

        // Get the node this character should try to attack and the node this character should move to
        curAttackNodePos = FindDesiredAttackNode(currentEnemy);
        Node desiredNode = FindDesiredMovementNode(currentEnemy);
        // If the node returns null, it means we cannot do anything with this enemy
        if (desiredNode == null)
        {
            return;
        }
        /*
        if (FindAllyOutOfRange() != null)
        {
            isMoving = true;
            enemyName = currentEnemy.name;

   
        }
    */
        // Calculate the pathing
        Node startNode = mAContRef.GetNodeByWorldPosition(currentEnemy.transform.position);
        mAContRef.Pathing(startNode, desiredNode, currentEnemy.WhatAmI);
        // Start moving the character
        currentEnemy.StartMove();
        
        // Attacking will be called after the enemy has moved
    }

    /// <summary>
    /// Finds if there is an enemy in range and gives its gridPositon, if there's not, it returns an invalid gridPosition
    /// </summary>
    /// <param name="enemy">The enemy we are testing from</param>
    /// <returns>Returns a Vector2Int on the grid if it finds an enemy, otherwise returns a Vector2Int off the grid</returns>
    private Vector2Int FindDesiredAttackNode(MoveAttack enemy)
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
            if (enemy.AttackTiles.Contains(allyNode))
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
            return;
        }
        // Otherwise attack it
        currentEnemy.StartAttack(curAttackNodePos);
    }

    /// <summary>
    /// Finds the node that the enemy "wants" to move to
    /// </summary>
    /// <param name="enemy"></param>
    /// <param name="attackNode"></param>
    /// <returns></returns>
    private Node FindDesiredMovementNode(MoveAttack enemy)
    {
        // Determine if we found a node with an ally on it, by just checking if the position is on the grid
        Node nodeToAttack = mAContRef.GetNodeAtPosition(curAttackNodePos);
        // If we found an ally to attack
        if (nodeToAttack != null)
        {
            // Get the potential nodes that the enemy would be able to attack the ally from
            List<Node> potentialMoveTiles = mAContRef.GetNodesDistFromNode(nodeToAttack, enemy.AttackRange);
            // The node we are currently on is not in MoveAttack's move tile, so we must test for that separately
            Node currentNode = mAContRef.GetNodeByWorldPosition(enemy.transform.position);
            // Iterate over the potenital nodes until we find one that we can move to
            foreach (Node node in potentialMoveTiles)
            {
                // Once we find one, return it
                if ((enemy.MoveTiles.Contains(node) && node.occupying == CharacterType.None) || (node == currentNode))
                {
                    //Debug.Log("Found node to move to " + node.position);
                    return node;
                }
            }
            // If we reach this point, something went wrong
            Debug.Log("nodeToAttack in FindDesiredMovementNode wasn't null, but was invalid");
            return null;
        }
        // If there is no ally to attack
        else
        {
            // If we have no ally to attack, we need to find the closest ally to me and move as close to them as possible
            Node closestAllyNode = FindAllyOutOfRange();
            // If there are no closest allies, we just should move
            if (closestAllyNode == null)
            {
                return mAContRef.GetNodeByWorldPosition(currentEnemy.transform.position);
            }
            //Debug.Log("ClosestAllyNode " + closestAllyNode.position);
            // Find the path to that ally, we don't care about if we can actually move there in this case
            Node startNode = mAContRef.GetNodeByWorldPosition(currentEnemy.transform.position);
            mAContRef.Pathing(startNode, closestAllyNode, CharacterType.Enemy, false);
            // In the case that the enemy is trying to move onto another enemy
            int testRange = currentEnemy.MoveRange;
            while (startNode != null && startNode.occupying != CharacterType.None)
            {
                startNode = mAContRef.GetNodeByWorldPosition(currentEnemy.transform.position);
                // Use the new pathing and the current enemies movement range to determine where we should move
                for (int i = 0; i < testRange; ++i)
                {
                    if (startNode != null)
                    {
                        startNode = startNode.whereToGo;
                    }
                }
                --testRange;
            }
            return startNode;
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
        bool allyIsClose = false;
        // The node the ally will be at if its found
        Node allyNode = null;
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

            allyNode = mAContRef.GetNodeByWorldPosition(ally.transform.position);
            if (Mathf.Abs(startNode.position.x - allyNode.position.x) + Mathf.Abs(startNode.position.y - allyNode.position.y) <= aggroRange)
            {
                allyIsClose = true;
                break;
            }
        }
        // Stop if there was no close ally
        if (!allyIsClose)
        {
            //Debug.Log("No ally is close");
            return null;
        }

        // Initialize the list and add the current node as the start node
        List<Node> alreadyTestedNodes = new List<Node>();    // The nodes that have been tested already
        List<Node> currentNodes = new List<Node>(); // The nodes we have yet to test
        currentNodes.Add(startNode);
        // While we haven't explored all the nodes yet or we havent explored the maxDepth yet
        int curDepth = 0;
        while (currentNodes.Count != 0 && curDepth <= aggroRange)
        {
            int amountNodes = currentNodes.Count;

            for (int i = 0; i < amountNodes; ++i)
            {
                // The current node equals the node with the least F
                Node currentNode = currentNodes[0];
                // First, find this node
                foreach (Node node in currentNodes)
                {
                    if (currentNode.F > node.F)
                        currentNode = node;
                }

                // Remove it from inProgressNodes and add it to testedNodes
                currentNodes.Remove(currentNode);
                alreadyTestedNodes.Add(currentNode);

                // Check if this node is the endNode
                if (currentNode != null && currentNode.occupying == CharacterType.Ally)
                {
                    return currentNode;
                }


                // Generate children
                Vector2Int inProgNodePos = currentNode.position; // For quick reference

                // Check above node
                Vector2Int testPos = new Vector2Int(inProgNodePos.x, inProgNodePos.y + 1);
                Node testNode = mAContRef.GetNodeAtPosition(testPos);
                // Test the node and see if it should be added to current nodes
                mAContRef.PathingTestNode(testPos, currentNodes, alreadyTestedNodes, currentNode, allyNode.position, CharacterType.Enemy, false);

                // Check left node
                testPos = new Vector2Int(inProgNodePos.x - 1, inProgNodePos.y);
                testNode = mAContRef.GetNodeAtPosition(testPos);
                // Test the node and see if it should be added to current nodes
                mAContRef.PathingTestNode(testPos, currentNodes, alreadyTestedNodes, currentNode, allyNode.position, CharacterType.Enemy, false);

                // Check right node
                testPos = new Vector2Int(inProgNodePos.x + 1, inProgNodePos.y);
                testNode = mAContRef.GetNodeAtPosition(testPos);
                // Test the node and see if it should be added to current nodes
                mAContRef.PathingTestNode(testPos, currentNodes, alreadyTestedNodes, currentNode, allyNode.position, CharacterType.Enemy, false);

                // Check down node
                testPos = new Vector2Int(inProgNodePos.x, inProgNodePos.y - 1);
                testNode = mAContRef.GetNodeAtPosition(testPos);
                // Test the node and see if it should be added to current nodes
                mAContRef.PathingTestNode(testPos, currentNodes, alreadyTestedNodes, currentNode, allyNode.position, CharacterType.Enemy, false);
            }
            //++curDepth;
        }
        // Found no allies on the map
        return null;
    }

    /// <summary>
    /// Starts a coroutine of nextEnemy
    /// </summary>
    public void StartNextEnemy()
    {
        StartCoroutine(NextEnemy());
    }
}
