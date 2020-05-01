using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls what skill is active for an ally
// Attached to every ally
public class AllySkillController : CharacterSkills
{

    // This allies special skill
    public Skill SpecialSkill
    {
        get {
            if (_availableSkills.Count >= 2)
                return _availableSkills[1];
            return null;
        }
    }
    // If the special skill is currently active
    private bool _specialActive;
    public bool SpecialActive
    {
        get { return _specialActive; }
    }

    // Events
    // When an ally gains a skill
    public delegate void SkillGain();
    public static event SkillGain OnSkillGain;

    // Called before the first frame
    // Initialize some variables
    protected new void Start()
    {
        base.Start();
        // Basic attack should already be on every ally, so add that skill as the first one
        _availableSkills.Add(this.GetComponent<BasicAttack>());
        // Set the default first skill to basic attack
        SetSkill(_availableSkills[0]);
        _availableSkills[0].EquipSkill();
        _specialActive = false;

        // Check if this character should start out with any skills by calling Grimoire's Magic increase
        Grimoire myGrim = this.GetComponent<Grimoire>();
        if (myGrim != null)
            myGrim.MagicIncrease();
        else
            Debug.Log("There is no Grimoire attached to " + this.name);
    }

    /// <summary>
    /// Adds a skill to the available skills list.
    /// </summary>
    /// <param name="skillIndex">Index of the skill to add</param>
    public void AcquireSkill(byte skillIndex)
    {
        // Try to give this character the skill
        Skill gottenSkill = _skillHoldRef.GiveSkill(this, skillIndex);
        // Make sure its a valid skill
        if (gottenSkill != null)
            _availableSkills.Add(gottenSkill);

        // Call the OnSkillGain event
        OnSkillGain?.Invoke();
    }

    /// <summary>
    /// Removes a skill to the available skills list.
    /// </summary>
    /// <param name="skillIndex">Index of the skill to lose</param>
    public void LoseSkill(byte skillIndex)
    {
        // We want to equip basic attack so that when we remove a skill, we don't have the
        // removed skill equipped
        DeactivateSkill();

        // Try to remove the skill from this character
        _skillHoldRef.RemoveSkill(this, skillIndex);
        // Try to remove all null references from the available skills list
        // Since the skill just removed will be null in the list
        int curIndex = 0;
        while (curIndex < _availableSkills.Count)
        {
            // If we found a null skill, remove it
            if (_availableSkills[curIndex] == null)
            {
                _availableSkills.RemoveAt(curIndex);
            }
            // Otherwise keep iterating
            else
            {
                ++curIndex;
            }
        }
    }

    /// <summary>
    /// Activates basic attack if special is active. Activates special if basic attack is active.
    /// Called from the swap controller when the skill button is pushed.
    /// </summary>
    public void SwapSkill()
    {
        // Change to basic
        if (_specialActive)
        {
            DeactivateSkill();
            _specialActive = false;
        }
        // Change to special
        else
        {
            // Set special active to true only if we successfully activated the skill
            _specialActive = ActivateSkill();
        }
    }

    /// <summary>
    /// If we ever want to let a character use more than 1 special skill
    /// </summary>
    /// <param name="curIndex">Index of the skill in availableSkills</param>
    private void ChangeSkill(int curIndex)
    {
        // Extra safe - make sure available skills has been initialized
        if (_availableSkills == null)
        {
            Debug.Log("availableSkills in AllySkillController attached to " + this.name + " has not been initialized yet");
            return;
        }
        // Make sure its a valid index
        if (curIndex >= _availableSkills.Count)
        {
            Debug.Log("Invalid skill index in AllySkillController attached to " + this.name);
            return;
        }

        // Change the skill
        SetSkill(_availableSkills[curIndex]);
        _availableSkills[curIndex].EquipSkill();
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
        _specialActive = false;
    }

    /// <summary>
    /// Calls the SpawnSkillAddition that spawns a prefab with another animation on it
    /// </summary>
    public void SpawnSkillAdditionForActive()
    {
        _mARef.SkillRef.SpawnSkillAddition();
    }
}
