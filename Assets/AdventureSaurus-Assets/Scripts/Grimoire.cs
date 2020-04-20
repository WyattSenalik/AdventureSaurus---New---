using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MagicBuff { DMGINC, RANGEINC, COOLLWR, SKILLACQ };

// This is attached to an ally for their skill increases
public class Grimoire : MonoBehaviour
{
    // Int value of the skill to gain
    [SerializeField] private byte _skillToGain = SkillHolder.THREE_SIXTY_SWING;
    public byte SkillToGain
    {
        get { return _skillToGain; }
    }
    // What to buff per magic level increase
    [SerializeField] private MagicBuff[] _grimoirePages = { MagicBuff.SKILLACQ, MagicBuff.DMGINC };
    // The amount of pages read already
    private int _pagesRead;
    // Reference to the ally's Stats
    private Stats _statRef;
    // Reference to the ally's AllySkillController
    private AllySkillController _allySkillContRef;


    // Called before start
    // Set references to itself
    private void Awake()
    {
        _statRef = this.GetComponent<Stats>();
        if (_statRef == null)
            Debug.Log("There was no Stats attached to " + this.name);
        _allySkillContRef = this.GetComponent<AllySkillController>();
        if (_allySkillContRef == null)
            Debug.Log("There was no AllySkillController attached to " + this.name);
    }

    // Start is called before the first frame update
    // Initialize variables
    private void Start()
    {
        _pagesRead = 0;
    }

    /// <summary>
    /// Called when magic is increased by 1.
    /// Handles how to upgrade the skill
    /// </summary>
    public void MagicIncrease()
    {
        // While we haven't read as many pages as we should have
        while (_pagesRead < _statRef.Magic)
        {
            // If we have read all the written pages, just increase damage
            if (_pagesRead >= _grimoirePages.Length)
            {
                Debug.Log("Read every page");
                IncreaseDamage();
            }
            // If we have not yet read all the pages, see what should happen
            else
            {
                switch (_grimoirePages[_pagesRead])
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
            ++_pagesRead;
        }
    }

    /// <summary>
    /// Increases the damage of the skill by one
    /// </summary>
    private void IncreaseDamage()
    {
        _allySkillContRef.SpecialSkill.UpgradeDamage();
    }

    /// <summary>
    /// Increases the range fo the skill by one
    /// </summary>
    private void IncreaseRange()
    {
        _allySkillContRef.SpecialSkill.UpgradeRange();
    }

    /// <summary>
    /// Lowers the cooldown of the skill by one
    /// </summary>
    private void LowerCooldown()
    {
        _allySkillContRef.SpecialSkill.UpgradeCooldown();
    }

    /// <summary>
    /// Grants the skill to the ally
    /// </summary>
    private void GainSkill()
    {
        // Acquire the skill
        _allySkillContRef.AcquireSkill(_skillToGain);
    }


    /// <summary>
    /// Called when magic is decreased by 1.
    /// Handles how to downgrade the skill
    /// </summary>
    public void MagicDecrease()
    {
        // While we have read too many pages
        while (_pagesRead >= _statRef.Magic)
        {
            // If we have read none the written pages, do nothing
            if (_pagesRead <= 0)
            {
                Debug.Log("Read no pages");
                return;
            }
            // If we have read some of the pages, see what should happen
            else
            {
                // Check the previous grimoir page to determine what to unlearn
                switch (_grimoirePages[_pagesRead - 1])
                {
                    case (MagicBuff.DMGINC):
                        Debug.Log("Increase Damage");
                        DecreaseDamage();
                        break;
                    case (MagicBuff.RANGEINC):
                        Debug.Log("Increase Range");
                        DecreaseRange();
                        break;
                    case (MagicBuff.COOLLWR):
                        Debug.Log("Lower Cooldown");
                        RaiseCooldown();
                        break;
                    case (MagicBuff.SKILLACQ):
                        Debug.Log("Gain Skill");
                        LoseSkill();
                        break;
                    default:
                        Debug.Log("WARNING - Unhandled MagicBuff");
                        break;
                }
            }
            // Decrement the pages we have read
            --_pagesRead;
        }
    }

    /// <summary>
    /// Decreases the damage of the skill by one
    /// </summary>
    private void DecreaseDamage()
    {
        _allySkillContRef.SpecialSkill.DowngradeDamage();
    }

    /// <summary>
    /// Decreases the range for the skill by one
    /// </summary>
    private void DecreaseRange()
    {
        _allySkillContRef.SpecialSkill.DowngradeRange();
    }

    /// <summary>
    /// Raises the cooldown of the skill by one
    /// </summary>
    private void RaiseCooldown()
    {
        _allySkillContRef.SpecialSkill.DowngradeCooldown();
    }

    /// <summary>
    /// Takes the skill from the ally
    /// </summary>
    private void LoseSkill()
    {
        // Lose the skill
        _allySkillContRef.LoseSkill(_skillToGain);
    }

    /// <summary>
    /// Peeks at the magic buff that is waiting in x pages
    /// </summary>
    /// <param name="pagesAhead">Pages ahead to peek</param>
    /// <returns>MagicBuff at current page + pagesAhead</returns>
    public MagicBuff PeekForward(int pagesAhead)
    {
        return _grimoirePages[_pagesRead - 1 + pagesAhead];
    }
}
