using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smite : Skill
{
    [SerializeField] private GameObject smiteEffect = null;

    private new void Awake()
    {
        base.Awake();
        skillNum = 1;
        cooldown = 4;
        range = 2;
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
                //enemiesHP = new List<Health>();
                //enemiesHP.Add(charToAttack.GetComponent<Health>());
                //if (enemiesHP[0] == null)
                //    Debug.Log("Ally to heal does not have a Health script attached to it");

                // Start the skill's animation
                StartSkillAnimation(attackNodePos);

                GameObject smiteBullet = Instantiate(smiteEffect, new Vector3(attackNodePos.x, attackNodePos.y, 0), Quaternion.identity);
                SmiteSpawn smiteSpawnRef = smiteBullet.GetComponent<SmiteSpawn>();
                smiteSpawnRef.Damage = damage;
                smiteSpawnRef.DealDamageTo = charToAttack.GetComponent<Health>();
                smiteSpawnRef.GiveXPTo = charToAttack.GetComponent<Stats>();
            }
            else
                Debug.Log("Enemy to attack does not have a MoveAttack script attached to it");
        }
    }

    public override void EndSkill()
    {
        EndSkillAnimation();
    }
}
