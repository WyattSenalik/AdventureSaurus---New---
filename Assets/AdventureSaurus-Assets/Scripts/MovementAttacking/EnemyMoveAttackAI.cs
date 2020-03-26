using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMoveAttackAI : MonoBehaviour
{
    // Range the enemies will aggro
    [SerializeField] private int _aggroRange = 16;

    // The parent of all the characters
    private Transform _charParent;

    // Reference to the MoveAttackController script
    private MoveAttackController _mAContRef;

    // List of all allies' MoveAttack scripts
    private List<MoveAttack> _alliesMA;
    // List of all enemies' MoveAttack scripts
    private List<MoveAttack> _enemiesMA;
    public List<MoveAttack> EnemiesList
    {
        get { return _enemiesMA; }
    }
    // The current enemy in enemiesMA that should be moved
    private int _enemyIndex;
    // Where the character should attack
    private Vector2Int _curAttackNodePos;
    // The currenet enemy reference
    private MoveAttack _currentEnemy;
    public MoveAttack CurrentEnemy
    {
        get { return _currentEnemy; }
    }
    // If the current enemy is an active enemy that will be moving
    private bool _curEnemyActive;
    // If an enemy is still taking its turn
    private bool _duringEnemyTurn;

    // Events
    // For when the enemy turn ends
    public delegate void EnemyTurnEnd();
    public static event EnemyTurnEnd OnEnemyTurnEnd;
    // For when a single enemy starts their turn
    public delegate void BeginSingleEnemy(MoveAttack enMARef);
    public static event BeginSingleEnemy OnBeginSingleEnemy;

    // Called when the gameobject is toggled on
    // Subscribe to events
    private void OnEnable()
    {
        // When the turn system says its the enemy's turn start taking turns
        TurnSystem.OnBeginEnemyTurn += StartTakeTurn;
        // When the camera finishes panning to an enemy on the enemy's turn, start that enemy's actions
        CamFollow.OnFinishEnemyPan += TakeSingleTurn;
    }

    // Called when the gameobject is toggled off
    // Unsubscribe to events
    private void OnDisable()
    {
        TurnSystem.OnBeginEnemyTurn -= StartTakeTurn;
        CamFollow.OnFinishEnemyPan -= TakeSingleTurn;
    }

    // Set References
    private void Awake()
    {
        GameObject gameController = GameObject.FindWithTag("GameController");
        // Make sure a GameController exists
        if (gameController == null)
            Debug.Log("Could not find any GameObject with the tag GameController");
        else
        {
            _mAContRef = gameController.GetComponent<MoveAttackController>();
            // Make sure we verify we actually found a MoveAttackController
            if (_mAContRef == null)
                Debug.Log("There was no MoveAttackController attached to " + gameController.name);
        }

        _enemiesMA = new List<MoveAttack>();
    }

    /// <summary>
    /// For setting variables and list each time we start a turn.
    /// Called from ProceduralGenerationController
    /// </summary>
    /// <param name="characterParent">The parent of all the characters</param>
    public void Initialize(Transform characterParent = null)
    {
        // If its not null, set the character parent
        if (characterParent != null)
            _charParent = characterParent;

        _alliesMA = new List<MoveAttack>();
        _enemyIndex = 0;
        // Iterate over each character to find all the allies
        foreach (Transform character in _charParent)
        {
            MoveAttack ma = character.GetComponent<MoveAttack>();
            if (ma == null)
            {
                //Debug.Log("There was no MoveAttack attached to " + character.name);
                continue;
            }


            if (ma.WhatAmI == CharacterType.Ally)
            {
                _alliesMA.Add(ma);
            }
        }
        // Bring out your dead *rings bell* Bring out your dead
        // We have to go over the enemies list and remove any dead enemies
        for (int i = 0; i < _enemiesMA.Count; ++i)
        {
            // If we find a dead enemy, get rid of them
            if (_enemiesMA[i] == null)
            {
                _enemiesMA.RemoveAt(i);
                // We also need to decrement i so that the list does not stop prematurely since the list now shrank by 1
                --i;
            }
        }
        _duringEnemyTurn = false;
    }

    /// <summary>
    /// Finds all the enmies and starts to make the first enemy move
    /// </summary>
    private void StartTakeTurn()
    {
        //Debug.Log("Starting the enemy AI");
        // Refind all the allies, set enemyIndex to 0, and remove any dead enemies from the list
        Initialize();
        // Start from the first enemy
        StartNextEnemy();
    }

    /// <summary>
    /// Has the current enemy move and then increments it so that the next time this is called, the next enemy will move
    /// </summary>
    private IEnumerator NextEnemy()
    {
        // So that the this enemy can't start until the previous one is done
        while (_duringEnemyTurn)
        {
            yield return null;
        }
        _duringEnemyTurn = true;

        if (_enemyIndex < _enemiesMA.Count)
        {
            // Try to get the current enemy we should move
            _currentEnemy = _enemiesMA[_enemyIndex];
            // If the enemy does not exist, do not try to move it
            if (_currentEnemy != null)
            {
                // Call the begin single enemy event
                if (OnBeginSingleEnemy != null)
                    OnBeginSingleEnemy(_currentEnemy);


                //Debug.Log("Begin " + _currentEnemy.name + "'s turn");
                // See if the current enemy will be active
                _curEnemyActive = false;
                Node enemyNode = _mAContRef.GetNodeByWorldPosition(_currentEnemy.transform.position);
                for (int i = 0; i < _alliesMA.Count; ++i)
                {
                    MoveAttack ally = _alliesMA[i];
                    // Make sure the enemy exists
                    if (ally == null)
                    {
                        _alliesMA.RemoveAt(i);
                        --i;
                        continue;
                    }

                    Node allyNode = _mAContRef.GetNodeByWorldPosition(ally.transform.position);
                    if (Mathf.Abs(enemyNode.position.x - allyNode.position.x) + Mathf.Abs(enemyNode.position.y - allyNode.position.y) <= _aggroRange)
                    {
                        _curEnemyActive = true;
                        break;
                    }
                }
            }
            // Have the current enemy take their turn
            // Now called from the CamFollow OnFinishEnemyPan event
            //TakeSingleTurn();
        }
        else
        {
            //Debug.Log("All enemies done");
            if (OnEnemyTurnEnd != null)
                OnEnemyTurnEnd();
        }

        _duringEnemyTurn = false;
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
        _currentEnemy = _enemiesMA[_enemyIndex];
        // If the enemy does not exist, do not try to move it
        if (_currentEnemy == null)
        {
            Debug.Log("We done bois, I'm don't exist");
            return;
        }
        //Debug.Log("Moving " + currentEnemy.name);

        Node startNode = _mAContRef.GetNodeByWorldPosition(_currentEnemy.transform.position);
        // Reset this enemies movement
        _currentEnemy.CalcMoveTiles();
        _currentEnemy.CalcAttackTiles();

        // Get the node this character should try to attack and the node this character should move to
        _curAttackNodePos = FindDesiredAttackNodePos();
        //Debug.Log("Found node to attack at " + curAttackNodePos);
        Node desiredNode = FindDesiredMovementNode();
        // If the node returns null, it means we cannot do anything with this enemy
        if (desiredNode == null)
        {
            // Debug.Log(currentEnemy.name + " Attack Node: " + curAttackNodePos);
            Debug.Log("Desired node is null");
            AttemptAttack();
            EndSingleTurn();
            return;
        }
        // See if they are trying to move where a character already is
        else if (desiredNode.occupying != CharacterType.None)
        {
            if (desiredNode != startNode)
            {
                Debug.Log("Wrong move pal");
                Debug.Log(_currentEnemy.name + " Start Node: " + startNode.position + ". End Node: " + desiredNode.position);
                Debug.Log(_currentEnemy.name + " Attack Node: " + _curAttackNodePos);
            }
            AttemptAttack();
            EndSingleTurn();
            return;
        }
        // Debug.Log(currentEnemy.name + " Start Node: " + startNode.position + ". End Node: " + desiredNode.position);
        // Debug.Log(currentEnemy.name + " Attack Node: " + curAttackNodePos);

        // Calculate the pathing

        // If they successfully pathed
        if (_mAContRef.Pathing(startNode, desiredNode, _currentEnemy.WhatAmI))
        {
            // Start moving the character
            _currentEnemy.StartMove();
        }
        // If they didn't just attempt an attack
        else
        {
            _mAContRef.ResetPathing();
            AttemptAttack();
        }

        // Attacking will be called after the enemy has moved

        // End the single turn
        EndSingleTurn();
    }

    /// <summary>
    /// Ends a single enemies turn. Makes it so that enemy has both moved and attacked,
    /// then increments the enemy index.
    /// </summary>
    private void EndSingleTurn()
    {
        _currentEnemy.HasMoved = true;
        _currentEnemy.HasAttacked = true;
        ++_enemyIndex;
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
        for (int i = 0; i < _alliesMA.Count; ++i)
        {
            MoveAttack ally = _alliesMA[i];
            // Make sure the enemy exists
            if (ally == null)
            {
                _alliesMA.RemoveAt(i);
                --i;
                continue;
            }
            // Get that ally's node
            Node allyNode = _mAContRef.GetNodeByWorldPosition(ally.transform.position);
            // If that ally's node is in this enemy's move tiles, they are in range, so that is the node we want to reach
            if (_currentEnemy.AttackTiles.Contains(allyNode))
            {
                //Debug.Log("Found ally to attack at " + allyGridPos);
                return allyNode.position;
            }
        }
        //Debug.Log("No ally in range");
        // If there is no ally in range, we don't want to attack anything with a character on it, so we return a position not on the grid
        return new Vector2Int(_mAContRef.GridTopLeft.x - 1, 0);
    }

    /// <summary>
    /// Called from MoveAttack by enemies. Attempts to have the enemy attack
    /// </summary>
    public void AttemptAttack()
    {
        // If the enemy does not exist, do not try to attack something
        if (_currentEnemy == null)
        {
            Debug.Log("No enemy is current");
            return;
        }
        // Try to cast the curAttackNodePos to a node, if there is no node there, don't attack
        Node nodeToAttack = _mAContRef.GetNodeAtPosition(_curAttackNodePos);
        if (nodeToAttack == null)
        {
            _currentEnemy.HasAttacked = true;
            StartNextEnemy();
            return;
        }
        // Otherwise attack it
        _currentEnemy.StartAttack(_curAttackNodePos);
    }

    /// <summary>
    /// Finds the node that the enemy "wants" to move to
    /// </summary>
    /// <returns>A Node that is the Node the enemy has chosen to move to</returns>
    private Node FindDesiredMovementNode()
    {
        // Determine if we found a node with an ally on it, by just checking if the position is on the grid
        Node nodeToAttack = _mAContRef.GetNodeAtPosition(_curAttackNodePos);
        Node currentEnemyNode = _mAContRef.GetNodeByWorldPosition(_currentEnemy.transform.position);
        if (currentEnemyNode == null)
        {
            Debug.Log("WARNING - BUG DETECTED: It seems there is a problem in the FindDesiredMovementNode function of EnemyMoveAttackAI " +
                    "attached to " + this.name + "" + ". Double click this message for more information.");
            // It seems there is no node where this character is standing. That doesn't make a lot of sense to me
        }
        // If we found an ally in range to attack
        if (nodeToAttack != null && nodeToAttack.occupying == CharacterType.Ally)
        {
            Debug.Log(_currentEnemy.name + " is close enough to strike");
            // Check to make sure the enemy has a place to stand to attack and that the enemy can reach that node
            //
            // Get the nodes the enemy can attack the ally from
            List<Node> allyAttackNodes = _mAContRef.GetNodesDistFromNode(nodeToAttack, _currentEnemy.AttackRange);
            // Debug.Log(currentEnemy.name + " ally attack nodes in FindDesiredMovementode: " + allyAttackNodes);
            // Test to make sure allyAttackNodes contains something
            if (allyAttackNodes.Count <= 0)
            {
                return null;
            }

            // The closest node to the current enemy that they can hit the enemy from. Assume its none of them
            Node closestAttackFromNode = null;
            // Assign this to basically infinity
            int closestAttackFromNodeDist = Mathf.Abs(_mAContRef.GridBotRight.x - _mAContRef.GridTopLeft.x) +
                Mathf.Abs(_mAContRef.GridTopLeft.y - _mAContRef.GridBotRight.y) + 2;

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
                        if (_mAContRef.Pathing(currentEnemyNode, allyAttackNodes[j], CharacterType.Enemy, false))
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
            Debug.Log(_currentEnemy.name + " can't attack an ally this turn");
            // If we have no ally to attack, we need to find the closest ally to me and move as close to them as possible
            Node[] closeNodes = FindAllyOutOfRange();
            Node closestAllyNode = closeNodes[0];
            Node closestAttackFromNode = closeNodes[1];
            // If there are no closest allies, we just should move in place
            if (closestAllyNode == null)
            {
                return currentEnemyNode;
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

            Debug.Log("Depth Test: 0 for " + _currentEnemy.name);
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
                    Debug.Log("Congratulations " + _currentEnemy.name + "!!! Even without being in striking distance of an ally, you managed to find" +
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
                        List<Node> potNodesToAdd = _mAContRef.GetNodesDistFromNode(touchOffNodes[0], 1);
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
                    Debug.Log("Depth Test: " + moveRangeDecrement + " for " + _currentEnemy.name);
                    // We want to sort the nodes in the list based on their F value
                    nodesToTest.Sort(new NodeComp());
                }

            } while (moveRangeDecrement < _currentEnemy.MoveRange);

            // If we made it here, we just give the lastResortNode
            return lastResortNode;
        }
    }

    /// <summary>
    /// Finds the closest ally to the current enemy
    /// </summary>
    /// <returns>Returns the node that the closest ally is on [0]. Also returns the closest node to attack that ally from [1]</returns>
    private Node[] FindAllyOutOfRange()
    {
        // Get the node the current enemy is at
        Node startNode = _mAContRef.GetNodeByWorldPosition(_currentEnemy.transform.position);

        // Find the allies, and see if they are more than maxDepth grid units away anyway
        // If they are, we just return null
        Node closestAllyNode = null; // The node of the closest ally
        // The closest allies distance from the currentEnemy
        int closestAllyDist = int.MaxValue;
        // The closest ally in terms of Node.F
        int closestF = int.MaxValue;
        // Closest node to attack the closest ally from
        Node closestAttackNode = null;
        for (int i = 0; i < _alliesMA.Count; ++i)
        {
            MoveAttack curAlly = _alliesMA[i];
            // Make sure the current ally exists
            if (curAlly == null)
            {
                continue;
            }

            Node curAllyNode = _mAContRef.GetNodeByWorldPosition(curAlly.transform.position);
            int curAllyDist = Mathf.Abs(startNode.position.x - curAllyNode.position.x) + Mathf.Abs(startNode.position.y - curAllyNode.position.y);
            // Quick check if ally is close to this enemy and is closer than the current closest ally
            if (curAllyDist <= _aggroRange &&  curAllyDist < closestAllyDist)
            {
                // Check to make sure the enemy has a place to stand to attack and that the enemy can reach that node
                //
                // Get the nodes the enemy can attack the ally from
                List<Node> allyAttackNodes = _mAContRef.GetNodesDistFromNode(curAllyNode, _currentEnemy.AttackRange);
                // Iterate over each of the nodes the enemy could potentially stand at to attack
                for (int j = 0; j < allyAttackNodes.Count; ++j)
                {
                    // See if the node exists and there is no character there
                    if (allyAttackNodes[j] != null && allyAttackNodes[j].occupying == CharacterType.None)
                    {
                        // See if there is a path to there for enemies
                        if (_mAContRef.Pathing(startNode, allyAttackNodes[j], CharacterType.Enemy))
                        {
                            // See if the F value is lower
                            if (closestF > startNode.F)
                            {
                                closestF = startNode.F;
                                _mAContRef.ResetPathing();
                                closestAllyNode = curAllyNode;
                                closestAllyDist = curAllyDist;
                                closestAttackNode = allyAttackNodes[j];
                            }
                        }
                    }
                }
            }
        }
        Node[] rtnList = { closestAllyNode, closestAttackNode };
        // Returns null if the closestAlly node was not found
        return rtnList;
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
        Debug.Log(_currentEnemy.name + " is trying to go to " + closestAttackFromNode.position);
        if (!_mAContRef.Pathing(startNode, closestAttackFromNode, CharacterType.Enemy))
        {
            _mAContRef.ResetPathing();
            Debug.Log("WARNING - BUG DETECTED: It seems there is a problem in the FindGoalNode function of EnemyMoveAttackAI " +
                "attached to " + this.name + "" + ". Double click this message for more information.");
            // See statement above "if" for more information
            return null;
        }

        // If our pathing was successful, we need to see where this enemy would stop along this path
        Node goalNode = startNode; // Start the goal node as the startNode
        // Iterate over the nodes, once we finish, the goal node will be the one the enemy would stop at
        for (int i = 0; i < _currentEnemy.MoveRange - moveRangeDecrement; ++i)
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
            if (!_enemiesMA.Contains(enemy))
                _enemiesMA.Add(enemy);

            Rainbow rainbowRef = enemy.GetComponent<Rainbow>();
            if (rainbowRef != null)
                rainbowRef.StartFlashing();
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
            _enemiesMA.Add(enemy);
        }
    }
}
