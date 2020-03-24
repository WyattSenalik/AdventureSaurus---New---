using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls what skill is active for an enemy
// Attached to every enemy
public class EnemySkillController : CharacterSkills
{
    // Start is called before the first frame update
    private void Start()
    {
        // We want to set the enemies skill to the one skill they have
        SetSkill(this.GetComponent<Skill>());
    }
}
