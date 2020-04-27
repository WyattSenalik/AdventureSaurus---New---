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
    // The experience this character has (total)
    private int _experience = 0;
    public int GetExperience() { return _experience; }
    public void SetExperience(int newExperience) { _experience = newExperience; }
    // The experience this character has gained on the current level
    private int _oneLevelExperience = 0;
    public int GetOneLevelExperience() { return _oneLevelExperience; }
    public void SetOneLevelExperience(int newOneLvlExperience) { _oneLevelExperience = newOneLvlExperience; }
    // The current level of this character
    private int _level = 1;
    public int GetLevel() { return _level; }
    public void SetLevel(int newLevel) { _level = newLevel; }
    // The amount of experience this character needs to level up (total)
    private int _nextLevelThreshold = 3;
    public int GetNextLevelThreshold() { return _nextLevelThreshold; }
    public void SetNextLevelThreshold(int newNextLevelThreshold) { _nextLevelThreshold = newNextLevelThreshold; }
    // The experience this character needs to gain since the current level
    private int _oneLevelNextLevelThreshold = 3;
    public int GetOneLevelNextLevelThreshold() { return _oneLevelNextLevelThreshold; }
    public void SetOneLevelNextLevelThreshold(int newOneLevelNextLevelThreshold) { _oneLevelNextLevelThreshold = newOneLevelNextLevelThreshold; }
    // If the character is currently in the process of leveling up
    private bool _isLevelingUp;
    // Reference to this allies level up button. Null always for enemies
    [SerializeField] private GameObject _levelUpButton = null;
    public GameObject LevelUpButton
    {
        set { _levelUpButton = value; }
    }
    // The amount of stat increases that this character has left
    private int _amountStatIncreases = 0;
    public int AmountStatIncreases
    {
        get { return _amountStatIncreases; }
        set { _amountStatIncreases = value; }
    }

    // For determining how close towards a particular stat increase the character is.
    // Also the amount of bubbles that should be filled in
    private int _vitalityBubblesFilled = 0;
    public int VitalityBubblesFilled
    {
        get { return _vitalityBubblesFilled; }
        set { _vitalityBubblesFilled = value; }
    }
    private int _magicBubblesFilled = 0;
    public int MagicBubblesFilled
    {
        get { return _magicBubblesFilled; }
        set { _magicBubblesFilled = value; }
    }
    private int _strBubblesFilled = 0;
    public int StrBubblesFilled
    {
        get { return _strBubblesFilled; }
        set { _strBubblesFilled = value; }
    }
    private int _speedBubblesFilled = 0;
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
        // Calculate how much xp to reach the next level
        _nextLevelThreshold = CalculateAmountToReachNextLevel(_level);
        _oneLevelNextLevelThreshold = _nextLevelThreshold - (_experience - _oneLevelExperience);

        // If this character has a level up button, hide it
        if (_levelUpButton != null)
            _levelUpButton.SetActive(false);
        StartCoroutine(CheckLevelUp());
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
    /// Gives this character reduced experience like exp share. Called from Health by Ascend unction
    /// </summary>
    /// <param name="reducedXpToGain">The amount of experience to gain</param>
    public void GainReducedExperience(int reducedXpToGain)
    {
        Debug.Log(reducedXpToGain);
        _experience += reducedXpToGain;
        _oneLevelExperience += reducedXpToGain;
        //Debug.Log(this.name + " gained " + xpToGain + "shared XP");
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
            if (!_isLevelingUp && _experience >= _nextLevelThreshold)
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
    private void IncreaseVitality(int amountToIncr)
    {
        // Don't bother if we aren't increasing anything
        if (amountToIncr <= 0)
            return;
        _vitality += amountToIncr; // Increase the literal stat
        _hpRef.MaxHP = _vitality; // Have the max health value reflect the change
        // Heal the character by the amount they just increased their vitality by, so they can start using that health right away.
        // This also serves to update the health bar visual
        _hpRef.Heal(amountToIncr);
    }

    /// <summary>
    /// Increases magic and updates the grimoire
    /// </summary>
    /// <param name="amountToIncr">The amount to increase magic by</param>
    private void IncreaseMagic(int amountToIncr)
    {
        // Don't bother if we aren't increasing anything
        if (amountToIncr <= 0)
            return;
        _magic += amountToIncr; // Increase the literal stat

        // Call the magic increase from this character's grimoire
        _grimRef.MagicIncrease();

    }

    /// <summary>
    /// Increases strength
    /// </summary>
    /// <param name="amountToIncr">The amount to increase strength by</param>
    private void IncreaseStrength(int amountToIncr)
    {
        // Don't bother if we aren't increasing anything
        if (amountToIncr <= 0)
            return;
        _strength += amountToIncr; // Increase the literal stat
    }

    /// <summary>
    /// Increases speed, 
    /// </summary>
    /// <param name="amountToIncr">The amount to increase speed by</param>
    private void IncreaseSpeed(int amountToIncr)
    {
        // Don't bother if we aren't increasing anything
        if (amountToIncr <= 0)
            return;
        // Increase the literal stat
        _speed += amountToIncr;
        // Have the distance this character can move reflect that change
        _mARef.MoveRange = _speed;
        // Just in case, calculate all the tiles again
        _mARef.CalculateAllTiles();

        // We don't have to do the below any more, since we make a max amount of tiles at the beginning anyway
        // Get rid of the character's old rangeVisuals
        //Destroy(_mARef.RangeVisualParent);
        //_mARef.RangeVisualParent = null;
        // Make new ones
        //_mAContRef.CreateVisualTiles(_mARef);
        // Recalculate the character's movement and attack
        // Done in CreateVisualTiles
        //_mARef.CalcMoveTiles();
        //_mARef.CalcAttackTiles();
    }

    // The following 4 functions are for decreasing stats upon removing an item
    /// <summary>
    /// Decreases vitality, Health.maxHP, Health.curHP, and updates the health bar to reflect that
    /// </summary>
    /// <param name="amountToDec">The amount to decrease vitality by</param>
    private void DecreaseVitality(int amountToDec)
    {
        // Don't bother if we aren't decreasing anything
        if (amountToDec <= 0)
            return;
        // Decrease the literal stat
        _vitality -= amountToDec;
        // Have the max health value reflect the change
        _hpRef.MaxHP = _vitality;
        // Deal dmg to the character by the amount they just decreased their vitality by,
        // so that the change is actually reflected.
        // This also serves to update the health bar visual
        _hpRef.TakeDamage(amountToDec, null);
    }

    /// <summary>
    /// Decreases magic and updates the grimoire
    /// </summary>
    /// <param name="amountToDec">The amount to decrease magic by</param>
    private void DecreaseMagic(int amountToDec)
    {
        // Don't bother if we aren't decreasing anything
        if (amountToDec <= 0)
            return;
        // Decrease the literal stat
        _magic -= amountToDec;

        // Call the magic decrease from this character's grimoire
        _grimRef.MagicDecrease();

    }

    /// <summary>
    /// Decreases strength
    /// </summary>
    /// <param name="amountToDec">The amount to decrease strength by</param>
    private void DecreaseStrength(int amountToDec)
    {
        // Don't bother if we aren't decreasing anything
        if (amountToDec <= 0)
            return;
        // Decrease the literal stat
        _strength -= amountToDec;
    }

    /// <summary>
    /// Decreases speed and remakes the visual tiles to account for that
    /// </summary>
    /// <param name="amountToDec">The amount to decrease speed by</param>
    private void DecreaseSpeed(int amountToDec)
    {
        // Don't bother if we aren't decreasing anything
        if (amountToDec <= 0)
            return;
        // Decrease the literal stat
        _speed -= amountToDec;
        // Have the distance this character can move reflect that change
        _mARef.MoveRange = _speed;
        // Just in case, calculate all the tiles again
        _mARef.CalculateAllTiles();

        // We don't have to do the below any more, since we make a max amount of tiles at the beginning anyway
        // Get rid of the character's old rangeVisuals
        //Destroy(_mARef.RangeVisualParent);
        //_mARef.RangeVisualParent = null;
        // Make new ones
        //_mAContRef.CreateVisualTiles(_mARef);
        // Recalculate the character's movement and attack
        // Done in CreateVisualTiles
        //_mARef.CalcMoveTiles();
        //_mARef.CalcAttackTiles();
    }

    /// <summary>
    /// General change function for vitality
    /// </summary>
    /// <param name="amountToChange">Value to change stat by. Can be positive or negative.</param>
    public void ChangeVitality(int amountToChange)
    {
        //Test if amount is positive to call increase
        if(amountToChange > 0)
            IncreaseVitality(amountToChange);

        //Test if amount is negative to call decrease
        if (amountToChange < 0)
            DecreaseVitality(-amountToChange);//Change amount to its opposite(which should be positive) for decrease function

        //Amount value 0 does nothing
    }

    /// <summary>
    /// General change function for magic
    /// </summary>
    /// <param name="amountToChange">Value to change stat by. Can be positive or negative.</param>
    public void ChangeMagic(int amountToChange)
    {
        //Test if amount is positive to call increase
        if (amountToChange > 0)
            IncreaseMagic(amountToChange);

        //Test if amount is negative to call decrease
        if (amountToChange < 0)
            DecreaseMagic(-amountToChange);//Change amount to its opposite(which should be positive) for decrease function

        //Amount value 0 does nothing
    }

    /// <summary>
    /// General change function for strength
    /// </summary>
    /// <param name="amountToChange">Value to change stat by. Can be positive or negative.</param>
    public void ChangeStrength(int amountToChange)
    {
        //Test if amount is positive to call increase
        if (amountToChange > 0)
            IncreaseStrength(amountToChange);

        //Test if amount is negative to call decrease
        if (amountToChange < 0)
            DecreaseStrength(-amountToChange);//Change amount to its opposite(which should be positive) for decrease function

        //Amount value 0 does nothing
    }

    /// <summary>
    /// General change function for speed
    /// </summary>
    /// <param name="amountToChange">Value to change stat by. Can be positive or negative.</param>
    public void ChangeSpeed(int amountToChange)
    {
        //Test if amount is positive to call increase
        if (amountToChange > 0)
            IncreaseSpeed(amountToChange);

        //Test if amount is negative to call decrease
        if (amountToChange < 0)
            DecreaseSpeed(-amountToChange);//Change amount to its opposite(which should be positive) for decrease function

        //Amount value 0 does nothing
    }

    /// <summary>
    /// Hides the little level up indicator next to this enemies portrait
    /// </summary>
    public void HideLevelUpButton()
    {
        _levelUpButton.gameObject.SetActive(false);
    }

    
}
