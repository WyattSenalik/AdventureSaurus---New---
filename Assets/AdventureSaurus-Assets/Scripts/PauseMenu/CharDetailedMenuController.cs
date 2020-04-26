using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharDetailedMenuController : MonoBehaviour
{
    // The stats of all 3 character. [0] = Ally 1, [1] = Ally 2, [2] = Ally 3 
    // (this is true for all the ally lists [hopefull])
    private List<AllyStats> _alliesStats;
    // The ally we are currently viewing by index
    private int _currentAllyIndex;

    // Canvas display things
    //
    // These things will all get swapped when we hit the next and previous character buttons
    [SerializeField] private Image _characterPortrait = null;
    [SerializeField] private Text _nameText = null;
    [SerializeField] private Text _lvlText = null;
    [SerializeField] private Text _vitalityNums = null;
    [SerializeField] private Text _magicNums = null;
    [SerializeField] private Text _strNums = null;
    [SerializeField] private Text _speedNums = null;
    // Bubbles stuff. The bubbles are children of these transforms
    [SerializeField] private Transform _vitalityBubblesParent = null;
    [SerializeField] private Transform _magicBubblesParent = null;
    [SerializeField] private Transform _strBubblesParent = null;
    [SerializeField] private Transform _speedBubblesParent = null;
    [SerializeField] private Sprite _emptyBubbleSpr = null;
    [SerializeField] private Sprite _fullBubbleSpr = null;
    // These are set true or false depending if there are points to spend for the current character
    [SerializeField] private Text _pointsText = null;
    [SerializeField] private Button _vitalityIncrButt = null;
    [SerializeField] private Button _magicIncrButt = null;
    [SerializeField] private Button _strIncrButt = null;
    [SerializeField] private Button _speedIncrButt = null;
    [SerializeField] private Button _resetButt = null;
    [SerializeField] private Button _confirmButt = null;

    // The level up buttons (for allies 1, 2, and 3 in order)
    [SerializeField] private GameObject[] _levelUpButtons = null;

    // For keeping track of how many points the player is putting into each stat upon a level up
    private int _amountPointsAvailable;
    private int _vitalityAmountIncr;
    private int _magicAmountIncr;
    private int _strAmountIncr;
    private int _speedAmountIncr;

    // For holding the bubbles' images
    private List<Image> _vitalityBubbles;
    private List<Image> _magicBubbles;
    private List<Image> _strBubbles;
    private List<Image> _speedBubbles;

    // A dead character's stats. Okay, so if an ally dies, we can't display their stats because they're dead.
    // But we need to display something in their place, so we display a "dead" character's stats
    private AllyStats _deadAlly;

    // This is shown when the user tries to submit their changes without using all their points
    [SerializeField] private GameObject _unappliedChangesPrompt = null;

    // Colors
    public static readonly Color DefTextCol = new Color(0.207843137254902f, 0.2196078431372549f, 0.2823529411764706f);
    public static readonly Color UpgradeTextCol = new Color(0.207843137254902f, 0.7450980392156863f, 0.2823529411764706f);
    public static readonly Color DowngradeTextCol = new Color(0.7450980392156863f, 0.2196078431372549f, 0.2823529411764706f);

    // Reference to the SkillPreviewController
    private SkillPreviewController _skillPrevContRef;


    // Called when the gameobject is toggled active
    // Subscribe to events
    private void OnEnable()
    {
        // When the character clicks to display the charcter detailed menu, update the details and diaply them
        PauseMenuController.OnCharDetailedMenuShown += DisplayCharacterDetails;
        // When the generation finishes, initialize this script
        ProceduralGenerationController.OnFinishGenerationNoParam += Initialize;
    }

    // Called when the gameobject is toggled active
    // Unsubscribe from events
    private void OnDisable()
    {
        PauseMenuController.OnCharDetailedMenuShown -= DisplayCharacterDetails;
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
    }

    // Called when the gameobject is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        PauseMenuController.OnCharDetailedMenuShown -= DisplayCharacterDetails;
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
    }

    // Called before start.
    // Set references
    private void Awake()
    {
        _skillPrevContRef = this.GetComponent<SkillPreviewController>();
        if (_skillPrevContRef == null)
            Debug.Log("WARNING - There is no SkillPreviewController attached to " + this.name);
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Make it so we are viewing the first enemy at first
        _currentAllyIndex = 0;

        // Set the amount the player is increasing to zero right off the bat since they aren't increasing anything yet
        _amountPointsAvailable = 0;
        _vitalityAmountIncr = 0;
        _magicAmountIncr = 0;
        _strAmountIncr = 0;
        _speedAmountIncr = 0;

        // Make the dead ally
        GameObject deadAllyObj = new GameObject("DeadAllyStats");
        _deadAlly = deadAllyObj.AddComponent<AllyStats>();

        // Do a lot of validation
        if (_characterPortrait == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "characterPortrait was not initialized properly, please set it in the editor");
        }
        if (_nameText == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "nameText was not initialized properly, please set it in the editor");
        }
        if (_lvlText == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "lvlText was not initialized properly, please set it in the editor");
        }
        if (_vitalityNums == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "vitalityNums was not initialized properly, please set it in the editor");
        }
        if (_magicNums == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "magicNums was not initialized properly, please set it in the editor");
        }
        if (_strNums == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "strNums was not initialized properly, please set it in the editor");
        }
        if (_speedNums == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "speedNums was not initialized properly, please set it in the editor");
        }
        if (_vitalityBubblesParent == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "vitalityBubblesParent was not initialized properly, please set it in the editor");
        }
        if (_magicBubblesParent == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "magicBubblesParent was not initialized properly, please set it in the editor");
        }
        if (_strBubblesParent == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "strBubblesParent was not initialized properly, please set it in the editor");
        }
        if (_speedBubblesParent == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "speedBubblesParent was not initialized properly, please set it in the editor");
        }
        if (_emptyBubbleSpr == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "emptyBubbleSpr was not initialized properly, please set it in the editor");
        }
        if (_fullBubbleSpr == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "fullBubbleSpr was not initialized properly, please set it in the editor");
        }
        if (_pointsText == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "pointsText was not initialized properly, please set it in the editor");
        }
        if (_vitalityIncrButt == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "vitalityIncrButt was not initialized properly, please set it in the editor");
        }
        if (_magicIncrButt == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "magicIncrButt was not initialized properly, please set it in the editor");
        }
        if (_strIncrButt == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "strIncrButt was not initialized properly, please set it in the editor");
        }
        if (_speedIncrButt == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "speedIncrButt was not initialized properly, please set it in the editor");
        }
        if (_resetButt == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "resetButt was not initialized properly, please set it in the editor");
        }
        if (_confirmButt == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "confirmButt was not initialized properly, please set it in the editor");
        }
        if (_levelUpButtons == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "levelUpButtons was not initialized properly, please set it in the editor");
        }
        if (_levelUpButtons.Length != 3)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "levelUpButtons was not initialized properly. It must have 3 values. Please set it correctly in the editor");
        }
        if (_unappliedChangesPrompt == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "unappliedChangesPrompt was not initialized properly, please set it in the editor");
        }

        // Create the lists for the bubbles
        _vitalityBubbles = new List<Image>();
        _magicBubbles = new List<Image>();
        _strBubbles = new List<Image>();
        _speedBubbles = new List<Image>();
        // Get the images on the bubbles and add them to the respective list
        foreach (Transform bubbleTrans in _vitalityBubblesParent)
        {
            Image bubbleImg = bubbleTrans.GetComponent<Image>();
            _vitalityBubbles.Add(bubbleImg);
        }
        foreach (Transform bubbleTrans in _magicBubblesParent)
        {
            Image bubbleImg = bubbleTrans.GetComponent<Image>();
            _magicBubbles.Add(bubbleImg);
        }
        foreach (Transform bubbleTrans in _strBubblesParent)
        {
            Image bubbleImg = bubbleTrans.GetComponent<Image>();
            _strBubbles.Add(bubbleImg);
        }
        foreach (Transform bubbleTrans in _speedBubblesParent)
        {
            Image bubbleImg = bubbleTrans.GetComponent<Image>();
            _speedBubbles.Add(bubbleImg);
        }
    }

    /// <summary>
    /// Initializes things for this script.
    /// Called from the FinishGenerating event
    /// </summary>
    private void Initialize()
    {
        Transform allyParent = GameObject.Find(ProceduralGenerationController.ALLY_PARENT_NAME).transform;

        // Initialize ally stats
        _alliesStats = new List<AllyStats>();
        // Iterate over the allies to get their stats
        foreach (Transform allyTrans in allyParent)
        {
            // Try to get the ally's stats
            AllyStats allyStats = allyTrans.GetComponent<AllyStats>();
            // If the stats aren't null, add it to the list
            if (allyStats != null)
                _alliesStats.Add(allyStats);
            else
                Debug.LogError("There was no AllyStats attached to " + allyTrans.name);
        }

        // Set the level up buttons for the characters
        for (int i = 0; i < _alliesStats.Count; ++i)
        {
            _alliesStats[i].LevelUpButton = _levelUpButtons[i];
        }
    }

    /// <summary>
    /// Shows the details of the ally with the given index
    /// </summary>
    /// <param name="allyIndex">Index of the ally to be displayed</param>
    private void DisplayCharacterDetails(int allyIndex)
    {
        _currentAllyIndex = allyIndex; // This is the new ally we are viewing

        AllyStats allyStats = null;
        // Check the index is valid and that the value is not null
        if (_alliesStats.Count > allyIndex && _alliesStats[allyIndex] != null)
            allyStats = _alliesStats[allyIndex]; // For quick reference
        // If the index is invalid, we make allyStats be a dead ally
        else
        {
            allyStats = _deadAlly;
            Debug.Log("This character is dead");
        }

        // Set the normal things for the character stats
        _characterPortrait.sprite = allyStats.CharacterSprite;
        _nameText.text = allyStats.CharacterName;
        _lvlText.text = "Lvl " + allyStats.GetLevel().ToString();
        _vitalityNums.text = allyStats.GetVitality().ToString();
        _magicNums.text = allyStats.GetMagic().ToString();
        _strNums.text = allyStats.GetStrength().ToString();
        _speedNums.text = allyStats.GetSpeed().ToString();

        // Fill in the appropriate amount of bubbles for each stat
        //Debug.Log("Updating Vitality Bubbles");
        UpdateBubbles(_vitalityBubbles, allyStats.VitalityBubblesFilled);
        //Debug.Log("Updating Magic Bubbles");
        UpdateBubbles(_magicBubbles, allyStats.MagicBubblesFilled);
        //Debug.Log("Updating Strength Bubbles");
        UpdateBubbles(_strBubbles, allyStats.StrBubblesFilled);
        //Debug.Log("Updating Speed Bubbles");
        UpdateBubbles(_speedBubbles, allyStats.SpeedBubblesFilled);

        // Do this just in case
        ResetStatChoices();

        // Deactive the unapplied changes screen in case it was active when we quit
        _unappliedChangesPrompt.SetActive(false);

        // Test if the character has points to spend, if they do we set a bunch of stuff active, if they don't we turn off a bunch of stuff
        _amountPointsAvailable = allyStats.AmountStatIncreases;
        bool arePointsAvailable = _amountPointsAvailable > 0;
        // Set things active or inactive
        LevelUpUISetActive(arePointsAvailable);

        // Update the skill preview
        _skillPrevContRef.DisplayUpdatePreviewMenu(allyStats);
    }

    /// <summary>
    /// Called from the next ally button, it changes the display to show the next ally
    /// </summary>
    public void DisplayNextAlly()
    {
        int allyIndex = _currentAllyIndex + 1;
        if (allyIndex > 2)
            allyIndex = 0;
        DisplayCharacterDetails(allyIndex);
    }

    /// <summary>
    /// Called from the prev ally button, it changes the display to show the prev ally
    /// </summary>
    public void DisplayPrevAlly()
    {
        int allyIndex = _currentAllyIndex - 1;
        if (allyIndex < 0)
            allyIndex = 2;
        DisplayCharacterDetails(allyIndex);
    }

    // The following functions are called from the increment stats buttons
    //
    /// <summary>
    /// Gives a plus 1 to vitality temporarily
    /// </summary>
    public void IncrementVitality()
    {
        // For quick reference
        AllyStats allyStats = _alliesStats[_currentAllyIndex];
        // Increase the stat
        IncrementStat(ref _vitalityAmountIncr, ref _vitalityNums, allyStats.GetVitality(), allyStats.VitalityBubblesFilled, _vitalityBubbles);
    }
    /// <summary>
    /// Gives a plus 1 to magic temporarily
    /// </summary>
    public void IncrementMagic()
    {
        // For quick reference
        AllyStats allyStats = _alliesStats[_currentAllyIndex];
        // Increase the stat
        IncrementStat(ref _magicAmountIncr, ref _magicNums, allyStats.GetMagic(), allyStats.MagicBubblesFilled, _magicBubbles);

        // Refelect temp magic changes in preview text
        if (_magicAmountIncr + allyStats.MagicBubblesFilled >= _magicBubbles.Count + 1)
        {
            // Get the grimoire of the current ally
            Grimoire allyGrim = allyStats.GetComponent<Grimoire>();
            // Preview the skill increase
            _skillPrevContRef.PreviewSkillUpgrade(allyGrim, Mathf.FloorToInt((_magicAmountIncr + allyStats.MagicBubblesFilled + 0.0f) / _magicBubbles.Count));
        }

    }
    /// <summary>
    /// Gives a plus 1 to strength temporarily
    /// </summary>
    public void IncrementStrength()
    {
        // For quick reference
        AllyStats allyStats = _alliesStats[_currentAllyIndex];
        // Increase the stat
        IncrementStat(ref _strAmountIncr, ref _strNums, allyStats.GetStrength(), allyStats.StrBubblesFilled, _strBubbles);
    }
    /// <summary>
    /// Gives a plus 1 to speed temporarily
    /// </summary>
    public void IncrementSpeed()
    {
        // For quick reference
        AllyStats allyStats = _alliesStats[_currentAllyIndex];
        // Check if there is the ASCII version of the max speed displayed, if ther is, don't increase it anymore
        if (_speedNums.text[0] != allyStats.MaxSpeed + 48)
        {
            // Increase the stat
            IncrementStat(ref _speedAmountIncr, ref _speedNums, allyStats.GetSpeed(), allyStats.SpeedBubblesFilled, _speedBubbles);
        }
        else
        {
            _speedNums.text = _speedNums.text[0] + " MAX";
        }
    }
    /// <summary>
    /// Increases the desired stat temporarily
    /// </summary>
    /// <param name="statIncrement">The statAmountIncr to increase</param>
    /// <param name="statNumsText">The statNums Text that will have to be updated after the increase</param>
    /// <param name="currentStatValue">The current value of the stat the ally currently has</param>
    /// <param name="currentBubbleValue">The current amount of bubbles the ally has filled</param>
    /// <param name="bubbles">The list of bubbles we need to update</param>
    private void IncrementStat(ref int statIncrement, ref Text statNumsText, int currentStatValue, int currentBubbleAmount, List<Image> bubbles)
    {
        int amountPointsSpent = _vitalityAmountIncr + _magicAmountIncr + _strAmountIncr + _speedAmountIncr;

        // Make sure there are more points to give
        if (_amountPointsAvailable > amountPointsSpent)
        {
            // Increase the stat by 1 and display the change
            ++statIncrement;

            // Update the bubbles
            //Debug.Log(currentBubbleAmount + " bubbles are currently filled in");
            //Debug.Log(statIncrement + " is the current statIncrement");
            //Debug.Log((currentBubbleAmount + statIncrement) % (bubbles.Count + 1) + " should be filled");
            UpdateBubbles(bubbles, (currentBubbleAmount + statIncrement) % (bubbles.Count + 1));
            // Check if this increment makes us reach the desired amount
            // If it does, we need to update the text
            if (statIncrement + currentBubbleAmount >= bubbles.Count + 1)
            {
                //Debug.Log("Updating text to " + (currentStatValue + ((currentBubbleAmount + statIncrement) / (bubbles.Count + 1))).ToString());
                statNumsText.text = (currentStatValue + ((currentBubbleAmount + statIncrement) / (bubbles.Count + 1))).ToString();
            }

            // Increase the amount of points we spent and display the change
            ++amountPointsSpent;
            _pointsText.text = "Point: " + (_amountPointsAvailable - amountPointsSpent).ToString();
        }
    }

    /// <summary>
    /// Called by the reset button. Gives the player back their stat points
    /// </summary>
    public void ResetStatChoices()
    {
        // For quick reference
        AllyStats allyStats = null;
        if (_alliesStats.Count > _currentAllyIndex && _alliesStats[_currentAllyIndex] != null)
            allyStats = _alliesStats[_currentAllyIndex];
        else
            allyStats = _deadAlly;

        // Give back the points and update that visual
        _amountPointsAvailable = allyStats.AmountStatIncreases;
        _pointsText.text = "Point: " + allyStats.AmountStatIncreases.ToString();
        // Reset all the increments to 0 and update their visuals
        _vitalityAmountIncr = 0;
        _vitalityNums.text = allyStats.GetVitality().ToString();
        _magicAmountIncr = 0;
        _magicNums.text = allyStats.GetMagic().ToString();
        _strAmountIncr = 0;
        _strNums.text = allyStats.GetStrength().ToString();
        _speedAmountIncr = 0;
        _speedNums.text = allyStats.GetSpeed().ToString();
        // Reset the bubbles
        UpdateBubbles(_vitalityBubbles, allyStats.VitalityBubblesFilled);
        UpdateBubbles(_magicBubbles, allyStats.MagicBubblesFilled);
        UpdateBubbles(_strBubbles, allyStats.StrBubblesFilled);
        UpdateBubbles(_speedBubbles, allyStats.SpeedBubblesFilled);

        // Update the skill preview
        _skillPrevContRef.DisplayUpdatePreviewMenu(allyStats);
    }

    /// <summary>
    /// Called from Confirm button. Confirms the applied stat changes.
    /// </summary>
    public void ConfirmStatChoices()
    {
        int statPointsUsed = _vitalityAmountIncr + _magicAmountIncr + _strAmountIncr + _speedAmountIncr;

        // If they have made no changes, don't do anything
        if (statPointsUsed == 0)
        {
            //Debug.Log("No changes have been made");
            return;
        }
        // If they have made changes, but haven't used all their stats
        else if (statPointsUsed < _amountPointsAvailable)
        {
            //prompt the user to let them know they have unapplied stats and would like to apply the current ones anyway
            //Debug.Log("There are unused changes");
            _unappliedChangesPrompt.SetActive(true);
        }
        // If they have made changes and used all their stats
        else if (statPointsUsed == _amountPointsAvailable)
        {
            //Debug.Log("Applying changes");
            // For quick reference
            AllyStats allyStats = _alliesStats[_currentAllyIndex];

            // Apply the temporary stats for real
            ApplyStatChanges();

            // We want to hide all the things we had displayed for increasing stats
            LevelUpUISetActive(false);

            // Hide the little level up indicator, since the character can no longer gain more stats
            allyStats.HideLevelUpButton();
        }
        // Any other scenario is an error
        else
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "in ConfirmStatChoices. The user either tried to apply negative stats, or tried to apply more stats than possible." +
                " Double click this for more context");
        }
    }

    /// <summary>
    /// Sets the things for levelling up on or off
    /// </summary>
    /// <param name="onOff">Whether to turn things on or off</param>
    private void LevelUpUISetActive(bool onOff)
    {
        _pointsText.gameObject.SetActive(onOff);
        if (onOff)
            _pointsText.text = "Points: " + _amountPointsAvailable;
        _vitalityIncrButt.gameObject.SetActive(onOff);
        _magicIncrButt.gameObject.SetActive(onOff);
        _strIncrButt.gameObject.SetActive(onOff);
        _speedIncrButt.gameObject.SetActive(onOff);
        _resetButt.gameObject.SetActive(onOff);
        _confirmButt.gameObject.SetActive(onOff);
    }

    /// <summary>
    /// Correctly emptys and fills in bubbles contained in the passed in list
    /// </summary>
    /// <param name="bubbles">List<Image> that contains the bubbles to be updated</param>
    /// <param name="amountToBeFilled">int that is the amount of bubbles that must be filled in the list</param>
    private void UpdateBubbles(List<Image> bubbles, int amountToBeFilled)
    {
        //Debug.Log("Amount to be filed " + amountToBeFilled);
        // Fill in the appropriate amount of bubbles
        for (int i = 0; i < bubbles.Count; ++i)
        {
            // If the bubble is to be filled, filled it
            if (i < amountToBeFilled)
            {
                //Debug.Log("Filling bubble");
                bubbles[i].sprite = _fullBubbleSpr;
            }
            // If it is not supposed to be filled, empty it
            else
            {
                //Debug.Log("Emptying bubble");
                bubbles[i].sprite = _emptyBubbleSpr;
            }
        }
    }

    /// <summary>
    /// Applies the temporaray stats for real. Called from ConfirmStatChoices and by the Yes button with unapplied changes
    /// </summary>
    public void ApplyStatChanges()
    {
        // For quick reference
        AllyStats allyStats = _alliesStats[_currentAllyIndex];

        int statPointsUsed = _vitalityAmountIncr + _magicAmountIncr + _strAmountIncr + _speedAmountIncr;

        // Vitality
        int currentVitBubbleIncrease = _vitalityAmountIncr + allyStats.VitalityBubblesFilled;
        int amountVitForIncrease = _vitalityBubbles.Count + 1;
        allyStats.ChangeVitality(currentVitBubbleIncrease / amountVitForIncrease);
        allyStats.VitalityBubblesFilled = currentVitBubbleIncrease % amountVitForIncrease;
        // Magic
        int currentMagBubbleIncrease = _magicAmountIncr + allyStats.MagicBubblesFilled;
        int amountMagForIncrease = _magicBubbles.Count + 1;
        allyStats.ChangeMagic(currentMagBubbleIncrease / amountMagForIncrease);
        allyStats.MagicBubblesFilled = currentMagBubbleIncrease % amountMagForIncrease;
        // Strength
        int currentStrBubbleIncrease = _strAmountIncr + allyStats.StrBubblesFilled;
        int amountStrForIncrease = _strBubbles.Count + 1;
        allyStats.ChangeStrength(currentStrBubbleIncrease / amountStrForIncrease);
        allyStats.StrBubblesFilled = currentStrBubbleIncrease % amountStrForIncrease;
        // Speed
        int currentSpdBubbleIncrease = _speedAmountIncr + allyStats.SpeedBubblesFilled;
        int amountSpdForIncrease = _speedBubbles.Count + 1;
        allyStats.ChangeSpeed(currentSpdBubbleIncrease / amountSpdForIncrease);
        allyStats.SpeedBubblesFilled = currentSpdBubbleIncrease % amountSpdForIncrease;

        // Reset the stats for if we can increase things
        allyStats.AmountStatIncreases -= statPointsUsed;

        // Reset all the variables
        ResetStatChoices();
    }
}
