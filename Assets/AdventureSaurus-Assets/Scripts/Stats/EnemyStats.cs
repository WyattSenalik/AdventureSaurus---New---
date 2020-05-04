using UnityEngine;

public class EnemyStats : Stats
{
    // For determining how much xp the killer of this character should get
    [SerializeField] private int _baseXPToGive = 0;
    public int GetBaseXpToGive() { return _baseXPToGive; }
    public void SetBaseXpToGive(int newBaseXPToGive) { _baseXPToGive = newBaseXPToGive; }

    // The enemies difficulty
    [SerializeField] private int _difficulty = 1;
    // Getter
    public int GetDifficulty() { return _difficulty; }
    // Setter
    public void SetDifficulty(int newDifficulty) { _difficulty = newDifficulty; }

    /// <summary>
    /// Calculates how much xp should be gained by the killer for killing this character
    /// </summary>
    /// <returns>int amount of xp the killer should gain</returns>
    public int KillReward()
    {
        // Do the calculation
        return (_difficulty + _baseXPToGive) * 2;
    }

    public int SharedKillReward(Stats helpers)
    {
        // Do the calculation
        return KillReward() / 2;
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
        if (_speed + spd > MaxSpeed)
            _speed = MaxSpeed;
        else
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
