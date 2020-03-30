using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MagicBuff { DMGINC, RANGEINC, COOLLWR, SKILLACQ };

// This is attached to an ally for their skill increases
public class Grimoire : MonoBehaviour
{
    // Int value of the skill to gain
    [SerializeField] private byte skillToGain = SkillHolder.THREE_SIXTY_SWING;
    // What to buff per magic level increase
    [SerializeField] private MagicBuff[] grimoirePages = { MagicBuff.SKILLACQ, MagicBuff.DMGINC };
    // The amount of pages read already
    private int pagesRead;
    // Reference to the ally's Stats
    private Stats statRef;
    // Reference to the ally's AllySkillController
    private AllySkillController allySkillContRef;

    // Called before start
    // Set references to itself
    private void Awake()
    {
        statRef = this.GetComponent<Stats>();
        if (statRef == null)
            Debug.Log("There was no Stats attached to " + this.name);
        allySkillContRef = this.GetComponent<AllySkillController>();
        if (allySkillContRef == null)
            Debug.Log("There was no AllySkillController attached to " + this.name);
    }

    // Start is called before the first frame update
    // Initialize variables
    private void Start()
    {
        pagesRead = 0;
    }

    // Called when the gameobject is set active
    // Subscribe to events
    private void OnEnable()
    {
        // Add magic increase to be called when magic is increased
        Stats.OnMagicIncrease += MagicIncrease;
    }

    // Called when the gameobject is disabled
    // Unsubscribe from events
    private void OnDisable()
    {
        // Remove magic increase from being called when magic is increased
        Stats.OnMagicIncrease -= MagicIncrease;
    }

    // Called when the gameobject is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        Stats.OnMagicIncrease -= MagicIncrease;
    }

    /// <summary>
    /// Called when magic is increased by magic is increased by 1.
    /// Handles how to upgrade the skill
    /// </summary>
    public void MagicIncrease()
    {
        // While we haven't read as many pages as we should have
        while (pagesRead < statRef.Magic)
        {
            // If we have read all the written pages, just increase damage
            if (pagesRead >= grimoirePages.Length)
            {
                Debug.Log("Read every page");
                IncreaseDamage();
            }
            // If we have not yet read all the pages, see what should happen
            else
            {
                switch (grimoirePages[pagesRead])
                {
                    case (MagicBuff.DMGINC):
                        Debug.Log("Increase Damage");
                        IncreaseDamage();
                        break;
                    case (MagicBuff.RANGEINC):
                        Debug.Log("Increase Range");
                        IncreaseRange();
                        break;
                    case (MagicBuff.COOLLWR):
                        Debug.Log("Lower Cooldown");
                        LowerCooldown();
                        break;
                    case (MagicBuff.SKILLACQ):
                        Debug.Log("Gain Skill");
                        GainSkill();
                        break;
                    default:
                        Debug.Log("WARNING - Unhandled MagicBuff");
                        break;
                }
            }
            // Increment the pages we have read
            ++pagesRead;
        }
    }

    /// <summary>
    /// Increases the damage of the skill by one
    /// </summary>
    private void IncreaseDamage()
    {
        allySkillContRef.SpecialSkill.UpgradeDamage();
    }

    /// <summary>
    /// Increases the range fo the skill by one
    /// </summary>
    private void IncreaseRange()
    {
        allySkillContRef.SpecialSkill.UpgradeRange();
    }

    /// <summary>
    /// Lowers the cooldown of the skill by one
    /// </summary>
    private void LowerCooldown()
    {
        allySkillContRef.SpecialSkill.UpgradeCooldown();
    }

    /// <summary>
    /// Grants the skill to the ally
    /// </summary>
    private void GainSkill()
    {
        // Acquire the skill
        allySkillContRef.AcquireSkill(skillToGain);
    }
}
