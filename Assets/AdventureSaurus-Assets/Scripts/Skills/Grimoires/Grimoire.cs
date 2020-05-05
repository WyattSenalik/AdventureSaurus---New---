using UnityEngine;

public enum MagicBuff { DMGINC, RANGEINC, COOLLWR, SKILLACQ };

public class Grimoire : MonoBehaviour
{
    // What to buff per magic level increase
    [SerializeField] protected MagicBuff[] _grimoirePages = { MagicBuff.SKILLACQ, MagicBuff.DMGINC };

    // The amount of pages read already
    protected int _pagesRead;
    public int GetPagesRead() { return _pagesRead; }
    public void SetPagesRead(int newPagesRead) { _pagesRead = newPagesRead; }

    // Reference to the ally's Stats
    protected Stats _statRef;


    // Called before start
    // Set references to itself
    protected virtual void Awake()
    {
        _statRef = this.GetComponent<Stats>();
        if (_statRef == null)
            Debug.Log("There was no Stats attached to " + this.name);

        _pagesRead = 0;
    }

    /// <summary>
    /// Called when magic is increased
    /// Handles how to upgrade the skill
    /// </summary>
    public void MagicIncrease()
    {
        // While we haven't read as many pages as we should have
        while (_pagesRead < _statRef.GetMagic())
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
                        IncreaseDamage();
                        break;
                    case (MagicBuff.RANGEINC):
                        IncreaseRange();
                        break;
                    case (MagicBuff.COOLLWR):
                        LowerCooldown();
                        break;
                    case (MagicBuff.SKILLACQ):
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
    /// Base Versions
    /// </summary>
    protected virtual void IncreaseDamage()
    {
        Debug.Log("Increase Damage not implemented");
    }

    /// <summary>
    /// Increases the range fo the skill by one
    /// Base Versions
    /// </summary>
    protected virtual void IncreaseRange()
    {
        Debug.Log("Increase Range not implemented");
    }

    /// <summary>
    /// Lowers the cooldown of the skill by one
    /// Base Versions
    /// </summary>
    protected virtual void LowerCooldown()
    {
        Debug.Log("Lower Cooldown not implemented");
    }

    /// <summary>
    /// Grants the skill to the ally
    /// Base Versions
    /// </summary>
    protected virtual void GainSkill()
    {
        Debug.Log("Gain Skill not implemented");
    }
}
