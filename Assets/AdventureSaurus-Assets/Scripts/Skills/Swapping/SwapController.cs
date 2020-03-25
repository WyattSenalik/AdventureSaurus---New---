using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapController : MonoBehaviour
{
    // References to other scripts
    // For getting the current ally selected
    private MoveAttackGUIController mAGUIRef;

    // Set references
    private void Awake()
    {
        mAGUIRef = this.GetComponent<MoveAttackGUIController>();
        if (mAGUIRef == null)
            Debug.Log("There was no MoveAttackGUIController attached to " + mAGUIRef.name);
    }

    /// <summary>
    /// Swaps the skill of the current selected player
    /// Called by the skill swap button.
    /// </summary>
    public void SwapSelectedPlayerSkill()
    {
        // Get the current selected allies skill controller
        AllySkillController curAllySkillCont = mAGUIRef.CharSelectedMA.GetComponent<AllySkillController>();
        // Swap their skills
        curAllySkillCont.SwapSkill();
    }
}
