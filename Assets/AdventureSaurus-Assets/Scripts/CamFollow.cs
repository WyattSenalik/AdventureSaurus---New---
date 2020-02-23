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
    public Transform enemy;


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
    public EnemyMoveAttackAI enemies;

    void Awake()
    {
        //defaults camera to player position
        player1Cam();
    }


    void FixedUpdate()
    {
        //Turn cams
        //
        //enemy turn cam
        if (turn.enemyTurn == true)
        {
            enemyCam();
            turn.enemyTurn = false;
        }
        //player turn cam
        if (turn.playerTurn == true)
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
        //

        // if character is selected camera moves to them
        if (control.areSelected1 == true)
        {
            player1Cam();
            control.areSelected1 = false;
        }
        else if (control.areSelected2 == true)
        {
            player2Cam();
            control.areSelected2 = false;
        }
        else if (control.areSelected3 == true)
        {
            player3Cam();
            control.areSelected3 = false;
        }
    



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
            Vector3 targetPosition = enemy.TransformPoint(new Vector3(0, 0, -10));
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
        //
    }


    //sets bool values to where they need to focus
    //
    //Cam follows player 1
    public void player1Cam()
    {
        isOnPlayer1 = true;
        isOnPlayer2 = false;
        isOnPlayer3 = false;
        isOnEnemy = false;
    }
    //Cam follows player 2
    public void player2Cam()
    {
        isOnPlayer1 = false;
        isOnPlayer2 = true;
        isOnPlayer3 = false;
        isOnEnemy = false;
    }
    //Cam follows player 3
    public void player3Cam()
    {
        isOnPlayer1 = false;
        isOnPlayer2 = false;
        isOnPlayer3 = true;
        isOnEnemy = false;
    }
    //Cam follows enemies
    public void enemyCam()
    {
        isOnPlayer1 = false;
        isOnPlayer2 = false;
        isOnPlayer3 = false;
        isOnEnemy = true;
    }
    //


}