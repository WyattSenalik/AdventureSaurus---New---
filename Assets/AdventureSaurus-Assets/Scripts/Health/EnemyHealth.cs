using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : Health
{
    // Reference to the Stats component of who killed this character
    // For giving xp
    private AllyStats _myKiller;
    private List<AllyStats> _helpers;
    /// <summary>
    /// Decrements curHP by dmgToTake.
    /// Also sets _myKiller to the dmg dealer
    /// </summary>
    /// <param name="dmgToTake">Amount curHP will go down by</param>
    /// <param name="dmgDealer">The Stats of the character who dealt damage</param>
    public override void TakeDamage(int dmgToTake, Stats dmgDealer)
    {
        // Set my killer, in the case this dies
        _myKiller = dmgDealer as AllyStats;


        //Sets the helpers for shared xp
        Transform allyParent = GameObject.Find(ProceduralGenerationController.ALLY_PARENT_NAME).transform;

        // Initialize ally stats
        _helpers = new List<AllyStats>();
        // Iterate over the allies to get their stats
        foreach (Transform allyTrans in allyParent)
        {
            int _numHelpers = 0;
            // Try to ge the ally's stats
            AllyStats allyStat = allyTrans.GetComponent<AllyStats>();
            // If the stats aren't null, add it to the list
            if (allyStat != null && _myKiller.name != allyTrans.name)
                _helpers.Add(allyStat);

        }

        // Call the super's function
        base.TakeDamage(dmgToTake, dmgDealer);
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

        //Give reduced xp to other allies
        foreach (AllyStats _helper in _helpers)
        {
            _helper.GainReducedExperience(myStats.SharedKillReward(_helper));
            Debug.Log(_helper);
        }
        // Call the base's version
        // (make sure to do this last, since the object is destroyed by this call)
        base.Ascend();
    }
}
