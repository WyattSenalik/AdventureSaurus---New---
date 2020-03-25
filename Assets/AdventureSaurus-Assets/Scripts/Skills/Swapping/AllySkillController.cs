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
    public Skill SpecialSkill
    {
        get {
            if (availableSkills.Count >= 2)
                return availableSkills[1];
            return null;
        }
    }
    // If the special skill is currently active
    private bool specialActive;
    public bool SpecialActive
    {
        get { return specialActive; }
    }

    // Called before the first frame
    private void Start()
    {
        // Initialize the list
        availableSkills = new List<Skill>();
        // Basic attack should already be on every ally, so add that skill as the first one
        availableSkills.Add(this.GetComponent<BasicAttack>());
        // Set the default first skill to basic attack
        SetSkill(availableSkills[0]);
        specialActive = false;


        // Giving all allies this for testing
        AcquireSkill(SkillHolder.THREE_SIXTY_SWING);
    }

    /// <summary>
    /// Adds a skill to the available skills list.
    /// </summary>
    /// <param name="skillIndex">Index of the skill to add</param>
    public void AcquireSkill(byte skillIndex)
    {
        // Try to give this character the skill
        Skill gottenSkill = skillHoldRef.GiveSkill(this, skillIndex);
        // Make sure its a valid skill
        if (gottenSkill != null)
            availableSkills.Add(gottenSkill);
    }
    
    /// <summary>
    /// Activates basic attack if special is active. Activates special if basic attack is active.
    /// Called from the swap controller when the skill button is pushed.
    /// </summary>
    public void SwapSkill()
    {
        // Change to basic
        if (specialActive)
        {
            DeactivateSkill();
            specialActive = false;
        }
        // Change to special
        else
        {
            // Set special active to true only if we successfully activated the skill
            specialActive = ActivateSkill();
        }
    }

    /// <summary>
    /// If we every want to let a character use more than 1 special skill
    /// </summary>
    /// <param name="curIndex">Index of the skill in availableSkills</param>
    private void ChangeSkill(int curIndex)
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
    /// <returns>True if the skill was activated, false otherwise</returns>
    private bool ActivateSkill()
    {
        ChangeSkill(1);
        return true;
    }

    /// <summary>
    /// Activates this characters basic attack
    /// </summary>
    public void DeactivateSkill()
    {
        ChangeSkill(0);
        specialActive = false;
    }
}
