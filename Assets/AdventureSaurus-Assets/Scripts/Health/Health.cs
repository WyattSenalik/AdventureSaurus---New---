using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Health : MonoBehaviour
{
    // The Bars are assumed to have the same sized sprite attached
    // The slider of the health bar
    [SerializeField] private Slider _healthBarSlider = null;
    // The text that display remaining health
    [SerializeField] private Text _healthText = null;
    // Speed to update the heath bar
    [Min(0.001f)]
    [SerializeField] protected float _healthBarSpeed = 1f;

    // The color of red to change the sprite renderer to
    [SerializeField] private Color _damageRed = new Color(0.6f, 0.0f, 0.0f);
    // Speed to do the red pulse
    [Min(0.001f)]
    [SerializeField] private float _redPulseSpeed = 1f;

    // Maximum health of the character
    private int _maxHP = 1;
    public int MaxHP
    {
        get { return _maxHP; }
        set { _maxHP = value; }
    }
    // The current health of the character
    private int _curHP = 1;
    public int CurHP
    {
        get { return _curHP; }
        set {
            _curHP = value;
            if (this.gameObject.activeInHierarchy)
                UpdateHealthDisplay();
        }
    }

    // References to things on this gameobject
    // Reference to the animator attached to this gameObject
    private Animator _animRef;
    // Reference to the sprite renderer attached to this gameObject
    private SpriteRenderer _sprRendRef;

    // Movement references
    // A reference to the MoveAttackController script, used to recalculate movement after death
    protected MoveAttackController _mAContRef;

    // Events
    // When a character dies
    public delegate void CharacterDeath();
    public static event CharacterDeath OnCharacterDeath;


    // Called before Start
    // Set references
    protected void Awake()
    {
        _curHP = _maxHP;

        // Get the animator attached to this gameobject
        _animRef = this.gameObject.GetComponent<Animator>();
        if (_animRef == null)
        {
            Debug.LogError("Could not find Animator attached to " + this.name);
        }
        // Get the spriterenderer
        _sprRendRef = this.gameObject.GetComponent<SpriteRenderer>();
        if (_sprRendRef == null)
        {
            Debug.LogError("Could not find SpriteRenderer attached to " + this.name);
        }

        SetReferences();
    }

    // Sets foreign references
    protected void SetReferences()
    {
        // Get the scripts from GameController
        GameObject gameContObj = GameObject.FindWithTag("GameController");
        if (gameContObj == null)
            Debug.Log("Could not find a gameobject with the tag GameController");
        // Get the MoveAttackController
        _mAContRef = gameContObj.GetComponent<MoveAttackController>();
        if (_mAContRef == null)
            Debug.Log("Could not MoveAttackController attached to " + gameContObj.name);
    }

    /// <summary>
    /// Calculates the health bar values and then updates the visuals on them
    /// </summary>
    protected void UpdateHealthDisplay()
    {
        // Update the health bar
        // If the character died, we call it will be called in the update health coroutine so that their hp goes down before they die
        // If the character didn't die, we return control to the proper authority
        StartCoroutine(UpdateHealth());
        // Set the text to display the correct values
        _healthText.text = _curHP.ToString() + "/" + _maxHP.ToString();
    }

    /// <summary>
    /// Slowly increases the health bars visuals. Also tests if the enemy is going to die, if they are it calls Die.
    /// Also returns control to the correct autority if no one died
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator UpdateHealth()
    {
        // Signal MoveAttack that a character's health bar is currently being updated.
        // We will remove it here after this ends or in Ascend if the damage is fatal
        MoveAttack.AddOngoingAction();

        // Get the target amount to work towards
        float targetAm = ((float)CurHP) / MaxHP;
        // If we are lower than the current amount
        while (_healthBarSlider.value < targetAm)
        {
            _healthBarSlider.value += Time.deltaTime * _healthBarSpeed;
            yield return null;
        }
        // If we are higher than the current amount
        while (_healthBarSlider.value > targetAm)
        {
            _healthBarSlider.value -= Time.deltaTime * _healthBarSpeed;
            yield return null;
        }

        // After we finish, just set it
        _healthBarSlider.value = targetAm;

        // If the character died. We call it here so that health goes down first
        if (_curHP == 0)
            Die();
        // Signal MoveAttack that a character's health bar is finished being updated
        else
        {
            MoveAttack.RemoveOngoingAction();
        }

        yield return null;
    }

    /// <summary>
    /// Decrements curHP by dmgToTake
    /// </summary>
    /// <param name="dmgToTake">Amount curHP will go down by</param>
    /// <param name="dmgDealer">The Stats of the character who dealt damage</param>
    public virtual void TakeDamage(int dmgToTake, Stats dmgDealer)
    {
        // Make sure the input is valid. If its not, print a debug and stop the function
        if (dmgToTake < 0)
        {
            Debug.Log("A negative value was passed into Health.TakeDamage() attached to " + 
                this.gameObject.name + "\nPlease do not try to heal the unit using this function");
            return;
        }
        // If damage will deal more than needed to take health to 0, just reduce health to 0
        else if (_curHP - dmgToTake <= 0)
        {
            _curHP = 0;
        }
        // Otherwise do it normally
        else
        {
            _curHP -= dmgToTake;
        }

        // Flash red
        StartCoroutine(BlinkRed());

        // If this unit has a health bar, update it to properly display the new health information
        if (_healthBarSlider != null)
        {
            UpdateHealthDisplay();
        }
    }

    /// <summary>
    /// Increments curHP by healAmount
    /// </summary>
    /// <param name="healAmount">the amount to heal</param>
    /// <returns>Returns true if the unit was healed. False otherwise (they had fullHP or a negative value was passed in)</returns>
    public virtual bool Heal(int healAmount)
    {
        // Make sure the input is valid. If its not, print a debug and stop the function
        if (healAmount < 0)
        {
            Debug.Log("A negative value was passed into Health.Heal() attached to " + this.gameObject.name + "\nPlease do not try to deal damage to the unit using this function");
            return false;
        }
        // If they are already at full health, they can't be healed
        else if (_curHP == _maxHP)
        {
            return false;
        }
        // If they will get healed up to full
        else if (_curHP + healAmount >= _maxHP)
        {
            _curHP = _maxHP;
        }
        // Otherwise we just heal normally
        else
        {
            _curHP += healAmount;
        }

        // If this unit has a health bar, update it to properly display the new health information
        if (_healthBarSlider != null)
        {
            UpdateHealthDisplay();
        }
        return true;
    }

    /// <summary>
    /// Starts the death animation. From that death animation, ascend should be called to destroy the object
    /// </summary>
    private void Die()
    {
        //Debug.Log("Dead enemy");
        _animRef.SetBool("Dead", true);
    }

    /// <summary>
    /// Actually destroys the object. Should only be called by an animation event
    /// </summary>
    protected virtual void Ascend()
    {
        // Since this is done, we need to let other character move to where this just was
        Node myNode = _mAContRef.GetNodeByWorldPosition(this.transform.position);
        myNode.Occupying = CharacterType.None;
        // We need to move this character out from the character's parent before we recalculate the visuals, or this will be included in those calculaiton
        GameObject graveyard = new GameObject("Graveyard");
        this.transform.parent = graveyard.transform;
        // We then need to recreate all the visuals, so that the user can see they can move over the dead body
        //NEEDTOFIXmAContRef.CreateAllVisualTiles();

        // Call the OnCharacterDeath event
        OnCharacterDeath?.Invoke();

        // Remove the ongoing action to signal we are done
        MoveAttack.RemoveOngoingAction();

        //Debug.Log(this.gameObject.name + " has died");
        Destroy(graveyard);
        Destroy(this.gameObject);
    }

    /// <summary>
    /// For flashing red when the character gets hurt
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator BlinkRed()
    {
        // The starting color, so that we can return to it after turning red
        Color originalCol = _sprRendRef.color;
        // A short hand for the current color of the spriterenderer
        Color curCol = _sprRendRef.color;
        // Calculating the amount to increase the color every time
        float steps = 20 / _redPulseSpeed;
        Color incrCol = (_damageRed - curCol) / steps;

        // Determine if r, g, and b are increasing or decreasing
        bool isRedInc = incrCol.r >= 0;
        bool isGreenInc = incrCol.g >= 0;
        bool isBlueInc = incrCol.b >= 0;
        // Change the color to red slowly
        bool shouldContinue = true;
        while (shouldContinue)
        {
            // Change color
            curCol += incrCol;
            _sprRendRef.color = curCol;

            // If the color is has passed the values it was aiming for, end the loop
            if ((curCol.r > _damageRed.r && isRedInc) ||
                (curCol.r < _damageRed.r && !isRedInc) ||
                (curCol.g > _damageRed.g && isGreenInc) ||
                (curCol.g < _damageRed.g && !isGreenInc) ||
                (curCol.b > _damageRed.b && isBlueInc) ||
                (curCol.b < _damageRed.b && !isBlueInc))
            {
                shouldContinue = false;
            }

            yield return null;
        }
        // Change the color back
        shouldContinue = true;
        while (shouldContinue)
        {
            // Change color
            curCol -= incrCol;
            _sprRendRef.color = curCol;

            // If the color is has passed the values it was aiming for, end the loop
            if ((curCol.r > originalCol.r && !isRedInc) ||
                (curCol.r < originalCol.r && isRedInc) ||
                (curCol.g > originalCol.g && !isGreenInc) ||
                (curCol.g < originalCol.g && isGreenInc) ||
                (curCol.b > originalCol.b && !isBlueInc) ||
                (curCol.b < originalCol.b && isBlueInc))
            {
                shouldContinue = false;
            }

            yield return null;
        }

        // Set the color back explicitly
        _sprRendRef.color = originalCol;
        yield return null;
    }
}
