using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttack : Skill
{
    /// <summary>
    /// Called before Start. Sets references.
    /// </summary>
    private new void Awake()
    {
        // Get references and such
        base.Awake();
        // Set the unique stats for this attack
        skillNum = 0;
    }

    /// <summary>
    /// Starts the skills animation and gets a reference to the enemy that will be hit.
    /// </summary>
    /// <param name="attackNodesPos">The position of the node that will be attacked</param>
    override public void StartSkill(Vector2Int attackNodePos)
    {
        // Get the damage
        damage = statsRef.Strength;

        // We have to set the enemy to attack, we just need to validate a bit first
        Node nodeToAttack = mAContRef.GetNodeAtPosition(attackNodePos);
        if (nodeToAttack != null)
        {
            MoveAttack charToAttack = mAContRef.GetCharacterMAByNode(nodeToAttack);
            if (charToAttack != null)
            {
                // Actually set the reference to the enemy HP
                enemiesHP = new List<Health>();
                enemiesHP.Add(charToAttack.GetComponent<Health>());
                if (enemiesHP[0] == null)
                    Debug.Log("Enemy to attack does not have a Health script attached to it");

                // Start the skill's animation
                StartSkillAnimation(attackNodePos);
            }
            else
                Debug.Log("Enemy to attack does not have a MoveAttack script attached to it");
        }
        // This occurs when an enemy isn't close enough to an ally to attack. Call end attack and don't start playing an animation
        else
        {
            //Debug.Log("Node to attack does not exist");
            EndSkill();
            return;
        }
    }

    /// <summary>
    /// Ends the skills animaton and does damage to the 1 enemy that was hit
    /// </summary>
    override public void EndSkill()
    {
        // Validate that we have an enemy to attack
        if (enemiesHP != null && enemiesHP[0] != null)
        {
            // End the skills animation
            EndSkillAnimation();
            // Deal the damage and get rid of our reference to the enemyHP
            enemiesHP[0].TakeDamage(damage, this.GetComponent<Stats>());


            // Reduce the special skill's cooldown by 1
            // If this is an ally
            if (maRef.WhatAmI == CharacterType.Ally)
            {
                // Get the AllySkillController
                AllySkillController allySkillContRef = maRef.GetComponent<AllySkillController>();
                if (allySkillContRef == null)
                {
                    Debug.Log("There is no AllySkillController attached to " + this.name);
                }
                // Increment the cooldown of special skill
                allySkillContRef.SpecialSkill.IncrementCooldown();
            }
        }
        // If we have no enemy to attack, give back control to the proper authority
        else
        {
            // We should not attack anything, so set attack animation to 0
            anime.SetInteger("AttackDirection", 0);

            //Debug.Log("There was no enemy to attack");
            // If this character is an enemy, have the next enemy attack
            if (maRef.WhatAmI == CharacterType.Enemy)
            {
                enMAAIRef.StartNextEnemy();
            }
            // If this character is an ally, give back control to the user
            else if (maRef.WhatAmI == CharacterType.Ally)
            {
                mAGUIContRef.AllowSelect();
                turnSysRef.IsPlayerDone();
            }
        }
    }
}
