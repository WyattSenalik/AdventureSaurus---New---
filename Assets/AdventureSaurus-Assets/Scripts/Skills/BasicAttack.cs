using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttack : Skill
{
    private void Awake()
    {
        skillNum = 0;
        rangeTiles = new List<Vector2Int>();
        rangeTiles.Add(Vector2Int.down);
        rangeTiles.Add(Vector2Int.up);
        rangeTiles.Add(Vector2Int.right);
        rangeTiles.Add(Vector2Int.left);
        
    }
    public new void DoSkill()
    {
        base.DoSkill();
        
    }

    public new void StartSkill(List<Vector2Int> attackNodesPos)
    {
        // We have to set the enemy to attack, we just need to validate a bit first
        Node nodeToAttack = mAContRef.GetNodeAtPosition(attackNodesPos[0]);
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

    public new void EndSkill()
    {
        // Validate that we have an enemy to attack
        if (enemiesHP[0] != null)
        {
            // Start attacking animation
            anime.SetInteger("AttackDirection", -anime.GetInteger("AttackDirection"));
            // Deal the damage and get rid of our reference to the enemyHP
            enemiesHP[0].TakeDamage(damage);
            enemiesHP[0] = null;
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
                enMAAIRef.NextEnemy();
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
