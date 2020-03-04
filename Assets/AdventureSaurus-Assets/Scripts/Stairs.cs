using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Stairs : MonoBehaviour
{
    
    // Start is called before the first frame update
    public Transform player1;
    public Transform player2;
    public Transform player3;
    public Transform stair;
    private bool touchedOnce;
    private string whoIsOn;
    public Prompter prompter;



    private void Awake()
    {
        touchedOnce = false;
    }

    private void Update()
    {
        touchStairs();
        if (player1 != null && whoIsOn == player1.name)
        {
            if (player1.transform.position != stair.transform.position )
            {
                touchedOnce = false;
            }
        }
        else if(player2 != null && whoIsOn == player2.name)
        {
            if (player2.transform.position != stair.transform.position)
            {
                touchedOnce = false;
            }
        }
        else if(player3 != null && whoIsOn == player3.name)
        {
            if (player3.transform.position != stair.transform.position)
            {
                touchedOnce = false;
            }
        }
    }

   
    private void touchStairs()
    {
        if (touchedOnce == false)
        {
            if (player1 != null && player1.transform.position == stair.transform.position)
            {
                prompt();
                touchedOnce = true;
                whoIsOn = player1.name;
            }
            else if (player2 != null && player2.transform.position == stair.transform.position)
            {
                prompt();
                touchedOnce = true;
                whoIsOn = player2.name;
            }
            else if (player3 != null && player3.transform.position == stair.transform.position)
            {
                prompt();
                touchedOnce = true;
                whoIsOn = player3.name;
            }
        }
    }
    
    private void prompt()
    {
        
        prompter.PromptGame();
        
    }

    public void yes()
    {

        SceneManager.LoadScene("Title Screen");
        prompter.PromptGame();
    }
    public void no()
    {
        prompter.PromptGame();
    }
    
}
