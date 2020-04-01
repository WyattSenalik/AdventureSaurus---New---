using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : Health
{
    // Reference to the Stats component of who killed this character
    // For giving xp
    private AllyStats _myKiller;

    /// <summary>
    /// Decrements curHP by dmgToTake.
    /// Also sets _myKiller to the dmg dealer
    /// </summary>
    /// <param name="dmgToTake">Amount curHP will go down by</param>
    /// <param name="dmgDealer">The Stats of the character who dealt damage</param>
    public override void TakeDamage(int dmgToTake, Stats dmgDealer)
    {
        // Call the super's function
        base.TakeDamage(dmgToTake, dmgDealer);

        // Set my killer, in the case this dies
        _myKiller = dmgDealer as AllyStats;
    }

    /// <summary>
    /// Actually destroys the object. Should only be called by an animation event.
    /// Also gives xp to the ally who killed this enemy
    /// </summary>
    protected override void Ascend()
    {
        // Give xp to the killer
        EnemyStats myStats = this.GetComponent<EnemyStats>();
        _myKiller.GainExperience(myStats.KillReward(_myKiller));

        // Call the base's version
        // (make sure to do this last, since the object is destroyed by this call)
        base.Ascend();
    }
}
