﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class is meant to be a parent of a similar script that will be attached to allies and enemies
public class CharacterSkills : MonoBehaviour
{
    // Reference to the MoveAttack script attached to this character
    protected MoveAttack _mARef;
    // Reference to the skill holder
    protected SkillHolder _skillHoldRef;

    // Called 0th
    // Set references to itself
    private void Awake()
    {
        _mARef = this.GetComponent<MoveAttack>();
        if (_mARef == null)
            Debug.Log("WARNING - There was no MoveAttack script attached to " + this.name);
        GameObject gameContObj = GameObject.FindWithTag("GameController");
        if (gameContObj == null)
            Debug.Log("WARNING - No GameObject with the tag GameController was found");
        _skillHoldRef = gameContObj.GetComponent<SkillHolder>();
        if (_skillHoldRef == null)
            Debug.Log("WARNING - There was no SkillHolder script attached to " + gameContObj.name);
    }

    /// <summary>
    /// Sets the character's active skill in MoveAttack
    /// </summary>
    /// <param name="skillToSet">The skill that will be active now</param>
    protected void SetSkill(Skill skillToSet)
    {
        _mARef.SkillRef = skillToSet;
    }
}
