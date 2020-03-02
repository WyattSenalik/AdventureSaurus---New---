using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stats : MonoBehaviour
{
    // Name of the character
    [SerializeField] private string charName = "Character";
    public string CharacterName
    {
        get { return charName; }
    }
    // How much damage this character deals
    [SerializeField] private int strength = 1;
    public int Strength
    {
        get { return strength; }
    }
    // Magic level affects spells
    [SerializeField] private int magic = 0;
    public int Magic
    {
        get { return magic; }
    }
    // How many tiles this character can move
    [SerializeField] private int speed = 2;
    public int Speed
    {
        get { return speed; }
    }
    // How much damage this character deflects
    [SerializeField] private int defense = 0;
    public int Defense
    {
        get { return defense; }
    }
    // Max health this character has
    [SerializeField] private int vitality = 2;
    public int Vitality
    {
        get { return vitality; }
    }
    // The experience to give to the killer of this character
    // Only used by allies
    [SerializeField] private int xpToGive = 0;
    public int Experience
    {
        get { return xpToGive; }
    }

    private MoveAttack mARef;   // Reference to the MoveAttack script attached to this character
    private Health hpRef;   // Reference to the Health script attached to this character

    // Stats display stuff
    [SerializeField] private GameObject statsDisplay = null;    // Reference to the stats display of this character
    [SerializeField] private Text nameText = null;  // Reference to the text of the stats display that shows the name
    [SerializeField] private Text attackText = null;    // Reference to the text of the stats display that shows the attack
    [SerializeField] private Text defenseText = null;   // Reference to the text of the stats display that shows the defense
    [SerializeField] private Text healthText = null;    // Reference to the text of the stats display that shows the max health
    [SerializeField] private Text speedText = null; // Reference to the text of the stats display that shows the speed

    private void Awake()
    {
        mARef = this.GetComponent<MoveAttack>();
        if (mARef == null)
            Debug.Log("Could not find MoveAttack attached to " + this.name);
        else
        {
            mARef.MoveRange = speed;
            mARef.DmgToDeal = strength;
        }

        hpRef = this.GetComponent<Health>();
        if (hpRef == null)
            Debug.Log("Could not find Health attached to " + this.name);
        else
        {
            hpRef.MaxHP = vitality;
        }
    }

    /// <summary>
    /// Sets all the appropriate variables to their correct values
    /// </summary>
    public void Initialize()
    {
        mARef.MoveRange = speed;
        mARef.DmgToDeal = strength;
        hpRef.MaxHP = vitality;
    }

    /// <summary>
    /// Shows or hides the stats of the character
    /// </summary>
    /// <param name="shouldShow">If the characters' stats should be shown or hidden</param>
    public void DisplayStats(bool shouldShow)
    {
        if (statsDisplay != null)
        {
            nameText.text = charName;
            attackText.text = "A: " + strength;
            defenseText.text = "D: " + defense;
            healthText.text = "H: " + vitality;
            speedText.text = "S: " + speed;
            statsDisplay.SetActive(shouldShow);
        }
    }

    /// <summary>
    /// Returns whether the stats are displayed or not
    /// </summary>
    /// <returns>Whether the stats are displayed or not</returns>
    public bool AreStatsDisplayed()
    {
        if (statsDisplay != null)
            return statsDisplay.activeSelf;
        return false;
    }
}
