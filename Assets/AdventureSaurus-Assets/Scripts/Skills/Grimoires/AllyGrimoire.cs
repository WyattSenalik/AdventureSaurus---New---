using UnityEngine;

// This is attached to an ally for their skill increases
public class AllyGrimoire : Grimoire
{
    // Int value of the skill to gain
    [SerializeField] private byte _skillToGain = SkillHolder.THREE_SIXTY_SWING;
    public byte SkillToGain
    {
        get { return _skillToGain; }
    }

    // Reference to the ally's AllySkillController
    private AllySkillController _allySkillContRef;


    // Called before start
    // Set references to itself
    protected override void Awake()
    {
        base.Awake();

        _allySkillContRef = this.GetComponent<AllySkillController>();
        if (_allySkillContRef == null)
            Debug.Log("There was no AllySkillController attached to " + this.name);
    }



    /// <summary>
    /// Increases the damage of the skill by one
    /// </summary>
    protected override void IncreaseDamage()
    {
        _allySkillContRef.SpecialSkill.UpgradeDamage();
    }

    /// <summary>
    /// Increases the range fo the skill by one
    /// </summary>
    protected override void IncreaseRange()
    {
        _allySkillContRef.SpecialSkill.UpgradeRange();
    }

    /// <summary>
    /// Lowers the cooldown of the skill by one
    /// </summary>
    protected override void LowerCooldown()
    {
        _allySkillContRef.SpecialSkill.UpgradeCooldown();
    }

    /// <summary>
    /// Grants the skill to the ally
    /// </summary>
    protected override void GainSkill()
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
        while (_pagesRead >= _statRef.GetMagic())
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
        if (_pagesRead - 1 + pagesAhead < _grimoirePages.Length)
        {
            return _grimoirePages[_pagesRead - 1 + pagesAhead];
        }
        return MagicBuff.DMGINC;
    }
}
