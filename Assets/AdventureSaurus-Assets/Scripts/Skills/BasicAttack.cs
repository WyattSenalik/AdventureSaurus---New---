using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttack : Skill
{
    // Called before Start. Sets references.
    private new void Awake()
    {
        // Get references and such
        base.Awake();
        // Set the unique stats for this attack
        _skillNum = 0;

        // Set the attack range of the character, since if basic attack is on a character,
        // it will be the first one equipped
        _maRef.AttackRange = _range;
    }

    /// <summary>
    /// Starts the skills animation and gets a reference to the enemy that will be hit.
    /// </summary>
    /// <param name="attackNodesPos">The position of the node that will be attacked</param>
    override public void StartSkill(Vector2Int attackNodePos)
    {
        // Initialize the list of targets
        _enemiesHP = new List<Health>();

        // Get the damage
        _damage = _statsRef.GetStrength();

        // We have to set the enemy to attack, we just need to validate a bit first
        Node nodeToAttack = _mAContRef.GetNodeAtPosition(attackNodePos);
        if (nodeToAttack != null)
        {
            MoveAttack charToAttack = _mAContRef.GetCharacterMAByNode(nodeToAttack);
            if (charToAttack != null)
            {
                // Add the one enemy we will be hitting
                _enemiesHP.Add(charToAttack.GetComponent<Health>());
                if (_enemiesHP[0] == null)
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
        if (_enemiesHP != null && _enemiesHP.Count > 0 && _enemiesHP[0] != null)
        {
            //Debug.Log("Ending attack on " + enemiesHP[0].name);
            // End the skills animation
            EndSkillAnimation();
            // Deal the damage and get rid of our reference to the enemyHP
            _enemiesHP[0].TakeDamage(_damage, this.GetComponent<Stats>());


            // Reduce the special skill's cooldown by 1
            // If this is an ally
            if (_maRef.WhatAmI == CharacterType.Ally)
            {
                // Get the AllySkillController
                AllySkillController allySkillContRef = _maRef.GetComponent<AllySkillController>();
                if (allySkillContRef == null)
                {
                    Debug.Log("There is no AllySkillController attached to " + this.name);
                }
                // Increment the cooldown of special skill
                if (allySkillContRef.SpecialSkill != null)
                    allySkillContRef.SpecialSkill.IncrementCooldownTimer();
            }
        }
        // If we have no enemy to attack, give back control to the proper authority
        else
        {
            // We should not attack anything, so set attack animation to 0
            _anime.SetInteger("AttackDirection", 0);
        }

        // Get rid of the references of the enemies to hit, so that we do not hit them again on accident
        // after the next time this enemy moves
        _enemiesHP = new List<Health>();
    }
}
