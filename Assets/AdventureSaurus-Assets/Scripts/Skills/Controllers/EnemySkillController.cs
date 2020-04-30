using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls what skill is active for an enemy
// Attached to every enemy
public class EnemySkillController : CharacterSkills
{
    // Called before the first frame
    // Initializes skill
    protected new void Start()
    {
        base.Start();
        // We want to set the enemies skill to the one skill they have
        Skill mySkill = this.GetComponent<Skill>();
        if (mySkill != null)
        {
            SetSkill(mySkill);
            mySkill.EquipSkill();
            _availableSkills.Add(mySkill);
        }
    }
}
