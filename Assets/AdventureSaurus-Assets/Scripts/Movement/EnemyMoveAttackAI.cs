using System.Collections;
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
        Debug.Log("Starting the enemy AI");
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
                Debug.Log("Begin " + currentEnemy.name + "'s turn");
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

                //Debug.Log("Will " + currentEnemy.name + " be active? " + curEnemyActive);
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
            Debug.Log("All enemies done");
            turnSysRef.StartPlayerTurn();
        }

        duringEnemyTurn = false;
        yield return null;
    }

    /// <summary>
    /// Starts a coroutine of nextEnemy
    /// </summary>
    public void StartNextEnemy()
    {
        StartCoroutine(NextEnemy());
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
            Debug.Log("We done bois, I'm don't exist");
            return;
        }
        else
        {
            //Debug.Log("Moving " + currentEnemy.name);
        }

        Node startNode = mAContRef.GetNodeByWorldPosition(currentEnemy.transform.position);
        // Reset this enemies movement
        currentEnemy.CalcMoveTiles();
        currentEnemy.CalcAttackTiles();

        // Get the node this character should try to attack and the node this character should move to
        curAttackNodePos = FindDesiredAttackNodePos();
        //Debug.Log("Found node to attack at " + curAttackNodePos);
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
            if (desiredNode != startNode)
            {
                Debug.Log("Wrong move pal");
                Debug.Log(currentEnemy.name + " Start Node: " + startNode.position + ". End Node: " + desiredNode.position);
                Debug.Log(currentEnemy.name + " Attack Node: " + curAttackNodePos);
            }
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
    private Vector2Int FindDesiredAttackNodePos()
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
        // Try to cast the curAttackNodePos to a node, if there is no node there, don't attack
        Node nodeToAttack = mAContRef.GetNodeAtPosition(curAttackNodePos);
        if (nodeToAttack == null)
        {
            currentEnemy.HasAttacked = true;
            StartNextEnemy();
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
        if (currentEnemyNode == null)
        {
            Debug.Log("WARNING - BUG DETECTED: It seems there is a problem in the FindDesiredMovementNode function of EnemyMoveAttackAI " +
                    "attached to " + this.name + "" + ". Double click this message for more information.");
            // It seems there is no node where this character is standing. That doesn't make a lot of sense to me
        }
        // If we found an ally in range to attack
        if (nodeToAttack != null && nodeToAttack.occupying == CharacterType.Ally)
        {
            Debug.Log(currentEnemy.name + " is close enough to strike");
            // Check to make sure the enemy has a place to stand to attack and that the enemy can reach that node
            //
            // Get the nodes the enemy can attack the ally from
            List<Node> allyAttackNodes = mAContRef.GetNodesDistFromNode(nodeToAttack, currentEnemy.AttackRange);
            // Debug.Log(currentEnemy.name + " ally attack nodes in FindDesiredMovementode: " + allyAttackNodes);
            // Test to make sure allyAttackNodes contains something
            if (allyAttackNodes.Count <= 0)
            {
                return null;
            }

            // The closest node to the current enemy that they can hit the enemy from. Assume its none of them
            Node closestAttackFromNode = null;
            // Assign this to basically infinity
            int closestAttackFromNodeDist = Mathf.Abs(mAContRef.GridBotRight.x - mAContRef.GridTopLeft.x) +
                Mathf.Abs(mAContRef.GridTopLeft.y - mAContRef.GridBotRight.y) + 2;

            // Iterate over each of the nodes the enemy could potentially stand at to attack
            for (int j = 0; j < allyAttackNodes.Count; ++j)
            {
                // Debug.Log(currentEnemy.name + " single node at j in FindDesiredMovementode: " + allyAttackNodes[j]);
                // See if the node exists and there is no character there (besides potentially this character
                if (allyAttackNodes[j] != null && (allyAttackNodes[j].occupying == CharacterType.None || allyAttackNodes[j] == currentEnemyNode))
                {
                    // Test if it is closer than the currently closest node
                    Node currentTestNode = allyAttackNodes[j];
                    int currentTestNodeDist = Mathf.Abs(currentTestNode.position.x - currentEnemyNode.position.x) +
                        Mathf.Abs(currentTestNode.position.y - currentEnemyNode.position.y);
                    //Debug.Log("Testing node at " + currentTestNode.position + ", which is " + currentTestNodeDist + " away from me at " + currentEnemyNode.position);
                    if (currentTestNodeDist < closestAttackFromNodeDist)
                    {
                        // Debug.Log(currentEnemy.name + " single node occupying at j in FindDesiredMovementode: " + allyAttackNodes[j].occupying);
                        // See if there is a path to there for the current enemy
                        if (mAContRef.Pathing(currentEnemyNode, allyAttackNodes[j], CharacterType.Enemy, false))
                        {
                            // Set the new closest node to attack
                            closestAttackFromNode = currentTestNode;
                            closestAttackFromNodeDist = currentTestNodeDist;
                            //Debug.Log("Found closer node to move to " + closestAttackFromNode.position + " in order to attack " + nodeToAttack.position);
                        }
                    }
                }
            }
            //Debug.Log("Found final node to move to " + closestAttackFromNode.position + " in order to attack " + nodeToAttack.position);
            return closestAttackFromNode;
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
                return currentEnemyNode;
            }

            // We want to get a list of the nodes that we can attack from arround the ally
            List<Node> potAttackFromNodes = mAContRef.GetNodesDistFromNode(closestAllyNode, currentEnemy.AttackRange);
            // We remove the nodes that already have something occupying them
            for (int i = 0; i < potAttackFromNodes.Count; ++i)
            {
                if (potAttackFromNodes[i].occupying != CharacterType.None)
                {
                    potAttackFromNodes.RemoveAt(i);
                    --i;
                }
            }

            // Determine the closest node out of those nodes
            Node closestAttackFromNode = null;
            if (potAttackFromNodes.Count > 0)
            {
                // Assume the first node is the closest
                closestAttackFromNode = potAttackFromNodes[0];
                int closestDist = Mathf.Abs(closestAttackFromNode.position.x - currentEnemyNode.position.x) +
                    Mathf.Abs(closestAttackFromNode.position.y - currentEnemyNode.position.y);
                for (int i = 1; i < potAttackFromNodes.Count; ++i)
                {
                    // Determine the distance from the current node
                    int currentDist = Mathf.Abs(potAttackFromNodes[i].position.x - currentEnemyNode.position.x) +
                        Mathf.Abs(potAttackFromNodes[i].position.y - currentEnemyNode.position.y);
                    // If we find a node that is closer than the closestAttackNode, save it as the new closest
                    if (currentDist < closestDist)
                    {
                        closestAttackFromNode = potAttackFromNodes[i];
                        closestDist = currentDist;
                    }
                }
            }

            // Test if we got a node or not
            // If we did not get a node, we have a problem. FindAllyOutOfRange should have only given us an ally with an opening to attack,
            // you will need to check that debug why it gave an ally this ally could not attack
            if (closestAttackFromNode == null)
            {
                Debug.Log("WARNING - BUG DETECTED: It seems there is a problem in the FindDesiredMovementNode function of EnemyMoveAttackAI " +
                    "attached to " + this.name + "" + ". Double click this message for more information.");
                // See statement above "if" for more information
                return currentEnemyNode;
            }

            // If we found a node to attack the ally from, we now need to start determining the path to that node. First, we try pathing
            // towards the node, not caring if we can actually make it the full there. Then we figure out which node along that path
            // this enemy would stop at. If that is a free spot, we just follow that path and done.
            //
            // Find the path to that found node, we don't care about if we can actually make it the full way there
            Node startNode = currentEnemyNode;
            int moveRangeDecrement = 0;
            Node lastResortNode = null; // If we are unable to fully move anywhere, this node will be returned. Will be updated in the do-while
            // The distance lastResortNode is from our target node. Start it out as infinite
            int lastResortNodeDist = int.MaxValue;

            // This list will hold the nodes we have already tested pathing from
            // and we have tested pathing from their adjacent nodes
            List<Node> finishedNodes = new List<Node>();
            // This list will hold the nodes that we have tested pathing from, but have not yet
            // tested pathing from their adjacent nodes
            List<Node> touchOffNodes = new List<Node>();
            // This list will hold the nodes we have not tested pathing from yet (obviously we have not
            // tested pathing from their adjacent nodes)
            List<Node> nodesToTest = new List<Node>();
            nodesToTest.Add(startNode);

            Debug.Log("Depth Test: 0 for " + currentEnemy.name);
            do
            {
                // Update the start node to be the first node to test
                startNode = nodesToTest[0];

                // We let FindGoalNode determine the path and the node the current enemy would stop at along that path
                // Pathing gets called in FindGoalNode
                Node goalNode = FindGoalNode(startNode, closestAttackFromNode, moveRangeDecrement);

                // Test if the newly found goalNode is already occupied
                // If its not, we're done! We just need to return this goal node
                if (goalNode.occupying == CharacterType.None)
                {
                    Debug.Log("Congratulations " + currentEnemy.name + "!!! Even without being in striking distance of an ally, you managed to find" +
                        " a good place to move: " + goalNode.position + ". Good for you.");
                    return goalNode;
                }
                // For if we can't get to the node, we need to update lastResort if this startNode is closer
                int currentStartNodeDist = startNode.F;
                if (startNode.occupying == CharacterType.None && currentStartNodeDist < lastResortNodeDist)
                {
                    lastResortNode = startNode;
                    lastResortNodeDist = currentStartNodeDist;
                }

                // If it is occupied, we need to devise a new solution. Pathing straight from this enemy to that closestAttackFromNode will not get this
                // enemy to move as we wish. We will now try pathing from the nodes adjacent to this enemy and just decrement the amount it is able to move
                // by pretending the enemy has moved to them as one of its steps. Then we will preform the same test we just did above from the initialization
                // of the startNode variable
                nodesToTest.Remove(startNode); // Remove the startNode from the nodesToTest, since we just finished testing it
                touchOffNodes.Add(startNode); // Add the startNode to the touchOffNodes, so that we may later add the adjacent nodes to nodesToTest
                // Make sure there are nodes in touchOffNodes before we attempt to get the 0th index
                if (touchOffNodes.Count <= 0)
                {
                    Debug.Log("WARNING - BUG DETECTED: It seems there is a problem in the FindDesiredMovementNode function of EnemyMoveAttackAI " +
                        "attached to " + this.name + "" + ". Double click this message for more information.");
                    // We encountered a situation where touchOffNodes was empty, this means that we couldn't reach the enemy and it was not properly tested
                    break;
                }
                // Test if nodes to test is empty, if it is, we increment moveRangeDecrement and the 4 adjacent nodes
                // of the touchOffNode at index 0 to the nodesToTest and add the touchOffNode to finishedNodes
                // This is a while, but will often only happen once
                while (nodesToTest.Count == 0)
                {
                    // We want to add all the adjacent nodes of the touchOffNodes to the current nodes so that we test all nodes
                    // n spaces out at the same time
                    while (touchOffNodes.Count > 0)
                    {
                        // Find the adjacent nodes from the first node in touchOffNodes
                        List<Node> potNodesToAdd = mAContRef.GetNodesDistFromNode(touchOffNodes[0], 1);
                        // Add the first touchOffNode to finished nodes since it has now been searched and we have gotten its adjacent nodes
                        // We can also now remove it from touchOffNodes
                        finishedNodes.Add(touchOffNodes[0]);
                        touchOffNodes.RemoveAt(0);
                        // Iterate over these potNodesToAdd and add any node that has not already been tested to the nodes to test
                        foreach (Node potNode in potNodesToAdd)
                        {
                            // See if it has already been tested or that it is impassable
                            if (finishedNodes.Contains(potNode) || touchOffNodes.Contains(potNode) ||
                                potNode.occupying == CharacterType.Wall || potNode.occupying == CharacterType.Ally)
                            {
                                continue;
                            }
                            // Otherwise add it to the nodesToTest
                            nodesToTest.Add(potNode);
                        }
                    }
                    // The next nodes are 1 further than we have moved before probably
                    ++moveRangeDecrement;
                    Debug.Log("Depth Test: " + moveRangeDecrement + " for " + currentEnemy.name);
                    // We want to sort the nodes in the list based on their F value
                    nodesToTest.Sort(new NodeComp());
                }

            } while (moveRangeDecrement < currentEnemy.MoveRange);

            // If we made it here, we just give the lastResortNode
            return lastResortNode;
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
    /// Called from FindDesiredMovementNode when an enemy is out of range.
    /// Finds the Node the currentEnemy would stop at when pathing from the startNode to the closestAttackFromNode
    /// </summary>
    /// <param name="startNode">Node to start pathing from</param>
    /// <param name="closestAttackFromNode">Node to end pathing at</param>
    /// <param name="moveRangeDecrement">The amount of steps from move range to ignore</param>
    /// <returns>The Node the currentEnemy would stop at on the path from startNode to closestAttackFromNode. Null if pathing failed.</returns>
    private Node FindGoalNode(Node startNode, Node closestAttackFromNode, int moveRangeDecrement=0)
    {
        // If our pathing was not sucessful, we have a big problem. It should never be the case that our pathing failed.
        // FindAllyOutOfRange should have returned an ally which it was possible to attack. Furthermore, if that function returned
        // null, we should have broken from this function long ago. It may be the case that the closestAttackFromNode is incorrect,
        // if that is the case, check the loop that finds this node above. Or this may be a Pathing bug in the MoveAttackController
        // script.
        Debug.Log(currentEnemy.name + " is trying to go to " + closestAttackFromNode.position);
        if (!mAContRef.Pathing(startNode, closestAttackFromNode, CharacterType.Enemy))
        {
            mAContRef.ResetPathing();
            Debug.Log("WARNING - BUG DETECTED: It seems there is a problem in the FindGoalNode function of EnemyMoveAttackAI " +
                "attached to " + this.name + "" + ". Double click this message for more information.");
            // See statement above "if" for more information
            return null;
        }

        // If our pathing was successful, we need to see where this enemy would stop along this path
        Node goalNode = startNode; // Start the goal node as the startNode
        // Iterate over the nodes, once we finish, the goal node will be the one the enemy would stop at
        for (int i = 0; i < currentEnemy.MoveRange - moveRangeDecrement; ++i)
        {
            if (goalNode.whereToGo == null)
            {
                Debug.Log("WARNING - BUG DETECTED: It seems there is a problem in the FindGoalNode function of EnemyMoveAttackAI " +
                "attached to " + this.name + "" + ". Double click this message for more information.");
                // Seems like pathing failed, but was not caught above
                break;
            }
            goalNode = goalNode.whereToGo;
        }

        return goalNode;
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
