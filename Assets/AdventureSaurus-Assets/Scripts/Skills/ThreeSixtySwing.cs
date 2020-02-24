using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeSixtySwing : Skill
{
    private new void Awake()
    {
        skillNum = 360;

    }

    override public void StartSkill(Vector2Int attackNodePos)
    {
        Node nodeToAttack = mAContRef.GetNodeAtPosition(attackNodePos);
        if (nodeToAttack != null)
        {
            Node center = mAContRef.GetNodeByWorldPosition(this.transform.position);



        }
        else
        {
            //Debug.Log("Node to attack does not exist");
            EndSkill();
            return;
        }
    }
}
