using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls what skill is active for an ally
// Attached to every ally
public class AllySkillController : CharacterSkills
{
    // The skills this ally current has available to them to use
    private List<Skill> availableSkills;

    // Called before the first frame
    private void Start()
    {
        // Initialize the list
        availableSkills = new List<Skill>();
        // Basic attack should already be on every ally, so add that skill as the first one
        availableSkills.Add(this.GetComponent<BasicAttack>());
    }

    /// <summary>
    /// Adds a skill to the available skills list.
    /// </summary>
    /// <param name="skillIndex">Index of the skill to add</param>
    public void AcquireSkill(int skillIndex)
    {
        // Try to give this character the skill
        Skill gottenSkill = skillHoldRef.GiveSkill(this, skillIndex);
        // Make sure its a valid skill
        if (gottenSkill != null)
            availableSkills.Add(gottenSkill);
    }

    /// <summary>
    /// If we every want to let a character use more than 1 special skill
    /// </summary>
    /// <param name="curIndex">Index of the skill in availableSkills</param>
    public void SwapSkill(int curIndex)
    {
        // Extra safe - make sure available skills has been initialized
        if (availableSkills == null)
        {
            Debug.Log("availableSkills in AllySkillController attached to " + this.name + " has not been initialized yet");
            return;
        }
        // Make sure its a valid index
        if (curIndex >= availableSkills.Count)
        {
            Debug.Log("Invalid skill index in AllySkillController attached to " + this.name);
            return;
        }

        // Change the skill
        SetSkill(availableSkills[curIndex]);
    }

    /// <summary>
    /// Activates this characters special skill
    /// </summary>
    public void ActivateSkill()
    {
        // Extra safe - make sure available skills has been initialized
        if (availableSkills == null)
        {
            Debug.Log("availableSkills in AllySkillController attached to " + this.name + " has not been initialized yet");
            return;
        }
        // Make sure its a valid index
        if (1 >= availableSkills.Count)
        {
            Debug.Log("There is at most 1 skill attached to " + this.name);
            return;
        }

        // Change the skill to the special skill (1)
        SetSkill(availableSkills[1]);
    }

    /// <summary>
    /// Activates this characters basic attack
    /// </summary>
    public void DeactivateSkill()
    {
        // Extra safe - make sure available skills has been initialized
        if (availableSkills == null)
        {
            Debug.Log("availableSkills in AllySkillController attached to " + this.name + " has not been initialized yet");
            return;
        }
        // Make sure its a valid index
        if (0 == availableSkills.Count)
        {
            Debug.Log("There are no skills attached to " + this.name);
            return;
        }

        // Change the skill to basic attack (0)
        SetSkill(availableSkills[0]);
    }
}
