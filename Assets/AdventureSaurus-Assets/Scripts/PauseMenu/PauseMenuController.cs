using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    // References to the different menus
    [SerializeField] private GameObject mapMenuObj = null;
    [SerializeField] private GameObject teamMenuObj = null;
    [SerializeField] private GameObject inventoryMenuObj = null;
    [SerializeField] private GameObject charDetailedMenuObj = null;

    // References for the team menu
    private Transform charParent = null;
    // The text that displays the character's names
    [SerializeField] private Text[] alliesNameText = null;
    // The stats of each of the allies [0] = ally1, [1] = ally2, [2] = ally3
    private List<Stats> alliesStats;
    public List<Stats> AlliesStats
    {
        get { return alliesStats; }
    }
    // The experience bars of each of the allies [0] = ally1, [1] = ally2, [2] = ally3
    [SerializeField] private Slider[] allyXPBars = null;
    // The portraits of each of the allies [0] = ally1, [1] = ally2, [2] = ally3
    [SerializeField] private Image[] allyPortraits = null;

    // Reference to CharDetailedMenuController script
    private CharDetailedMenuController charDetMenuContRef;

    // Set References
    private void Awake()
    {
        GameObject gameController = GameObject.FindWithTag("GameController");
        // Make sure a GameController exists
        if (gameController == null)
            Debug.Log("Could not find any GameObject with the tag GameController");
        else
        {
            charDetMenuContRef = gameController.GetComponent<CharDetailedMenuController>();
            if (charDetMenuContRef == null)
            {
                Debug.Log("Could not find CharDetailedMenuController attached to " + gameController.name);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // We're going to do some validation here
        // Menus validation
        if (mapMenuObj == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "mapMenuObj was not initialized properly, please set it in the editor");
        }
        if (teamMenuObj == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "teamMenuObj was not initialized properly, please set it in the editor");
        }
        if (inventoryMenuObj == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "inventoryMenuObj was not initialized properly, please set it in the editor");
        }
        if (charDetailedMenuObj == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "charDetailedMenuObj was not initialized properly, please set it in the editor");
        }

        // Ally Name Text validation
        if (alliesNameText == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "alliesNameText was not initialized properly, please set it in the editor");
        }
        else if (alliesNameText.Length != 3)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "There should be 3 elements in alliesNameText");
        }


        // Experience Bars validation
        if (allyXPBars == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "allyXPBars was not initialized properly, please set it in the editor");
        }
        else if (allyXPBars.Length != 3)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "There should be 3 elements in allyXPBars");
        }

        // Portrait validation
        if (allyPortraits == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "allyPortraits was not initialized properly, please set it in the editor");
        }
        else if (allyPortraits.Length != 3)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "There should be 3 elements in allyPortraits");
        }
    }

    /// <summary>
    /// Called from Procedural Generation after everything is created.
    /// Sets the character parent and gets the stats attached to all allies
    /// </summary>
    /// <param name="charPar">Transform that is the parent of all the characters</param>
    public void Initialize(Transform charPar)
    {
        // Set the character parent
        charParent = charPar;

        // We make the list here so that other scripts can access it in Start
        alliesStats = new List<Stats>();
        // Get the allies Stats (assumes there are three)
        foreach (Transform charTrans in charParent)
        {
            MoveAttack charMA = charTrans.GetComponent<MoveAttack>();
            if (charMA != null && charMA.WhatAmI == CharacterType.Ally)
            {
                Stats allyStatsRef = charMA.GetComponent<Stats>();
                alliesStats.Add(charMA.GetComponent<Stats>());
            }
        }

        // Character Parent validation
        if (charParent == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "charParent was not initialized properly");
        }
        // Ally Stats validation
        if (alliesStats == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "alliesStats was not initialized properly");
        }
        else if (alliesStats.Count != 3)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "3 allies were not found. Instead " + alliesStats.Count + " allies were found.");
        }
    }

    /// <summary>
    /// Turns off the other menus and turns on the map menu
    /// </summary>
    public void ShowMapMenu()
    {
        // Turn off other menus, turn on this one
        mapMenuObj.SetActive(true);
        teamMenuObj.SetActive(false);
        inventoryMenuObj.SetActive(false);
        charDetailedMenuObj.SetActive(false);
    }

    /// <summary>
    /// Turns off the other menus and turns on the team menu
    /// </summary>
    public void ShowTeamMenu()
    {
        // Turn off other menus, turn on this one
        mapMenuObj.SetActive(false);
        teamMenuObj.SetActive(true);
        inventoryMenuObj.SetActive(false);
        charDetailedMenuObj.SetActive(false);

        UpdateValues();
    }

    /// <summary>
    /// Turns off the other menus and turns on the inventory menu
    /// </summary>
    public void ShowInventoryMenu()
    {
        // Turn off other menus, turn on this one
        mapMenuObj.SetActive(false);
        teamMenuObj.SetActive(false);
        inventoryMenuObj.SetActive(true);
        charDetailedMenuObj.SetActive(false);
    }

    /// <summary>
    /// Turns off the other menus and turns on the character detailed menu
    /// </summary>
    /// <param name="allyIndex">The index of the ally we want to show</param>
    public void ShowCharDetailedMenu(int allyIndex)
    {
        // Turn off other menus, turn on this one
        mapMenuObj.SetActive(false);
        teamMenuObj.SetActive(false);
        inventoryMenuObj.SetActive(false);
        charDetailedMenuObj.SetActive(true);

        charDetMenuContRef.DisplayCharacterDetails(allyIndex);
    }

    /// <summary>
    /// Turns off the other menus and turns on the character detailed menu
    /// Overloaded to take allyStats so that pressing the levelUp button takes us to the correct character
    /// </summary>
    /// <param name="allyStats">The stats of the ally we want to show</param>
    public void ShowCharDetailedMenu(Stats allyStats)
    {
        // Convert the allyStats to an index and pass that index into the overloaded function
        for (int i = 0; i < alliesStats.Count; ++i)
        {
            if (alliesStats[i] == allyStats)
            {
                ShowCharDetailedMenu(i);
                break;
            }
        }
    }

    /// <summary>
    /// Updates the information that is about to be displayed in the menus.
    /// Called from Pause in TogglePauseMenu and called in various show menus functions in this script
    /// </summary>
    public void UpdateValues()
    {
        // Update the names and xp
        for (int i = 0; i < 3; ++i)
        {
            // If that ally exists in the list
            if (i < alliesStats.Count && alliesStats[i] != null)
            {
                // Update the names of the allies
                alliesNameText[i].text = alliesStats[i].CharacterName;
                // Set the sliders to be the allies current xp;
                allyXPBars[i].value = ((float)alliesStats[i].OneLevelExperience) / alliesStats[i].OneLevelNextLevelThreshold;
                // Update the portrait of the allies
                allyPortraits[i].sprite = alliesStats[i].CharacterSprite;
            }
            // If that ally doesn't exist, they dead. Reflect that
            else
            {
                // Update the names of the allies
                alliesNameText[i].text = "Deceased";
                // Set the sliders to be the allies current xp;
                allyXPBars[i].value = 0;
            }
        }
    }
}
