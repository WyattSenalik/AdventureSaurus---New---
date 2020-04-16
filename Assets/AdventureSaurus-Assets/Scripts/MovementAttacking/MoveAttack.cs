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
    // The valid tiles this character can move to
    private List<Node> _moveTiles;
    public List<Node> MoveTiles
    {
        get { return _moveTiles; }
        set { _moveTiles = value; }
    }
    // The valid tiles this character can attack
    private List<Node> _attackTiles;
    public List<Node> AttackTiles
    {
        get { return _attackTiles; }
        set { _attackTiles = value; }
    }
    // If the attack tiles are actually buff/heal tiles
    private bool _targetFriendly = false;
    public bool TargetFriendly
    {
        get { return _targetFriendly; }
        set { _targetFriendly = value; }
    }
    // The valid tiles this character can interact with
    private List<Node> _interactTiles;
    public List<Node> InteractTiles
    {
        get { return _interactTiles; }
        set { _interactTiles = value; }
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
    /// Finds and saves the nodes that can be moved to, attacked, and interacted with in the
    /// appropriate lists.
    /// Called from MoveAttackGUIController when this character is selected
    /// </summary>
    public void CalculateAllTiles()
    {
        // Initialize the three lists
        _moveTiles = new List<Node>();
        _attackTiles = new List<Node>();
        _interactTiles = new List<Node>();

        // Get the node this character is currently standing at
        Node startNode = _mAContRef.GetNodeByWorldPosition(this.transform.position);

        // This list is what has already been tested
        List<Node> testedNodes = new List<Node>();

        // This list holds the nodes that have yet to be tested for validity
        List<Node> currentNodes = new List<Node>();
        currentNodes.Add(startNode);

        // This is how many iterations of checks we have gone over. Aka, how many tiles have been traversed in one path
        int depth = 0;
        // 1. First iterate only over the range of tiles we can move
        while (depth < _moveRange)
        {
            int amountNodes = currentNodes.Count;
            for (int i = 0; i < amountNodes; ++i)
            {
                // If the current node is null, end this iteration and start the next one
                if (currentNodes[i] != null)
                {
                    Vector2Int curNodePos = currentNodes[i].Position;
                    // Check above node
                    Vector2Int testPos = curNodePos + Vector2Int.up;
                    MoveTestNode(testPos, testedNodes, currentNodes);

                    // Check left node
                    testPos = curNodePos + Vector2Int.left;
                    MoveTestNode(testPos, testedNodes, currentNodes);

                    // Check right node
                    testPos = curNodePos + Vector2Int.right;
                    MoveTestNode(testPos, testedNodes, currentNodes);

                    // Check down node
                    testPos = curNodePos + Vector2Int.down;
                    MoveTestNode(testPos, testedNodes, currentNodes);
                }

            }
            // Removes the nodes that have already been iterated over
            for (int i = 0; i < amountNodes; ++i)
            {
                currentNodes.RemoveAt(0);
            }
            ++depth;
        }
        // 2. Now iterate one more tile to check for interactables (There is little need for a while statement,
        // since we are only iterating one more tile, but it keeps it looking similar)
        while (depth < _moveRange + 1)
        {
            int amountNodes = currentNodes.Count;
            for (int i = 0; i < amountNodes; ++i)
            {
                // If the current node is null, end this iteration and start the next one
                if (currentNodes[i] != null)
                {
                    Vector2Int curNodePos = currentNodes[i].Position;
                    // Check above node
                    Vector2Int testPos = curNodePos + Vector2Int.up;
                    InteractTestNode(testPos, testedNodes, currentNodes);

                    // Check left node
                    testPos = curNodePos + Vector2Int.left;
                    InteractTestNode(testPos, testedNodes, currentNodes);

                    // Check right node
                    testPos = curNodePos + Vector2Int.right;
                    InteractTestNode(testPos, testedNodes, currentNodes);

                    // Check down node
                    testPos = curNodePos + Vector2Int.down;
                    InteractTestNode(testPos, testedNodes, currentNodes);
                }

            }
            // Removes the nodes that have already been iterated over
            for (int i = 0; i < amountNodes; ++i)
            {
                currentNodes.RemoveAt(0);
            }
            ++depth;
        }
        // 3. Finally, iterate over the remaining depth to check for attacking
        while (depth < _moveRange + _attackRange)
        {
            int amountNodes = currentNodes.Count;
            for (int i = 0; i < amountNodes; ++i)
            {
                // If the current node is null, end this iteration and start the next one
                if (currentNodes[i] != null)
                {
                    Vector2Int curNodePos = currentNodes[i].Position;
                    // Check above node
                    Vector2Int testPos = curNodePos + Vector2Int.up;
                    AttackTestNode(testPos, testedNodes, currentNodes);

                    // Check left node
                    testPos = curNodePos + Vector2Int.left;
                    AttackTestNode(testPos, testedNodes, currentNodes);

                    // Check right node
                    testPos = curNodePos + Vector2Int.right;
                    AttackTestNode(testPos, testedNodes, currentNodes);

                    // Check down node
                    testPos = curNodePos + Vector2Int.down;
                    AttackTestNode(testPos, testedNodes, currentNodes);
                }

            }
            // Removes the nodes that have already been iterated over
            for (int i = 0; i < amountNodes; ++i)
            {
                currentNodes.RemoveAt(0);
            }
            ++depth;
        }
    }

    /// <summary>
    /// Used to test what kind of tile the current testNode in CalculateAllTiles.
    /// This is the Move specific variety. That means, nodes will be appended to currentNodes only when
    /// the character can legally travel to that location (when the node is occupied by no one or somone
    /// on the same team).
    /// It also means that we test for all 3 different interactions: movement, attacking, and interactables
    /// </summary>
    /// <param name="testPos">Position of the node being tested</param>
    /// <param name="testedNodes">Reference to the List of Nodes that have been tested before</param>
    /// <param name="currentNodes">Reference to the List of Nodes that still need to be tested</param>
    private void MoveTestNode(Vector2Int testPos, List<Node> testedNodes, List<Node> currentNodes)
    {
        Node testNode = _mAContRef.GetNodeAtPosition(testPos);
        // If there is a node there are we have not tested it before
        if (testNode != null && !testedNodes.Contains(testNode))
        {
            // If the node is not occupied, I can move there
            if (testNode.Occupying == CharacterType.None)
            {
                _moveTiles.Add(testNode);
                currentNodes.Add(testNode);
            }
            // If it is occupied by someone on my team, I can't move there, but I can move past there
            else if (testNode.Occupying == _whatAmI)
            {
                currentNodes.Add(testNode);
                // Also put an attack/heal tile here
                _attackTiles.Add(testNode);
            }
            // If it is occupied by a character and I have attack range of at least one, I might want to attack/heal there
            // Or if its a wall, because consistency
            else if ((testNode.Occupying == CharacterType.Ally || testNode.Occupying == CharacterType.Enemy
                || testNode.Occupying == CharacterType.Wall) && _attackRange > 0)
            {
                _attackTiles.Add(testNode);
            }
            // If the tile is occupied by an interactable
            else if (testNode.Occupying == CharacterType.Interactable)
            {
                // Get the interactable at that node
                Interactable interactAtNode = _mAContRef.GetInteractableByNode(testNode);
                // Only add the interactable to interact tiles if it can currently be interacted with
                if (interactAtNode != null && interactAtNode.CanInteractWith)
                    _interactTiles.Add(testNode);
            }

            // Add the node to the tested nodes
            testedNodes.Add(testNode);
        }
    }

    /// <summary>
    /// Used to test what kind of tile the current testNode in CalculateAllTiles.
    /// This is the Interact specific variety. That means, nodes will be appended regardless of what is at that tile.
    /// It also means that we test for only 2 different interactions: attacking and interactables (movement has been done already).
    /// If the attack range is more than 0, all these tiles will become attack tiles.
    /// </summary>
    /// <param name="testPos">Position of the node being tested</param>
    /// <param name="testedNodes">Reference to the List of Nodes that have been tested before</param>
    /// <param name="currentNodes">Reference to the List of Nodes that still need to be tested</param>
    private void InteractTestNode(Vector2Int testPos, List<Node> testedNodes, List<Node> currentNodes)
    {
        Node testNode = _mAContRef.GetNodeAtPosition(testPos);
        // If there is a node there are we have not tested it before
        if (testNode != null && !testedNodes.Contains(testNode))
        {
            // If the tile is occupied by an interactable
            if (testNode.Occupying == CharacterType.Interactable)
            {
                // Get the interactable at that node
                Interactable interactAtNode = _mAContRef.GetInteractableByNode(testNode);
                // Only add the interactable to interact tiles if it can currently be interacted with
                if (interactAtNode != null && interactAtNode.CanInteractWith)
                    _interactTiles.Add(testNode);
            }
            // If I can reach with an attack
            else if (_attackRange >= 1)
            {
                _attackTiles.Add(testNode);
            }

            // Add the node to the tested nodes and the current nodes
            testedNodes.Add(testNode);
            currentNodes.Add(testNode);
        }
    }

    /// <summary>
    /// Used to test what kind of tile the current testNode in CalculateAllTiles.
    /// This is the Attack specific variety. That means, nodes will be appended regardless of what is at that tile
    /// It also means that we test for only 1 interaction: attacking (movement and interaction have been done already)
    /// </summary>
    /// <param name="testPos">Position of the node being tested</param>
    /// <param name="testedNodes">Reference to the List of Nodes that have been tested before</param>
    /// <param name="currentNodes">Reference to the List of Nodes that still need to be tested</param>
    private void AttackTestNode(Vector2Int testPos, List<Node> testedNodes, List<Node> currentNodes)
    {
        Node testNode = _mAContRef.GetNodeAtPosition(testPos);
        // If there is a node there are we have not tested it before
        if (testNode != null && !testedNodes.Contains(testNode))
        {
            // We do not even test anything here, it is at the attack range
            _attackTiles.Add(testNode);

            // Add the node to the tested nodes and the current nodes
            testedNodes.Add(testNode);
            currentNodes.Add(testNode);
        }
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

        // Recalculate the move and attack nodes, so that it does not look like the character can move
        CalculateAllTiles();

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
        int infiniteAvoid = 1000;
        int counter = 0;
        // Check if there are any ongoing actions stopping us from finishing this enemy's turn
        while (_ongoingActions.Count != 0)
        {
            if (infiniteAvoid < ++counter)
            {
                GameObject nullObj = null;
                Grimoire errorCause = nullObj.GetComponent<Grimoire>();
            }
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

 