using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwapController : MonoBehaviour
{
    // Reference to the MoveAttackGUIController script
    // For getting the current ally selected
    private MoveAttackGUIController _mAGUIRef;

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
        MoveAttack charMA = _mAGUIRef.RecentCharSelectedMA;
        if (charMA != null)
        {
            // Get a refrence to the currently selected ally's skill controller
            AllySkillController curAllySkillCont = charMA.GetComponent<AllySkillController>();
            // Swap the equipped skill
            curAllySkillCont.SwapSkill();
        }
    }
}
