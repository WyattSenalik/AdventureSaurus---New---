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
    private bool nextArea;
    private bool touchedOnce;

    public Prompter prompter;



    private void Awake()
    {
        nextArea = false;
        touchedOnce = false;
    }

    private void Update()
    {
        touchStairs();
    }

   
    private void touchStairs()
    {
        if (touchedOnce == false)
        {
            if (player1 != null && player1.transform.position == stair.transform.position)
            {
                prompt();
                touchedOnce = true;
            }
            else if (player2 != null && player2.transform.position == stair.transform.position)
            {
                prompt();
                touchedOnce = true;
            }
            else if (player3 != null && player3.transform.position == stair.transform.position)
            {
                prompt();
                touchedOnce = true;
            }
        }
    }
    
    private void prompt()
    {
        
        prompter.PromptGame();
        
    }

    public void yes()
    {
        nextArea = true;
        prompter.PromptGame();
        SceneManager.LoadScene("Title Screen");
    }
    public void no()
    {
        nextArea = false;
        prompter.PromptGame();
        
    }
    
}
