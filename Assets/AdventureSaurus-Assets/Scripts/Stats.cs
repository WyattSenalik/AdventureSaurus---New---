using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stats : MonoBehaviour
{
    // Stats
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

    // Experience
    // For determining how much xp the killer of this character should get
    // Allies give 0
    [SerializeField] private int baseXPToGive = 0;
    // The experience this character has
    private int experience;
    // The current level of this character
    private int level;
    public int Level
    {
        get { return level; }
    }
    // The amount of experience this character needs to level up
    private int nextLevelThreshold;
    // If the character is currently in the process of leveling up
    private bool isLevelingUp;

    // References
    private MoveAttack mARef;   // Reference to the MoveAttack script attached to this character
    private Health hpRef;   // Reference to the Health script attached to this character

    // Stats display stuff
    [SerializeField] private GameObject statsDisplay = null;    // Reference to the stats display of this character
    [SerializeField] private Text nameText = null;  // Reference to the text of the stats display that shows the name
    [SerializeField] private Text attackText = null;    // Reference to the text of the stats display that shows the attack
    [SerializeField] private Text defenseText = null;   // Reference to the text of the stats display that shows the defense
    [SerializeField] private Text healthText = null;    // Reference to the text of the stats display that shows the max health
    [SerializeField] private Text speedText = null; // Reference to the text of the stats display that shows the speed

    // Set references
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

    // Inititalize variables
    private void Start()
    {
        // Start the character out at level 1 with no experience
        experience = 0;
        level = 1;
        // Calculate how much xp to reach the next level
        nextLevelThreshold = CalculateAmountToReachNextLevel(level);
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

    /// <summary>
    /// Gives this character experience. Called from Health by Ascend unction
    /// </summary>
    /// <param name="xpToGain">The amount of experience to gain</param>
    public void GainExperience(int xpToGain)
    {
        experience += xpToGain;
        Debug.Log(this.name + " gained " + xpToGain + " XP");
        StartCoroutine(CheckLevelUp());
    }

    /// <summary>
    /// Checks if the character should level up or not after gaining experience
    /// Is called as a coroutine in case this character has enough experience to level up multiple times
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator CheckLevelUp()
    {
        isLevelingUp = false;   // Assume we are not leveling up
        do
        {
            // Test if the character has enough exp to level up and isn't already currently leveling up
            if (experience >= nextLevelThreshold && !isLevelingUp)
            {
                // Start leveling the character up
                isLevelingUp = true;
                ++level; // Increment level
                nextLevelThreshold = CalculateAmountToReachNextLevel(level); // Make sure we do this so that the next loop test works as intended
                StartCoroutine(LevelUp());
            }
            yield return null;
        } while (experience >= nextLevelThreshold); // If the character can still level up more, we will do the loop again

        yield return null;
    }

    /// <summary>
    /// Increments this character's level. Probably will do some visual stuff in the future
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator LevelUp()
    {
        Debug.Log("Congratulations!! " + this.name + " has reached level " + level);

        // Give the character skill points
        ////TODO

        // Show level up visuals
        ////TODO

        // Stop leveling up, so that we can increment the next level if desired
        isLevelingUp = false;

        yield return null;
    }

    /// <summary>
    /// Calculates the amount needed to reach the next level based on the currentLevel
    /// </summary>
    /// <param name="currentLevel">The current level to calculate from</param>
    /// <returns>The amount of xp needed to reach level (currentLevel + 1)</returns>
    private int CalculateAmountToReachNextLevel(int currentLevel)
    {
        // Do the simple calculaiton (n^3)
        return currentLevel * currentLevel * currentLevel + 2;
    }

    /// <summary>
    /// Calculates how much xp should be gained by the killer for killing this character
    /// </summary>
    /// <param name="killer">Stats component of the character who killed this unit</param>
    /// <returns>int amount of xp the killer should gain</returns>
    public int KillReward(Stats killer)
    {
        // If this character is not supposed to give any xp
        if (baseXPToGive == 0)
        {
            return 0;
        }
        // Do the calculation
        return Mathf.RoundToInt(baseXPToGive * (vitality + strength + magic) / 7.0f) + 1;
    }
}
