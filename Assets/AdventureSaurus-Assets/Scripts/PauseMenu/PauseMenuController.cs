﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    // References to the different menus
    [SerializeField] private GameObject _mapMenuObj = null;
    [SerializeField] private GameObject _teamMenuObj = null;
    [SerializeField] private GameObject _inventoryMenuObj = null;
    [SerializeField] private GameObject _charDetailedMenuObj = null;

    // References for the team menu
    // Parent of all allies
    private Transform _allyParent;
    // Parent of all enemies
    private Transform _enemyParent;
    // The text that displays the character's names
    [SerializeField] private Text[] _alliesNameText = null;
    // The stats of each of the allies [0] = ally1, [1] = ally2, [2] = ally3
    private List<AllyStats> _alliesStats;
    // The experience bars of each of the allies [0] = ally1, [1] = ally2, [2] = ally3
    [SerializeField] private Slider[] _allyXPBars = null;
    // The portraits of each of the allies [0] = ally1, [1] = ally2, [2] = ally3
    [SerializeField] private Image[] _allyPortraits = null;

    // References for the side HUD
    // Portraits on the side
    [SerializeField] private Image[] _sidePortraits = null;
    // Health bars on the side
    [SerializeField] private Slider[] _sideHPBars = null;
    // Exp bars on the side
    [SerializeField] private Slider[] _sideExpBars = null;

    // Name of the title screen
    [SerializeField] private string _titleSceneName = "Title Screen";

    // Portrait of a deceased character
    [SerializeField] private Sprite _deceasedPortrait = null;
    // Things that need to be turned on/off for alive/dead characters
    [SerializeField] private GameObject[] _expBorders = null;
    // Side Portrait of a deceased character
    [SerializeField] private Sprite _sidePortraitDeceased = null;

    // Events
    // For when the character detailed menu is shown
    public delegate void CharDetailedMenuShown(int index);
    public static event CharDetailedMenuShown OnCharDetailedMenuShown;
    // For when the game is quit
    public delegate void QuitGame();
    public static event QuitGame OnQuitGame;

    // Called when the script is toggled active
    // Subscribe to events
    private void OnEnable()
    {
        // When the game pauses, update the visualization of the character stats
        Pause.OnPauseGame += UpdateValues;
        // When generation is finished, initialize this script
        ProceduralGenerationController.OnFinishGenerationNoParam += Initialize;
        // When an ally dies, re set the side info 
        Health.OnCharacterDeath += SetSideInfo;
    }

    // Called when the script is toggled off
    // Unsubscribe from events
    private void OnDisable()
    {
        Pause.OnPauseGame -= UpdateValues;
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
       Health.OnCharacterDeath -= SetSideInfo;
    }

    // Called when the gameobject is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        Pause.OnPauseGame -= UpdateValues;
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
        Health.OnCharacterDeath -= SetSideInfo;
    }

    // Start is called before the first frame update
    void Start()
    {
        // We're going to do some validation here
        // Menus validation
        if (_mapMenuObj == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "mapMenuObj was not initialized properly, please set it in the editor");
        }
        if (_teamMenuObj == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "teamMenuObj was not initialized properly, please set it in the editor");
        }
        if (_inventoryMenuObj == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "inventoryMenuObj was not initialized properly, please set it in the editor");
        }
        if (_charDetailedMenuObj == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "charDetailedMenuObj was not initialized properly, please set it in the editor");
        }

        // Ally Name Text validation
        if (_alliesNameText == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "alliesNameText was not initialized properly, please set it in the editor");
        }
        else if (_alliesNameText.Length != 3)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "There should be 3 elements in alliesNameText");
        }


        // Experience Bars validation
        if (_allyXPBars == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "allyXPBars was not initialized properly, please set it in the editor");
        }
        else if (_allyXPBars.Length != 3)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "There should be 3 elements in allyXPBars");
        }

        // Portrait validation
        if (_allyPortraits == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "allyPortraits was not initialized properly, please set it in the editor");
        }
        else if (_allyPortraits.Length != 3)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "There should be 3 elements in allyPortraits");
        }

        // Side HUD validation
        // Side portrait validation
        if (_sidePortraits == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "sidePortraits was not initialized properly, please set it in the editor");
        }
        else if (_sidePortraits.Length != 3)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "There should be 3 elements in sidePortraits");
        }
        // Side hp bars validation
        if (_sideHPBars == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "sideHPBars was not initialized properly, please set it in the editor");
        }
        else if (_sideHPBars.Length != 3)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "There should be 3 elements in sideHPBars");
        }
        // Side exp bars validation
        if (_sideExpBars == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "sideExpBars was not initialized properly, please set it in the editor");
        }
        else if (_sideExpBars.Length != 3)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "There should be 3 elements in sideExpBars");
        }
    }

    /// <summary>
    /// Initializes things for this script.
    /// Called from the FinishGenerating event
    /// </summary>
    private void Initialize()
    {
        // Set the character parent
        _allyParent = GameObject.Find(ProceduralGenerationController.ALLY_PARENT_NAME).transform;

        // Set the character side picture for each ally
        // Also give the character's health script the side slider
        SetSideInfo();

        // Ally Stats validation
        if (_alliesStats == null)
        {
            Debug.Log("ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
                "alliesStats was not initialized properly");
        }
        //else if (_alliesStats.Count != 3)
        //{
        //    Debug.Log("POTENTIAL ERROR WARNING - from PauseMenuController attached to " + this.name + ". " +
        //        "3 allies were not found. Instead " + _alliesStats.Count + " allies were found.");
        //}
    }

    /// <summary>
    /// Turns off the other menus and turns on the map menu
    /// </summary>
    public void ShowMapMenu()
    {
        // Turn off other menus, turn on this one
        _mapMenuObj.SetActive(true);
        _teamMenuObj.SetActive(false);
        _inventoryMenuObj.SetActive(false);
        _charDetailedMenuObj.SetActive(false);
    }

    /// <summary>
    /// Turns off the other menus and turns on the team menu
    /// </summary>
    public void ShowTeamMenu()
    {
        // Turn off other menus, turn on this one
        _mapMenuObj.SetActive(false);
        _teamMenuObj.SetActive(true);
        _inventoryMenuObj.SetActive(false);
        _charDetailedMenuObj.SetActive(false);

        UpdateValues();
    }

    /// <summary>
    /// Turns off the other menus and turns on the inventory menu
    /// </summary>
    public void ShowInventoryMenu()
    {
        // Turn off other menus, turn on this one
        _mapMenuObj.SetActive(false);
        _teamMenuObj.SetActive(false);
        _inventoryMenuObj.SetActive(true);
        _charDetailedMenuObj.SetActive(false);
    }

    /// <summary>
    /// Turns off the other menus and turns on the character detailed menu
    /// </summary>
    /// <param name="allyIndex">The index of the ally we want to show</param>
    public void ShowCharDetailedMenu(int allyIndex)
    {
        // Turn off other menus, turn on this one
        _mapMenuObj.SetActive(false);
        _teamMenuObj.SetActive(false);
        _inventoryMenuObj.SetActive(false);
        _charDetailedMenuObj.SetActive(true);

        // Call the event for showing the character detailed menu
        if (OnCharDetailedMenuShown != null)
            OnCharDetailedMenuShown(allyIndex);
    }

    /// <summary>
    /// Turns off the other menus and turns on the character detailed menu
    /// Overloaded to take allyStats so that pressing the levelUp button takes us to the correct character
    /// </summary>
    /// <param name="allyStats">The stats of the ally we want to show</param>
    private void ShowCharDetailedMenu(Stats allyStats)
    {
        // Convert the allyStats to an index and pass that index into the overloaded function
        for (int i = 0; i < _alliesStats.Count; ++i)
        {
            if (_alliesStats[i] == allyStats)
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
            if (i < _alliesStats.Count && _alliesStats[i] != null)
            {
                // Update the names of the allies
                _alliesNameText[i].text = _alliesStats[i].CharacterName;
                // Set the sliders to be the allies current xp;
                _allyXPBars[i].value = ((float)_alliesStats[i].GetOneLevelExperience()) / _alliesStats[i].GetOneLevelNextLevelThreshold();
                // Update the portrait of the allies
                _allyPortraits[i].sprite = _alliesStats[i].GetCharacterPortrait();

                // Turn on stuff that was off when a character was dead
                TurnOnOffForAliveCharacter(true, i);
            }
            // If that ally doesn't exist, they dead. Reflect that
            else
            {
                // Deceased ally
                _alliesNameText[i].text = "Deceased";
                // No xp for deceased allies
                _allyXPBars[i].value = 0;
                // Set the sprite to the deceased sprite
                _allyPortraits[i].sprite = _deceasedPortrait;

                // Turn off stuff that was on when a character was alive
                TurnOnOffForAliveCharacter(false, i);
            }
        }
    }


    /// <summary>
    /// Loads the title scene.
    /// Called from the exit game button in the pause menu
    /// </summary>
    public void LoadTitleScene()
    {
        // Call the quit game event
        if (OnQuitGame != null)
            OnQuitGame();

        // Load the main menu
        SceneManager.LoadScene(_titleSceneName);
    }

    /// <summary>
    /// Turns the stuff a character should see on or off depending on if they are dead
    /// </summary>
    /// <param name="onOff">True - character is alive. False - character is dead</param>
    private void TurnOnOffForAliveCharacter(bool onOff, int charIndex)
    {
        // Turn on if alive
        _allyXPBars[charIndex].gameObject.SetActive(onOff);
        _expBorders[charIndex].gameObject.SetActive(onOff);
    }

    /// <summary>
    /// Sets the side portraits, hp, and exp for the characters
    /// </summary>
    private void SetSideInfo()
    {
        // We make the list here so that other scripts can access it in Start
        _alliesStats = new List<AllyStats>();
        // Get the allies Stats (assumes there are three)
        foreach (Transform allyTrans in _allyParent)
        {
            AllyStats allyStatsRef = allyTrans.GetComponent<AllyStats>();
            if (allyStatsRef != null)
                _alliesStats.Add(allyStatsRef);
            else
                Debug.LogError("No AllyStats attached to " + _allyParent);
        }

        // Set the character side picture for each ally
        // Also give the character's health script the side slider
        for (int i = 0; i < _sidePortraits.Length; ++i)
        {
            // If there is an ally at this point in the list
            if (i < _alliesStats.Count && _alliesStats[i] != null && _alliesStats[i].transform.parent == _allyParent.transform)
            {
                // Change the side picture
                _sidePortraits[i].sprite = _alliesStats[i].GetSideSprite();
                // Set the side health bar
                AllyHealth hpScriptRef = _alliesStats[i].GetComponent<AllyHealth>();
                hpScriptRef.SideSlider = _sideHPBars[i];
                _sideHPBars[i].value = (float)hpScriptRef.CurHP / hpScriptRef.MaxHP;
                // Set the side exp bar
                _alliesStats[i].ExpSlider = _sideExpBars[i];
                _sideExpBars[i].value = (float)_alliesStats[i].GetOneLevelExperience() / _alliesStats[i].GetOneLevelNextLevelThreshold();
                _alliesStats[i].UpdateExpSlider();
            }
            // If there is not ally at this point, they dead
            else
            {
                // Change the side picture
                _sidePortraits[i].sprite = _sidePortraitDeceased;
                // Change the sliders to be 0
                _sideHPBars[i].value = 0;
                _sideExpBars[i].value = 0;
            }
        }
    }
}
