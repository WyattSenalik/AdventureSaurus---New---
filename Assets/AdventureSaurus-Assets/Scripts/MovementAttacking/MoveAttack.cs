using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAttack : MonoBehaviour
{
    // What kind of character is this script attached to?
    [SerializeField] private CharacterType _whatAmI = CharacterType.None;
    public CharacterType WhatAmI
    {
        get { return _whatAmI; }
    }

    // How many tiles this character can move
    private int _moveRange;
    public int MoveRange
    {
        get { return _moveRange; }
        set { _moveRange = value; }
    }
    // How many tiles away this character can attack
    private int _attackRange;
    public int AttackRange
    {
        get { return _attackRange; }
        set { _attackRange = value; }
    }
    private List<Node> _moveTiles;   // The valid tiles this character can move to
    public List<Node> MoveTiles
    {
        get { return _moveTiles; }
        set { _moveTiles = value; }
    }
    private List<Node> _attackTiles; // The valid tiles this character can attack
    public List<Node> AttackTiles
    {
        get { return _attackTiles; }
        set { _attackTiles = value; }
    }

    // A reference to the MoveAttackController script
    private MoveAttackController _mAContRef;


    // For actual movement calculations/animations
    // Speed the character moves to transition from one tile to another
    [SerializeField] private float _transSpeed = 3;
    // Reference to the spriteRenderer attached to this character
    private SpriteRenderer _sprRendRef;
    // Reference to the animator attached to this character
    private Animator _animRef;
    // The node this object wants to move to next
    private Node _currentNode;
    // Whether this character should be moving or not
    private bool _transition;
    public bool Transition
    {
        get { return _transition; }
    }
    // If this character has finished moving in the x direction
    private bool _doneTransX;
    // If this character has finished moving in the y direction
    private bool _doneTransY;
    // If this character has already moved in the "turn"
    private bool _hasMoved;
    public bool HasMoved
    {
        get { return _hasMoved; }
        set { _hasMoved = value; }
    }
    // If this character has already attacked in the "turn"
    private bool _hasAttacked;
    public bool HasAttacked
    {
        get { return _hasAttacked; }
        set { _hasAttacked = value; }
    }

    // For displaying the tile visuals
    private GameObject _rangeVisualParent;
    public GameObject RangeVisualParent
    {
        get { return _rangeVisualParent; }
        set { _rangeVisualParent = value; }
    }

    // For attacking
    // Reference to the health script attached to the enemy I start attacking
    private Health _enemyHP;

    // Reference to the skill script attached to this character (and active)
    private Skill _skillRef;
    // For changing the character's skill in CharacterSkill
    public Skill SkillRef
    {
        set { _skillRef = value; }
    }

    // For displaying the information about this character
    // Reference to this character's stats
    private Stats _statsRef;
    public Stats MyStats
    {
        get { return _statsRef; }
    }

    // Which direction this character was moving previously
    private Vector2 _lastVel;

    // For the things that happen after using a skill (like lowering health), 
    // to determine if we should signal the if the character is finished
    private static List<bool> _ongoingActions = new List<bool>();
    public static void AddOngoingAction()
    {
        _ongoingActions.Add(true);
    }
    public static void RemoveOngoingAction()
    {
        _ongoingActions.RemoveAt(0);
    }

    // Events
    // When a character finishes moving
    public delegate void CharacterFinishedMoving();
    public static event CharacterFinishedMoving OnCharacterFinishedMoving;
    // When a character finishes attacking/doing its action
    public delegate void CharacterFinishedAction();
    public static event CharacterFinishedAction OnCharacterFinishedAction;



    // Called when the component is toggled active
    // Subscribe to events
    private void OnEnable()
    {
        // When the game is paused, disable this script
        Pause.OnPauseGame += HideScript;
        // Unsubscribe to the unpause event (since if this is active, the game is unpaused)
        Pause.OnUnpauseGame -= ShowScript;

        // When generation is done, do some initialization
        ProceduralGenerationController.OnFinishGenerationNoParam += SetReferences;
        ProceduralGenerationController.OnFinishGenerationNoParam += Initialize;
        ProceduralGenerationController.OnFinishGenerationNoParam += ResetMyTurn;
        //ProceduralGenerationController.OnFinishGenerationNoParam += CalcMoveTiles;
        //ProceduralGenerationController.OnFinishGenerationNoParam += CalcAttackTiles;
    }

    // Called when the component is toggled inactive
    // Unsubscribe to events
    private void OnDisable()
    {
        // Unsubscribe to the pause event (since if this is inactive, the game is paused)
        Pause.OnPauseGame -= HideScript;
        // When the game is unpaused, re-enable this script
        Pause.OnUnpauseGame += ShowScript;

        ProceduralGenerationController.OnFinishGenerationNoParam -= SetReferences;
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
        ProceduralGenerationController.OnFinishGenerationNoParam -= ResetMyTurn;
        //ProceduralGenerationController.OnFinishGenerationNoParam -= CalcMoveTiles;
        //ProceduralGenerationController.OnFinishGenerationNoParam -= CalcAttackTiles;
    }

    // Called when the game object is destroyed
    // Unsubscribe to ALL events
    private void OnDestroy()
    {
        Pause.OnPauseGame -= HideScript;
        Pause.OnUnpauseGame -= ShowScript;
        ProceduralGenerationController.OnFinishGenerationNoParam -= SetReferences;
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
        ProceduralGenerationController.OnFinishGenerationNoParam -= ResetMyTurn;
        //ProceduralGenerationController.OnFinishGenerationNoParam -= CalcMoveTiles;
        //ProceduralGenerationController.OnFinishGenerationNoParam -= CalcAttackTiles;
    }

    // Called before start
    private void Awake()
    {
        // These will have to be set a few times [allies only]
        SetReferences();
        // These will only be set once, since they are attached to the same object
        _sprRendRef = this.GetComponent<SpriteRenderer>();
        if (_sprRendRef == null)
        {
            Debug.Log("Could not find SpriteRenderer attached to " + this.name);
        }
        _animRef = this.GetComponent<Animator>();
        if (_animRef == null)
        {
            Debug.Log("Could not find Animator attached to " + this.name);
        }
        _statsRef = this.GetComponent<Stats>();
        if (_statsRef == null)
        {
            Debug.Log("Could not find Stats attached to " + this.name);
        }
    }


    /// <summary>
    /// Sets the reference to the MoveAttackController
    /// </summary>
    private void SetReferences()
    {
        GameObject gameControllerObj = GameObject.FindWithTag("GameController");
        if (gameControllerObj == null)
        {
            Debug.Log("Could not find any GameObject with the tag GameController");
        }
        else
        {
            _mAContRef = gameControllerObj.GetComponent<MoveAttackController>();
            if (_mAContRef == null)
            {
                Debug.Log("Could not find MoveAttackController attached to " + gameControllerObj.name);
            }
        }
    }

    // Called before the first frame
    private void Start()
    {
        Initialize();
    }

    /// <summary>
    /// Initializes variables for walking
    /// </summary>
    private void Initialize()
    {
        _currentNode = null;
        _transition = false;
        _doneTransX = true;
        _doneTransY = true;
        _hasMoved = false;
        _hasAttacked = false;
    }

    /// <summary>
    /// Figures out what nodes are valid for me to move to and saves them in moveTiles
    /// </summary>
    public void CalcMoveTiles()
    {
        // Use that my position to get the node I'm on
        Node myNode = _mAContRef.GetNodeByWorldPosition(this.transform.position);
        // Find the valid move tiles and save them
        _moveTiles = _mAContRef.GetValidMovementNodes(myNode, _moveRange, _whatAmI);
    }

    /// <summary>
    /// Figures out what nodes are valid for me to atack and saves them in attackTiles
    /// </summary>
    public void CalcAttackTiles()
    {
        // Use my move tiles to figure out where I can attack
        _attackTiles = _mAContRef.GetValidAttackNodes(_moveTiles, _attackRange);
    }

    /// <summary>
    /// Called every frame. Used to call the move function if the character should be transitioning
    /// </summary>
    private void Update()
    {
        if (_transition)
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
        if (_currentNode.WhereToGo != null)
        {
            // If not finished moving in the x
            if (!_doneTransX)
            {
                // While this is left of the node it wants to get to
                if (_currentNode.WhereToGo.Position.x - this.gameObject.transform.position.x > 0.03f && _lastVel.x <= 0)
                {
                    _animRef.SetInteger("MoveState", 1);
                    _sprRendRef.flipX = false;
                    this.gameObject.transform.Translate(Vector3.right * _transSpeed * Time.deltaTime);
                    _lastVel.x = -1;
                    //this.gameObject.transform.position += Vector3.right * transSpeed * Time.deltaTime;
                }
                // While this is right of the node it wants to get to
                else if (this.gameObject.transform.position.x - _currentNode.WhereToGo.Position.x > 0.03f && _lastVel.x >= 0)
                {
                    _animRef.SetInteger("MoveState", 1);
                    _sprRendRef.flipX = true;
                    this.gameObject.transform.Translate(Vector3.left * _transSpeed * Time.deltaTime);
                    _lastVel.x = 1;
                    //this.gameObject.transform.position += Vector3.left * transSpeed * Time.deltaTime;
                }
                // Once we get to the x value of the node
                else
                {
                    //this.gameObject.transform.position = new Vector3(Mathf.RoundToInt(currentNode.whereToGo.position.x), Mathf.RoundToInt(this.gameObject.transform.position.y), this.gameObject.transform.position.z);
                    _doneTransX = true;
                    _lastVel.x = 0;
                }
            }

            // If not finished moving in the y
            if (!_doneTransY)
            {
                // While the node this wants to get to is above where this is
                if (_currentNode.WhereToGo.Position.y - this.gameObject.transform.position.y > 0.03f && _lastVel.y >= 0)
                {
                    _sprRendRef.flipX = false;
                    _animRef.SetInteger("MoveState", 2);
                    this.gameObject.transform.Translate(Vector3.up * _transSpeed * Time.deltaTime);
                    _lastVel.y = 1;
                    //this.gameObject.transform.position += Vector3.up * transSpeed * Time.deltaTime;
                }
                // While the node this wants to get to is below where this is
                else if (this.gameObject.transform.position.y - _currentNode.WhereToGo.Position.y > 0.03f && _lastVel.y <= 0)
                {
                    _sprRendRef.flipX = false;
                    _animRef.SetInteger("MoveState", 0);
                    this.gameObject.transform.Translate(Vector3.down * _transSpeed * Time.deltaTime);
                    _lastVel.y = -1;
                    //this.gameObject.transform.position += Vector3.down * transSpeed * Time.deltaTime;
                }
                // Once we get to the y value of the node
                else
                {
                    //this.gameObject.transform.position = new Vector3(Mathf.RoundToInt(this.gameObject.transform.position.x), Mathf.RoundToInt(currentNode.whereToGo.position.y), this.gameObject.transform.position.z);
                    _doneTransY = true;
                    _lastVel.y = 0;
                }
            }

            // Once I have reached the node I was trying to get to
            if (_doneTransX && _doneTransY)
            {
                // If that node is the last node, stop moving
                if (_currentNode.WhereToGo == _currentNode)
                { 
                    this.gameObject.transform.position = new Vector3(Mathf.RoundToInt(_currentNode.WhereToGo.Position.x), Mathf.RoundToInt(_currentNode.WhereToGo.Position.y), this.gameObject.transform.position.z);
                    EndMove();
                }
                // Otherwise, find the next node
                else
                {
                    // If there is no character still at the node I just came from, make it have no character on it, so that others can pass through it
                    if (_mAContRef.GetCharacterMAByNode(_currentNode) == null)
                    {
                        _currentNode.Occupying = CharacterType.None;
                    }
                    //animRef.SetInteger("MoveState", -1);
                    _currentNode = _currentNode.WhereToGo;
                    _doneTransX = false;
                    _doneTransY = false;
                }
            }
        }
        // If it doesn't exist, we shouldn't be moving
        else
        {
            _transition = false;
            Debug.Log("Current Tile does not exist");
        }
    }

    /// <summary>
    /// Sets the movement variables to initialize movement of the character
    /// </summary>
    public void StartMove()
    {
        //Debug.Log("Start Moving");
        // Assumes transToMove is on the grid
        _currentNode = _mAContRef.GetNodeByWorldPosition(this.gameObject.transform.position);
        if (_currentNode != null)
        {
            //Debug.Log("currentNode at " + currentNode.position + " wants to move to the node at " + currentNode.whereToGo.position);
            _lastVel = Vector2.zero;
            _doneTransX = false;
            _doneTransY = false;
            _transition = true;
        }
        else
        {
            Debug.Log("That node does not exist");
        }
    }

    /// <summary>
    /// Ends the movement of the character
    /// </summary>
    private void EndMove()
    {
        //Debug.Log("Finished Moving");
        // Set the node I am ending on to occupied with my type
        _currentNode.Occupying = _whatAmI;
        // Reset the animator stuff
        _animRef.SetInteger("MoveState", -2);
        // Make it so we are not transitioning between tiles anymore
        _transition = false;
        _currentNode = null;
        // We have now moved
        _hasMoved = true;
        // This character cannot attack again until the next turn, so their moveRange is now 0
        _moveRange = 0;
        //Debug.Log("Reached destination");

        // Recalculate the move and attack nodes, so that it does not look like the 
        CalcMoveTiles();
        CalcAttackTiles();

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
        //Debug.Log("Start Attack");
        if (_skillRef != null)
        {
            // We have attacked
            _hasAttacked = true;
            //Debug.Log("Start Skill");
            _skillRef.StartSkill(attackNodePos);
        }
    }

    /// <summary>
    /// Call EndSkill from skillRef
    /// </summary>
    public void EndAttack()
    {
        //Debug.Log("Ending attack1");
        if (_skillRef != null)
        {
            //Debug.Log("End skill " + skillRef.SkillNum);
            _skillRef.EndSkill();
        }

        // Wait until signaling the end of the action subsuquent things (like taking damage or death) are done
        StartCoroutine(WaitFinishAction());
    }

    /// <summary>
    /// Waits on calling the OnCharacterFinishedAction until there are no ongoing things
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator WaitFinishAction()
    {
        // Check if there are any ongoing actions stopping us from finishing this enemy's turn
        while (true)
        {
            // If there are no ongoing actions, break from the loop
            if (_ongoingActions.Count == 0)
                break;

            yield return null;
        }

        // Call the event for finishing the action
        if (OnCharacterFinishedAction != null)
            OnCharacterFinishedAction();

        yield return null;
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
        //CalcMoveTiles();
        //CalcAttackTiles();

        _hasAttacked = false;
        _hasMoved = false;
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

 