using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class is meant to be a parent of a similar script that will be attached to allies and enemies
public class CharacterSkills : MonoBehaviour
{
    // Reference to the MoveAttack script attached to this character
    private MoveAttack mARef;
    // Reference to the skill holder
    protected SkillHolder skillHoldRef;

    // Called 0th
    // Set references to itself
    private void Awake()
    {
        mARef = this.GetComponent<MoveAttack>();
        if (mARef == null)
            Debug.Log("WARNING - There was no MoveAttack script attached to " + this.name);
        GameObject skillHoldObj = GameObject.FindWithTag("SkillHolder");
        if (skillHoldObj == null)
            Debug.Log("WARNING - No GameObject with the tag SkillHolder was found");
        skillHoldRef = skillHoldObj.GetComponent<SkillHolder>();
        if (skillHoldRef == null)
            Debug.Log("WARNING - There was no SkillHolder script attached to " + skillHoldObj.name);
    }

    /// <summary>
    /// Sets the character's active skill in MoveAttack
    /// </summary>
    /// <param name="skillToSet">The skill that will be active now</param>
    protected void SetSkill(Skill skillToSet)
    {
        mARef.SkillRef = skillToSet;
    }
}
