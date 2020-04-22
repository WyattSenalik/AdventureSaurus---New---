using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillPreviewController : MonoBehaviour
{
    // The skill preview stuff
    // Name text
    [SerializeField] private Text _skillNameText = null;
    // Damage text
    [SerializeField] private Text _skillDmgText = null;
    // Range text
    [SerializeField] private Text _skillRangeText = null;
    // Cooldown text
    [SerializeField] private Text _skillCooldownText = null;

    // Reference to SkillHolder for if the upgrade is to get a skill
    private SkillHolder _skillHolderRef;


    // Called before start
    private void Awake()
    {
        _skillHolderRef = this.GetComponent<SkillHolder>();
        if (_skillHolderRef == null)
            Debug.Log("WARNING - No SkillHolder attached to " + this.name);
    }

    // Start is called before the first frame update
    private void Start()
    {
        if (_skillNameText == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "skillNameText was not initialized properly, please set it in the editor");
        }
        if (_skillDmgText == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "skillDmgText was not initialized properly, please set it in the editor");
        }
        if (_skillRangeText == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "skillRangeText was not initialized properly, please set it in the editor");
        }
        if (_skillCooldownText == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "skillCooldownText was not initialized properly, please set it in the editor");
        }
    }

    /// <summary>
    /// Display the preview menu with the ally's special skill
    /// </summary>
    /// <param name="allyStats">Reference to the ally who is parent of the skill</param>
    public void DisplayUpdatePreviewMenu(AllyStats allyStats)
    {
        // Update the skill preview
        string skillName = "Upgrade magic to gain a skill";
        string skillDmg = "";
        string skillRange = "";
        string skillCooldown = "";
        // Get the reference to the skill
        if (allyStats != null)
        {
            AllySkillController skillContRef = allyStats.GetComponent<AllySkillController>();
            if (skillContRef != null)
            {
                Skill activeSkill = skillContRef.SpecialSkill;
                if (activeSkill != null)
                {
                    skillName = "Skill: " + SkillHolder.GetSkillName(activeSkill.SkillNum);
                    skillDmg = "Damage: " + activeSkill.Damage.ToString();
                    skillRange = "Range: " + activeSkill.Range.ToString();
                    skillCooldown = "Cooldown: " + activeSkill.Cooldown.ToString();
                }
            }
        }
        // Set the values
        _skillNameText.text = skillName;
        _skillDmgText.text = skillDmg;
        _skillRangeText.text = skillRange;
        _skillCooldownText.text = skillCooldown;
        // Set their colors to default
        _skillNameText.color = CharDetailedMenuController.DefTextCol;
        _skillDmgText.color = CharDetailedMenuController.DefTextCol;
        _skillRangeText.color = CharDetailedMenuController.DefTextCol;
        _skillCooldownText.color = CharDetailedMenuController.DefTextCol;
    }

    /// <summary>
    /// Replaces text and such with what would happen if the skill was upgraded
    /// </summary>
    /// <param name="allyGrim">Grimoire of the selected</param>
    /// <param name="previewMagicStat">The amount the magic will potentially increase</param>
    public void PreviewSkillUpgrade(Grimoire allyGrim, int amountIncrease)
    {
        // Default values
        string skillName = "Upgrade magic to gain a skill";
        int skillDmg = 0;
        int skillRange = 0;
        int skillCooldown = 0;

        // For if the values were changed at all
        bool isSkillNameBuff = false;
        bool isSkillDmgBuff = false;
        bool isSkillRangeBuff = false;
        bool isSkillCooldownBuff = false;

        // Get the starting values for the skill
        AllySkillController skillContRef = allyGrim.GetComponent<AllySkillController>();
        if (skillContRef != null)
        {
            Skill activeSkill = skillContRef.SpecialSkill;
            if (activeSkill != null)
            {
                skillName = "Skill: " + SkillHolder.GetSkillName(activeSkill.SkillNum);
                skillDmg = activeSkill.Damage;
                skillRange = activeSkill.Range;
                skillCooldown = activeSkill.Cooldown;
            }


            // Iterate over the buffs
            for (int i = 1; i <= amountIncrease; ++i)
            {
                // Get the next buff
                MagicBuff nextBuff = allyGrim.PeekForward(i);

                switch (nextBuff)
                {
                    // If the next buff is to acquire a skill, get that skill and set the defaults
                    case (MagicBuff.SKILLACQ):
                        // We are going to temporarily give this character the skill in question to get the starting values
                        Skill gainSkill = _skillHolderRef.GiveSkill(skillContRef, allyGrim.SkillToGain);

                        skillName = "Skill: " + SkillHolder.GetSkillName(allyGrim.SkillToGain);
                        skillDmg = gainSkill.Damage;
                        skillRange = gainSkill.Range;
                        skillCooldown = gainSkill.Cooldown;

                        isSkillNameBuff = true;
                        isSkillDmgBuff = true;
                        isSkillRangeBuff = true;
                        isSkillCooldownBuff = true;

                        // Get rid of the temporary skill
                        Destroy(gainSkill);
                        break;
                    // If the next buff is just an increment, increment them
                    case (MagicBuff.DMGINC):
                        ++skillDmg;
                        isSkillDmgBuff = true;
                        break;
                    case (MagicBuff.RANGEINC):
                        isSkillRangeBuff = true;
                        ++skillRange;
                        break;
                    case (MagicBuff.COOLLWR):
                        isSkillCooldownBuff = true;
                        --skillCooldown;
                        break;
                    default:
                        Debug.Log("Unhandled MagicBuff in SkillPreviewController");
                        break;
                }
            }
            // Set the values
            _skillNameText.text = skillName;
            _skillDmgText.text = "Damage: " + skillDmg.ToString();
            _skillRangeText.text = "Range: " + skillRange.ToString();
            _skillCooldownText.text = "Cooldown: " + skillCooldown.ToString();
            // Set the ones that were buffed to green
            if (isSkillNameBuff)
                _skillNameText.color = CharDetailedMenuController.UpgradeTextCol;
            if (isSkillDmgBuff)
                _skillDmgText.color = CharDetailedMenuController.UpgradeTextCol;
            if (isSkillRangeBuff)
                _skillRangeText.color = CharDetailedMenuController.UpgradeTextCol;
            if (isSkillCooldownBuff)
                _skillCooldownText.color = CharDetailedMenuController.UpgradeTextCol;
        }
    }
}
