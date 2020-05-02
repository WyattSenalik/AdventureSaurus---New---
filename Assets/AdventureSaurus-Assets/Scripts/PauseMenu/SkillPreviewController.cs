using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillPreviewController : MonoBehaviour
{
    // The skill preview stuff
    // Name text
    [SerializeField] private Text _skillNameText = null;
    // Description text
    [SerializeField] private Text _skillDescriptionText = null;
    // Damage text
    [SerializeField] private Text _skillDmgText = null;
    // Range text
    [SerializeField] private Text _skillRangeText = null;
    // Cooldown text
    [SerializeField] private Text _skillCooldownText = null;
    // Skill icon
    [SerializeField] private Image _skillIconImage = null;

    // Default skill text color
    [SerializeField] private Color _defSkillTextCol = new Color(1, 1, 1);

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
        string skillName = " None";
        string skillDescription = "Upgrade magic to gain a skill";
        string skillDmg = "0";
        string skillRange = "0";
        string skillCooldown = "0";
        Sprite skillIcon = null;
        // Get the reference to the skill
        if (allyStats != null)
        {
            AllySkillController skillContRef = allyStats.GetComponent<AllySkillController>();
            if (skillContRef != null)
            {
                Skill activeSkill = skillContRef.SpecialSkill;
                if (activeSkill != null)
                {
                    skillName = " " + SkillHolder.GetSkillName(activeSkill.SkillNum);
                    skillDescription = SkillHolder.GetSkillDescription(activeSkill.SkillNum);
                    skillDmg = activeSkill.Damage.ToString();
                    skillRange = activeSkill.Range.ToString();
                    skillCooldown = activeSkill.Cooldown.ToString();
                    skillIcon = SkillHolder.GetSkillImage(activeSkill.SkillNum);
                }
            }
        }
        // Set the values
        _skillNameText.text = skillName;
        _skillDescriptionText.text = skillDescription;
        _skillDmgText.text = skillDmg;
        _skillRangeText.text = skillRange;
        _skillCooldownText.text = skillCooldown;
        _skillIconImage.sprite = skillIcon;
        if (skillIcon == null)
            _skillIconImage.color = new Color(0, 0, 0, 0);
        else
            _skillIconImage.color = new Color(1, 1, 1, 1);
        // Set their colors to default
        _skillNameText.color = _defSkillTextCol;
        _skillDescriptionText.color = _defSkillTextCol;
        _skillDmgText.color = _defSkillTextCol;
        _skillRangeText.color = _defSkillTextCol;
        _skillCooldownText.color = _defSkillTextCol;
    }

    /// <summary>
    /// Replaces text and such with what would happen if the skill was upgraded
    /// </summary>
    /// <param name="allyGrim">Grimoire of the selected</param>
    /// <param name="previewMagicStat">The amount the magic will potentially increase</param>
    public void PreviewSkillUpgrade(AllyGrimoire allyGrim, int amountIncrease)
    {
        // Default values
        string skillName = " None";
        string skillDescription = "Upgrade magic to gain a skill";
        int skillDmg = 0;
        int skillRange = 0;
        int skillCooldown = 0;
        Sprite skillIcon = null;

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
                skillName = " " + SkillHolder.GetSkillName(activeSkill.SkillNum);
                skillDescription = SkillHolder.GetSkillDescription(activeSkill.SkillNum);
                skillIcon = SkillHolder.GetSkillImage(activeSkill.SkillNum);
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

                        skillName = " " + SkillHolder.GetSkillName(allyGrim.SkillToGain);
                        skillDescription = SkillHolder.GetSkillDescription(allyGrim.SkillToGain);
                        skillIcon = SkillHolder.GetSkillImage(allyGrim.SkillToGain);
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
            _skillDescriptionText.text = skillDescription;
            _skillIconImage.sprite = skillIcon;
            if (skillIcon == null)
                _skillIconImage.color = new Color(0, 0, 0, 0);
            else
                _skillIconImage.color = new Color(1, 1, 1, 1);
            _skillDmgText.text = skillDmg.ToString();
            _skillRangeText.text = skillRange.ToString();
            _skillCooldownText.text = skillCooldown.ToString();
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
