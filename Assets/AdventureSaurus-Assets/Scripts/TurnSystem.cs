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
    // The parent of all the characters on the floor
    private Transform _characterTeam;

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
    }

    // Called when gameobject is destroyed
    // Unsubscribe to ALL events
    private void OnDestroy()
    {
        EnemyTurnController.OnEndEnemyTurn -= StartPlayerTurn;
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
        Pause.OnPauseGame -= HideScript;
        Pause.OnUnpauseGame -= ShowScript;
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
        _characterTeam = GameObject.Find(ProceduralGenerationController.charParentName).transform;
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
        foreach(Transform potAlly in _characterTeam)
        {
            MoveAttack potAllyMA = potAlly.GetComponent<MoveAttack>();
            if (potAllyMA == null)
            {
                Debug.Log("There is a non character object in " + _characterTeam.name);
            }
            else
            {
                potAllyMA.ResetMyTurn();
            }
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
    ///// <summary>
    ///// Called from 
    ///// </summary>
    //private void IsPlayerDone()//checks if player has done everything that they can do with thier characters
    //{
    //    bool areDone = true;
    //    foreach (Transform player in _characterTeam)
    //    {
    //        MoveAttack ma = player.GetComponent<MoveAttack>();
    //        if(ma == null)
    //        {
    //            Debug.Log("There was no move attack attached to " + player.name);
    //            continue;
    //        }

    //        if(ma.WhatAmI == CharacterType.Ally)//checks if its a player character
    //        {
    //            if( !(ma.HasMoved && ma.HasAttacked))
    //            {
    //                areDone = false;
    //                break;
    //            }
    //        }
            
    //    }
    //    if(areDone)
    //    {
    //        EndPlayerTurn();
    //    }
    //}
    ////checks all enemies to see if they are done with their turn
    //private void IsEnemyDone()//gets called each time an enemy is done taking actions
    //{
    //    bool areDone = true;//Starts true to assume the player is done but if any have actions left sets it to false
    //    foreach (Transform player in _characterTeam)
    //    {
    //        MoveAttack ma = player.GetComponent<MoveAttack>();
    //        if (ma == null)
    //        {
    //            Debug.Log("There was no move attack attached to " + player.name);
    //            continue;
    //        }

    //        if (ma.WhatAmI == CharacterType.Enemy)
    //        {
    //            if (!(ma.HasMoved && ma.HasAttacked))
    //            {
    //                areDone = false;
    //                break;
    //            }
    //        }

    //    }
    //    if (areDone == true)
    //    {

    //        StartPlayerTurn();

    //    }

    //}

    ///// <summary>
    ///// Sets state to GameStop which will be used for pausing or cutscenes
    ///// </summary>
    //private void SetToGameStop()
    //{
    //    _state = TurnState.GAMESTOP;
    //}

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

}
