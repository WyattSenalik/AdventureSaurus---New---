using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAttackController : MonoBehaviour
{
    [SerializeField] private Vector2Int gridTopLeft = new Vector2Int(0, 0); // Top-Left coordinates of the grid
    [SerializeField] private Vector2Int gridBotRight = new Vector2Int(0, 0);    // Bottom-Right coordinates of the grid
    public Vector2Int GridTopLeft
    {
        get { return gridTopLeft; }
    }
    public Vector2Int GridBotRight
    {
        get { return gridBotRight; }
    }
    [SerializeField] private Transform wallParent = null;   // Parent of all wall objects
    [SerializeField] private Transform charParent = null;   // Parent of all character (ally and enemy) objects
    private List<List<Node>> grid;  // The movement grid the characters are situated on

    /// <summary>
    /// We have to wait for characters to set all their references first, so we go in start
    /// </summary>
    private void Start()
    {
        CreateGrid();
        FindWalls();
        FindCharacters();
        CreateAllVisualTiles();
    }

    /// <summary>
    /// Initialize the grid to have nodes at all possible grid points
    /// </summary>
    private void CreateGrid()
    {
        grid = new List<List<Node>>();  // Initialize the grid to be an empty list of empty lists of nodes
        // Iterate to create rows of the grid
        for (int i = gridBotRight.y; i <= gridTopLeft.y; ++i)
        {
            List<Node> row = new List<Node>();  // Create the next row
            grid.Add(row);  // Add the new row
            // Iterate to create nodes in each row
            for (int j = gridTopLeft.x; j <= gridBotRight.x; ++j)
            {
                row.Add(new Node(new Vector2Int(j, i)));    // Add the node to the row
            }
        }
    }

    /// <summary>
    /// Sets all nodes that share a location with a child of wallParent to be occupied by a wall
    /// </summary>
    private void FindWalls()
    {
        foreach (Transform wall in wallParent)
        {
            UpdateGrid(wall, CharacterType.Wall);
        }
    }

    /// <summary>
    /// Sets all nodes that share a location with a child of charParent to be occupied by the kind of 
    /// </summary>
    private void FindCharacters()
    {
        foreach (Transform character in charParent)
        {
            UpdateGrid(character, character.GetComponent<MoveAttack>().WhatAmI);
        }
    }

    /// <summary>
    /// Finds the node at the position of the occupant and sets it to the passed charType
    /// </summary>
    /// <param name="occupant">The Transform of what is located on the node</param>
    /// <param name="charType">The CharacterType of what kind of thing is on the node</param>
    private void UpdateGrid(Transform occupant, CharacterType charType)
    {
        // Get the node at the position of occupant
        Node occupantNode = GetNodeAtPosition(new Vector2Int(Mathf.RoundToInt(occupant.position.x), Mathf.RoundToInt(occupant.position.y)));
        // Set that node to be occupied
        occupantNode.occupying = charType;
    }

    /// <summary>
    /// Creates the visual tiles for when a character is clicked on
    /// </summary>
    public void CreateAllVisualTiles()
    {
        foreach (Transform character in charParent)
        {
            MoveAttack mARef = character.GetComponent<MoveAttack>();
            mARef.CalcMoveTiles();
            mARef.CalcAttackTiles();
            mARef.CreateVisualTiles(false);
        }
    }

    /// <summary>
    /// Returns a node at the given position. If it finds no nodes, it returns null
    /// </summary>
    /// <param name="pos">The position to look for the node</param>
    /// <returns>A node with the same position as the entered position. Or null</returns>
    public Node GetNodeAtPosition(Vector2Int pos)
    {
        // Iterate over every row
        foreach (List<Node> row in grid)
        {
            // Iterate over each node in that row
            foreach (Node node in row)
            {
                // If we find a node with the desired position
                if (node.position == pos)
                {
                    return node;
                }
            }
        }
        // If we found no node with the desired position
        return null;
    }

    /// <summary>
    /// Finds if there is a character at the specified node. Returns a reference to that character's MoveAttack script
    /// </summary>
    /// <param name="testNode">The node that is being tested to see if there is a character there</param>
    /// <returns>If it finds a character on it, it returns that character's MoveAttack. If it finds no character, returns null</returns>
    public MoveAttack GetCharacterMAByNode(Node testNode)
    {
        foreach (Transform character in charParent)
        {
            // Convert the character's position to grid point
            Vector2Int charGridPos = new Vector2Int(Mathf.RoundToInt(character.position.x), Mathf.RoundToInt(character.position.y));
            // If the character's position on the grid is the same as the testNode's position
            if (charGridPos == testNode.position)
            {
                return character.GetComponent<MoveAttack>();
            }
        }
        return null;
    }

    /// <summary>
    /// Should always be called before Pathing. Resets the whereToGo variable on each node that determines where it should go
    /// </summary>
    public void ResetPathing()
    {
        foreach (List<Node> row in grid)
        {
            foreach (Node node in row)
            {
                node.whereToGo = null;
            }
        }
    }

    /// <summary>
    /// Sets all the whereToGo variables on reachable nodes on the grid from the end node to go towards the endNode in the fastest path.
    /// Characters of the same type can move through one another, but not onto each other.
    /// Walls cannot be traversed by any character
    /// </summary>
    /// <param name="endNode">The node that the pathing is trying to reach</param>
    /// <param name="requesterType">The type of the character who requested the pathing</param>
    /// <param name="shouldCare">If we should test if the node we are moving to is empty or not</param>
    /// <returns>True if the node that we tried to get to was valid, false otherwise</returns>
    public bool Pathing(Node endNode, CharacterType requesterType, bool shouldCare=true)
    {
        // Make the first nodes end node be itself
        endNode.whereToGo = endNode;

        // Make sure the node I want to go to is not occupied
        if (!shouldCare || endNode.occupying == CharacterType.None)
        {
            List<Node> InProgressNodes = new List<Node>();  // The nodes that are being tested
            InProgressNodes.Add(endNode);   // We start with the one we want to reach

            // While we are still testing nodes
            while (InProgressNodes.Count != 0)
            {
                int amountNodes = InProgressNodes.Count;    // The Count will change, so we set it be what we began with
                //Debug.Log("There are " + amountNodes + " nodes in InProgressNodes");

                // Iterate over only the nodes that were in the list before we started testing
                for (int i = 0; i < amountNodes; ++i)
                {
                    Vector2Int inProgNodePos = InProgressNodes[i].position; // For quick reference
                    //Debug.Log("Telling adjacent nodes to come to " + InProgressNodes[i].position);
                    // Check above node
                    Vector2Int testPos = new Vector2Int(inProgNodePos.x, inProgNodePos.y + 1);
                    PathingTestNode(testPos, InProgressNodes, i, requesterType);

                    // Check left node
                    testPos = new Vector2Int(inProgNodePos.x - 1, inProgNodePos.y);
                    PathingTestNode(testPos, InProgressNodes, i, requesterType);

                    // Check right node
                    testPos = new Vector2Int(inProgNodePos.x + 1, inProgNodePos.y);
                    PathingTestNode(testPos, InProgressNodes, i, requesterType);

                    // Check down node
                    testPos = new Vector2Int(inProgNodePos.x, inProgNodePos.y - 1);
                    PathingTestNode(testPos, InProgressNodes, i, requesterType);
                }
                for (int i = 0; i < amountNodes; ++i)
                {
                    InProgressNodes.RemoveAt(0);
                }
            }
            return true;    // Was a valid spot to move
        }
        else
        {
            Debug.Log("You cannot move here, it is occupied by " + endNode.occupying);
            return false;   // Was an invalid node
        }
    }

    /// <summary>
    /// Used to test and set whereToGo of the current testNode in Pathing
    /// </summary>
    /// <param name="testPos">The position of the node being tested</param>
    /// <param name="InProgressNodes">Reference to the List of Nodes that have not been tested yet and need to be</param>
    /// <param name="i">Index of the InProgressNodes we are testing</param>
    /// <param name="requestType">CharacterType the requester is. They can move through their allys</param>
    public void PathingTestNode(Vector2Int testPos, List<Node> InProgressNodes, int i, CharacterType requestType)
    {
        Node testNode = GetNodeAtPosition(testPos);
        if (testNode != null)
        {
            // If the node I am trying to go to has not already been set
            if (testNode.whereToGo == null)
            {
                // If the node I am trying to go to is not occupied or is occupied by someone on my team
                if (testNode.occupying == CharacterType.None || testNode.occupying == requestType)
                {
                    testNode.whereToGo = InProgressNodes[i];
                    InProgressNodes.Add(testNode);
                }
            }
        }
    }

    /// <summary>
    /// Finds a list of all the nodes that can be reached from startNode in moveRadius moves or less
    /// </summary>
    /// <param name="startNode">The node that we are starting the test from</param>
    /// <param name="moveRadius">Amount of nodes that the requester can move</param>
    /// <param name="requesterType">What kind of character the requester is</param>
    /// <returns>A list of nodes that can be reached from the startNode in moveRadius moves or less</returns>
    public List<Node> GetValidMovementNodes(Node startNode, int moveRadius, CharacterType requesterType)
    {
        List<Node> validNodes = new List<Node>();   // This list is what will be returned. It is the nodes that can be moved to
        validNodes.Add(startNode);

        List<Node> currentNodes = new List<Node>(); // This list holds the nodes that have yet to be tested for validity
        currentNodes.Add(startNode);

        int depth = 0;  // This is how many iterations of checks we have gone over. Aka, how many tiles have been traversed in one path
        while (depth < moveRadius)
        {
            int amountNodes = currentNodes.Count;
            for (int i = 0; i < amountNodes; ++i)
            {
                // If the current node is null, end this iteration and start the next one
                if (currentNodes[i] == null)
                    continue;

                Vector2Int curNodePos = currentNodes[i].position;
                // Check above node
                Vector2Int testPos = new Vector2Int(curNodePos.x, curNodePos.y + 1);
                ValidMoveTestNode(testPos, validNodes, currentNodes, requesterType);

                // Check left node
                testPos = new Vector2Int(curNodePos.x - 1, curNodePos.y);
                ValidMoveTestNode(testPos, validNodes, currentNodes, requesterType);

                // Check right node
                testPos = new Vector2Int(curNodePos.x + 1, curNodePos.y);
                ValidMoveTestNode(testPos, validNodes, currentNodes, requesterType);

                // Check down node
                testPos = new Vector2Int(curNodePos.x, curNodePos.y - 1);
                ValidMoveTestNode(testPos, validNodes, currentNodes, requesterType);

            }
            // Removes the nodes that have already been iterated over
            for (int i = 0; i < amountNodes; ++i)
            {
                currentNodes.RemoveAt(0);
            }
            ++depth;
        }
        return validNodes;
    }

    /// <summary>
    /// Used to test the current testNode in GetValidMovementNodes
    /// </summary>
    /// <param name="testPos">Position of the node being tested</param>
    /// <param name="validNodes">Reference to the List of Nodes that have been deemed valid to move to</param>
    /// <param name="currentNodes">Reference to the List of Nodes that still need to be tested</param>
    /// <param name="requestType">Kind of character the requester is. They can move through their allies</param>
    private void ValidMoveTestNode(Vector2Int testPos, List<Node> validNodes, List<Node> currentNodes, CharacterType requestType)
    {
        Node testNode = GetNodeAtPosition(testPos);
        if (testNode != null)
        {
            // If the node is not occupied, I can move there
            if (testNode.occupying == CharacterType.None)
            {
                validNodes.Add(testNode);
                currentNodes.Add(testNode);
            }
            // If it is occupied by someone on my team, I can't move there, but I can move past there
            else if (testNode.occupying == requestType)
            {
                currentNodes.Add(testNode);
            }
        }
    }

    /// <summary>
    /// Finds a list of all the nodes that can be attacked by the requester
    /// </summary>
    /// <param name="moveNodes">The nodes that the requester can attack</param>
    /// <param name="attackRadius">Distance the requester can attack from</param>
    /// <param name="requesterType">What kind of character the requester is</param>
    /// <returns>A list of nodes that can be attacked by the requester</returns>
    public List<Node> GetValidAttackNodes(List<Node> moveNodes, int attackRadius, CharacterType requesterType)
    {
        List<Node> validNodes = new List<Node>();   // This list is what will be returned. It is the nodes that can be attacked

        List<Node> currentNodes = new List<Node>(); // This list holds the nodes that have yet to be tested for validity
        foreach (Node node in moveNodes)
        {
            currentNodes.Add(node);
        }

        int depth = 0;  // This is how many iterations of checks we have gone over. Aka, how many tiles have been traversed in one path
        while (depth < attackRadius)
        {
            int amountNodes = currentNodes.Count;
            for (int i = 0; i < amountNodes; ++i)
            {
                // If the current node is null, end this iteration and start the next one
                if (currentNodes[i] == null)
                    continue;

                Vector2Int curNodePos = currentNodes[i].position;
                // Check above node
                Vector2Int testPos = new Vector2Int(curNodePos.x, curNodePos.y + 1);
                ValidAttackTestNode(testPos, validNodes, currentNodes, requesterType);

                // Check left node
                testPos = new Vector2Int(curNodePos.x - 1, curNodePos.y);
                ValidAttackTestNode(testPos, validNodes, currentNodes, requesterType);

                // Check right node
                testPos = new Vector2Int(curNodePos.x + 1, curNodePos.y);
                ValidAttackTestNode(testPos, validNodes, currentNodes, requesterType);

                // Check down node
                testPos = new Vector2Int(curNodePos.x, curNodePos.y - 1);
                ValidAttackTestNode(testPos, validNodes, currentNodes, requesterType);

            }
            // Removes the nodes that have already been iterated over
            for (int i = 0; i < amountNodes; ++i)
            {
                currentNodes.RemoveAt(0);
            }
            ++depth;
        }
        return validNodes;
    }

    /// <summary>
    /// Used to test the current testNode in GetValidAttackNodes
    /// </summary>
    /// <param name="testPos">Position of the node being tested</param>
    /// <param name="validNodes">Reference to the List of Nodes that have been deemed valid to attack</param>
    /// <param name="currentNodes">Reference to the List of Nodes that still need to be tested for validity</param>
    /// <param name="requestType">Kind of character the requester is. They can only attack character not on their team</param>
    private void ValidAttackTestNode(Vector2Int testPos, List<Node> validNodes, List<Node> currentNodes, CharacterType requesterType)
    {
        Node testNode = GetNodeAtPosition(testPos); // Get the current node
        //Debug.Log("Test tile at " + testPos);
        // If the node exists
        if (testPos != null)
        {
            // If the testNode is not already in the validNodes list, we add it.
            // No other tests are necessary since GetValidAttackNodes will stop calling this once the range is reached
            if (!(validNodes.Contains(testNode)))
            {
                validNodes.Add(testNode);
                currentNodes.Add(testNode);
            }
        }
    }

    /// <summary>
    /// Very similar to GetValidMovementNodes, except only gets the nodes exactly distance from the startNode
    /// </summary>
    /// <param name="startNode">The node that we are calculating away from</param>
    /// <param name="distance">The amount of nodes away from the startNode we want</param>
    /// <param name="requesterType">The kind of character the requester is</param>
    /// <returns>Returns nodes exactly the passed in distance from the startNode</returns>
    public List<Node> GetNodesDistFromNode(Node startNode, int distance)
    {
        List<Node> validNodes = new List<Node>();   // This list is what will be returned. It is the nodes that can be moved to

        List<Node> currentNodes = new List<Node>(); // This list holds the nodes that have yet to be tested for validity
        currentNodes.Add(startNode);

        for (int depth = 0; depth < distance; ++depth)
        {
            //validNodes.Clear(); // Get rid of whatever is in the valid nodes, these are not exactly distance away from the node

            int amountNodes = currentNodes.Count;
            for (int i = 0; i < amountNodes; ++i)
            {
                // If the current node is null, end this iteration and start the next one
                if (currentNodes[i] == null)
                    continue;

                Vector2Int curNodePos = currentNodes[i].position;
                // Check above node
                Vector2Int testPos = new Vector2Int(curNodePos.x, curNodePos.y + 1);
                ValidDistTestNode(testPos, validNodes, currentNodes);

                // Check left node
                testPos = new Vector2Int(curNodePos.x - 1, curNodePos.y);
                ValidDistTestNode(testPos, validNodes, currentNodes);

                // Check right node
                testPos = new Vector2Int(curNodePos.x + 1, curNodePos.y);
                ValidDistTestNode(testPos, validNodes, currentNodes);

                // Check down node
                testPos = new Vector2Int(curNodePos.x, curNodePos.y - 1);
                ValidDistTestNode(testPos, validNodes, currentNodes);

            }
            // Removes the nodes that have already been iterated over
            for (int i = 0; i < amountNodes; ++i)
            {
                currentNodes.RemoveAt(0);
            }
        }
        return validNodes;
    }

    /// <summary>
    /// Used to test the current testNode in GetNodesDistFromNode
    /// </summary>
    /// <param name="testPos">Position of the node being tested</param>
    /// <param name="validNodes">Reference to the List of Nodes that have been deemed valid to move to</param>
    /// <param name="currentNodes">Reference to the List of Nodes that still need to be tested</param>
    private void ValidDistTestNode(Vector2Int testPos, List<Node> validNodes, List<Node> currentNodes)
    {
        Node testNode = GetNodeAtPosition(testPos);
        // If the node exists add it, we don't care what is there
        if (testNode != null)
        {
            validNodes.Add(testNode);
            currentNodes.Add(testNode);
        }
    }
}
