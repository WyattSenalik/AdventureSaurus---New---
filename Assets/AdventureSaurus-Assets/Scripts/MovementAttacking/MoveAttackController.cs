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
    // Parent of all ally objects
    private Transform _allyParent;
    // Parent of all eney objects
    private Transform _enemyParent;
    // Parent of all the interactables
    private Transform _interactParent;
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
    // The sprite that will be put on the visual interact tile
    [SerializeField] private Sprite _interactTileSprite = null;
    // The sprite that will be put on the visual heal/buff tile
    [SerializeField] private Sprite _healTileSprite = null;
    // The sorting layer that the tiles will be put on
    [SerializeField] private string _visualSortingLayer = "VisualTile";
    // The material that will be applied to the sprite renderer of the tiles
    [SerializeField] private Material _tileMaterial = null;
    // The "can take action there" alpha
    private const float opaqueVal = 0.6f;
    // The "nothing to do there" (more of range indicator) alpha
    private const float transparentVal = 0.2f;

    // Only for testing. This is a list of the spawned canvas objects that display numbers on the nodes
    private List<GameObject> _visualTests;

    // Max moveRange + atkRange for a character
    private const int MAX_RANGE = 20;

    // Events
    // When the grid finishes calculating
    public delegate void GridFinishedCalculating(Vector2Int topLeft, Vector2Int botRight);
    public static event GridFinishedCalculating OnGridFinishedCalculating;


    // Called when the gameobject is toggled on
    // Subscribe to events
    private void OnEnable()
    {
        // When the floor is finished generating, initialize this script
        ProceduralGenerationController.OnFinishGenerationNoParam += Initialize;

        // When the game is paused, disable this script
        Pause.OnPauseGame += HideScript;
        // Unsubscribe to the unpause event (since if this is active, the game is unpaused)
        Pause.OnUnpauseGame -= ShowScript;

    }

    // Called when the gameobject is toggled off
    // Unsubscribe to events
    private void OnDisable()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;

        // Unsubscribe to the pause event (since if this is inactive, the game is paused)
        Pause.OnPauseGame -= HideScript;
        // When the game is unpaused, re-enable this script
        Pause.OnUnpauseGame += ShowScript;
    }

    // Called when the gameobject is destroyed
    // Unsubscribe to ALL events
    private void OnDestroy()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
        Pause.OnPauseGame -= HideScript;
        Pause.OnUnpauseGame -= ShowScript;
    }

    /// <summary>
    /// Initializes things for this script.
    /// Called from the FinishGenerating event
    /// </summary>
    private void Initialize()
    {
        _allyParent = GameObject.Find(ProceduralGenerationController.ALLY_PARENT_NAME).transform;
        _enemyParent = GameObject.Find(ProceduralGenerationController.ENEMY_PARENT_NAME).transform;
        _wallParent = GameObject.Find(ProceduralGenerationController.WALL_PARENT_NAME).transform;
        Transform roomParent = GameObject.Find(ProceduralGenerationController.ROOM_PARENT_NAME).transform;
        _interactParent = GameObject.Find(ProceduralGenerationController.INTERACT_PARENT_NAME).transform;

        // Get the bounds of the grid
        _gridTopLeft = FindTopLeftPosition(roomParent);
        _gridBotRight = FindBotRightPosition(roomParent);
        // Create the grid
        CreateGrid();
        // Find the walls
        FindWalls();
        // Find the characters
        FindCharacters();
        // Find the interactables
        FindInteractables();

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

        // Do this for each ally
        foreach (Transform allyTrans in _allyParent)
        {
            MoveAttack mARef = allyTrans.GetComponent<MoveAttack>();
            if (mARef == null)
            {
                Debug.Log(allyTrans.name + " does not have a MoveAttack script attached to it");
            }
            else
            {
                // Create the visual tiles ahead of time
                CreateVisualTiles(mARef);
            }
        }
        // Do this for each enemy
        foreach (Transform enemyTrans in _enemyParent)
        {
            MoveAttack mARef = enemyTrans.GetComponent<MoveAttack>();
            if (mARef == null)
            {
                Debug.Log(enemyTrans.name + " does not have a MoveAttack script attached to it");
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
        // Allies
        foreach (Transform allyTrans in _allyParent)
        {
            MoveAttack charMA = allyTrans.GetComponent<MoveAttack>();
            // If the character has no MoveAttack script
            if (charMA == null)
            {
                Debug.Log(allyTrans.name + " has no MoveAttack script attached to it");
                continue;
            }
            UpdateGrid(allyTrans, charMA.WhatAmI);

            // Add the ally to the ally list
            _allyMA.Add(charMA);
        }
        // Enemeies
        foreach (Transform enemyTrans in _enemyParent)
        {
            MoveAttack charMA = enemyTrans.GetComponent<MoveAttack>();
            // If the character has no MoveAttack script
            if (charMA == null)
            {
                Debug.Log(enemyTrans.name + " has no MoveAttack script attached to it");
                continue;
            }
            UpdateGrid(enemyTrans, charMA.WhatAmI);

            // Add enemy to the enemy list
            _enemyMA.Add(charMA);
        }
    }

    /// <summary>
    /// Sets the node the interactables are on to interactable
    /// </summary>
    private void FindInteractables()
    {
        // Iterate over the interactables
        foreach (Transform interact in _interactParent)
        {
            UpdateGrid(interact.transform, CharacterType.Interactable);
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
        Node occupantNode = GetNodeByWorldPosition(occupant.transform.position);
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
            if (mARef.RangeVisualParent == null)
            {
                InitializeVisualTiles(mARef);
            }
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
        // Create one game object that is a child of rangeVisualParent to serve as the parents for move, attack, and interact
        // We will swap the sprites as needed
        GameObject moveTileParent = new GameObject("MoveVisualParent");
        moveTileParent.transform.parent = mARef.RangeVisualParent.transform;
        moveTileParent.transform.localPosition = Vector3.zero;
        CreateSingleVisualTile(0, 0, mARef, true, moveTileParent.transform);

        // The depth of tiles to make
        int totalTileDepth = mARef.MoveRange + mARef.AttackRange;
        if (mARef.WhatAmI == CharacterType.Ally)
        {
            totalTileDepth = MAX_RANGE;
        }

        // Make the rest of the movement tiles around the character
        bool isMoveTile = true;
        for (int i = 1; i <= totalTileDepth; ++i)
        {
            // We now just say they are all both
            // If we have finished the move tiles
            //if (i >= mARef.MoveRange + 1)
                //isMoveTile = false;

            Vector2Int placementPos = new Vector2Int(0, i);
            // Go down, right
            while (placementPos.y > 0)
            {
                CreateSingleVisualTile(placementPos.x, placementPos.y, mARef, isMoveTile,
                    moveTileParent.transform);
                placementPos.x += 1;
                placementPos.y -= 1;
            }
            // Go down, left
            while (placementPos.x > 0)
            {
                CreateSingleVisualTile(placementPos.x, placementPos.y, mARef, isMoveTile, 
                    moveTileParent.transform);
                placementPos.x -= 1;
                placementPos.y -= 1;
            }
            // Go up, left
            while (placementPos.y < 0)
            {
                CreateSingleVisualTile(placementPos.x, placementPos.y, mARef, isMoveTile, 
                    moveTileParent.transform);
                placementPos.x -= 1;
                placementPos.y += 1;
            }
            // Go up, right
            while (placementPos.x < 0)
            {
                CreateSingleVisualTile(placementPos.x, placementPos.y, mARef, isMoveTile, 
                    moveTileParent.transform);
                placementPos.x += 1;
                placementPos.y += 1;
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
    private void CreateSingleVisualTile(int x, int y, MoveAttack charMA, bool isMoveTile, Transform moveTileParent)
    {
        SpawnVisualTile(moveTileParent, new Vector2(x, y), _moveTileSprite, 1);
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

        sprRend.color = new Color(sprRend.color.r, sprRend.color.g, sprRend.color.b, opaqueVal);
    }

    /// <summary>
    /// Turns on the range visuals for movement and attack for this character
    /// </summary>
    /// <param name="shouldTurnOn">Is true, turn on the visuals. If false, turn them off</param>
    public void SetActiveVisuals(MoveAttack mARef)
    {
        //// New algorithm attemp
        // Get the grid position of the character
        Vector2Int charGridPos = new Vector2Int(Mathf.RoundToInt(mARef.transform.position.x),
            Mathf.RoundToInt(mARef.transform.position.y));
        // We now only have one tile parent. We change the sprites of its children
        Transform tileParent = mARef.RangeVisualParent.transform.GetChild(0);

        /// Step 1: Set attack tiles
        /// 
        foreach (Node atkNode in mARef.AttackTiles)
        {
            // Get the local grid position of the current node
            Vector2Int tileLocalGridPos = atkNode.Position - charGridPos;
            // Get the index of the visual tile
            int tileIndex = GetVisualTileIndex(tileLocalGridPos.x, tileLocalGridPos.y);
            // If the tile doesn't exceed the list's bounds, set it to active
            if (tileIndex < tileParent.childCount)
            {
                // Get the transform of the visual tile
                Transform tileTrans = tileParent.GetChild(tileIndex);
                // Pull the sprite renderer off it
                SpriteRenderer tileSprRend = tileTrans.GetComponent<SpriteRenderer>();

                // If we are to attack a foe
                if (!mARef.TargetFriendly)
                {
                    // We set the sprite to the attack sprite
                    tileSprRend.sprite = _attackTileSprite;
                    // If we there is an attackable opponent, make it more opaque
                    if (atkNode.Occupying == CharacterType.Ally && mARef.WhatAmI == CharacterType.Enemy ||
                            atkNode.Occupying == CharacterType.Enemy && mARef.WhatAmI == CharacterType.Ally)
                        tileSprRend.color = new Color(tileSprRend.color.r, tileSprRend.color.g, tileSprRend.color.b, opaqueVal);
                    // If there is nothing to attack there, make it more transparent
                    else
                        tileSprRend.color = new Color(tileSprRend.color.r, tileSprRend.color.g, tileSprRend.color.b, transparentVal);
                }
                // If we are to help a friendly
                else
                {
                    // We set the sprite to the heal/buff sprite
                    tileSprRend.sprite = _healTileSprite;
                    // If there is a friendly (that isn't the character), make it more opaque
                    MoveAttack charAtNode = GetCharacterMAByNode(atkNode);
                    if (atkNode.Occupying == mARef.WhatAmI && mARef != charAtNode)
                        tileSprRend.color = new Color(tileSprRend.color.r, tileSprRend.color.g, tileSprRend.color.b, opaqueVal);
                    // If there is nothing to heal/buff there
                    else
                        tileSprRend.color = new Color(tileSprRend.color.r, tileSprRend.color.g, tileSprRend.color.b, transparentVal);
                }

                // Set it active
                tileTrans.gameObject.SetActive(true);
            }
        }
        /// Step 2: Set move tiles to on
        /// 
        foreach (Node moveNode in mARef.MoveTiles)
        {
            // Get the local grid position of the current node
            Vector2Int tileLocalGridPos = moveNode.Position - charGridPos;
            // Get the index of the visual tile
            int tileIndex = GetVisualTileIndex(tileLocalGridPos.x, tileLocalGridPos.y);
            // If the tile doesn't exceed the list's bounds, set it to active
            if (tileIndex < tileParent.childCount)
            {
                // Get the transform of the visual tile
                Transform tileTrans = tileParent.GetChild(tileIndex);
                // Pull the sprite renderer off it
                SpriteRenderer tileSprRend = tileTrans.GetComponent<SpriteRenderer>();

                // We set the sprite to the move sprite
                tileSprRend.sprite = _moveTileSprite;

                // All movement tiles are opaque
                tileSprRend.color = new Color(tileSprRend.color.r, tileSprRend.color.g, tileSprRend.color.b, opaqueVal);

                // Set it active
                tileTrans.gameObject.SetActive(true);
            }
        }
        /// Step 3: Set tiles that contain interactable objects to be interact tiles
        /// 
        foreach (Node interactNode in mARef.InteractTiles)
        {
            // Get the local grid position of the current node
            Vector2Int tileLocalGridPos = interactNode.Position - charGridPos;
            // Get the index of the visual tile
            int tileIndex = GetVisualTileIndex(tileLocalGridPos.x, tileLocalGridPos.y);
            // If the tile doesn't exceed the list's bounds, set it to active
            if (tileIndex < tileParent.childCount)
            {
                // Get the transform of the visual tile
                Transform tileTrans = tileParent.GetChild(tileIndex);
                // Pull the sprite renderer off it
                SpriteRenderer tileSprRend = tileTrans.GetComponent<SpriteRenderer>();

                // We set the sprite to the move sprite
                tileSprRend.sprite = _interactTileSprite;

                // All interact tiles are opaque
                tileSprRend.color = new Color(tileSprRend.color.r, tileSprRend.color.g, tileSprRend.color.b, opaqueVal);

                // Set it active
                tileTrans.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Calculates the index of the visual tile we want
    /// </summary>
    /// <param name="x">Local x grid position of the tile</param>
    /// <param name="y">Local y grid position of the tile</param>
    /// <returns>int index of the tile</returns>
    private int GetVisualTileIndex(int x, int y)
    {
        int rtnIndex = -1;

        // How many tiles from the initial tile this tile is
        int depth = Mathf.Abs(x) + Mathf.Abs(y);

        // If the tile we want is the first tile
        if (depth == 0)
            rtnIndex = 0;
        // If the tile we want is in the right semicircle
        else if (x >= 0)
            rtnIndex = depth * (2 * depth - 1) + 1 - y;
        // If the tile we want is in the left semicircle
        else
            rtnIndex = depth * (2 * depth + 1) + 1 + y;

        //Debug.Log("Visual tile at (" + x + ", " + y + ") has index " + rtnIndex);
        return rtnIndex;
    }

    /// <summary>
    /// Turns off all the visual tiles
    /// </summary>
    /// <param name="mARef">Reference to the MoveAttack script attached to the character whose visuals we are turning off</param>
    public void TurnOffVisuals(MoveAttack mARef)
    {
        // Iterate over all the tiles and turn them off
        foreach (Transform tileParent in mARef.RangeVisualParent.transform)
        {
            foreach (Transform tileTrans in tileParent)
            {
                tileTrans.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Recalculates the move attack tiles for allies.
    /// Called on the 
    /// </summary>
    private void RecalculateAllyTiles()
    {
        // Iterate over each ally
        foreach (MoveAttack singleAllyMA in _allyMA)
        {
            // Recalculate the ally's movement and attack tiles
            if (singleAllyMA != null)
            {
                singleAllyMA.CalculateAllTiles();
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
        // Go over each character
        // Allies
        foreach (Transform allyTrans in _allyParent)
        {
            // Convert the character's position to grid point
            Vector2Int charGridPos = new Vector2Int(Mathf.RoundToInt(allyTrans.position.x), Mathf.RoundToInt(allyTrans.position.y));
            // If the character's position on the grid is the same as the testNode's position
            if (charGridPos == testNode.Position)
            {
                return allyTrans.GetComponent<MoveAttack>();
            }
        }
        // Enemies
        foreach (Transform enemyTrans in _enemyParent)
        {
            // Convert the character's position to grid point
            Vector2Int charGridPos = new Vector2Int(Mathf.RoundToInt(enemyTrans.position.x), Mathf.RoundToInt(enemyTrans.position.y));
            // If the character's position on the grid is the same as the testNode's position
            if (charGridPos == testNode.Position)
            {
                return enemyTrans.GetComponent<MoveAttack>();
            }
        }
        return null;
    }

    /// <summary>
    /// Finds if there is an interactable at the specified node. Returns a reference to that interactable's Interactable script
    /// </summary>
    /// <param name="testNode">The node that is being tested to see if there is an interactbale there</param>
    /// <returns>If it finds an interactable, it returns that interactable's Interactable script. If it finds no interactable, returns null</returns>
    public Interactable GetInteractableByNode(Node testNode)
    {
        if (testNode == null)
            return null;
        
        foreach(Transform interactTrans in _interactParent)
        {
            // Convert the interactable's position to a node
            Node interactNode = GetNodeByWorldPosition(interactTrans.position);
            // If the interactable's node is the same as the test node, then we found it
            if (testNode == interactNode)
                return interactTrans.GetComponent<Interactable>();
        }
        // If we didn't find it
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
            // The nodes that are being tested
            List<Node> inProgressNodes = new List<Node>();
            // The nodes that have already been tested
            List<Node> testedNodes = new List<Node>();
            // We start with the one we want to reach
            inProgressNodes.Add(endNode);

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
