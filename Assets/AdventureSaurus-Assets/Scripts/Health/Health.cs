using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Health : MonoBehaviour
{
    // The Bars are assumed to have the same sized sprite attached
    // The slider of the health bar
    [SerializeField] private Slider _healthBarSlider = null;

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

    // Movement references
    // A reference to the MoveAttackController script, used to recalculate movement after death
    protected MoveAttackController _mAContRef;


    // Called before Start
    // Set references
    protected void Awake()
    {
        _curHP = _maxHP;

        // Get the animator attached to this gameobject
        _animRef = this.gameObject.GetComponent<Animator>();
        if (_animRef == null)
        {
            Debug.Log("Could not find Animator attached to " + this.name);
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
    }

    /// <summary>
    /// Slowly increases the health bars visuals. Also tests if the enemy is going to die, if they are it calls Die.
    /// Also returns control to the correct autority if no one died
    /// </summary>
    /// <param name="speed">scalar for how fast the health bar moves. Defaults to 1</param>
    /// <returns>IEnumerator</returns>
    private IEnumerator UpdateHealth(float speed = 0.5f)
    {
        // Signal MoveAttack that a character's health bar is currently being updated.
        // We will remove it here after this ends or in Ascend if the damage is fatal
        MoveAttack.AddOngoingAction();

        // Get the target amount to work towards
        float targetAm = ((float)CurHP) / MaxHP;
        // If we are lower than the current amount
        while (_healthBarSlider.value < targetAm)
        {
            _healthBarSlider.value += Time.deltaTime * speed;
            yield return null;
        }
        // If we are higher than the current amount
        while (_healthBarSlider.value > targetAm)
        {
            _healthBarSlider.value -= Time.deltaTime * speed;
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

        // Remove the ongoing action to signal we are done
        MoveAttack.RemoveOngoingAction();

        //Debug.Log(this.gameObject.name + " has died");
        Destroy(graveyard);
        Destroy(this.gameObject);
    }
}
