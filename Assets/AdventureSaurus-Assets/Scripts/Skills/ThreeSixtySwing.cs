using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeSixtySwing : Skill
{
    private new void Awake()
    {
        base.Awake();
        skillNum = 360;
        diagnols = true;
        cooldown = 4;
    }
    /// <summary>
    /// Starts the animation for 360. Also gets refences to the enemies that got hit by skill.!!MAy need arltering since skill activation might be different.
    /// </summary>
    /// <param name="attackNodePos">The point the player used to start the skill. Dictates direction of animations.</param>
    override public void StartSkill(Vector2Int attackNodePos)
    {
        Node nodeToAttack = mAContRef.GetNodeAtPosition(attackNodePos);
        

        if (nodeToAttack != null)
        {
            Vector2Int center = new Vector2Int(Mathf.RoundToInt(this.transform.position.x), Mathf.RoundToInt(this.transform.position.y));//gets a initial reference point at the position of charcter
            

            List<Node> areaOfAttack = new List<Node>();//list of the 8 tiles around character

            areaOfAttack.Add(mAContRef.GetNodeAtPosition(center + Vector2Int.up));
            areaOfAttack.Add(mAContRef.GetNodeAtPosition(center + Vector2Int.right));
            areaOfAttack.Add(mAContRef.GetNodeAtPosition(center + Vector2Int.down));
            areaOfAttack.Add(mAContRef.GetNodeAtPosition(center + Vector2Int.left));

            //These are the diagnol tiles
            areaOfAttack.Add(mAContRef.GetNodeAtPosition(center + Vector2Int.up + Vector2Int.left));
            areaOfAttack.Add(mAContRef.GetNodeAtPosition(center + Vector2Int.up + Vector2Int.right));
            areaOfAttack.Add(mAContRef.GetNodeAtPosition(center + Vector2Int.down + Vector2Int.left));
            areaOfAttack.Add(mAContRef.GetNodeAtPosition(center + Vector2Int.down + Vector2Int.right));

            MoveAttack charToAttack;
            int HPindex = 0;
            enemiesHP = new List<Health>();
            
            foreach (Node attack in areaOfAttack){//iterates through the area of attack to test for enemies

                charToAttack = mAContRef.GetCharacterMAByNode(attack);
                if (charToAttack != null)//checks if character exist on node
                {
                    if(charToAttack.WhatAmI == CharacterType.Enemy)//checks if charcter is an enemy
                    {
                        enemiesHP.Add(charToAttack.GetComponent<Health>());
                        if (enemiesHP[HPindex] == null)
                            Debug.Log("Enemy to attack does not have a Health script attached to it");
                        HPindex++;
                    }
                }
                

            }
            StartSkillAnimation(attackNodePos);
        }
        else
        {
            //Debug.Log("Node to attack does not exist");
            EndSkill();
            return;
        }
    }

    /// <summary>
    /// Ends the skill animation. Deals damage to all participating enemies.
    /// </summary>
    public override void EndSkill()
    {
        //Debug.Log("360 End Skill");
        if (enemiesHP != null)
        {
            //Debug.Log("EndAnimation");
            // End the skills animation
            EndSkillAnimation();

            //Debug.Log("Deal damage");
            // Deal the damage and get rid of our reference to the enemyHP
            for (int i = 0; i < enemiesHP.Count; i++)
            {
                if (enemiesHP[i] != null)
                {
                    enemiesHP[i].TakeDamage(damage, this.GetComponent<Stats>());
                    enemiesHP[i] = null;
                }
            }
            //Debug.Log("Finished doing damage");
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
