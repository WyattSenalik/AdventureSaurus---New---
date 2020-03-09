using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class DeathCheck : MonoBehaviour
{
    public Transform player1;
    public Transform player2;
    public Transform player3;

    // Update is called once per frame
    void Update()
    {
        if(player1==null && player2==null && player3==null)
        {
            SceneManager.LoadScene("GameOver");
        }
    }
}
