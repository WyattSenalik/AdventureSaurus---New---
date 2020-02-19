using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    //Character 1
    public Transform player1;
    //Character 2
    public Transform player2;
    //Character 3
    public Transform player3;
    //Enemies
    public Transform enemy;

    public float cameraDistance = 30.0f;
    //bools checking for who cam is on
    public bool isOnPlayer1;
    public bool isOnPlayer2;
    public bool isOnPlayer3;
    public bool isOnEnemy;

    //
    public TurnSystem turn;


    void Awake()
    {
        //defaults camera to player position
        transform.position = new Vector3(player1.position.x, player1.position.y, -1); 
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
            player1Cam();
            turn.playerTurn = false;
        }

        //defaults to follow player1 if no actions to change camera 
        transform.position = new Vector3(player1.position.x, player1.position.y, -1); 


        //checks bool to see who camera needs to be on
        //
        if (isOnPlayer1 == true)
        {
            transform.position = new Vector3(player1.position.x, player1.position.y, -1);
        }
        if (isOnPlayer2 == true)
        {
            transform.position = new Vector3(player2.position.x, player2.position.y, -1);
        }
        if (isOnPlayer3 == true)
        {
            transform.position = new Vector3(player3.position.x, player3.position.y, -1);
        }
        if (isOnEnemy == true)
        {
            transform.position = new Vector3(enemy.position.x, enemy.position.y, -1);
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
  

