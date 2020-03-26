using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    //Allows access to player and enemy values//
    //Character 1
    private Transform player1;
    //Character 2
    private Transform player2;
    //Character 3
    private Transform player3;
    //Enemy to follow
    private Transform enemy1;
    //List of allies
    
    //[SerializeField] private Transform charParent = null;
    public List<Transform> Allies;
    //private PauseMenuController AllyList;
    
    //

    //Camera variables//
    public float smoothTime = 0.3f;
    public float cameraDistance = 30.0f;
    //Camera Velocity
    private Vector3 velocity = Vector3.zero;
    //

    //bools checking for who cam is on
    private bool isOnPlayer1;
    private bool isOnPlayer2;
    private bool isOnPlayer3;
    private bool isOnEnemy;
    //

    //allows access to TurnSystem
    private TurnSystem turn;
    private MoveAttackGUIController control;
    private EnemyMoveAttackAI whoIsEnemy;

    /// Wyatt added
    // Stuff for getting the camera to follow each enemy
    private bool amFollowing;   // If the camera is following an enemy or not
    public bool AmFollowing
    {
        get { return amFollowing; }
    }
    /// End Wyatt added

    void Awake()
    {
        // Get references
        GameObject gameContObj = GameObject.FindWithTag("GameController");
        if (gameContObj == null)
            Debug.Log("Could not find any object with the tag GameController");
        turn = gameContObj.GetComponent<TurnSystem>();
        if (turn == null)
            Debug.Log("There was no PlaceAllies attached to " + gameContObj.name);
        control = gameContObj.GetComponent<MoveAttackGUIController>();
        if (control == null)
            Debug.Log("There was no MoveAttackGUIController attached to " + gameContObj.name);
        whoIsEnemy = gameContObj.GetComponent<EnemyMoveAttackAI>();
        if (whoIsEnemy == null)
            Debug.Log("There was no EnemyMoveAttackAI attached to " + gameContObj.name);

        //defaults camera to player position
        player1Cam();
    }

    /// Wyatt added
    // Called before the first frame update
    private void Start()
    {
        //
        amFollowing = false;
        //
        /*
        Allies = new List<Transform>();
        foreach (Transform charTrans in charParent)
        {
            MoveAttack charMA = charTrans.GetComponent<MoveAttack>();
            if (charMA != null && charMA.WhatAmI == CharacterType.Ally)
            {
                Stats allyStatsRef = charMA.GetComponent<Stats>();
                alliesStats.Add(charMA.GetComponent<Stats>());
            }
        }

            Debug.Log(Allies[0]);
            */
    }
    /// End Wyatt added


    /// <summary>
    /// Called from Procedural Generation after everything is created.
    /// Gets the allies from the character parent
    /// </summary>
    /// <param name="charParent">Parent of all characters</param>
    public void Initialize(Transform charParent)
    {
        // Initializes the list of allies and finds them
        Allies = new List<Transform>();
        foreach (Transform charTrans in charParent)
        {
            MoveAttack charMA = charTrans.GetComponent<MoveAttack>();
            if (charMA != null && charMA.WhatAmI == CharacterType.Ally)
            {
                Allies.Add(charTrans);
            }
        }
        if (Allies.Count >= 1)
            player1 = Allies[0];
        if (Allies.Count >= 2)
            player2 = Allies[1];
        if (Allies.Count >= 3)
            player3 = Allies[2];

        //defaults camera to player position
        player1Cam();
    }

    void FixedUpdate()
    {
        //Turn cams
        camTurn();

        //Selecting character cam
        camSelection();
       
        //Switch between enemy cams
        /*
        enemySwitch();
        */

        //checks bool to see who camera needs to be on
        //
        if (player1!=null && isOnPlayer1 == true)
        {
            Vector3 targetPosition = player1.TransformPoint(new Vector3(0, 0, -10));
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            
        }
        if (player2 != null && isOnPlayer2 == true)
        {
            Vector3 targetPosition = player2.TransformPoint(new Vector3(0, 0, -10));
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            
        }
        if (player3 != null && isOnPlayer3 == true)
        {
            Vector3 targetPosition = player3.TransformPoint(new Vector3(0, 0, -10));
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
           
        }

     
        if (isOnEnemy == true)
        {
            Vector3 targetPosition = enemy1.TransformPoint(new Vector3(0, 0, -10));
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

            /// Wyatt added
            if (Mathf.Abs(transform.position.x - enemy1.position.x) + Mathf.Abs(transform.position.y - enemy1.position.y) <= 1)
            {
                amFollowing = true;
            }
            else
            {
                amFollowing = false;
            }
            /// End Wyatt added
        }
   
        //
    }


    //sets bool values to where they need to focus
    //
    //Cam follows player 1
    public void player1Cam()
    {
        amFollowing = false;
        isOnPlayer1 = true;
        isOnPlayer2 = false;
        isOnPlayer3 = false;
        isOnEnemy = false;
    }
    //Cam follows player 2
    public void player2Cam()
    {
        amFollowing = false;
        isOnPlayer1 = false;
        isOnPlayer2 = true;
        isOnPlayer3 = false;
        isOnEnemy = false;
    }
    //Cam follows player 3
    public void player3Cam()
    {
        amFollowing = false;
        isOnPlayer1 = false;
        isOnPlayer2 = false;
        isOnPlayer3 = true;
        isOnEnemy = false;
    }
    //Cam follows enemies
    public void enemyCam()
    {
        amFollowing = false;
        isOnPlayer1 = false;
        isOnPlayer2 = false;
        isOnPlayer3 = false;
        isOnEnemy = true;
    }
    //
    private void camSelection()
    {
        if (player1!=null && control.areSelected1 == true)
        {
            player1Cam();
            control.areSelected1 = false;
        }
        else if (player2 != null && control.areSelected2 == true)
        {
            player2Cam();
            control.areSelected2 = false;
        }
        else if (player3 != null && control.areSelected3 == true)
        {
            player3Cam();
            control.areSelected3 = false;
        }
    
    }

    private void camTurn()
    {
        if (turn.enemyTurn == true)
        {
  
            //enemyCam();
            /*
            enemySwitch();
             * */
            turn.enemyTurn = false;
        }
        //player turn cam
        if (turn.playerTurn == true && turn.enemyTurn==false)
        {
            if (player1)
            {
                player1Cam();
                turn.playerTurn = false;
            }
            else if (player2)
            {
                player2Cam();
                turn.playerTurn = false;
            }
            else if (player3)
            {
                player3Cam();
                turn.playerTurn = false;
            }
        }
    }

    /// Wyatt added
    /// <summary>
    /// Sets the camera to go to the enemy specified by enemyTrans
    /// </summary>
    /// <param name="enemyTrans">The transform of the enemy to follow</param>
    public void FollowEnemy(Transform enemyTrans)
    {
        enemy1 = enemyTrans;
        enemyCam();
    }
    /// End Wyatt added

   
}