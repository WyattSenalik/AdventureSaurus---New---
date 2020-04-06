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

    /// <summary>
    /// Increases an enemies stats by the given amounts
    /// </summary>
    /// <param name="str">Amount to increase strength by</param>
    /// <param name="mgc">Amount to increase magic by</param>
    /// <param name="spd">Amount to increase speed by</param>
    /// <param name="vit">Amount to increase vitality by</param>
    public void BuffEnemy(int str, int mgc, int spd, int vit)
    {
        _strength += str;
        _magic += mgc;
        _speed += spd;
        _vitality += vit;
    }

    /// <summary>
    /// Scales the amount of exp an enemy is supposed to give by a scalar
    /// </summary>
    /// <param name="scaleAm">Amount to scale the base xp by</param>
    public void ScaleExpToGive(int scaleAm)
    {
        _baseXPToGive *= scaleAm;
    }
}
