using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal : Skill
{
    private new void Awake()
    {
        base.Awake();
        skillNum = 1334;//spells heel in numbers upside down
        healing = true;
        cooldown = 6;
    }

    public override void StartSkill(Vector2Int attackNodePos)
    {
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
                    Debug.Log("Ally to heal does not have a Health script attached to it");

                // Start the skill's animation
                StartSkillAnimation(attackNodePos);
            }
            else
                Debug.Log("Enemy to attack does not have a MoveAttack script attached to it");
        }
        
    }

    public override void EndSkill()
    {
        if (enemiesHP != null && enemiesHP[0] != null)
        {
            // End the skills animation
            EndSkillAnimation();
            // Deal the damage and get rid of our reference to the enemyHP
            enemiesHP[0].Heal(damage);
            
            enemiesHP[0] = null;
        }
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
                //mAGUIContRef.AllowSelect();
                turnSysRef.IsPlayerDone();
            }
        }
    }

}
