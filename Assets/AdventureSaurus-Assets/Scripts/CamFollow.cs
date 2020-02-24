using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    //Allows access to player and enemy values//
    //Character 1
    public Transform player1;
    //Character 2
    public Transform player2;
    //Character 3
    public Transform player3;
    //Enemies
    public Transform enemy1;
    public Transform enemy2;
    public Transform enemy3;
    public Transform enemy4;
    public Transform enemy5;
    public Transform enemy6;
    public Transform enemy7;
    public Transform enemy8;
   

    //Camera variables//
    public float smoothTime = 0.3f;
    public float cameraDistance = 30.0f;
    //Camera Velocity
    private Vector3 velocity = Vector3.zero;
    //

    //bools checking for who cam is on
    public bool isOnPlayer1;
    public bool isOnPlayer2;
    public bool isOnPlayer3;
    public bool isOnEnemy;
    //

    //allows access to TurnSystem
    public TurnSystem turn;
    public MoveAttackGUIController control;
    public EnemyMoveAttackAI whoIsEnemy;

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
        //defaults camera to player position
        player1Cam();
    }

    /// Wyatt added
    // Called before the first frame update
    private void Start()
    {
        amFollowing = false;
    }
    /// End Wyatt added

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
        if (isOnPlayer1 == true)
        {
            Vector3 targetPosition = player1.TransformPoint(new Vector3(0, 0, -10));
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
        if (isOnPlayer2 == true)
        {
            Vector3 targetPosition = player2.TransformPoint(new Vector3(0, 0, -10));
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
        if (isOnPlayer3 == true)
        {
            Vector3 targetPosition = player3.TransformPoint(new Vector3(0, 0, -10));
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }

     
        if (isOnEnemy == true)
        {
            Vector3 targetPosition = enemy1.TransformPoint(new Vector3(0, 0, -10));
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

            /// Wyatt added
            if (Mathf.Abs(transform.position.x - targetPosition.x) + Mathf.Abs(transform.position.y - targetPosition.y) <= 1)
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
        if (control.areSelected1 == true && player1)
        {
            player1Cam();
            control.areSelected1 = false;
        }
        else if (control.areSelected2 == true && player2)
        {
            player2Cam();
            control.areSelected2 = false;
        }
        else if (control.areSelected3 == true && player3)
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

    /*
    public void enemySwitch()
    {

            if ( whoIsEnemy.enemyName==enemy1.name && enemy1 )
            {
                Vector3 targetPosition = enemy1.TransformPoint(new Vector3(0, 0, -10));
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            }

            
            else if (whoIsEnemy.enemyName == enemy2.name && enemy2)
            {
                Vector3 targetPosition = enemy2.TransformPoint(new Vector3(0, 0, -10));
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            }
            else if (whoIsEnemy.enemyName == enemy3.name && enemy3)
            {
                Vector3 targetPosition = enemy3.TransformPoint(new Vector3(0, 0, -10));
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            }
            else if (whoIsEnemy.enemyName == enemy4.name && enemy4)
            {
                Vector3 targetPosition = enemy4.TransformPoint(new Vector3(0, 0, -10));
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            }
            else if (whoIsEnemy.enemyName == enemy5.name && enemy5)
            {
                Vector3 targetPosition = enemy5.TransformPoint(new Vector3(0, 0, -10));
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            }
            else if (whoIsEnemy.enemyName == enemy6.name && enemy6)
            {
                Vector3 targetPosition = enemy6.TransformPoint(new Vector3(0, 0, -10));
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            }
            else if (whoIsEnemy.enemyName == enemy7.name && enemy7)
            {
                Vector3 targetPosition = enemy7.TransformPoint(new Vector3(0, 0, -10));
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            }
            else if (whoIsEnemy.enemyName == enemy8.name && enemy8)
            {
                Vector3 targetPosition = enemy8.TransformPoint(new Vector3(0, 0, -10));
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            }
        
        
    }
    */
}