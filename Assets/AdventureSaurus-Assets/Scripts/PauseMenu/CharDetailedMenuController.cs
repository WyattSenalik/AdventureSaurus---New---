using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharDetailedMenuController : MonoBehaviour
{
    // References
    private PauseMenuController pauseMenuContRef;

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
    // These are set true or false depending if there are points to spend for the current character
    [SerializeField] private Text pointsText = null;
    [SerializeField] private Button vitalityIncrButt = null;
    [SerializeField] private Button magicIncrButt = null;
    [SerializeField] private Button strIncrButt = null;
    [SerializeField] private Button speedIncrButt = null;
    [SerializeField] private Button resetButt = null;
    [SerializeField] private Button confirmButt = null;

    // For keeping track of how many points the player is putting into each stat upon a level up
    private int amountPointsAvailable;
    private int vitalityAmountIncr;
    private int magicAmountIncr;
    private int strAmountIncr;
    private int speedAmountIncr;

    // A dead character's stats. Okay, so if an ally dies, we can't display their stats because they're dead.
    // But we need to display something in their place, so we display a "dead" character's stats
    private Stats deadAlly;

    // Set references
    private void Awake()
    {
        GameObject gameController = GameObject.FindWithTag("GameController");
        // Make sure a GameController exists
        if (gameController == null)
            Debug.Log("Could not find any GameObject with the tag GameController");
        else
        {
            pauseMenuContRef = gameController.GetComponent<PauseMenuController>();
            if (pauseMenuContRef == null)
            {
                Debug.Log("Could not find PauseMenuController attached to " + gameController.name);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get the alliesStats
        alliesStats = pauseMenuContRef.AlliesStats;
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
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "characterPortrait was not initialized properly, please set it in the editor");
        }
        if (nameText == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "nameText was not initialized properly, please set it in the editor");
        }
        if (lvlText == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "lvlText was not initialized properly, please set it in the editor");
        }
        if (vitalityNums == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "vitalityNums was not initialized properly, please set it in the editor");
        }
        if (magicNums == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "magicNums was not initialized properly, please set it in the editor");
        }
        if (strNums == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "strNums was not initialized properly, please set it in the editor");
        }
        if (speedNums == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "speedNums was not initialized properly, please set it in the editor");
        }
        if (pointsText == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "pointsText was not initialized properly, please set it in the editor");
        }
        if (vitalityIncrButt == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "vitalityIncrButt was not initialized properly, please set it in the editor");
        }
        if (magicIncrButt == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "magicIncrButt was not initialized properly, please set it in the editor");
        }
        if (strIncrButt == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "strIncrButt was not initialized properly, please set it in the editor");
        }
        if (speedIncrButt == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "speedIncrButt was not initialized properly, please set it in the editor");
        }
        if (resetButt == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "resetButt was not initialized properly, please set it in the editor");
        }
        if (confirmButt == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "confirmButt was not initialized properly, please set it in the editor");
        }
    }

    /// <summary>
    /// Shows the details of the ally with the given index
    /// </summary>
    /// <param name="allyIndex">Index of the ally to be displayed</param>
    public void DisplayCharacterDetails(int allyIndex)
    {
        currentAllyIndex = allyIndex; // This is the new ally we are viewing

        Stats allyStats = null;
        // Check the index is valid and that the value is not null
        if (alliesStats.Count > allyIndex && alliesStats[allyIndex] != null)
            allyStats = alliesStats[allyIndex]; // For quick reference
        // If the index is invalid, we make allyStats be a dead ally
        else
            allyStats = deadAlly;

        // Set the normal things for the character stats
        ////TODO Set the characterPortrait
        nameText.text = allyStats.CharacterName;
        lvlText.text = "Lvl " + allyStats.Level.ToString();
        vitalityNums.text = allyStats.Vitality.ToString();
        magicNums.text = allyStats.Magic.ToString();
        strNums.text = allyStats.Strength.ToString();
        speedNums.text = allyStats.Speed.ToString();

        // Do this just in case
        ResetStatChoices();

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
        IncrementStat(ref vitalityAmountIncr, ref vitalityNums, allyStats.Vitality);
    }
    /// <summary>
    /// Gives a plus 1 to magic temporarily
    /// </summary>
    public void IncrementMagic()
    {
        Stats allyStats = alliesStats[currentAllyIndex]; // For quick reference
        // Increase the stat
        IncrementStat(ref magicAmountIncr, ref magicNums, allyStats.Magic);
    }
    /// <summary>
    /// Gives a plus 1 to strength temporarily
    /// </summary>
    public void IncrementStrength()
    {
        Stats allyStats = alliesStats[currentAllyIndex]; // For quick reference
        // Increase the stat
        IncrementStat(ref strAmountIncr, ref strNums, allyStats.Strength);
    }
    /// <summary>
    /// Gives a plus 1 to speed temporarily
    /// </summary>
    public void IncrementSpeed()
    {
        Stats allyStats = alliesStats[currentAllyIndex]; // For quick reference
        if (allyStats.Speed + speedAmountIncr < allyStats.MaxSpeed)
        {
            // Increase the stat
            IncrementStat(ref speedAmountIncr, ref speedNums, allyStats.Speed);
        }
    }
    /// <summary>
    /// Increases the desired stat temporarily
    /// </summary>
    /// <param name="statIncrement">The statAmountIncr to increase</param>
    /// <param name="statNumsText">The statNums Text that will have to be updated after the increase</param>
    /// <param name="currentStatValue">The current value of the stat the ally currently has</param>
    private void IncrementStat(ref int statIncrement, ref Text statNumsText, int currentStatValue)
    {
        int amountPointsSpent = vitalityAmountIncr + magicAmountIncr + strAmountIncr + speedAmountIncr;

        // Make sure there are more points to give
        if (amountPointsAvailable > amountPointsSpent)
        {
            // Increase vitality by 1 and display the change
            ++statIncrement;
            statNumsText.text = (currentStatValue + statIncrement).ToString();
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
        Stats allyStats = alliesStats[currentAllyIndex]; // For quick reference
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
    }

    /// <summary>
    /// Called from Confirm button. Confirms the applied stat changes.
    /// </summary>
    public void ConfirmStatChoices()
    {
        int statPointsUsed = vitalityAmountIncr + magicAmountIncr + speedAmountIncr;
        // If they have made no changes, don't do anything
        if (statPointsUsed == 0)
            return;
        // If they have made changes, but haven't used all their stats
        else if (statPointsUsed < amountPointsAvailable)
        {
            ////TODO prompt the user to let them know they have unapplied stats and would like to apply the current ones anyway
        }
        // If they have made changes and used all their stats
        else if (statPointsUsed == amountPointsAvailable)
        {
            Stats allyStats = alliesStats[currentAllyIndex]; // For quick reference

            // Apply the temporary stats for real
            allyStats.IncreaseVitality(vitalityAmountIncr);
            allyStats.IncreaseMagic(magicAmountIncr);
            allyStats.IncreaseStrength(strAmountIncr);
            allyStats.IncreaseSpeed(speedAmountIncr);

            // We want to hide all the things we had displayed for increasing stats
            LevelUpUISetActive(false);

            // Reset the stats for if we can increase things
            allyStats.AmountStatIncreases = 0;

            // Hide the little level up indicator, since the character can no longer gain more stats
            allyStats.HideLevelUpButton();
        }
        // Any other scenario is an error
        else
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
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
}
