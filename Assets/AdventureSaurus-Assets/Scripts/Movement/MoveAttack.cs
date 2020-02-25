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


    // For actual movement calculations/animations
    [SerializeField] private float transSpeed = 4;  // Speed the character moves to transition from one tile to another
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
    }
    private bool hasAttacked;   // If this character has already attacked in the "turn"
    public bool HasAttacked
    {
        get { return hasAttacked; }
    }

    // For displaying the tile visuals
    public GameObject rangeVisualParent;

    // For attacking
    private Health enemyHP; // Reference to the health script attached to the enemy I start attacking
    private Skill skillRef; // Reference to the skill script attached to this character

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
        skillRef = this.GetComponent<Skill>();
        if (skillRef == null)
        {
            Debug.Log("Could not find Skill attached to " + this.name);
        }
        statsRef = this.GetComponent<Stats>();
        if (statsRef == null)
        {
            Debug.Log("Could not find Stats attached to " + this.name);
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
                if (currentNode.whereToGo.position.x - this.gameObject.transform.position.x > 0.03f)
                {
                    animRef.SetInteger("MoveState", 1);
                    sprRendRef.flipX = false;
                    this.gameObject.transform.position += Vector3.right * transSpeed * Time.deltaTime;
                }
                // While this is right of the node it wants to get to
                else if (this.gameObject.transform.position.x - currentNode.whereToGo.position.x > 0.03f)
                {
                    animRef.SetInteger("MoveState", 1);
                    sprRendRef.flipX = true;
                    this.gameObject.transform.position += Vector3.left * transSpeed * Time.deltaTime;
                }
                // Once we get to the x value of the node
                else
                {
                    //this.gameObject.transform.position = new Vector3(Mathf.RoundToInt(currentNode.whereToGo.position.x), Mathf.RoundToInt(this.gameObject.transform.position.y), this.gameObject.transform.position.z);
                    doneTransX = true;
                }
            }

            // If not finished moving in the y
            if (!doneTransY)
            {
                // While the node this wants to get to is above where this is
                if (currentNode.whereToGo.position.y - this.gameObject.transform.position.y > 0.03f)
                {
                    sprRendRef.flipX = false;
                    animRef.SetInteger("MoveState", 2);
                    this.gameObject.transform.position += Vector3.up * transSpeed * Time.deltaTime;
                }
                // While the node this wants to get to is below where this is
                else if (this.gameObject.transform.position.y - currentNode.whereToGo.position.y > 0.03f)
                {
                    sprRendRef.flipX = false;
                    animRef.SetInteger("MoveState", 0);
                    this.gameObject.transform.position += Vector3.down * transSpeed * Time.deltaTime;
                }
                // Once we get to the y value of the node
                else
                {
                    //this.gameObject.transform.position = new Vector3(Mathf.RoundToInt(this.gameObject.transform.position.x), Mathf.RoundToInt(currentNode.whereToGo.position.y), this.gameObject.transform.position.z);
                    doneTransY = true;
                }
            }

            // Once I have reached the node I was trying to get to
            if (doneTransX && doneTransY)
            {
                // If that node is the last node, stop moving
                if (currentNode.whereToGo == currentNode)
                { 
                    this.gameObject.transform.position = new Vector3(Mathf.RoundToInt(currentNode.whereToGo.position.x), Mathf.RoundToInt(currentNode.whereToGo.position.y), this.gameObject.transform.position.z);
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
                    //animRef.SetInteger("MoveState", -1);
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
            // Recalculate move and attack tiles before allowing to select again so that it doesn't look like you can move after moving
            // We need to recalculate the move and attack tiles
            mAGUIContRef.AllowSelect();
        }
        // If the character is an enemy, they need to attempt to attack now
        else if (whatAmI == CharacterType.Enemy)
        {
            enMAAIRef.AttemptAttack();
        }
    }

    /// <summary>
    /// Calls the StartSkill from skillRef and sets hasAttacked to true
    /// </summary>
    /// <param name="attackNodePos">The grid position of the center of the attack</param>
    public void StartAttack(Vector2Int attackNodePos)
    {
        // We have attacked
        hasAttacked = true;

        skillRef.StartSkill(attackNodePos);
        /*
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
        // This occurs when an enemy isn't close enough to an ally to attack. Call end attack and don't start playing an animation
        else
        {
            //Debug.Log("Node to attack does not exist");
            EndAttack();
            return;
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
        */
    }

    /// <summary>
    /// Call EndSkill from skillRef
    /// </summary>
    public void EndAttack()
    {
        skillRef.EndSkill();
        /*
        //Debug.Log("Finished Attacking");

        // Validate that we have an enemy to attack
        if (enemyHP != null)
        {
            // Start attacking animation
            animRef.SetInteger("AttackDirection", -animRef.GetInteger("AttackDirection"));
            // Deal the damage and get rid of our reference to the enemyHP
            enemyHP.TakeDamage(dmgToDeal);
            enemyHP = null;
        }
        // If we have no enemy to attack, give back control to the proper authority
        else
        {
            // We should not attack anything, so set attack animation to 0
            animRef.SetInteger("AttackDirection", 0);

            //Debug.Log("There was no enemy to attack");
            // If this character is an enemy, have the next enemy attack
            if (whatAmI == CharacterType.Enemy)
            {
                enMAAIRef.StartNextEnemy();
            }
            // If this character is an ally, give back control to the user
            if (whatAmI == CharacterType.Ally)
            {
                mAGUIContRef.AllowSelect();
                turnSysRef.IsPlayerDone();
            }
        }
        */
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

 