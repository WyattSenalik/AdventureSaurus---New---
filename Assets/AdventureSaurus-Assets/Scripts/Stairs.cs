using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stairs : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform player1;
    public Transform player2;
    public Transform player3;
    public Transform stair;

    

    private void FixedUpdate()
    {
        winCondition();
    }

    private void win()
    {
        Debug.Log("Winner winner chicken dinner");
    }

    private void winCondition()
    {
        if(player1!=null && player1.transform.position==stair.transform.position)
            {
            win();
            }
        else if(player2 !=null && player2.transform.position == stair.transform.position)
            {
            win();
            }
        else if(player3 !=null && player3.transform.position == stair.transform.position)
            {
            win();
            }
    }
}
