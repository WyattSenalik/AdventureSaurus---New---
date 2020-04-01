using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveAttackController : MonoBehaviour
{
    // Top-Left coordinates of the grid
    private Vector2Int _gridTopLeft;
    public Vector2Int GridTopLeft
    {
        get { return _gridTopLeft; }
    }
    // Bottom-Right coordinates of the grid
    private Vector2Int _gridBotRight;
    public Vector2Int GridBotRight
    {
        get { return _gridBotRight; }
    }
    // Parent of all wall objects
    private Transform _wallParent;
    // Parent of all character (ally and enemy) objects
    private Transform _charParent;
    // References to all the allies MoveAttack scripts
    private List<MoveAttack> _allyMA;
    // References to all the allies MoveAttack script
    private List<MoveAttack> _enemyMA;
    // The movement grid the characters are situated on
    private List<List<Node>> _grid;

    // For creating the visuals of move/attack tiles
    // The sprite that will be put on the visual move tile
    [SerializeField] private Sprite _moveTileSprite = null;
    // The sprite that will be put on the visual attack tile
    [SerializeField] private Sprite _attackTileSprite = null;
    // The sorting layer that the tiles will be put on
    [SerializeField] private string _visualSortingLayer = "VisualTile";
    // The material that will be applied to the sprite renderer of the tiles
    [SerializeField] private Material _tileMaterial = null;

    // Only for testing. This is a list of the spawned canvas objects that display numbers on the nodes
    private List<GameObject> _visualTests;

    // Events
    // When the grid finishes calculating
    public delegate void GridFinishedCalculating(Vector2Int topLeft, Vector2Int botRight);
    public static event GridFinishedCalculating OnGridFinishedCalculating;


    // Called when the gameobject is toggled on
    // Subscribe to events
    private void OnEnable()
    {
        // When the player is allowed to select, recalculate the allies' move and attack tiles
        MoveAttackGUIController.OnPlayerAllowedSelect += RecalculateAllMoveAttackTiles;
        // When the floor is finished generating, initialize this script
        ProceduralGenerationController.OnFinishGeneration += Initialize;

        // When the game is paused, disable this script
        Pause.OnPauseGame += HideScript;
        // Unsubscribe to the unpause event (since if this is active, the game is unpaused)
        Pause.OnUnpauseGame -= ShowScript;

    }

    // Called when the gameobject is toggled off
    // Unsubscribe to events
    private void OnDisable()
    {
        MoveAttackGUIController.OnPlayerAllowedSelect -= RecalculateAllMoveAttackTiles;
        ProceduralGenerationController.OnFinishGeneration -= Initialize;

        // Unsubscribe to the pause event (since if this is inactive, the game is paused)
        Pause.OnPauseGame -= HideScript;
        // When the game is unpaused, re-enable this script
        Pause.OnUnpauseGame += ShowScript;
    }

    // Called when the gameobject is destroyed
    // Unsubscribe to ALL events
    private void OnDestroy()
    {
        MoveAttackGUIController.OnPlayerAllowedSelect -= RecalculateAllMoveAttackTiles;
        ProceduralGenerationController.OnFinishGeneration -= Initialize;
        Pause.OnPauseGame -= HideScript;
        Pause.OnUnpauseGame -= ShowScript;
    }

    /// <summary>
    /// Initializes things for this script.
    /// Called from the FinishGenerating event
    /// </summary>
    /// <param name="charParent">The parent of all the characters</param>
    /// <param name="roomParent">The parent of all the rooms </param>
    /// <param name="wallParent">The parent of all the walls </param>
    /// <param name="stairsTrans">The transform of the stairs (unused)</param>
    private void Initialize(Transform charParent, Transform roomParent, Transform wallParent, Transform stairsTrans)
    {
        // Get the bounds of the grid
        _gridTopLeft = FindTopLeftPosition(roomParent);
        _gridBotRight = FindBotRightPosition(roomParent);
        // Create the grid
        CreateGrid();
        // Set the wall parent and find the walls
        _wallParent = wallParent;
        FindWalls();
        // Set the character parent and find the characters
        _charParent = charParent;
        FindCharacters();

        // For testing only - holds the little numbers that spawn when pathin
        _visualTests = new List<GameObject>();

        // Create the initial visual tiles
        InitialCreateVisuals();
    }

    /// <summary>
    /// Creates the visual tiles for each character
    /// </summary>
    private void InitialCreateVisuals()
    {
        // For each character create the visual tiles
        // based on their starting moveRange and attackRange.
        // Also initializes each character
        foreach (Transform charTrans in _charParent)
        {
            MoveAttack mARef = charTrans.GetComponent<MoveAttack>();
            if (mARef == null)
            {
                Debug.Log(charTrans.name + " does not have a MoveAttack script attached to it");
            }
            else
            {
                // Create the visual tiles ahead of time
                CreateVisualTiles(mARef);
            }
        }
    }


    // Grid functions 

    /// <summary>
    /// Initialize the grid to have nodes at all possible grid points
    /// </summary>
    private void CreateGrid()
    {
        _grid = new List<List<Node>>();  // Initialize the grid to be an empty list of empty lists of nodes
        // Iterate to create rows of the grid
        for (int i = _gridBotRight.y; i <= _gridTopLeft.y; ++i)
        {
            List<Node> row = new List<Node>();  // Create the next row
            _grid.Add(row);  // Add the new row
            // Iterate to create nodes in each row
            for (int j = _gridTopLeft.x; j <= _gridBotRight.x; ++j)
            {
                row.Add(new Node(new Vector2Int(j, i)));    // Add the node to the row
            }
        }

        // Call the Grid Finished Calculating event
        if (OnGridFinishedCalculating != null)
            OnGridFinishedCalculating(_gridTopLeft, _gridBotRight);
    }

    /// <summary>
    /// Sets all nodes that share a location with a child of wallParent to be occupied by a wall
    /// </summary>
    private void FindWalls()
    {
        foreach (Transform wall in _wallParent)
        {
            UpdateGrid(wall, CharacterType.Wall);
        }
    }

    /// <summary>
    /// Sets all nodes that share a location with a child of charParent to be occupied by the kind of 
    /// </summary>
    private void FindCharacters()
    {
        // Initialize the lists
        _allyMA = new List<MoveAttack>();
        _enemyMA = new List<MoveAttack>();

        // Iterate over all characters
        foreach (Transform character in _charParent)
        {
            MoveAttack charMA = character.GetComponent<MoveAttack>();
            // If the character has no MoveAttack script
            if (charMA == null)
            {
                Debug.Log(character.name + " has no MoveAttack script attached to it");
                continue;
            }
            UpdateGrid(character, charMA.WhatAmI);

            // Add the characters to their proper list
            if (charMA.WhatAmI == CharacterType.Ally)
                _allyMA.Add(charMA);
            else if (charMA.WhatAmI == CharacterType.Enemy)
                _enemyMA.Add(charMA);
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
        occupantNode.Occupying = charType;
    }
    // End Grid Functions

    
    // Visual Tile Functions

    /// <summary>
    /// Creates the visual tiles for when a character is clicked on
    /// </summary>
    /// <param name="mARef">Reference to the character whose visual tiles will be created</param>
    public void CreateVisualTiles(MoveAttack mARef)
    {
        // Make sure it exists
        if (mARef != null)
        {
            // Calculate its move tiles and attack tiles, then initialize the visual tiles for it
            mARef.CalcMoveTiles();
            mARef.CalcAttackTiles();
            // If it is the first time displaying visuals for this character, we need to make brand new visual tiles
            if (mARef.RangeVisualParent == null)
            {
                InitializeVisualTiles(mARef);
            }
            // If the character already has the visuals and just needs some of them to be turned on
            //else
            //{
            //    SetActiveVisuals(mARef);
            //}
        }
        else
        {
            Debug.Log(mARef.name + " has no MoveAttack script attached to it");
        }
    }


    /// <summary>
    /// Generates the visuals for movement and attack. Called once at the start of the game
    /// </summary>
    /// <param name="mARef">Reference to the character we are calculating visual tiles for</param>
    private void InitializeVisualTiles(MoveAttack mARef)
    {
        // Create the actual game object
        mARef.RangeVisualParent = new GameObject("RangeVisualParent");
        mARef.RangeVisualParent.transform.parent = mARef.transform;
        mARef.RangeVisualParent.transform.localPosition = Vector3.zero;
        // Create two game objects that are chilren of rangeVisualParent to serve as the parents for move and attack
        GameObject moveTileParent = new GameObject("MoveVisualParent");
        moveTileParent.transform.parent = mARef.RangeVisualParent.transform;
        moveTileParent.transform.localPosition = Vector3.zero;
        GameObject attackTileParent = new GameObject("AttackVisualParent");
        attackTileParent.transform.parent = mARef.RangeVisualParent.transform;
        attackTileParent.transform.localPosition = Vector3.zero;
        // Make the first movement tile under the character
        CreateSingleVisualTile(0, 0, mARef, true, moveTileParent.transform, attackTileParent.transform);

        // Make the rest of the movement tiles around the character
        bool isMoveTile = true;
        for (int i = 1; i <= mARef.MoveRange + mARef.AttackRange; ++i)
        {
            // If we have finished the move tiles
            if (i >= mARef.MoveRange + 1)
                isMoveTile = false;

            Vector2Int placementPos = new Vector2Int(i, 0);
            // Go down, left
            while (placementPos.x > 0)
            {
                CreateSingleVisualTile(placementPos.x, placementPos.y, mARef, isMoveTile, moveTileParent.transform, attackTileParent.transform);
                placementPos.x -= 1;
                placementPos.y -= 1;
            }
            // Go up, left
            while (placementPos.y < 0)
            {
                CreateSingleVisualTile(placementPos.x, placementPos.y, mARef, isMoveTile, moveTileParent.transform, attackTileParent.transform);
                placementPos.x -= 1;
                placementPos.y += 1;
            }
            // Go up, right
            while (placementPos.x < 0)
            {
                CreateSingleVisualTile(placementPos.x, placementPos.y, mARef, isMoveTile, moveTileParent.transform, attackTileParent.transform);
                placementPos.x += 1;
                placementPos.y += 1;
            }
            // Go down, right
            while (placementPos.y > 0)
            {
                CreateSingleVisualTile(placementPos.x, placementPos.y, mARef, isMoveTile, moveTileParent.transform, attackTileParent.transform);
                placementPos.x += 1;
                placementPos.y -= 1;
            }
        }
    }

    /// <summary>
    /// Creates one visual tile for the visual tiles of this character
    /// </summary>
    /// <param name="x">X component of this tiles localPosition</param>
    /// <param name="y">Y component of this tiles localPosition</param>
    /// <param name="charMA">Reference to the character's MoveAttack script</param>
    /// <param name="isMoveTile">If the current tile is a move tile</param>
    /// <param name="moveTileParent">The parent of moveTiles</param>
    /// <param name="attackTileParent">The parent of attackTiles</param>
    private void CreateSingleVisualTile(int x, int y, MoveAttack charMA, bool isMoveTile, Transform moveTileParent, Transform attackTileParent)
    {
        // Make a move tile and attack tile at the location
        if (isMoveTile)
        {
            SpawnVisualTile(moveTileParent, new Vector2(x, y), _moveTileSprite, 1);
            SpawnVisualTile(attackTileParent, new Vector2(x, y), _attackTileSprite, 0);
        }
        // Make only an attack tile at the location
        else
        {
            SpawnVisualTile(attackTileParent, new Vector2(x, y), _attackTileSprite, 0);
        }
    }
    
    /// <summary>
    /// Instantiates a single tile object
    /// </summary>
    /// <param name="parent">The parent of the new object</param>
    /// <param name="pos">The local positiuon of the new object</param>
    /// <param name="sprToUse">The sprite that will be added to the object</param>
    /// <param name="orderOnLayer">The order on the sprite layer it will be</param>
    private void SpawnVisualTile(Transform parent, Vector2 pos, Sprite sprToUse, int orderOnLayer)
    {
        // Create the tile, set it as a child of rangeVisualParent, and place it in a localPosition determined by the passed in values
        GameObject newTile = new GameObject("RangeVisual" + pos.x + " " + pos.y);
        newTile.SetActive(false);
        newTile.transform.parent = parent;
        newTile.transform.localPosition = new Vector2(pos.x, pos.y);
        // Attach a sprite renderer to the object, put the correct sprite on it, place it in the correct sorting layer, and give it an order
        SpriteRenderer sprRend = newTile.AddComponent<SpriteRenderer>();
        sprRend.sprite = sprToUse;
        sprRend.sortingLayerName = _visualSortingLayer;
        sprRend.sortingOrder = orderOnLayer;
        sprRend.material = _tileMaterial;

        sprRend.color = new Color(sprRend.color.r, sprRend.color.g, sprRend.color.b, 0.6f);
    }

    /// <summary>
    /// Turns on the range visuals for movement and attack for this character
    /// </summary>
    /// <param name="shouldTurnOn">Is true, turn on the visuals. If false, turn them off</param>
    public void SetActiveVisuals(MoveAttack mARef)
    {
        // Turn on all the movement ones that are in our moveTiles
        foreach (Node moveNode in mARef.MoveTiles)
        {
            // Find a tile transform that matches the movement node's position
            foreach (Transform tileTrans in mARef.RangeVisualParent.transform.GetChild(0))
            {
                // Find the node at the tiles location
                Node tilesNode = GetNodeByWorldPosition(tileTrans.position);
                // If the node doesn't exist, don't worry about it
                // If the node is the same node as the one we are searching for, turn it on and break from this for
                if (tilesNode != null && tilesNode.Position == moveNode.Position)
                {
                    // If the node is empty we want it to be more highlighted, signifying we can move there
                    SpriteRenderer tileSprRend = tileTrans.GetComponent<SpriteRenderer>();
                    if (tilesNode.Occupying == CharacterType.None)
                        tileSprRend.color = new Color(tileSprRend.color.r, tileSprRend.color.g, tileSprRend.color.b, 0.6f);
                    else
                        tileSprRend.color = new Color(tileSprRend.color.r, tileSprRend.color.g, tileSprRend.color.b, 0.2f);

                    tileTrans.gameObject.SetActive(true);
                    break;
                }
            }
        }
        // Turn on all the attack nodes that is not also a move node
        foreach (Node attackNode in mARef.AttackTiles)
        {
            // Test if the attack node is in moveTiles, if it is we keep moving
            if (!mARef.MoveTiles.Contains(attackNode))
            {
                // Find a tile transform that matches the attack node's position
                foreach (Transform tileTrans in mARef.RangeVisualParent.transform.GetChild(1))
                {
                    // Find the node at the tiles location
                    Node tilesNode = GetNodeByWorldPosition(tileTrans.position);
                    // If the node doesn't exist, don't worry about it
                    // If the node is the same node as the one we are searching for, turn it on and break from this for
                    if (tilesNode != null && tilesNode.Position == attackNode.Position)
                    {
                        // If the node is contains an enemy/ally (depending on the character's team) we want it to be more highlighted, signifying we can attack it
                        SpriteRenderer tileSprRend = tileTrans.GetComponent<SpriteRenderer>();
                        if (tilesNode.Occupying == CharacterType.Ally && mARef.WhatAmI == CharacterType.Enemy ||
                            tilesNode.Occupying == CharacterType.Enemy && mARef.WhatAmI == CharacterType.Ally)
                            tileSprRend.color = new Color(tileSprRend.color.r, tileSprRend.color.g, tileSprRend.color.b, 0.6f);
                        else
                            tileSprRend.color = new Color(tileSprRend.color.r, tileSprRend.color.g, tileSprRend.color.b, 0.2f);

                        tileTrans.gameObject.SetActive(true);
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Turns off all the visual tiles
    /// </summary>
    /// <param name="mARef">Reference to the MoveAttack script attached to the character whose visuals we are turning off</param>
    public void TurnOffVisuals(MoveAttack mARef)
    {
        // Iterate over each move tile and turn them off
        foreach (Transform tileTrans in mARef.RangeVisualParent.transform.GetChild(0))
        {
            tileTrans.gameObject.SetActive(false);
        }
        // Iterate over each attack tile and turn them off
        foreach (Transform tileTrans in mARef.RangeVisualParent.transform.GetChild(1))
        {
            tileTrans.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Recalculates the move attack tiles for allies.
    /// Called on the 
    /// </summary>
    private void RecalculateAllMoveAttackTiles()
    {
        // Iterate over each character
        foreach (Transform character in _charParent)
        {
            // Try to get that character's MoveAttack script
            MoveAttack mARef = character.GetComponent<MoveAttack>();
            // Recalculate the character's movement and attack tiles
            if (mARef != null)
            {
                mARef.CalcMoveTiles();
                mARef.CalcAttackTiles();
            }
        }
    }
    // End Visual Tile Functions


    // Grid Getter Functions

    /// <summary>
    /// Returns a node at the given position. If it finds no nodes, it returns null
    /// </summary>
    /// <param name="pos">The position to look for the node</param>
    /// <returns>A node with the same position as the entered position. Or null</returns>
    public Node GetNodeAtPosition(Vector2Int pos)
    {
        // Make sure its not out of bounds
        if (pos.x < _gridTopLeft.x || pos.x > _gridBotRight.x || pos.y < _gridBotRight.y || pos.y > _gridTopLeft.y)
            return null;

        int rowIndex = pos.y - _gridBotRight.y;
        int colIndex = pos.x - _gridTopLeft.x;
        return _grid[rowIndex][colIndex];
    }

    /// <summary>
    /// Accepts the world position of an object and returns what node is closest to that. If it's too far, null is returned.
    /// </summary>
    /// <param name="worldPos">The world position of the object as a Vector3</param>
    /// <returns>Node closest to worldPos, or null if its too far</returns>
    public Node GetNodeByWorldPosition(Vector3 worldPos)
    {
        // Cast the position to grid position
        Vector2Int curEnGridPos = new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
        return GetNodeAtPosition(curEnGridPos);
    }

    /// <summary>
    /// Finds if there is a character at the specified node. Returns a reference to that character's MoveAttack script
    /// </summary>
    /// <param name="testNode">The node that is being tested to see if there is a character there</param>
    /// <returns>If it finds a character on it, it returns that character's MoveAttack. If it finds no character, returns null</returns>
    public MoveAttack GetCharacterMAByNode(Node testNode)
    {
        if (testNode == null)
        {
            return null;
        }
        foreach (Transform character in _charParent)
        {
            // Convert the character's position to grid point
            Vector2Int charGridPos = new Vector2Int(Mathf.RoundToInt(character.position.x), Mathf.RoundToInt(character.position.y));
            // If the character's position on the grid is the same as the testNode's position
            if (charGridPos == testNode.Position)
            {
                return character.GetComponent<MoveAttack>();
            }
        }
        return null;
    }
    // End Grid Getter Functions


    // Pathing Functions

    /// <summary>
    /// Should always be called before Pathing. Resets the whereToGo variable on each node that determines where it should go
    /// </summary>
    public void ResetPathing()
    {
        // For testing only
        foreach (GameObject obj in _visualTests)
            Destroy(obj);
        _visualTests.Clear();

        foreach (List<Node> row in _grid)
        {
            foreach (Node node in row)
            {
                node.WhereToGo = null;
                node.G = 0;
                node.H = 0;
                node.F = 0;
            }
        }
    }

    /// <summary>
    /// Sets all the whereToGo variables on reachable nodes on the grid from the end node to go towards the endNode in the fastest path.
    /// Characters of the same type can move through one another, but not onto each other.
    /// Walls cannot be traversed by any character
    /// </summary>
    /// <param name="startNode">The node that we start at</param>
    /// <param name="endNode">The node that the pathing is trying to reach</param>
    /// <param name="requesterType">The type of the character who requested the pathing</param>
    /// <param name="shouldCare">If we should test if the node we are moving to is empty or not</param>
    /// <returns>True if the node that we tried to get to was valid, false otherwise</returns>
    public bool Pathing(Node startNode, Node endNode, CharacterType requesterType, bool shouldCare=true)
    {
        ResetPathing();
        //Debug.Log("Looking to go to node at " + endNode.position + " from node at " + startNode.position);

        if (endNode == null)
            return false;

        // Make the last nodes' end node be itself
        // We do this before so that enemies can walk in place. Don't question it
        endNode.WhereToGo = endNode;

        // Make sure the node I want to go to is not occupied
        if (!shouldCare || endNode.Occupying == CharacterType.None)
        {
            List<Node> inProgressNodes = new List<Node>();  // The nodes that are being tested
            List<Node> testedNodes = new List<Node>();
            inProgressNodes.Add(endNode);   // We start with the one we want to reach

            // While we are still testing nodes
            while (inProgressNodes.Count != 0)
            {
                // The current node equals the node with the least F
                Node currentNode = inProgressNodes[0];
                // First, find this node
                foreach (Node node in inProgressNodes)
                {
                    if (currentNode.F > node.F)
                        currentNode = node;
                }

                // Remove it from inProgressNodes and add it to testedNodes
                inProgressNodes.Remove(currentNode);
                testedNodes.Add(currentNode);

                // Check if this node is the startNode
                if (currentNode.Position == startNode.Position)
                {
                    // Calculate the A* Values of the end node
                    CalculateAStarValues(endNode, endNode.Parent, startNode.Position);
                    //Debug.Log("Finished A* of " + currentNode.Position + " G: " + currentNode.G + " H: " + currentNode.H + " F: " + currentNode.F);
                    // We found the path, so return that it was a success
                    return true;
                }

                // Generate children
                Vector2Int inProgNodePos = currentNode.Position; // For quick reference
                //Debug.Log("Generating children of node at " + inProgNodePos);

                // Check above node
                Vector2Int testPos = new Vector2Int(inProgNodePos.x, inProgNodePos.y + 1);
                PathingTestNode(testPos, inProgressNodes, testedNodes, currentNode, startNode.Position, requesterType, shouldCare);

                // Check left node
                testPos = new Vector2Int(inProgNodePos.x - 1, inProgNodePos.y);
                PathingTestNode(testPos, inProgressNodes, testedNodes, currentNode, startNode.Position, requesterType, shouldCare);

                // Check right node
                testPos = new Vector2Int(inProgNodePos.x + 1, inProgNodePos.y);
                PathingTestNode(testPos, inProgressNodes, testedNodes, currentNode, startNode.Position, requesterType, shouldCare);

                // Check down node
                testPos = new Vector2Int(inProgNodePos.x, inProgNodePos.y - 1);
                PathingTestNode(testPos, inProgressNodes, testedNodes, currentNode, startNode.Position, requesterType, shouldCare);
            }
        }
        return false;   // Was an invalid node, or we could not reach that node
    }

    /// <summary>
    /// Used to test and set whereToGo of the current testNode in Pathing
    /// </summary>
    /// <param name="testPos">The position of the node being tested</param>
    /// <param name="inProgressNodes">Reference to the List of Nodes that have not been tested yet and need to be</param>
    /// <param name="testedNodes">Reference to the list of Nodes that have been tested and should not be tested again</param>
    /// <param name="currentNode">The node whose children are being created</param>
    /// <param name="endPos">The position we are trying to get to with the path. Used to calculate H</param>
    /// <param name="requestType">CharacterType the requester is. They can move through their allys</param>
    /// <param name="shouldCare">If this was called by enemyMoveAttackAI just to see how one would get to the closest enemy</param>
    public void PathingTestNode(Vector2Int testPos, List<Node> inProgressNodes, List<Node> testedNodes, Node currentNode, Vector2Int endPos, CharacterType requestType, bool shouldCare)
    {
        Node testNode = GetNodeAtPosition(testPos);
        // Make sure the node exists
        if (testNode == null)
        {
            return;
        }

        // If the node I am trying to go to is not occupied or is occupied by someone on my team
        // or if the node is occupied by someone, but I don't care and its the last node
        if (testNode.Occupying == CharacterType.None || testNode.Occupying == requestType || (testPos == endPos && !shouldCare))
        {
            // Make sure this node is not already on the tested list
            if (testedNodes.Contains(testNode))
            {
                return;
            }

            if (testNode != currentNode)
            {
                // The new node's whereToGo is the current node
                testNode.WhereToGo = currentNode;
                currentNode.Parent = testNode;
            }
            else
            {
                Debug.Log("TestNode is the current node");
                return;
            }

            // Set the g, h, and f values
            CalculateAStarValues(testNode, currentNode, endPos);
            //Debug.Log("Finished A* of " + testNode.Position + " G: " + testNode.G + " H: " + testNode.H + " F: " + testNode.F);

            // If the node is already in the inProgressNodes
            if (inProgressNodes.Contains(testNode))
            {
                return;
            }

            // Otherwise, add it
            inProgressNodes.Add(testNode);
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

                Vector2Int curNodePos = currentNodes[i].Position;
                // Check above node
                Vector2Int testPos = curNodePos + Vector2Int.up;
                ValidMoveTestNode(testPos, validNodes, currentNodes, requesterType);

                // Check left node
                testPos = curNodePos + Vector2Int.left;
                ValidMoveTestNode(testPos, validNodes, currentNodes, requesterType);

                // Check right node
                testPos = curNodePos + Vector2Int.right;
                ValidMoveTestNode(testPos, validNodes, currentNodes, requesterType);

                // Check down node
                testPos = curNodePos + Vector2Int.down;
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
            if (testNode.Occupying == CharacterType.None)
            {
                validNodes.Add(testNode);
                currentNodes.Add(testNode);
            }
            // If it is occupied by someone on my team, I can't move there, but I can move past there
            else if (testNode.Occupying == requestType)
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
    public List<Node> GetValidAttackNodes(List<Node> moveNodes, int attackRadius)
    {
        // This list is what will be returned. It is the nodes that can be attacked
        List<Node> validNodes = new List<Node>();

        // This list holds the nodes that have yet to be tested for validity
        List<Node> currentNodes = new List<Node>();
        // This list holds the nodes that have already been tested
        // We don't actually need this, since we accept all nodes to be valid, so valid nodes doubles are testedNodes
        //List<Node> testedNodes = new List<Node>();

        // Add all the move nodes to the current nodes list
        foreach (Node node in moveNodes)
        {
            currentNodes.Add(node);
        }

        // This is how many iterations of checks we have gone over. Aka, how many tiles have been traversed in one path
        int depth = 0;
        while (depth < attackRadius)
        {
            int amountNodes = currentNodes.Count;
            for (int i = 0; i < amountNodes; ++i)
            {
                // If the current node isn't null, check the nodes around it
                if (currentNodes[i] != null)
                {
                    // Check above node
                    Vector2Int testPos = currentNodes[i].Position + Vector2Int.up;
                    ValidAttackTestNode(testPos, validNodes, currentNodes);

                    // Check left node
                    testPos = currentNodes[i].Position + Vector2Int.left;
                    ValidAttackTestNode(testPos, validNodes, currentNodes);

                    // Check right node
                    testPos = currentNodes[i].Position + Vector2Int.right;
                    ValidAttackTestNode(testPos, validNodes, currentNodes);

                    // Check down node
                    testPos = currentNodes[i].Position + Vector2Int.down;
                    ValidAttackTestNode(testPos, validNodes, currentNodes);
                }

            }
            // Removes the nodes that have already been iterated over
            // but not the ones we just added
            for (int i = 0; i < amountNodes; ++i)
            {
                currentNodes.RemoveAt(0);
            }
            // Increment the depth
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
    private void ValidAttackTestNode(Vector2Int testPos, List<Node> validNodes, List<Node> currentNodes)
    {
        Node testNode = GetNodeAtPosition(testPos); // Get the current node
        //Debug.Log("Test tile at " + testPos);
        // If the node exists and it is not already in the validNodes list
        if (testPos != null && !(validNodes.Contains(testNode)))
        {
            validNodes.Add(testNode);
            currentNodes.Add(testNode);
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

                Vector2Int curNodePos = currentNodes[i].Position;
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
    // End Pathing Functions

    /// <summary>
    /// This is used for testing only. It spawns a text object at the nodes location
    /// </summary>
    /// <param name="node">The node to spawn the text at</param>
    /// <param name="textToWrite">The text to write at the node</param>
    private void ShowTextAtNode(Node node, string textToWrite)
    {
        GameObject visualComp = new GameObject("VisualComp: " + textToWrite);
        RectTransform rectTransRef = visualComp.AddComponent<RectTransform>();
        rectTransRef.position = Vector3.zero;
        rectTransRef.localScale = Vector3.one;
        rectTransRef.sizeDelta = Vector2.one;
        Canvas canRef = visualComp.AddComponent<Canvas>();
        canRef.renderMode = RenderMode.WorldSpace;
        canRef.worldCamera = Camera.main;
        canRef.sortingLayerName = "Canvas";
        visualComp.AddComponent<CanvasScaler>();
        visualComp.AddComponent<GraphicRaycaster>();
        GameObject textComp = new GameObject("Text");
        RectTransform textRectTrans = textComp.AddComponent<RectTransform>();
        textComp.transform.SetParent(visualComp.transform);
        textRectTrans.localPosition = Vector3.zero;
        textRectTrans.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        textRectTrans.sizeDelta = new Vector2(1000f, 1000f);
        textComp.AddComponent<CanvasRenderer>();
        Text textRef = textComp.AddComponent<Text>();
        textRef.text = textToWrite;
        textRef.fontSize = 300;
        textRef.font = Resources.Load<Font>("dpcomic");
        textRef.alignment = TextAnchor.MiddleCenter;
        textRef.color = new Color(255, 255, 0);
        canRef.transform.position = new Vector3(node.Position.x, node.Position.y, 0);

        _visualTests.Add(visualComp);
    }

    /// <summary>
    /// Finds the bottom right most position of a room
    /// </summary>
    /// <returns>Vector2Int that is the bottom right most position</returns>
    public Vector2Int FindBotRightPosition(Transform roomParent)
    {
        Vector2Int mostBotRightRoomPos = Vector2Int.zero;
        foreach (Transform curRoom in roomParent)
        {
            Vector2Int botRightRoomPos = new Vector2Int(Mathf.RoundToInt(curRoom.position.x + (curRoom.localScale.x - 1) / 2f),
                Mathf.RoundToInt(curRoom.position.y - (curRoom.localScale.y - 1) / 2f));
            // If the current room is more right
            if (botRightRoomPos.x > mostBotRightRoomPos.x)
            {
                mostBotRightRoomPos.x = botRightRoomPos.x;
            }
            // If the current room is more bot
            if (botRightRoomPos.y < mostBotRightRoomPos.y)
            {
                mostBotRightRoomPos.y = botRightRoomPos.y;
            }
        }

        return mostBotRightRoomPos;
    }

    /// <summary>
    /// Finds the top left most position of a room
    /// </summary>
    /// <returns>Vector2Int that is the top left most position</returns>
    public Vector2Int FindTopLeftPosition(Transform roomParent)
    {
        Vector2Int mostTopLeftRoomPos = Vector2Int.zero;
        foreach (Transform curRoom in roomParent)
        {
            Vector2Int topLeftRoomPos = new Vector2Int(Mathf.RoundToInt(curRoom.position.x - (curRoom.localScale.x - 1) / 2f),
                Mathf.RoundToInt(curRoom.position.y + (curRoom.localScale.y - 1) / 2f));
            // If the current room is more left
            if (topLeftRoomPos.x < mostTopLeftRoomPos.x)
            {
                mostTopLeftRoomPos.x = topLeftRoomPos.x;
            }
            // If the current room is more top
            if (topLeftRoomPos.y > mostTopLeftRoomPos.y)
            {
                mostTopLeftRoomPos.y = topLeftRoomPos.y;
            }
        }

        return mostTopLeftRoomPos;
    }

    /// <summary>
    /// Calcualtes the A* Values and sets them
    /// </summary>
    /// <param name="testNode">Node to set the values for</param>
    /// <param name="prevNode">Node that has testNode as its nextNode</param>
    /// <param name="endPos">End position we are trying to reach</param>
    private void CalculateAStarValues(Node testNode, Node prevNode, Vector2Int endPos)
    {
        // Set the g, h, and f values
        if (prevNode != null)
            testNode.G = prevNode.G + 1;
        else
            testNode.G = 0;
        int xDist = Mathf.Abs(testNode.Position.x - endPos.x);
        int yDist = Mathf.Abs(testNode.Position.y - endPos.y);
        testNode.H = xDist * xDist + yDist * yDist;
        testNode.F = testNode.G + testNode.H;
        //Debug.Log("Calcualting A* of " + testNode.Position + " G: " + testNode.G + " H: " + testNode.H + " F: " + testNode.F);
        // For testing, creates text at nodes displaying their F value
        //ShowTextAtNode(testNode, testNode.F.ToString());
    }

    /// <summary>
    /// Toggles off this script
    /// </summary>
    private void HideScript()
    {
        this.enabled = false;
    }

    /// <summary>
    /// Toggles on this script
    /// </summary>
    private void ShowScript()
    {
        this.enabled = true;
    }
}
