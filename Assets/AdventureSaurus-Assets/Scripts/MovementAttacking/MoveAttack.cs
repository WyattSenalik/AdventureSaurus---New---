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
    private int attackRange;   // How many tiles away this character can attack
    public int AttackRange
    {
        get { return attackRange; }
        set { attackRange = value; }
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


    // For actual movement calculations/animations
    [SerializeField] private float transSpeed = 3;  // Speed the character moves to transition from one tile to another
    private SpriteRenderer sprRendRef;  // Reference to the spriteRenderer attached to this character
    private Animator animRef;   // Reference to the animator attached to this character
    private MoveAttackGUIController mAGUIContRef;   // Reference to the moveAttackGUIController
    private Node currentNode;   // The node this object wants to move to next
    public bool transition;    // Whether this character should be moving or not

    private bool doneTransX;    // If this character has finished moving in the x direction
    private bool doneTransY;    // If this character has finished moving in the y direction
    private bool hasMoved;  // If this character has already moved in the "turn"
    public bool HasMoved
    {
        get { return hasMoved; }
        set { hasMoved = value; }
    }
    private bool hasAttacked;   // If this character has already attacked in the "turn"
    public bool HasAttacked
    {
        get { return hasAttacked; }
        set { hasAttacked = value; }
    }

    // For displaying the tile visuals
    public GameObject rangeVisualParent;

    // For attacking
    private Health enemyHP; // Reference to the health script attached to the enemy I start attacking
    private Skill skillRef; // Reference to the skill script attached to this character
    // For changing the character's skill in CharacterSkill
    public Skill SkillRef
    {
        set { skillRef = value; }
    }

    // For enemy movement AI
    private EnemyMoveAttackAI enMAAIRef;    // Reference to the EnemyMoveAttackAI script

    // For turns
    private TurnSystem turnSysRef;  // Reference to the TurnSystem script

    // For displaying the information about this character
    private Stats statsRef; // Reference to this character's stats
    public Stats MyStats
    {
        get { return statsRef; }
    }


    private Vector2 lastVel;

    // Events
    // When a character finishes moving
    public delegate void CharacterFinishedMoving();
    public static event CharacterFinishedMoving OnCharacterFinishedMoving;
    // When a character finishes attacking/doing its action
    public delegate void CharacterFinishedAction();
    public static event CharacterFinishedAction OnCharacterFinishedAction;

    /// <summary>
    /// Set references
    /// Called by Awake and called from Persistant Controller [allies only]
    /// </summary>
    public void SetReferences()
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
    }

    // Called before start
    private void Awake()
    {
        // These will have to be set a few times [allies only]
        SetReferences();
        // These will only be set once, since they are attached to the same object'
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
        statsRef = this.GetComponent<Stats>();
        if (statsRef == null)
        {
            Debug.Log("Could not find Stats attached to " + this.name);
        }
    }

    /// <summary>
    /// Initialize variables
    /// Called from Start and from PersistantController [allies only]
    /// </summary>
    public void Initialize()
    {
        currentNode = null;
        transition = false;
        doneTransX = true;
        doneTransY = true;
        hasMoved = false;
        hasAttacked = false;
    }

    // Called before the first frame
    private void Start()
    {
        Initialize();
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
            lastVel = Vector2.zero;
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
        if (currentNode.WhereToGo != null)
        {
            // If not finished moving in the x
            if (!doneTransX)
            {
                // While this is left of the node it wants to get to
                if (currentNode.WhereToGo.Position.x - this.gameObject.transform.position.x > 0.03f && lastVel.x <= 0)
                {
                    animRef.SetInteger("MoveState", 1);
                    sprRendRef.flipX = false;
                    this.gameObject.transform.Translate(Vector3.right * transSpeed * Time.deltaTime);
                    lastVel.x = -1;
                    //this.gameObject.transform.position += Vector3.right * transSpeed * Time.deltaTime;
                }
                // While this is right of the node it wants to get to
                else if (this.gameObject.transform.position.x - currentNode.WhereToGo.Position.x > 0.03f && lastVel.x >= 0)
                {
                    animRef.SetInteger("MoveState", 1);
                    sprRendRef.flipX = true;
                    this.gameObject.transform.Translate(Vector3.left * transSpeed * Time.deltaTime);
                    lastVel.x = 1;
                    //this.gameObject.transform.position += Vector3.left * transSpeed * Time.deltaTime;
                }
                // Once we get to the x value of the node
                else
                {
                    //this.gameObject.transform.position = new Vector3(Mathf.RoundToInt(currentNode.whereToGo.position.x), Mathf.RoundToInt(this.gameObject.transform.position.y), this.gameObject.transform.position.z);
                    doneTransX = true;
                    lastVel.x = 0;
                }
            }

            // If not finished moving in the y
            if (!doneTransY)
            {
                // While the node this wants to get to is above where this is
                if (currentNode.WhereToGo.Position.y - this.gameObject.transform.position.y > 0.03f && lastVel.y >= 0)
                {
                    sprRendRef.flipX = false;
                    animRef.SetInteger("MoveState", 2);
                    this.gameObject.transform.Translate(Vector3.up * transSpeed * Time.deltaTime);
                    lastVel.y = 1;
                    //this.gameObject.transform.position += Vector3.up * transSpeed * Time.deltaTime;
                }
                // While the node this wants to get to is below where this is
                else if (this.gameObject.transform.position.y - currentNode.WhereToGo.Position.y > 0.03f && lastVel.y <= 0)
                {
                    sprRendRef.flipX = false;
                    animRef.SetInteger("MoveState", 0);
                    this.gameObject.transform.Translate(Vector3.down * transSpeed * Time.deltaTime);
                    lastVel.y = -1;
                    //this.gameObject.transform.position += Vector3.down * transSpeed * Time.deltaTime;
                }
                // Once we get to the y value of the node
                else
                {
                    //this.gameObject.transform.position = new Vector3(Mathf.RoundToInt(this.gameObject.transform.position.x), Mathf.RoundToInt(currentNode.whereToGo.position.y), this.gameObject.transform.position.z);
                    doneTransY = true;
                    lastVel.y = 0;
                }
            }

            // Once I have reached the node I was trying to get to
            if (doneTransX && doneTransY)
            {
                // If that node is the last node, stop moving
                if (currentNode.WhereToGo == currentNode)
                { 
                    this.gameObject.transform.position = new Vector3(Mathf.RoundToInt(currentNode.WhereToGo.Position.x), Mathf.RoundToInt(currentNode.WhereToGo.Position.y), this.gameObject.transform.position.z);
                    EndMove();
                }
                // Otherwise, find the next node
                else
                {
                    // If there is no character still at the node I just came from, make it have no character on it, so that others can pass through it
                    if (mAContRef.GetCharacterMAByNode(currentNode) == null)
                    {
                        currentNode.Occupying = CharacterType.None;
                    }
                    //animRef.SetInteger("MoveState", -1);
                    currentNode = currentNode.WhereToGo;
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
        Debug.Log("Finished Moving");
        currentNode.Occupying = whatAmI;    // Set the node I am ending on to occupied with my type
        animRef.SetInteger("MoveState", -2);
        transition = false;
        currentNode = null;
        hasMoved = true;
        // This character cannot attack again until the next turn, so their moveRange is now 0
        moveRange = 0;
        //Debug.Log("Reached destination");

        // If the character is an ally, then we have to allow the user to select again
        if (whatAmI == CharacterType.Ally)
        {
            // Recalculate move and attack tiles before allowing to select again so that it doesn't look like you can move after moving
            // We need to recalculate the move and attack tiles
            mAGUIContRef.AllowSelect();
        }
        //// If the character is an enemy, they need to attempt to attack now
        //else if (whatAmI == CharacterType.Enemy)
        //{
        //    enMAAIRef.AttemptAttack();
        //}

        // Call the event for when a character finished moving
        if (OnCharacterFinishedMoving != null)
            OnCharacterFinishedMoving();
    }

    /// <summary>
    /// Calls the StartSkill from skillRef and sets hasAttacked to true
    /// </summary>
    /// <param name="attackNodePos">The grid position of the center of the attack</param>
    public void StartAttack(Vector2Int attackNodePos)
    {
        if (skillRef != null)
        {
            // We have attacked
            hasAttacked = true;
            skillRef.StartSkill(attackNodePos);
        }
    }

    /// <summary>
    /// Call EndSkill from skillRef
    /// </summary>
    public void EndAttack()
    {
        //Debug.Log("Ending attack1");
        if (skillRef != null)
        {
            //Debug.Log("End skill " + skillRef.SkillNum);
            skillRef.EndSkill();
        }

        // Call the event for finishing the action
        if (OnCharacterFinishedAction != null)
            OnCharacterFinishedAction();
    }

    /// <summary>
    /// Called when a new turn begins, lets the character move and attack again
    /// </summary>
    public void ResetMyTurn()
    {
        // Reset stats like speed
        Stats myStats = this.gameObject.GetComponent<Stats>();
        myStats.Initialize();

        // We need to recalculate the move and attack tiles
        CalcMoveTiles();
        CalcAttackTiles();

        hasAttacked = false;
        hasMoved = false;
    }
}

 