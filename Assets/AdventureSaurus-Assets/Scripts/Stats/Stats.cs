using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Stats : MonoBehaviour
{
    // Mostly for performance reasons, but there should probably be a limit
    protected const int _maxSpeed = 7;
    public int MaxSpeed
    {
        get { return _maxSpeed; }
    }

    // Name of the character
    [SerializeField] protected string _charName = "Deceased";
    public string CharacterName
    {
        get { return _charName; }
    }

    // Stats
    // How much damage this character deals
    [SerializeField] protected int _strength = 0;
    public int Strength
    {
        get { return _strength; }
    }
    // Magic level affects spells
    [SerializeField] protected int _magic = 0;
    public int Magic
    {
        get { return _magic; }
    }
    // How many tiles this character can move
    [SerializeField] protected int _speed = 0;
    public int Speed
    {
        get { return _speed; }
    }
    // How much damage this character deflects
    [SerializeField] protected int _defense = 0;
    public int Defense
    {
        get { return _defense; }
    }
    // Max health this character has
    [SerializeField] protected int _vitality = 0;
    public int Vitality
    {
        get { return _vitality; }
    }

    

    // References to itself
    // Reference to the MoveAttack script attached to this character
    protected MoveAttack _mARef;
    // Reference to the Health script attached to this character
    protected Health _hpRef;

    // Stats display stuff
    // Reference to the stats display of this character
    [SerializeField] private GameObject _statsDisplay = null;
    // Reference to the text of the stats display that shows the name
    [SerializeField] private Text _nameText = null;
    // Reference to the text of the stats display that shows the attack
    [SerializeField] private Text _attackText = null;
    // Reference to the text of the stats display that shows the defense
    [SerializeField] private Text _defenseText = null;
    // Reference to the text of the stats display that shows the max health
    [SerializeField] private Text _healthText = null;
    // Reference to the text of the stats display that shows the speed
    [SerializeField] private Text _speedText = null;


    // Called before start
    protected void Awake()
    {
        // These references are attached to this game object, so they will only need to be set once
        _mARef = this.GetComponent<MoveAttack>();
        if (_mARef == null)
        {
            if (this.name != "DeadAllyStats")
                Debug.Log("Could not find MoveAttack attached to " + this.name);
        }
        else
        {
            _mARef.MoveRange = _speed;
        }

        _hpRef = this.GetComponent<Health>();
        if (_hpRef == null)
        {
            if (this.name != "DeadAllyStats")
                Debug.Log("Could not find Health attached to " + this.name);
        }
        else
        {
            _hpRef.MaxHP = _vitality;
            _hpRef.CurHP = _hpRef.MaxHP;
        }
    }

    /// <summary>
    /// Sets all the appropriate variables to their correct values.
    /// Called from MoveAttack in ResetMyTurn
    /// </summary>
    public void Initialize()
    {
        _mARef.MoveRange = _speed;
        _hpRef.MaxHP = _vitality;
    }

    /// <summary>
    /// Shows or hides the stats of the character
    /// </summary>
    /// <param name="shouldShow">If the characters' stats should be shown or hidden</param>
    public void DisplayStats(bool shouldShow)
    {
        if (_statsDisplay != null)
        {
            _nameText.text = _charName;
            _attackText.text = "A: " + _strength;
            _defenseText.text = "D: " + _defense;
            _healthText.text = "H: " + _vitality;
            _speedText.text = "S: " + _speed;
            _statsDisplay.SetActive(shouldShow);
        }
    }

    /// <summary>
    /// Returns whether the stats are displayed or not
    /// </summary>
    /// <returns>Whether the stats are displayed or not</returns>
    public bool AreStatsDisplayed()
    {
        if (_statsDisplay != null)
            return _statsDisplay.activeSelf;
        return false;
    }
}
