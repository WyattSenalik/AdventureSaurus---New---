﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwapController : MonoBehaviour
{
    // The button that swaps the skills of the current character
    [SerializeField] private Button _swapButt = null;

    // Reference to the MoveAttackGUIController script
    // For getting the current ally selected
    private MoveAttackGUIController _mAGUIRef;

    // Called when this gameobject is enabled
    // Add UpdateSkillButton to be called when we select a character
    private void OnEnable()
    {
        MoveAttackGUIController.OnCharacterSelect += UpdateSkillButton;
        MoveAttackGUIController.OnCharacterDeselect += UpdateSkillButton;
    }

    // Called when this gameobject is disabled
    // Remove UpdateSkillButton from being called when we select a character
    private void OnDisable()
    {
        MoveAttackGUIController.OnCharacterSelect -= UpdateSkillButton;
        MoveAttackGUIController.OnCharacterDeselect -= UpdateSkillButton;
    }

    // Called when this gameobject is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        MoveAttackGUIController.OnCharacterSelect -= UpdateSkillButton;
        MoveAttackGUIController.OnCharacterDeselect -= UpdateSkillButton;
    }

    // Set references
    private void Awake()
    {
        _mAGUIRef = this.GetComponent<MoveAttackGUIController>();
        if (_mAGUIRef == null)
            Debug.Log("There was no MoveAttackGUIController attached to " + _mAGUIRef.name);
    }

    /// <summary>
    /// Swaps the skill of the current selected player
    /// Called by the skill swap button.
    /// </summary>
    public void SwapSelectedPlayerSkill()
    {
        // Get the current selected allies skill controller
        MoveAttack charMA = _mAGUIRef.CharSelectedMA;
        if (charMA == null)
            return;
        AllySkillController curAllySkillCont = charMA.GetComponent<AllySkillController>();

        // Call the swap skill event
        curAllySkillCont.SwapSkill();
    }

    /// <summary>
    /// Update the button when we select a new character
    /// </summary>
    /// <param name="charMARef">The currently selected character</param>
    private void UpdateSkillButton(MoveAttack charMARef)
    {
        // If there is no character selected or that character is an enemy, just set the button to default and disable it
        if (charMARef == null || charMARef.WhatAmI == CharacterType.Enemy)
        {
            _swapButt.interactable = false;
            // TODO Hide cooldown
        }
        // If that character is an ally, let the button be interactable if their cooldown is up
        else if (charMARef.WhatAmI == CharacterType.Ally)
        {
            // Get the allySkillController to determine the character's skill information
            AllySkillController allySkillContRef = charMARef.GetComponent<AllySkillController>();
            if (allySkillContRef == null)
            {
                Debug.Log("No AllySkillController was attached to " + charMARef.name);
                return;
            }

            // If the special skill is currently active, let the player swap, since the other skill is basic attack
            // This will activate basic attacked when the button is pressed
            if (allySkillContRef.SpecialActive)
            {
                _swapButt.interactable = true;
                // TODO Hide cooldown
            }
            // If basic attack is currently active (special skill isn't) and the ally has a special skill
            else if (allySkillContRef.SpecialSkill != null)
            {
                // If there is no current cooldown, let the player hit the button
                // This will activate the special skill when the button is pressed
                if (allySkillContRef.SpecialSkill.CooldownTimer <= 0)
                {
                    _swapButt.interactable = true;
                    // TODO Hide cooldown
                }
                // If the cooldown is not up yet, don't let the player hit the button and display the remaining cooldown
                else
                {
                    _swapButt.interactable = false;
                    // TODO Display cooldown
                }
            }
            // If basic attack is currently active (special skill isn't) and the ally has no special skill, don't let them press the button
            else
            {
                _swapButt.interactable = false;
                // TODO Hide cooldown
            }
        }
    }
}
