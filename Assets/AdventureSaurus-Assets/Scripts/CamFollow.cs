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
    public p1 select1;
    public p2 select2;
    public p3 select3;


    void Awake()
    {
        //defaults camera to player position
        player1Cam();
    }


    void FixedUpdate()
    {

        if (turn.enemyTurn == true)
        {
            enemyCam();
            turn.enemyTurn = false;
        }

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
        /*
        if (select1.p1Select == true)
        {
            player1Cam();
            select1.p1Select = false;
        }
        if (select2.p2Select == true)
        {
            player2Cam();
            select2.p2Select = false;
        }
        if (select3.p3Select == true)
        {
            player3Cam();
            select3.p3Select = false;
        }
        */
        //defaults to follow player1 if no actions to change camera 



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