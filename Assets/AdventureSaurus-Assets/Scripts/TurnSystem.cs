using UnityEngine;
using UnityEngine.UI;

public enum TurnState {GAMESTOP, PLAYERTURN, ENEMYTURN }//the states the game can be in

public class TurnSystem : MonoBehaviour
{
    // Reference to the endturn button so that we can turn it off when needed
    [SerializeField] Button _endTurnButt = null;
    // The current state of the game
    private TurnState _state;
    public TurnState State {
        get { return _state; }
    }
    // The parent of the allies
    private Transform _allyParent;
    // The parent of the enemies
    private Transform _enemyParent;

    // References to other GameController scripts
    private EnemyTurnController _enTurnContRef;

    // Events
    // For when it is the enemy's turn
    public delegate void BeginEnemyTurn();
    public static event BeginEnemyTurn OnBeginEnemyTurn;
    // For when it is the player's turn
    public delegate void BeginPlayerTurn();
    public static event BeginPlayerTurn OnBeginPlayerTurn;
    // For when the player's turn ends
    public delegate void FinishPlayerTurn();
    public static event FinishPlayerTurn OnFinishPlayerTurn;

    // Called when gameobject is toggled on
    // Subscribe to events
    private void OnEnable()
    {
        // When the enemy's turn ends, start the player's turn
        EnemyTurnController.OnEndEnemyTurn += StartPlayerTurn;
        // When the generation is over, initialize this script
        ProceduralGenerationController.OnFinishGenerationNoParam += Initialize;

        // When the game is paused, disable this script
        Pause.OnPauseGame += HideScript;
        // Unsubscribe to the unpause event (since if this is active, the game is unpaused)
        Pause.OnUnpauseGame -= ShowScript;

        // When there are no enemy characters active, let the player go again
        MoveAttack.OnCharacterFinishedMoving += LetPlayerGoAgain;
        MoveAttack.OnCharacterFinishedAction += LetPlayerGoAgain;
        Health.OnCharacterDeath += LetPlayerGoAgain;
    }

    // Called when gameobject is toggled off
    // Unsubscribe to events
    private void OnDisable()
    {
        EnemyTurnController.OnEndEnemyTurn -= StartPlayerTurn;
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;

        // Unsubscribe to the pause event (since if this is inactive, the game is paused)
        Pause.OnPauseGame -= HideScript;
        // When the game is unpaused, re-enable this script
        Pause.OnUnpauseGame += ShowScript;

        MoveAttack.OnCharacterFinishedMoving -= LetPlayerGoAgain;
        MoveAttack.OnCharacterFinishedAction -= LetPlayerGoAgain;
        Health.OnCharacterDeath -= LetPlayerGoAgain;
    }

    // Called when gameobject is destroyed
    // Unsubscribe to ALL events
    private void OnDestroy()
    {
        EnemyTurnController.OnEndEnemyTurn -= StartPlayerTurn;
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
        Pause.OnPauseGame -= HideScript;
        Pause.OnUnpauseGame -= ShowScript;
        MoveAttack.OnCharacterFinishedMoving -= LetPlayerGoAgain;
        MoveAttack.OnCharacterFinishedAction -= LetPlayerGoAgain;
        Health.OnCharacterDeath -= LetPlayerGoAgain;
    }

    // Called before Start
    // Set referecnes
    private void Awake()
    {
        _enTurnContRef = this.GetComponent<EnemyTurnController>();
        if (_enTurnContRef == null)
            Debug.Log(this.name + " has no EnemyTurnController attached to it");
    }

    // Start is called before the first frame update
    private void Start()
    {
        _state = TurnState.GAMESTOP;//code between GAMESTOP and PLAYERTURN is for setup
    }

    /// <summary>
    /// Initializes things for this script.
    /// Called from the OnFinishGenerating event
    /// </summary>
    private void Initialize()
    {
        _allyParent = GameObject.Find(ProceduralGenerationController.ALLY_PARENT_NAME).transform;
        _enemyParent = GameObject.Find(ProceduralGenerationController.ENEMY_PARENT_NAME).transform;
    }

    /// <summary>
    /// Called by the OnEnemyTurnEnd event from EnemyMoveAttackAI after all enemies have moved.
    /// Starts Players Turn, Enables player to control characters
    /// </summary>
    private void StartPlayerTurn()
    {
        //Debug.Log("StartPlayerTurn");
        _endTurnButt.interactable = true;
        _state = TurnState.PLAYERTURN;
        // We reset the turns of all characters
        // Allies
        foreach (Transform allyTrans in _allyParent)
        {
            MoveAttack allyMA = allyTrans.GetComponent<MoveAttack>();
            if (allyMA != null)
                allyMA.ResetMyTurn();
            else
                Debug.LogError("There is no MoveAttack script attached to " + allyTrans.name);
        }
        // Enemies
        foreach (Transform enemyTrans in _enemyParent)
        {
            MoveAttack enemyMA = enemyTrans.GetComponent<MoveAttack>();
            if (enemyMA != null)
                enemyMA.ResetMyTurn();
            else
                Debug.LogError("There is no MoveAttack script attached to " + enemyTrans.name);
        }

        // Call the event that the player's turn has begun
        if (OnBeginPlayerTurn != null)
        {
            //Debug.Log("OnBeginPlayerTurn");
            OnBeginPlayerTurn();
        }
    }

    /// <summary>
    /// Starts Enemies Turn, Starts their AI
    /// </summary>
    private void StartEnemyTurn()
    {
        // Start the enemy's turn 
        if (OnBeginEnemyTurn != null)
        {
            _endTurnButt.interactable = false;
            _state = TurnState.ENEMYTURN;
            //Debug.Log("OnBeginEnemyTurn");
            // Call the OnBeginEnemyTurn Event
            OnBeginEnemyTurn();
            // Begin the first enemies turn
            _enTurnContRef.BeginFirstEnemyTurn();
        }
    }

    /// <summary>
    /// Ends the player's turn and starts the enemy's turn
    /// </summary>
    public void EndPlayerTurn()
    {
        // Call the finish player turn event
        if (OnFinishPlayerTurn != null)
            OnFinishPlayerTurn();

        StartEnemyTurn();
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

    /// <summary>
    /// Checks if there are any enemies if there are not, restarts the player turn
    /// </summary>
    private void LetPlayerGoAgain()
    {
        if (_enTurnContRef.GetAmountEnemies() == 0)
            StartPlayerTurn();
    }
}
