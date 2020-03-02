using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeSixtySwing : Skill
{
    private new void Awake()
    {
        base.Awake();
        skillNum = 360;

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
            areaOfAttack.Add(mAContRef.GetNodeAtPosition(center + Vector2Int.up + Vector2Int.left));
            areaOfAttack.Add(mAContRef.GetNodeAtPosition(center + Vector2Int.up + Vector2Int.right));
            areaOfAttack.Add(mAContRef.GetNodeAtPosition(center + Vector2Int.down + Vector2Int.left));
            areaOfAttack.Add(mAContRef.GetNodeAtPosition(center + Vector2Int.up + Vector2Int.right));

            MoveAttack charToAttack;
            int HPindex = 0;
            enemiesHP = new List<Health>();
            
            foreach (Node attack in areaOfAttack){//iterates through the area of attack to test for enemies

                charToAttack = mAContRef.GetCharacterMAByNode(attack);
                if (charToAttack != null)//checks if character exist on node
                {
                    if(charToAttack.gameObject.tag == "Enemy")//checks if charcter is an enemy
                    {
                        enemiesHP.Add(charToAttack.GetComponent<Health>());
                        if (enemiesHP[HPindex] == null)
                            Debug.Log("Enemy to attack does not have a Health script attached to it");
                        HPindex++;
                    }
                }
                else
                    Debug.Log("Enemy to attack does not have a MoveAttack script attached to it");

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
}
