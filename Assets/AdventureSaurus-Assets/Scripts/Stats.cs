using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stats : MonoBehaviour
{
    private const int _maxSpeed = 7; // Mostly for performance reasons, but there should probably be a limit
    public int MaxSpeed
    {
        get { return _maxSpeed; }
    }

    // Name of the character
    [SerializeField] private string charName = "Deceased";
    public string CharacterName
    {
        get { return charName; }
    }
    // Character's portrait picture
    [SerializeField] private Sprite charSpr = null;
    public Sprite CharacterSprite
    {
        get { return charSpr; }
    }
    // Charcter's side picture
    [SerializeField] private Sprite sideSpr = null;
    public Sprite SideSprte
    {
        get { return sideSpr; }
    }

    // Stats
    // How much damage this character deals
    [SerializeField] private int strength = 0;
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
    [SerializeField] private int speed = 0;
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
    [SerializeField] private int vitality = 0;
    public int Vitality
    {
        get { return vitality; }
    }

    // Experience
    // For determining how much xp the killer of this character should get
    // Allies give 0
    [SerializeField] private int baseXPToGive = 0;
    // The experience this character has
    private int _experience; // Total experience
    private int _oneLevelExperience; // The experience this character has gained on the current level
    public int OneLevelExperience
    {
        get { return _oneLevelExperience; }
    }
    // The current level of this character
    private int _level;
    public int Level
    {
        get { return _level; }
    }
    // The amount of experience this character needs to level up
    private int _nextLevelThreshold; // Total experience needed
    private int _oneLevelNextLevelThreshold; // The experience this character needs to gain since the current level
    public int OneLevelNextLevelThreshold
    {
        get { return _oneLevelNextLevelThreshold; }
    }
    // If the character is currently in the process of leveling up
    private bool _isLevelingUp;
    // Reference to this allies level up button. Null always for enemies
    [SerializeField] private GameObject levelUpButton = null;
    public GameObject LevelUpButton
    {
        set { levelUpButton = value; }
    }
    // The amount of stat increases that this character has left
    private int _amountStatIncreases;
    public int AmountStatIncreases
    {
        get { return _amountStatIncreases; }
        set { _amountStatIncreases = value; }
    }

    // For determining how close towards a particular stat increase the character is.
    // Also the amount of bubbles that should be filled in
    private int _vitalityBubblesFilled;
    public int VitalityBubblesFilled
    {
        get { return _vitalityBubblesFilled; }
        set { _vitalityBubblesFilled = value; }
    }
    private int _magicBubblesFilled;
    public int MagicBubblesFilled
    {
        get { return _magicBubblesFilled; }
        set { _magicBubblesFilled = value; }
    }
    private int _strBubblesFilled;
    public int StrBubblesFilled
    {
        get { return _strBubblesFilled; }
        set { _strBubblesFilled = value; }
    }
    private int _speedBubblesFilled;
    public int SpeedBubblesFilled
    {
        get { return _speedBubblesFilled; }
        set { _speedBubblesFilled = value; }
    }

    // References
    // Reference to the MoveAttack script attached to this character
    private MoveAttack _mARef;
    // Reference to the Health script attached to this character
    private Health _hpRef;
    // Reference to the MoveAttackController. Used when updating speed
    private MoveAttackController _mAContRef;

    // Stats display stuff
    [SerializeField] private GameObject statsDisplay = null;    // Reference to the stats display of this character
    [SerializeField] private Text nameText = null;  // Reference to the text of the stats display that shows the name
    [SerializeField] private Text attackText = null;    // Reference to the text of the stats display that shows the attack
    [SerializeField] private Text defenseText = null;   // Reference to the text of the stats display that shows the defense
    [SerializeField] private Text healthText = null;    // Reference to the text of the stats display that shows the max health
    [SerializeField] private Text speedText = null; // Reference to the text of the stats display that shows the speed

    // Side HUD references
    // Experience bar on the side. Will be set from PauseMenuController
    private Slider _expSlider;
    public Slider ExpSlider
    {
        set { _expSlider = value; }
    }

    // Events
    // When magic is increased
    public delegate void MagicIncrease();
    public static event MagicIncrease OnMagicIncrease;


    // Called when the component is set active
    // Subscribe to events
    private void OnEnable()
    {
        // When generation is done, do some initialization
        ProceduralGenerationController.OnFinishGenerationNoParam += SetReferences;
        ProceduralGenerationController.OnFinishGenerationNoParam += UpdateExpSlider;
    }

    // Called when the component is toggled off
    // Unsubscribe from events
    private void OnDisable()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= SetReferences;
        ProceduralGenerationController.OnFinishGenerationNoParam -= UpdateExpSlider;
    }

    // Called when the gameobject is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= SetReferences;
        ProceduralGenerationController.OnFinishGenerationNoParam -= UpdateExpSlider;
    }

    // Called before start
    private void Awake()
    {
        // These references are attached to foreign objects and will need to be set multiple times [allies only]
        SetReferences();
        // These references are attached to this game object, so they will only need to be set once
        _mARef = this.GetComponent<MoveAttack>();
        if (_mARef == null)
        {
            if (this.name != "DeadAllyStats")
                Debug.Log("Could not find MoveAttack attached to " + this.name);
        }
        else
        {
            _mARef.MoveRange = speed;
        }

        _hpRef = this.GetComponent<Health>();
        if (_hpRef == null)
        {
            if (this.name != "DeadAllyStats")
                Debug.Log("Could not find Health attached to " + this.name);
        }
        else
        {
            _hpRef.MaxHP = vitality;
        }
    }

    /// <summary>
    /// Sets references to foreign scripts.
    /// Called from Awake and from PersistantController
    /// </summary>
    private void SetReferences()
    {
        GameObject gameController = GameObject.FindWithTag("GameController");
        // Make sure a GameController exists
        if (gameController == null)
            Debug.Log("Could not find any GameObject with the tag GameController");
        else
        {
            _mAContRef = gameController.GetComponent<MoveAttackController>();
            if (_mAContRef == null)
            {
                Debug.Log("Could not find MoveAttackController attached to " + gameController.name);
            }
        }
    }

    // Inititalize variables
    private void Start()
    {
        // These will have to be set once, since these we will need to keep the changes associated with them
        // Start the character out at level 1 with no experience
        _experience = 0;
        _oneLevelExperience = _experience;
        _level = 1;
        // Calculate how much xp to reach the next level
        _nextLevelThreshold = CalculateAmountToReachNextLevel(_level);
        _oneLevelNextLevelThreshold = _nextLevelThreshold;
        // Don't start the player out with any potential stat increases
        _amountStatIncreases = 0;

        // If this character has a level up button, hide it
        if (levelUpButton != null)
            levelUpButton.SetActive(false);

        // Initialize the bubbles filled to 0
        _vitalityBubblesFilled = 0;
        _magicBubblesFilled = 0;
        _strBubblesFilled = 0;
        _speedBubblesFilled = 0;
    }

    /// <summary>
    /// Sets all the appropriate variables to their correct values.
    /// Called from MoveAttack in ResetMyTurn
    /// </summary>
    public void Initialize()
    {
        _mARef.MoveRange = speed;
        _hpRef.MaxHP = vitality;
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
        _experience += xpToGain;
        _oneLevelExperience += xpToGain;
        //Debug.Log(this.name + " gained " + xpToGain + " XP");
        StartCoroutine(CheckLevelUp());
    }

    /// <summary>
    /// Checks if the character should level up or not after gaining experience
    /// Is called as a coroutine in case this character has enough experience to level up multiple times
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator CheckLevelUp()
    {
        _isLevelingUp = false;   // Assume we are not leveling up
        do
        {
            // Test if the character has enough exp to level up and isn't already currently leveling up
            if (_experience >= _nextLevelThreshold && !_isLevelingUp)
            {
                // Start leveling the character up
                _isLevelingUp = true;
                ++_level; // Increment level
                _nextLevelThreshold = CalculateAmountToReachNextLevel(_level); // Make sure we do this so that the next loop test works as intended

                // Give the character skill points
                _amountStatIncreases += 3;

                // Set the oneLevel variables accordingly
                _oneLevelExperience -= _oneLevelNextLevelThreshold;
                _oneLevelNextLevelThreshold = _nextLevelThreshold - _oneLevelNextLevelThreshold;

                StartCoroutine(LevelUp());
            }
            yield return null;
        } while (_experience >= _nextLevelThreshold); // If the character can still level up more, we will do the loop again

        // Update the side slider for allies
        UpdateExpSlider();

        yield return null;
    }

    /// <summary>
    /// Sets the experience slider on the left to the correct value
    /// </summary>
    private void UpdateExpSlider()
    {
        // Will be null for enemies
        if (_expSlider != null)
            // Set the side exp bar
            _expSlider.value = ((float)_oneLevelExperience) / _oneLevelNextLevelThreshold;
    }

    /// <summary>
    /// Increments this character's level. Probably will do some visual stuff in the future
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator LevelUp()
    {
        Debug.Log("Congratulations!! " + this.name + " has reached level " + _level);

        // Show level up visuals
        ////TODO
        if (levelUpButton != null)
            levelUpButton.SetActive(true);

        // Stop leveling up, so that we can increment the next level if desired
        _isLevelingUp = false;

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
        return (((vitality + strength + magic) / 3) + 1) * baseXPToGive;
    }

    // The following 4 functions are for increasing stats upon a level up
    // They are all called from CharDetailedMenuController
    /// <summary>
    /// Increases vitality, Health.maxHP, Health.curHP, and updates the health bar to reflect that
    /// </summary>
    /// <param name="amountToIncr">The amount to increase vitality by</param>
    public void IncreaseVitality(int amountToIncr)
    {
        // Don't bother if we aren't increasing anything
        if (amountToIncr == 0)
            return;
        vitality += amountToIncr; // Increase the literal stat
        _hpRef.MaxHP = vitality; // Have the max health value reflect the change
        // Heal the character by the amount they just increased their vitality by, so they can start using that health right away.
        // This also serves to update the health bar visual
        _hpRef.Heal(amountToIncr);
    }
    /// <summary>
    /// Increases magic
    /// </summary>
    /// <param name="amountToIncr">The amount to increase magic by</param>
    public void IncreaseMagic(int amountToIncr)
    {
        // Don't bother if we aren't increasing anything
        if (amountToIncr == 0)
            return;
        magic += amountToIncr; // Increase the literal stat

        // Call the magic increase event
        if (OnMagicIncrease != null)
            OnMagicIncrease();
    }
    /// <summary>
    /// Increases strength and MoveAttack.damageToDeal
    /// </summary>
    /// <param name="amountToIncr">The amount to increase strength by</param>
    public void IncreaseStrength(int amountToIncr)
    {
        // Don't bother if we aren't increasing anything
        if (amountToIncr == 0)
            return;
        strength += amountToIncr; // Increase the literal stat
    }
    /// <summary>
    /// Increases speed, 
    /// </summary>
    /// <param name="amountToIncr">The amount to increase speed by</param>
    public void IncreaseSpeed(int amountToIncr)
    {
        // Don't bother if we aren't increasing anything
        if (amountToIncr == 0)
            return;
        speed += amountToIncr; // Increase the literal stat
        _mARef.MoveRange = speed; // Have the distance this character can move reflect that change
        Destroy(_mARef.RangeVisualParent); // Get rid of the character's old rangeVisuals
        _mARef.RangeVisualParent = null;
        _mAContRef.CreateVisualTiles(_mARef); // Make new ones
        // Recalculate the character's movement and attack
        _mARef.CalcMoveTiles();
        _mARef.CalcAttackTiles();
    }

    /// <summary>
    /// Hides the little level up indicator next to this enemies portrait
    /// </summary>
    public void HideLevelUpButton()
    {
        levelUpButton.gameObject.SetActive(false);
    }
}
