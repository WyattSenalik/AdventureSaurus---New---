using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls what skill is active for an enemy
// Attached to every enemy
public class EnemySkillController : CharacterSkills
{
    // Initialzies skill
    // Called from MoveAttackController
    public void Initialize()
    {
        // We want to set the enemies skill to the one skill they have
        Skill mySkill = this.GetComponent<Skill>();
        SetSkill(mySkill);
        mySkill.EquipSkill();
    }
}
