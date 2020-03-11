using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mum : MonoBehaviour
{
    public int unga;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        unga = 5;
    }
}
