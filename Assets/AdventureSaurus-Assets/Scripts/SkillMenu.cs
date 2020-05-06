using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillMenu : MonoBehaviour
{
    // List of the side skill icons
    [SerializeField] private List<GameObject> _sideSkillObjs = new List<GameObject>();
    // List of the Images of the skill icons to show which one is active
    [SerializeField] private List<Image> _sideSkillIcons = new List<Image>();
    // List of Text that displays the cooldown of each skill
    [SerializeField] private List<Text> _skillCoolTexts = new List<Text>();
    // The color of an active and inactive skill
    [SerializeField] private Color _inactiveCol = new Color(1, 1, 1, 0.5f);
    [SerializeField] private Color _activeCol = new Color(1, 1, 1, 1);

    // For swapping the text for details about the skills
    [SerializeField] private Image _skillIcon = null;
    [SerializeField] private Text _skillNameText = null;
    [SerializeField] private Text _skillDescription = null;
    [SerializeField] private Text _damageText = null;
    [SerializeField] private Text _rangeText = null;
    [SerializeField] private Text _cooldownText = null;

    // Reference to the MoveAttackGUIController to get the selected character
    private MoveAttackGUIController _mAGUIContRef;


    // Called when the component is enabled
    // Subscribe to events
    private void OnEnable()
    {
        // When a new character is selected, update the side skill icons to reflect the new character
        MoveAttackGUIController.OnCharacterSelect += UpdateSkillButtons;
        // When a character finishes using a skill, update teh side skill icons in case one just went on cooldown
        MoveAttack.OnCharacterFinishedAction += RefreshSkillButtons;
        // When an ally gains a skill, upate the side skill icons
        AllySkillController.OnSkillGain += RefreshSkillButtons;
    }
    // Called when the component is disabled
    // Unsubscribe from events
    private void OnDisable()
    {
        MoveAttackGUIController.OnCharacterSelect -= UpdateSkillButtons;
        MoveAttack.OnCharacterFinishedAction -= RefreshSkillButtons;
        AllySkillController.OnSkillGain -= RefreshSkillButtons;
    }
    // Called then the component is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        MoveAttackGUIController.OnCharacterSelect -= UpdateSkillButtons;
        MoveAttack.OnCharacterFinishedAction -= RefreshSkillButtons;
        AllySkillController.OnSkillGain -= RefreshSkillButtons;
    }

    // Called before Start
    // Set references
    private void Awake()
    {
        _mAGUIContRef = this.GetComponent<MoveAttackGUIController>();
    }

    /// <summary>
    /// Changes the skill detailed menu about the skill
    /// </summary>
    /// <param name="skillIndex">Index of the button pressed</param>
    public void ChangeSkillDetails(int skillIndex)
    {
        // Get the current character's available skills
        MoveAttack charMA = _mAGUIContRef.RecentCharSelectedMA;
        List<Skill> availSkills = charMA.GetComponent<CharacterSkills>().GetAvailableSkills();
        // Get the skill coresponding to the clicked button
        Skill skillToCareAbout = availSkills[skillIndex];

        // Set the display information to go with the skill
        _skillIcon.sprite = SkillHolder.GetSkillImage(skillToCareAbout.GetSkillNum());
        _skillNameText.text = " " + SkillHolder.GetSkillName(skillToCareAbout.GetSkillNum());
        _skillDescription.text = SkillHolder.GetSkillDescription(skillToCareAbout.GetSkillNum());
        _damageText.text = skillToCareAbout.GetDamage().ToString();
        _rangeText.text = skillToCareAbout.GetRange().ToString();
        _cooldownText.text = skillToCareAbout.GetCoolDown().ToString();
    }

    /// <summary>
    /// Updates the skill skill buttons to reflect the selected character's skills
    /// </summary>
    private void UpdateSkillButtons(MoveAttack selChara)
    {
        // Get the current character's available skills
        List<Skill> availSkills = new List<Skill>();
        if (selChara != null)
            availSkills = selChara.GetComponent<CharacterSkills>().GetAvailableSkills();
        // Determine how many skills they have
        int amountSkills = availSkills.Count;
        if (amountSkills > _sideSkillObjs.Count)
        {
            Debug.LogError("The character has too many skills. Or there are too few side skill objects");
            amountSkills = _sideSkillObjs.Count;
        }

        // Turn on only as many side skills as needed
        for (int i = 0; i < amountSkills; ++i)
        {
            _sideSkillObjs[i].SetActive(true);
            _sideSkillIcons[i].sprite = SkillHolder.GetSkillImage(availSkills[i].GetSkillNum());
            if (availSkills[i].GetCooldownTimer() > 0)
                _skillCoolTexts[i].text = availSkills[i].GetCooldownTimer().ToString();
            else
                _skillCoolTexts[i].text = "";
            // If the skill is the active one, make it have the active color
            if (availSkills[i] == selChara.SkillRef)
                _sideSkillIcons[i].color = _activeCol;
            else
                _sideSkillIcons[i].color = _inactiveCol;
        }
        // Turn off the uneeded ones
        for (int i = amountSkills; i < _sideSkillObjs.Count; ++i)
        {
            _sideSkillObjs[i].SetActive(false);
        }
    }

    /// <summary>
    /// Swaps the skill of the current selected player
    /// Called by pressing a side skill button
    /// </summary>
    public void SwapCharacterSkill(int index)
    {
        // Get the current selected allies skill controller
        MoveAttack charMA = _mAGUIContRef.RecentCharSelectedMA;
        // Get the current character's available skills
        List<Skill> availSkills = charMA.GetComponent<CharacterSkills>().GetAvailableSkills();
        // If the skill is on cooldown, don't swap to it
        if (availSkills[index].GetCooldownTimer() > 0)
        {

        }
        // If the character does not already have that skill active, swap their skills
        else if (charMA.SkillRef != availSkills[index])
        {
            // Get a refrence to the currently selected ally's skill controller
            AllySkillController curAllySkillCont = charMA.GetComponent<AllySkillController>();
            // Swap the equipped skill
            curAllySkillCont.SwapSkill();
            // Reselect the character
            _mAGUIContRef.RefreshSelectedVisualTiles();
        }
    }

    /// <summary>
    /// Calls UpdateSkillButtons with the most recently selected character.
    /// Refreshes the skill buttons to reflect any changes
    /// </summary>
    private void RefreshSkillButtons()
    {
        // Get the current selected allies skill controller
        MoveAttack charMA = _mAGUIContRef.RecentCharSelectedMA;
        UpdateSkillButtons(charMA);
    }
}
