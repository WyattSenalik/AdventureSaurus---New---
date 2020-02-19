using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAttack : MonoBehaviour
{
    private int moveRange;  // How many tiles this character can move
    public int MoveRange
    {
        get { return moveRange; }
        set { moveRange = value; }
    }
    [SerializeField] private int attackRange = 1;   // How many tiles away this character can attack
    public int AttackRange
    {
        get { return attackRange; }
    }
    private int dmgToDeal;  // How much damage this character's attack should deal
    public int DmgToDeal
    {
        set { dmgToDeal = value; }
    }
    [SerializeField] private CharacterType whatAmI = CharacterType.None;    // What kind of character is this script attached to?
    public CharacterType WhatAmI
    {
        get { return whatAmI; }
    }
    private MoveAttackController mAContRef; // A reference to the MoveAttackController script
    private List<Node> moveTiles;   // The valid tiles this character can move to
    public List<Node> MoveTiles
    {
        get { return moveTiles; }
        set { moveTiles = value; }
    }
    private List<Node> attackTiles; // The valid tiles this character can attack
    public List<Node> AttackTiles
    {
        get { return attackTiles; }
        set { attackTiles = value; }
    }
    private GameObject rangeVisualParent;   // The gameObject thats transform is the parent of the rangeVisuals

    // For creating the visuals of move/attack tiles
    [SerializeField] private Sprite moveTileSprite = null;      // The sprite that will be put on the visual move tile
    [SerializeField] private Sprite attackTileSprite = null;    // The sprite that will be put on the visual attack tile
    [SerializeField] private string visualSortingLayer = "Default"; // The sorting layer that the tiles will be put on

    // For actual movement calculations/animations
    [SerializeField] private float transSpeed = 4;  // Speed the character moves to transition from one tile to another
    private SpriteRenderer sprRendRef;  // Reference to the spriteRenderer attached to this character
    private Animator animRef;   // Reference to the animator attached to this character
    private MoveAttackGUIController mAGUIContRef;   // Reference to the moveAttackGUIController
    private Node currentNode;   // The node this object wants to move to next
    private bool transition;    // Whether this character should be moving or not
    private bool doneTransX;    // If this character has finished moving in the x direction
    private bool doneTransY;    // If this character has finished moving in the y direction
    private bool hasMoved;  // If this character has already moved in the "turn"
    public bool HasMoved
    {
        get { return hasMoved; }
    }
    private bool hasAttacked;   // If this character has already attacked in the "turn"
    public bool HasAttacked
    {
        get { return hasAttacked; }
    }

    // For attacking
    private Health enemyHP; // Reference to the health script attached to the enemy I start attacking

    // For enemy movement AI
    private EnemyMoveAttackAI enMAAIRef;    // Reference to the EnemyMoveAttackAI script

    // For turns
    private TurnSystem turnSysRef;  // Reference to the TurnSystem script

    // Set references
    private void Awake()
    {
        GameObject gameControllerObj = GameObject.FindWithTag("GameController");
        if (gameControllerObj == null)
        {
            Debug.Log("Could not find any GameObject with the tag GameController");
        }
        else
        {
            mAContRef = gameControllerObj.GetComponent<MoveAttackController>();
            if (mAContRef == null)
            {
                Debug.Log("Could not find MoveAttackController attached to " + gameControllerObj.name);
            }
            mAGUIContRef = gameControllerObj.GetComponent<MoveAttackGUIController>();
            if (mAGUIContRef == null)
            {
                Debug.Log("Could not find MoveAttackGUIController attached to " + gameControllerObj.name);
            }
            enMAAIRef = gameControllerObj.GetComponent<EnemyMoveAttackAI>();
            if (enMAAIRef == null)
            {
                Debug.Log("Could not find EnemyMoveAttackAI attached to " + gameControllerObj.name);
            }
            turnSysRef = gameControllerObj.GetComponent<TurnSystem>();
            if (turnSysRef == null)
            {
                Debug.Log("Could not find TurnSystem attached to " + gameControllerObj.name);
            }
        }

        sprRendRef = this.GetComponent<SpriteRenderer>();
        if (sprRendRef == null)
        {
            Debug.Log("Could not find SpriteRenderer attached to " + this.name);
        }
        animRef = this.GetComponent<Animator>();
        if (animRef == null)
        {
            Debug.Log("Could not find Animator attached to " + this.name);
        }
    }

    // Called before the first frame
    // Initialize variables
    private void Start()
    {
        currentNode = null;
        transition = false;
        doneTransX = true;
        doneTransY = true;
        hasMoved = false;
        hasAttacked = false;
    }

    /// <summary>
    /// Figures out what nodes are valid for me to move to and saves them in moveTiles
    /// </summary>
    public void CalcMoveTiles()
    {
        // Use that my position to get the node I'm on
        Node myNode = mAContRef.GetNodeByWorldPosition(this.transform.position);
        // Find the valid move tiles and save them
        moveTiles = mAContRef.GetValidMovementNodes(myNode, moveRange, whatAmI);
    }

    /// <summary>
    /// Figures out what nodes are valid for me to atack and saves them in attackTiles
    /// </summary>
    public void CalcAttackTiles()
    {
        // Use my move tiles to figure out where I can attack
        attackTiles = mAContRef.GetValidAttackNodes(moveTiles, attackRange, whatAmI);
    }

    /// <summary>
    /// Generates the visuals for movement and attack
    /// </summary>
    public void CreateVisualTiles(bool shouldActivate)
    {
        // Destroy the last visuals, they are probably inaccurate now
        Destroy(rangeVisualParent);
        // Recreate it again
        rangeVisualParent = new GameObject("RangeVisualParent");
        rangeVisualParent.SetActive(false);
        rangeVisualParent.transform.parent = this.gameObject.transform;
        rangeVisualParent.transform.localPosition = Vector3.zero;

        // If there is not already a beginning tile
        if (rangeVisualParent.transform.childCount < 1)
        {
            CreateSingleVisualTile(0, 0, rangeVisualParent.transform); // Make the first tile under the character
        }
        for (int i = 1; i <= moveRange + attackRange; ++i)
        {
            // Create a new child to serve as the parent for all the tiles about to be made
            GameObject tilesParent = new GameObject("VisualTile" + i + " Parent");
            tilesParent.transform.parent = rangeVisualParent.transform;
            tilesParent.transform.localPosition = Vector3.zero;

            Vector2Int placementPos = new Vector2Int(i, 0);
            // Go down, left
            while (placementPos.x > 0)
            {
                CreateSingleVisualTile(placementPos.x, placementPos.y, tilesParent.transform);
                placementPos.x -= 1;
                placementPos.y -= 1;
            }
            // Go up, left
            while (placementPos.y < 0)
            {
                CreateSingleVisualTile(placementPos.x, placementPos.y, tilesParent.transform);
                placementPos.x -= 1;
                placementPos.y += 1;
            }
            // Go up, right
            while (placementPos.x < 0)
            {
                CreateSingleVisualTile(placementPos.x, placementPos.y, tilesParent.transform);
                placementPos.x += 1;
                placementPos.y += 1;
            }
            // Go down, right
            while (placementPos.y > 0)
            {
                CreateSingleVisualTile(placementPos.x, placementPos.y, tilesParent.transform);
                placementPos.x += 1;
                placementPos.y -= 1;
            }
        }
        // If we are supposed to show the visuals after creating them
        rangeVisualParent.SetActive(shouldActivate);
    }

    /// <summary>
    /// Creates one visual tile for the visual tiles of this character
    /// </summary>
    /// <param name="x">X component of this tiles localPosition</param>
    /// <param name="y">Y component of this tiles localPosition</param>
    /// <param name="parent">What will be the parent of the newly created tile</param>
    private void CreateSingleVisualTile(int x, int y, Transform parent)
    {
        // Get the tile in question
        Vector2Int tileGridPos = new Vector2Int(Mathf.RoundToInt(this.transform.position.x + x), Mathf.RoundToInt(this.transform.position.y + y));
        Node testNode = mAContRef.GetNodeAtPosition(tileGridPos);
        if (testNode == null)
            return;

        // Initialize the variables that will change depending on if this is a move tile or an attack tile
        Sprite sprToUse = null;
        int orderOnLayer = -1;
        float alpha = 0.2f;

        bool isMoveTileVisual = moveTiles.Contains(testNode);
        bool isAttackTileVisual = attackTiles.Contains(testNode);
        // See if the tile is a move tile
        if (isMoveTileVisual)
        {
            isAttackTileVisual = false; // Can't be both attack and move
            sprToUse = moveTileSprite;
            orderOnLayer = 1;
            // If there is no character there, we want it to be brighter
            if (mAContRef.GetCharacterMAByNode(testNode) == null)
                alpha = 0.6f;
        }
        // See if the tile is an attack tile
        if (isAttackTileVisual)
        {
            sprToUse = attackTileSprite;
            orderOnLayer = 0;
            // If there is an enemy character there, we want it to be brighter
            MoveAttack mARef = mAContRef.GetCharacterMAByNode(testNode);
            if (mARef != null)
            {
                if ((mARef.WhatAmI == CharacterType.Enemy && this.WhatAmI == CharacterType.Ally) ||
                    (mARef.WhatAmI == CharacterType.Ally && this.WhatAmI == CharacterType.Enemy))
                {
                    alpha = 0.6f;
                }
            }
        }
        // See if the tile is neither
        if (!isMoveTileVisual && !isAttackTileVisual)
        {
            return;
        }

        // Create the tile, set it as a child of rangeVisualParent, and place it in a localPosition determined by the passed in values
        GameObject newTile = new GameObject("RangeVisual" + x + " " + y);
        newTile.transform.parent = parent;
        newTile.transform.localPosition = new Vector2(x, y);
        // Attach a sprite renderer to the object, put the correct sprite on it, place it in the correct sorting layer, and give it an order
        SpriteRenderer sprRend = newTile.AddComponent<SpriteRenderer>();
        sprRend.sprite = sprToUse;
        sprRend.sortingLayerName = visualSortingLayer;
        sprRend.sortingOrder = orderOnLayer;

        sprRend.color = new Color(sprRend.color.r, sprRend.color.g, sprRend.color.b, alpha);
    }

    /// <summary>
    /// Turns on or off the range visuals for movement and attack for this character
    /// </summary>
    /// <param name="shouldTurnOn">Is true, turn on the visuals. If false, turn them off</param>
    public void SetActiveVisuals(bool shouldTurnOn)
    {
        rangeVisualParent.SetActive(shouldTurnOn);
    }

    /// <summary>
    /// Sets the movement variables to initialize movement of the character
    /// </summary>
    public void StartMove()
    {
        //Debug.Log("Start Moving");
        // Assumes transToMove is on the grid
        currentNode = mAContRef.GetNodeByWorldPosition(this.gameObject.transform.position);
        if (currentNode != null)
        {
            //Debug.Log("currentNode at " + currentNode.position + " wants to move to the node at " + currentNode.whereToGo.position);
            doneTransX = false;
            doneTransY = false;
            transition = true;
        }
        else
        {
            Debug.Log("That node does not exist");
        }
    }

    /// <summary>
    /// Called every frame. Used to call the move function if the character should be transitioning
    /// </summary>
    private void Update()
    {
        if (transition)
        {
            Move();
        }
    }

    /// <summary>
    /// Moves the object slightly towards the next tile. Called every frame this place can transition
    /// Also plays the correct animations
    /// </summary>
    private void Move()
    {
        // If the currentNode exists
        if (currentNode.whereToGo != null)
        {
            // If not finished moving in the x
            if (!doneTransX)
            {
                // While this is left of the node it wants to get to
                if (currentNode.whereToGo.position.x - this.gameObject.transform.position.x > 0.1f)
                {
                    animRef.SetInteger("MoveState", 1);
                    sprRendRef.flipX = false;
                    this.gameObject.transform.position += Vector3.right * transSpeed * Time.deltaTime;
                }
                // While this is right of the node it wants to get to
                else if (this.gameObject.transform.position.x - currentNode.whereToGo.position.x > 0.1f)
                {
                    animRef.SetInteger("MoveState", 1);
                    sprRendRef.flipX = true;
                    this.gameObject.transform.position += Vector3.left * transSpeed * Time.deltaTime;
                }
                // Once we get to the x value of the node
                else
                {
                    this.gameObject.transform.position = new Vector3(Mathf.RoundToInt(currentNode.whereToGo.position.x), Mathf.RoundToInt(this.gameObject.transform.position.y), this.gameObject.transform.position.z);
                    doneTransX = true;
                }
            }

            // If not finished moving in the y
            if (!doneTransY)
            {
                // While the node this wants to get to is above where this is
                if (currentNode.whereToGo.position.y - this.gameObject.transform.position.y > 0.1f)
                {
                    sprRendRef.flipX = false;
                    animRef.SetInteger("MoveState", 2);
                    this.gameObject.transform.position += Vector3.up * transSpeed * Time.deltaTime;
                }
                // While the node this wants to get to is below where this is
                else if (this.gameObject.transform.position.y - currentNode.whereToGo.position.y > 0.1f)
                {
                    sprRendRef.flipX = false;
                    animRef.SetInteger("MoveState", 0);
                    this.gameObject.transform.position += Vector3.down * transSpeed * Time.deltaTime;
                }
                // Once we get to the y value of the node
                else
                {
                    this.gameObject.transform.position = new Vector3(Mathf.RoundToInt(this.gameObject.transform.position.x), Mathf.RoundToInt(currentNode.whereToGo.position.y), this.gameObject.transform.position.z);
                    doneTransY = true;
                }
            }

            // Once I have reached the node I was trying to get to
            if (doneTransX && doneTransY)
            {
                // If that node is the last node, stop moving
                if (currentNode.whereToGo == currentNode)
                {
                    EndMove();
                }
                // Otherwise, find the next node
                else
                {
                    // If there is no character still at the node I just came from, make it have no character on it, so that others can pass through it
                    if (mAContRef.GetCharacterMAByNode(currentNode) == null)
                    {
                        currentNode.occupying = CharacterType.None;
                    }
                    animRef.SetInteger("MoveState", -1);
                    currentNode = currentNode.whereToGo;
                    doneTransX = false;
                    doneTransY = false;
                }
            }
        }
        // If it doesn't exist, we shouldn't be moving
        else
        {
            transition = false;
            Debug.Log("Current Tile does not exist");
        }
    }

    /// <summary>
    /// Ends the movement of the character
    /// </summary>
    private void EndMove()
    {
        //Debug.Log("Finished Moving");
        currentNode.occupying = whatAmI;    // Set the node I am ending on to occupied with my type
        animRef.SetInteger("MoveState", -2);
        transition = false;
        currentNode = null;
        hasMoved = true;
        moveRange = 0;  // This character cannot attack again until the next turn, so their moveRange is now 0
        //Debug.Log("Reached destination");

        // If the character is an ally, then we have to allow the user to select again
        if (whatAmI == CharacterType.Ally)
        {
            mAGUIContRef.AllowSelect();
        }
        // If the character is an enemy, they need to attempt to attack now
        else if (whatAmI == CharacterType.Enemy)
        {
            enMAAIRef.AttemptAttack();
        }
    }

    /// <summary>
    /// Sets the animtion direction and sets hasAttacked to true
    /// </summary>
    public void StartAttack(Vector2Int attackNodePos)
    {
        // We have to set the enemy to attack, we just need to validate a bit first
        Node nodeToAttack = mAContRef.GetNodeAtPosition(attackNodePos);
        if (nodeToAttack != null)
        {
            MoveAttack charToAttack = mAContRef.GetCharacterMAByNode(nodeToAttack);
            if (charToAttack != null)
            {
                // Actually set the reference to the enemy HP
                enemyHP = charToAttack.GetComponent<Health>();
                if (enemyHP == null)
                    Debug.Log("Enemy to attack does not have a Health script attached to it");
            }
            else
                Debug.Log("Enemy to attack does not have a MoveAttack script attached to it");
        }
        else
        {
            //Debug.Log("Node to attack does not exist");
        }

        int attackDirection = -1;
        // If I am below the node I am striking, I should attack up
        if (attackNodePos.y - this.transform.position.y > 0)
        {
            sprRendRef.flipX = false;
            attackDirection = 1;
        }
        // If I am right the node I am striking, I should attack left
        if (this.transform.position.x - attackNodePos.x > 0)
        {
            sprRendRef.flipX = true;
            attackDirection = 2;
        }
        // If I am left the node I am striking, I should attack right
        if (attackNodePos.x - this.transform.position.x > 0)
        {
            sprRendRef.flipX = false;
            attackDirection = 3;
        }
        // If I am above the node I am striking, I should attack down
        if (this.transform.position.y - attackNodePos.y > 0)
        {
            sprRendRef.flipX = false;
            attackDirection = 4;
        }

        //Debug.Log("Start Attack");
        animRef.SetInteger("AttackDirection", attackDirection);
        hasAttacked = true;
    }

    /// <summary>
    /// Stops the attack animation and lets the player have control again
    /// If the attack kills the unit, we don't let the user have control again, we wait for them to die for that, same thing for the next enemy.
    /// </summary>
    public void EndAttack()
    {
        // Start attacking animation
        animRef.SetInteger("AttackDirection", -animRef.GetInteger("AttackDirection"));
        //Debug.Log("Finished Attacking");

        bool isFatal = false;   // If the attack will kill
        // Validate that we have an enemy to attack
        if (enemyHP != null)
        {
            // Find out if the attack will kill
            isFatal = false;
            if (enemyHP.CurHP - dmgToDeal <= 0)
            {
                isFatal = true;
            }
            // Deal the damage and get rid of our reference to the enemyHP
            enemyHP.TakeDamage(dmgToDeal);
            enemyHP = null;//
        }
        else
        {
            //Debug.Log("There was no enemy to attack");
        }

        // If we did kill the unit, then don't allow for select or allow the next enemy to move yet
        if (!isFatal)
        {
            // If the character is an ally, then we have to allow the user to select again
            if (whatAmI == CharacterType.Ally)
            {
                mAGUIContRef.AllowSelect();
                turnSysRef.IsPlayerDone();
            }
            // If the character is an enemy, then we have to tell the enemy AI script to move the next enemy
            if (whatAmI == CharacterType.Enemy)
            {
                enMAAIRef.NextEnemy();
            }
        }
    }

    /// <summary>
    /// Called when a new turn begins, lets the character move and attack again
    /// </summary>
    public void ResetMyTurn()
    {
        Stats myStats = this.gameObject.GetComponent<Stats>();
        myStats.Initialize();

        hasAttacked = false;
        hasMoved = false;
    }
}
