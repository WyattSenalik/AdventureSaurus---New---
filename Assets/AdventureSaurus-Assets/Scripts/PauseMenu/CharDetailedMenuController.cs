using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharDetailedMenuController : MonoBehaviour
{
    // The stats of all 3 character. [0] = Ally 1, [1] = Ally 2, [2] = Ally 3 (this is true for all the ally lists)
    private List<Stats> alliesStats; // We are going to steal this list from PauseMenuController
    private int currentAllyIndex; // The ally we are currently viewing by index

    // Canvas display things
    //
    // These things will all get swapped when we hit the next and previous character buttons
    [SerializeField] private Image characterPortrait = null;
    [SerializeField] private Text nameText = null;
    [SerializeField] private Text lvlText = null;
    [SerializeField] private Text vitalityNums = null;
    [SerializeField] private Text magicNums = null;
    [SerializeField] private Text strNums = null;
    [SerializeField] private Text speedNums = null;
    // Bubbles stuff. The bubbles are children of these transforms
    [SerializeField] private Transform vitalityBubblesParent = null;
    [SerializeField] private Transform magicBubblesParent = null;
    [SerializeField] private Transform strBubblesParent = null;
    [SerializeField] private Transform speedBubblesParent = null;
    [SerializeField] private Sprite emptyBubbleSpr = null;
    [SerializeField] private Sprite fullBubbleSpr = null;
    // These are set true or false depending if there are points to spend for the current character
    [SerializeField] private Text pointsText = null;
    [SerializeField] private Button vitalityIncrButt = null;
    [SerializeField] private Button magicIncrButt = null;
    [SerializeField] private Button strIncrButt = null;
    [SerializeField] private Button speedIncrButt = null;
    [SerializeField] private Button resetButt = null;
    [SerializeField] private Button confirmButt = null;

    // The level up buttons (for allies 1, 2, and 3 in order)
    [SerializeField] private GameObject[] levelUpButtons = null;

    // For keeping track of how many points the player is putting into each stat upon a level up
    private int amountPointsAvailable;
    private int vitalityAmountIncr;
    private int magicAmountIncr;
    private int strAmountIncr;
    private int speedAmountIncr;

    // For holding the bubbles' images
    private List<Image> vitalityBubbles;
    private List<Image> magicBubbles;
    private List<Image> strBubbles;
    private List<Image> speedBubbles;

    // A dead character's stats. Okay, so if an ally dies, we can't display their stats because they're dead.
    // But we need to display something in their place, so we display a "dead" character's stats
    private Stats deadAlly;

    // This is shown when the user tries to submit their changes without using all their points
    [SerializeField] private GameObject unappliedChangesPrompt = null;


    // Called when the gameobject is toggled active
    // Subscribe to events
    private void OnEnable()
    {
        // When the character clicks to display the charcter detailed menu, update the details and diaply them
        PauseMenuController.OnCharDetailedMenuShown += DisplayCharacterDetails;
    }

    // Called when the gameobject is toggled active
    // Unsubscribe form events
    private void OnDisable()
    {
        // When the character clicks to display the charcter detailed menu, update the details and diaply them
        PauseMenuController.OnCharDetailedMenuShown -= DisplayCharacterDetails;
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Make it so we are viewing the first enemy at first
        currentAllyIndex = 0;

        // Set the amount the player is increasing to zero right off the bat since they aren't increasing anything yet
        amountPointsAvailable = 0;
        vitalityAmountIncr = 0;
        magicAmountIncr = 0;
        strAmountIncr = 0;
        speedAmountIncr = 0;

        // Make the dead ally
        GameObject deadAllyObj = new GameObject("DeadAllyStats");
        deadAlly = deadAllyObj.AddComponent<Stats>();

        // Do a lot of validation
        if (characterPortrait == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "characterPortrait was not initialized properly, please set it in the editor");
        }
        if (nameText == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "nameText was not initialized properly, please set it in the editor");
        }
        if (lvlText == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "lvlText was not initialized properly, please set it in the editor");
        }
        if (vitalityNums == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "vitalityNums was not initialized properly, please set it in the editor");
        }
        if (magicNums == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "magicNums was not initialized properly, please set it in the editor");
        }
        if (strNums == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "strNums was not initialized properly, please set it in the editor");
        }
        if (speedNums == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "speedNums was not initialized properly, please set it in the editor");
        }
        if (vitalityBubblesParent == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "vitalityBubblesParent was not initialized properly, please set it in the editor");
        }
        if (magicBubblesParent == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "magicBubblesParent was not initialized properly, please set it in the editor");
        }
        if (strBubblesParent == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "strBubblesParent was not initialized properly, please set it in the editor");
        }
        if (speedBubblesParent == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "speedBubblesParent was not initialized properly, please set it in the editor");
        }
        if (emptyBubbleSpr == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "emptyBubbleSpr was not initialized properly, please set it in the editor");
        }
        if (fullBubbleSpr == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "fullBubbleSpr was not initialized properly, please set it in the editor");
        }
        if (pointsText == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "pointsText was not initialized properly, please set it in the editor");
        }
        if (vitalityIncrButt == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "vitalityIncrButt was not initialized properly, please set it in the editor");
        }
        if (magicIncrButt == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "magicIncrButt was not initialized properly, please set it in the editor");
        }
        if (strIncrButt == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "strIncrButt was not initialized properly, please set it in the editor");
        }
        if (speedIncrButt == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "speedIncrButt was not initialized properly, please set it in the editor");
        }
        if (resetButt == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "resetButt was not initialized properly, please set it in the editor");
        }
        if (confirmButt == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "confirmButt was not initialized properly, please set it in the editor");
        }
        if (levelUpButtons == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "levelUpButtons was not initialized properly, please set it in the editor");
        }
        if (levelUpButtons.Length != 3)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "levelUpButtons was not initialized properly. It must have 3 values. Please set it correctly in the editor");
        }
        if (unappliedChangesPrompt == null)
        {
            Debug.Log("ERROR WARNING - from CharDetailedMenuController attached to " + this.name + ". " +
                "unappliedChangesPrompt was not initialized properly, please set it in the editor");
        }

        // Create the lists for the bubbles
        vitalityBubbles = new List<Image>();
        magicBubbles = new List<Image>();
        strBubbles = new List<Image>();
        speedBubbles = new List<Image>();
        // Get the images on the bubbles and add them to the respective list
        foreach (Transform bubbleTrans in vitalityBubblesParent)
        {
            Image bubbleImg = bubbleTrans.GetComponent<Image>();
            vitalityBubbles.Add(bubbleImg);
        }
        foreach (Transform bubbleTrans in magicBubblesParent)
        {
            Image bubbleImg = bubbleTrans.GetComponent<Image>();
            magicBubbles.Add(bubbleImg);
        }
        foreach (Transform bubbleTrans in strBubblesParent)
        {
            Image bubbleImg = bubbleTrans.GetComponent<Image>();
            strBubbles.Add(bubbleImg);
        }
        foreach (Transform bubbleTrans in speedBubblesParent)
        {
            Image bubbleImg = bubbleTrans.GetComponent<Image>();
            speedBubbles.Add(bubbleImg);
        }
    }

    /// <summary>
    /// Called from ProceduralGenerationController.
    /// Sets the allies stats
    /// </summary>
    /// <param name="allyStats">What to set alliesStats to</param>
    public void Initialize(List<Stats> allyStats)
    {
        // Get the alliesStats
        alliesStats = allyStats;

        // Set the level up buttons for the characters
        for (int i = 0; i < alliesStats.Count; ++i)
        {
            alliesStats[i].LevelUpButton = levelUpButtons[i];
        }
    }

    /// <summary>
    /// Shows the details of the ally with the given index
    /// </summary>
    /// <param name="allyIndex">Index of the ally to be displayed</param>
    private void DisplayCharacterDetails(int allyIndex)
    {
        currentAllyIndex = allyIndex; // This is the new ally we are viewing

        Stats allyStats = null;
        // Check the index is valid and that the value is not null
        if (alliesStats.Count > allyIndex && alliesStats[allyIndex] != null)
            allyStats = alliesStats[allyIndex]; // For quick reference
        // If the index is invalid, we make allyStats be a dead ally
        else
        {
            allyStats = deadAlly;
            Debug.Log("This character is dead");
        }

        // Set the normal things for the character stats
        characterPortrait.sprite = allyStats.CharacterSprite;
        nameText.text = allyStats.CharacterName;
        lvlText.text = "Lvl " + allyStats.Level.ToString();
        vitalityNums.text = allyStats.Vitality.ToString();
        magicNums.text = allyStats.Magic.ToString();
        strNums.text = allyStats.Strength.ToString();
        speedNums.text = allyStats.Speed.ToString();

        // Fill in the appropriate amount of bubbles for each stat
        //Debug.Log("Updating Vitality Bubbles");
        UpdateBubbles(vitalityBubbles, allyStats.VitalityBubblesFilled);
        //Debug.Log("Updating Magic Bubbles");
        UpdateBubbles(magicBubbles, allyStats.MagicBubblesFilled);
        //Debug.Log("Updating Strength Bubbles");
        UpdateBubbles(strBubbles, allyStats.StrBubblesFilled);
        //Debug.Log("Updating Speed Bubbles");
        UpdateBubbles(speedBubbles, allyStats.SpeedBubblesFilled);

        // Do this just in case
        ResetStatChoices();

        // Deactive the unapplied changes screen in case it was active when we quit
        unappliedChangesPrompt.SetActive(false);

        // Test if the character has points to spend, if they do we set a bunch of stuff active, if they don't we turn off a bunch of stuff
        amountPointsAvailable = allyStats.AmountStatIncreases;
        bool arePointsAvailable = amountPointsAvailable > 0;
        // Set things active or inactive
        LevelUpUISetActive(arePointsAvailable);
    }

    /// <summary>
    /// Called from the next ally button, it changes the display to show the next ally
    /// </summary>
    public void DisplayNextAlly()
    {
        int allyIndex = currentAllyIndex + 1;
        if (allyIndex > 2)
            allyIndex = 0;
        DisplayCharacterDetails(allyIndex);
    }

    /// <summary>
    /// Called from the prev ally button, it changes the display to show the prev ally
    /// </summary>
    public void DisplayPrevAlly()
    {
        int allyIndex = currentAllyIndex - 1;
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
        Stats allyStats = alliesStats[currentAllyIndex]; // For quick reference
        // Increase the stat
        IncrementStat(ref vitalityAmountIncr, ref vitalityNums, allyStats.Vitality, allyStats.VitalityBubblesFilled, vitalityBubbles);
    }
    /// <summary>
    /// Gives a plus 1 to magic temporarily
    /// </summary>
    public void IncrementMagic()
    {
        Stats allyStats = alliesStats[currentAllyIndex]; // For quick reference
        // Increase the stat
        IncrementStat(ref magicAmountIncr, ref magicNums, allyStats.Magic, allyStats.MagicBubblesFilled, magicBubbles);
    }
    /// <summary>
    /// Gives a plus 1 to strength temporarily
    /// </summary>
    public void IncrementStrength()
    {
        Stats allyStats = alliesStats[currentAllyIndex]; // For quick reference
        // Increase the stat
        IncrementStat(ref strAmountIncr, ref strNums, allyStats.Strength, allyStats.StrBubblesFilled, strBubbles);
    }
    /// <summary>
    /// Gives a plus 1 to speed temporarily
    /// </summary>
    public void IncrementSpeed()
    {
        Stats allyStats = alliesStats[currentAllyIndex]; // For quick reference
        // Check if there is the ASCII version of the max speed displayed, if ther is, don't increase it anymore
        if (speedNums.text[0] != allyStats.MaxSpeed + 48)
        {
            // Increase the stat
            IncrementStat(ref speedAmountIncr, ref speedNums, allyStats.Speed, allyStats.SpeedBubblesFilled, speedBubbles);
        }
        else
        {
            speedNums.text = speedNums.text[0] + " MAX";
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
        int amountPointsSpent = vitalityAmountIncr + magicAmountIncr + strAmountIncr + speedAmountIncr;

        // Make sure there are more points to give
        if (amountPointsAvailable > amountPointsSpent)
        {
            // Increase the stat by 1 and display the change
            ++statIncrement;

            // Update the bubbles
            Debug.Log(currentBubbleAmount + " bubbles are currently filled in");
            Debug.Log(statIncrement + " is the current statIncrement");
            Debug.Log((currentBubbleAmount + statIncrement) % (bubbles.Count + 1) + " should be filled");
            UpdateBubbles(bubbles, (currentBubbleAmount + statIncrement) % (bubbles.Count + 1));
            // Check if this increment makes us reach the desired amount
            // If it does, we need to update the text
            if (statIncrement + currentBubbleAmount >= bubbles.Count + 1)
            {
                Debug.Log("Updating text to " + (currentStatValue + ((currentBubbleAmount + statIncrement) / (bubbles.Count + 1))).ToString());
                statNumsText.text = (currentStatValue + ((currentBubbleAmount + statIncrement) / (bubbles.Count + 1))).ToString();
            }

            // Increase the amount of points we spent and display the change
            ++amountPointsSpent;
            pointsText.text = "Point: " + (amountPointsAvailable - amountPointsSpent).ToString();
        }
    }

    /// <summary>
    /// Called by the reset button. Gives the player back their stat points
    /// </summary>
    public void ResetStatChoices()
    {
        Stats allyStats = null; // For quick reference
        if (alliesStats.Count > currentAllyIndex && alliesStats[currentAllyIndex] != null)
            allyStats = alliesStats[currentAllyIndex];
        else
            allyStats = deadAlly;

        // Give back the points and update that visual
        amountPointsAvailable = allyStats.AmountStatIncreases;
        pointsText.text = "Point: " + allyStats.AmountStatIncreases.ToString();
        // Reset all the increments to 0 and update their visuals
        vitalityAmountIncr = 0;
        vitalityNums.text = allyStats.Vitality.ToString();
        magicAmountIncr = 0;
        magicNums.text = allyStats.Magic.ToString();
        strAmountIncr = 0;
        strNums.text = allyStats.Strength.ToString();
        speedAmountIncr = 0;
        speedNums.text = allyStats.Speed.ToString();
        // Reset the bubbles
        UpdateBubbles(vitalityBubbles, allyStats.VitalityBubblesFilled);
        UpdateBubbles(magicBubbles, allyStats.MagicBubblesFilled);
        UpdateBubbles(strBubbles, allyStats.StrBubblesFilled);
        UpdateBubbles(speedBubbles, allyStats.SpeedBubblesFilled);
    }

    /// <summary>
    /// Called from Confirm button. Confirms the applied stat changes.
    /// </summary>
    public void ConfirmStatChoices()
    {
        int statPointsUsed = vitalityAmountIncr + magicAmountIncr + strAmountIncr + speedAmountIncr;

        // If they have made no changes, don't do anything
        if (statPointsUsed == 0)
        {
            //Debug.Log("No changes have been made");
            return;
        }
        // If they have made changes, but haven't used all their stats
        else if (statPointsUsed < amountPointsAvailable)
        {
            //prompt the user to let them know they have unapplied stats and would like to apply the current ones anyway
            //Debug.Log("There are unused changes");
            unappliedChangesPrompt.SetActive(true);
        }
        // If they have made changes and used all their stats
        else if (statPointsUsed == amountPointsAvailable)
        {
            //Debug.Log("Applying changes");
            Stats allyStats = alliesStats[currentAllyIndex]; // For quick reference

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
        pointsText.gameObject.SetActive(onOff);
        if (onOff)
            pointsText.text = "Points: " + amountPointsAvailable;
        vitalityIncrButt.gameObject.SetActive(onOff);
        magicIncrButt.gameObject.SetActive(onOff);
        strIncrButt.gameObject.SetActive(onOff);
        speedIncrButt.gameObject.SetActive(onOff);
        resetButt.gameObject.SetActive(onOff);
        confirmButt.gameObject.SetActive(onOff);
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
                bubbles[i].sprite = fullBubbleSpr;
            }
            // If it is not supposed to be filled, empty it
            else
            {
                //Debug.Log("Emptying bubble");
                bubbles[i].sprite = emptyBubbleSpr;
            }
        }
    }

    /// <summary>
    /// Applies the temporaray stats for real. Called from ConfirmStatChoices and by the Yes button with unapplied changes
    /// </summary>
    public void ApplyStatChanges()
    {
        Stats allyStats = alliesStats[currentAllyIndex]; // For quick reference

        int statPointsUsed = vitalityAmountIncr + magicAmountIncr + strAmountIncr + speedAmountIncr;

        // Vitality
        int currentVitBubbleIncrease = vitalityAmountIncr + allyStats.VitalityBubblesFilled;
        int amountVitForIncrease = vitalityBubbles.Count + 1;
        allyStats.IncreaseVitality(currentVitBubbleIncrease / amountVitForIncrease);
        allyStats.VitalityBubblesFilled = currentVitBubbleIncrease % amountVitForIncrease;
        // Magic
        int currentMagBubbleIncrease = magicAmountIncr + allyStats.MagicBubblesFilled;
        int amountMagForIncrease = magicBubbles.Count + 1;
        allyStats.IncreaseMagic(currentMagBubbleIncrease / amountMagForIncrease);
        allyStats.MagicBubblesFilled = currentMagBubbleIncrease % amountMagForIncrease;
        // Strength
        int currentStrBubbleIncrease = strAmountIncr + allyStats.StrBubblesFilled;
        int amountStrForIncrease = strBubbles.Count + 1;
        allyStats.IncreaseStrength(currentStrBubbleIncrease / amountStrForIncrease);
        allyStats.StrBubblesFilled = currentStrBubbleIncrease % amountStrForIncrease;
        // Speed
        int currentSpdBubbleIncrease = speedAmountIncr + allyStats.SpeedBubblesFilled;
        int amountSpdForIncrease = speedBubbles.Count + 1;
        allyStats.IncreaseSpeed(currentSpdBubbleIncrease / amountSpdForIncrease);
        allyStats.SpeedBubblesFilled = currentSpdBubbleIncrease % amountSpdForIncrease;

        // Reset the stats for if we can increase things
        allyStats.AmountStatIncreases -= statPointsUsed;

        // Reset all the variables
        ResetStatChoices();
    }
}
