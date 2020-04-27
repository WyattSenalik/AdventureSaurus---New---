using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : Stats
{
    // For determining how much xp the killer of this character should get
    [SerializeField] private int _baseXPToGive = 0;
    public int GetBaseXpToGive() { return _baseXPToGive; }
    public void SetBaseXpToGive(int newBaseXPToGive) { _baseXPToGive = newBaseXPToGive; }

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

    public int SharedKillReward(Stats helpers)
    {
        if (_baseXPToGive == 0)
        {
            return 0;
        }
        // Do the calculation
        return ((((_vitality + _strength + _magic) / 3) + 1)/2) * _baseXPToGive;
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
        // TODO, if we ever make enemies with skills besides basic attack, after
        // updating magic, we will need to have an enemy grimoire of sorts be updated
        _magic += mgc;
        _speed += spd;
        _vitality += vit;
        _hpRef.MaxHP = _vitality;
        _hpRef.CurHP = _hpRef.MaxHP;
        _hpRef.Heal(0);
    }

    /// <summary>
    /// Adds an amount of experience they the player should get
    /// </summary>
    /// <param name="scaleAm">Amount to add the experience by</param>
    public void AddExpToGive(int addAm)
    {
        _baseXPToGive += addAm;
    }
}
