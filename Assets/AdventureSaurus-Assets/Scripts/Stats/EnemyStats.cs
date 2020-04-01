using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : Stats
{
    // For determining how much xp the killer of this character should get
    [SerializeField] private int _baseXPToGive = 0;

    /// <summary>
    /// Calculates how much xp should be gained by the killer for killing this character
    /// </summary>
    /// <param name="killer">Stats component of the character who killed this unit</param>
    /// <returns>int amount of xp the killer should gain</returns>
    public int KillReward(Stats killer)
    {
        // If this character is not supposed to give any xp
        if (_baseXPToGive == 0)
        {
            return 0;
        }
        // Do the calculation
        return (((_vitality + _strength + _magic) / 3) + 1) * _baseXPToGive;
    }
}
