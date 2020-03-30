﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal : Skill
{
    // Called before start
    // Set the skill specific variables
    private new void Awake()
    {
        base.Awake();
        // Spells heel in numbers upside down
        _skillNum = 1334;
        _healing = true;
        _cooldown = 6;
    }

    /// <summary>
    /// Starts the skills animation and gets a reference to the ally that will be healed
    /// </summary>
    /// <param name="healNodesPos">The position of the node that will be healed</param>
    public override void StartSkill(Vector2Int healNodePos)
    {
        // Get the node at the position
        Node nodeToHeal = _mAContRef.GetNodeAtPosition(healNodePos);
        if (nodeToHeal != null)
        {
            // Get the character at that node
            MoveAttack charToAttack = _mAContRef.GetCharacterMAByNode(nodeToHeal);
            if (charToAttack != null)
            {
                // Actually set the reference to the enemy HP
                _enemiesHP = new List<Health>();
                _enemiesHP.Add(charToAttack.GetComponent<Health>());
                if (_enemiesHP[0] == null)
                    Debug.Log("Ally to heal does not have a Health script attached to it");

                // Start the skill's animation
                StartSkillAnimation(healNodePos);
            }
            else
                Debug.Log("Enemy to attack does not have a MoveAttack script attached to it");
        }
        
    }

    /// <summary>
    /// Ends the skills animaton and heals the character to heal
    /// </summary>
    public override void EndSkill()
    {
        // If we have a character to heal
        if (_enemiesHP != null && _enemiesHP[0] != null)
        {
            // End the skills animation
            EndSkillAnimation();
            // Heal the character and get rid of our reference to the enemyHP
            _enemiesHP[0].Heal(_damage);
        }
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
