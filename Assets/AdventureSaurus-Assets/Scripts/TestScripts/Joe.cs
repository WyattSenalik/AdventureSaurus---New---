using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joe : MonoBehaviour
{
    public int joe;
    private Mum number;
    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log(joe);
    }

    private void Start()
    {
        GameObject mama = GameObject.FindGameObjectWithTag("Mama");
        if (mama == null)
            Debug.Log("Could not find any GameObject with the tag Mama");
        else
        {
            number = mama.GetComponent<Mum>();
        }
        joe = number.unga;
        Debug.Log (joe);
    }
}
