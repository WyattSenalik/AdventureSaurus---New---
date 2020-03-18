using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDifficulty : MonoBehaviour
{
    // The enemies difficulty
    [SerializeField] private int difficulty = 1;
    public int Difficulty
    {
        get { return difficulty; }
    }
}
