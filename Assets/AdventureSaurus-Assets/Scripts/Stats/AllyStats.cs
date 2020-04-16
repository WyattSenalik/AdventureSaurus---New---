using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllyStats : Stats
{
    // Character's portrait picture
    [SerializeField] private Sprite _charPortrait = null;
    public Sprite CharacterSprite
    {
        get { return _charPortrait; }
    }
    // Charcter's side picture
    [SerializeField] private Sprite _sideSpr = null;
    public Sprite SideSprite
    {
        get { return _sideSpr; }
    }

    // Experience
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
    [SerializeField] private GameObject _levelUpButton = null;
    public GameObject LevelUpButton
    {
        set { _levelUpButton = value; }
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

    // Side HUD references
    // Experience bar on the side. Will be set from PauseMenuController
    private Slider _expSlider;
    public Slider ExpSlider
    {
        set { _expSlider = value; }
    }

    // References to itself
    // Reference to the Grimoire script attached to this character
    private Grimoire _grimRef;

    // Reference to the MoveAttackController. Used when updating speed
    private MoveAttackController _mAContRef;


    // Called when the component is set active
    // Subscribe to events
    private void OnEnable()
    {
        // When generation is done, do some initialization
        ProceduralGenerationController.OnFinishGenerationNoParam += SetReferences;
    }

    // Called when the component is toggled off
    // Unsubscribe from events
    private void OnDisable()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= SetReferences;
    }

    // Called when the gameobject is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= SetReferences;
    }

    // Called before start
    // Set references   
    private new void Awake()
    {
        // Call the base's version
        base.Awake();

        // These references are attached to foreign objects and will need to be set multiple times [allies only]
        SetReferences();
        // These references are attached to this game object, so they will only need to be set once
        _grimRef = this.GetComponent<Grimoire>();
        if (_grimRef == null)
        {
            if (this.name != "DeadAllyStats")
                Debug.Log("Could not find Grimoire attached to " + this.name);
        }
    }

    /// <summary>
    /// Sets references to foreign scripts.
    /// Called from PersistantController.OnFinishGenerationNoParam event
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

    // Inititalize variables and set beginning of game states
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
        if (_levelUpButton != null)
            _levelUpButton.SetActive(false);

        // Initialize the bubbles filled to 0
        _vitalityBubblesFilled = 0;
        _magicBubblesFilled = 0;
        _strBubblesFilled = 0;
        _speedBubblesFilled = 0;
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

                // Give the character skill points
                _amountStatIncreases += 3;

                // Set the oneLevel variables accordingly
                _oneLevelExperience = _experience - _nextLevelThreshold;
                _nextLevelThreshold = CalculateAmountToReachNextLevel(_level); // Make sure we do this so that the next loop test works as intended
                _oneLevelNextLevelThreshold = _nextLevelThreshold - (_experience - _oneLevelExperience);

                StartCoroutine(LevelUp());
            }
            yield return null;
        } while (_experience >= _nextLevelThreshold); // If the character can still level up more, we will do the loop again

        // Update the side slider for allies
        UpdateExpSlider();

        yield return null;
    }

    /// <summary>
    /// Sets the experience slider on the left to the correct value.
    /// Called when gaining exp and from PauseMenuController when new floor genned
    /// </summary>
    public void UpdateExpSlider()
    {
        if (_expSlider != null)
            // Set the side exp bar
            _expSlider.value = ((float)_oneLevelExperience) / _oneLevelNextLevelThreshold;
        else
            Debug.Log("No exp slider found");
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
        if (_levelUpButton != null)
            _levelUpButton.SetActive(true);

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
        _vitality += amountToIncr; // Increase the literal stat
        _hpRef.MaxHP = _vitality; // Have the max health value reflect the change
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
        _magic += amountToIncr; // Increase the literal stat

        // Call the magic increase from this character's grimoire
        _grimRef.MagicIncrease();

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
        _strength += amountToIncr; // Increase the literal stat
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
        // Increase the literal stat
        _speed += amountToIncr;
        // Have the distance this character can move reflect that change
        _mARef.MoveRange = _speed;
        // Get rid of the character's old rangeVisuals
        Destroy(_mARef.RangeVisualParent);
        _mARef.RangeVisualParent = null;
        // Make new ones
        _mAContRef.CreateVisualTiles(_mARef);
        // Recalculate the character's movement and attack
        // Done in CreateVisualTiles
        //_mARef.CalcMoveTiles();
        //_mARef.CalcAttackTiles();
    }

    /// <summary>
    /// Hides the little level up indicator next to this enemies portrait
    /// </summary>
    public void HideLevelUpButton()
    {
        _levelUpButton.gameObject.SetActive(false);
    }

    
}
