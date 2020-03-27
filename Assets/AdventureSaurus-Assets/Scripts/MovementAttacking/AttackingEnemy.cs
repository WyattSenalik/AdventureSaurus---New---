using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingEnemy : SingleEnemy
{
    /// <summary>
    /// Finds and returns the tile this enemy should move to.
    /// Find the closest tile they can attack an enemy from.
    /// </summary>
    /// <returns>Node that this enemy should move to</returns>
    override protected Node FindTileToMoveTo()
    {
        return StandingNode;
    }

    /// <summary>
    /// Attempts to do whatever action this enemy does.
    /// Called after the character finishes moving.
    /// Uses the character's skill on an enemy in range.
    /// </summary>
    override protected void AttemptAction()
    {

    }
}