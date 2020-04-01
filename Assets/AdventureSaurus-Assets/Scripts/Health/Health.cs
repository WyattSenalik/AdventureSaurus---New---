using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Health : MonoBehaviour
{
    // The Bars are assumed to have the same sized sprite attached
    // The transform that holds the sprite of the health
    [SerializeField] private Transform _redHealthBar = null;
    // The transform that holds the sprite of the outline of the health
    [SerializeField] private Transform _frameHealthBar = null;
    // The offset for the red health bar to be at
    [SerializeField] private float _redBarOffset = 0.0625f;

    // Maximum health of the character
    private int _maxHP;
    public int MaxHP
    {
        get { return _maxHP; }
        set { _maxHP = value; }
    }
    // The current health of the character
    private int _curHP;
    public int CurHP
    {
        get { return _curHP; }
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
        // Get the animator attached to this gameobject
        _animRef = this.gameObject.GetComponent<Animator>();
        if (_animRef == null)
        {
            Debug.Log("Could not find Animator attached to " + this.name);
        }

        // Get the scripts from GameController
        GameObject gameContObj = GameObject.FindWithTag("GameController");
        if (gameContObj == null)
            Debug.Log("Could not find a gameobject with the tag GameController");
        // Get the MoveAttackController
        _mAContRef = gameContObj.GetComponent<MoveAttackController>();
        if (_mAContRef == null)
            Debug.Log("Could not MoveAttackController attached to " + gameContObj.name);
    }

    // Start is called before the first frame update
    private void Start()
    {
        _curHP = _maxHP;
    }

    /// <summary>
    /// Calculates the health bar values and then updates the visuals on them
    /// </summary>
    protected void UpdateHealthDisplay()
    {
        // Calculate where the health bar should go
        Vector3 startScale = Vector3.zero, targetScale = Vector3.zero, startPos = Vector3.zero, targetPos = Vector3.zero;
        CalculateHealthBar(ref startScale, ref targetScale, ref startPos, ref targetPos);
        // Update the health bar
        // If the character died, we call it will be called in the update health coroutine so that their hp goes down before they die
        // If the character didn't die, we return control to the proper authority
        StartCoroutine(UpdateHealth(startScale, targetScale, startPos, targetPos, false));
    }

    /// <summary>
    /// Calculates the health bar so that it will fit inside the empty health bar
    /// </summary>
    /// <param name="startScale">Passed by reference. Gets set to the redHealthBar's current localScale</param>
    /// <param name="targetScale">Passed by reference. The localPosition the redHealthBar should be at to appear like the character has the correct amount of health</param>
    /// <param name="startPos">Passed by reference. Gets set to the redHealthBar's current localPosition</param>
    /// <param name="targetPos">Passed by reference. The localPosition the redHealthBar should be at to appear like the character has the correct amount of health</param>
    private void CalculateHealthBar(ref Vector3 startScale, ref Vector3 targetScale, ref Vector3 startPos, ref Vector3 targetPos)
    {
        // Change the localScale of the redHealthBar so that it is startingHP/curHP of its "full health" size
        startScale = _redHealthBar.transform.localScale;
        targetScale = new Vector3((_frameHealthBar.localScale.x - 2*_redBarOffset) * _curHP / _maxHP, _redHealthBar.localScale.y, _redHealthBar.localScale.z);

        // Change the localPosition of the redHealthBar so that it looks like it is coming from the left of the frame
        // -(spritePixelSize / pixelsPerUnit) * blankBar.localScale.x / 2 + (spritePixelSize / pixelsPerUnit) * redBar.localScale.x / 2 + blankBar.localPosition.x
        // ((spritePixelSize / pixelsPerUnit) / 2) * (redBar.localScale.x - blankBar.localScale.x) + blankBar.localPosition.x
        startPos = _redHealthBar.transform.localPosition;
        float pixelsPerUnit = _redHealthBar.gameObject.GetComponent<SpriteMask>().sprite.pixelsPerUnit;
        float spritePixelSize = _redHealthBar.gameObject.GetComponent<SpriteMask>().sprite.rect.size.x;
        float xComponent = ((spritePixelSize / pixelsPerUnit) / 2) * (targetScale.x - _frameHealthBar.localScale.x) + _frameHealthBar.localPosition.x;
        targetPos = new Vector3(xComponent + _redBarOffset, _redHealthBar.localPosition.y, _redHealthBar.localPosition.z);
    }

    /// <summary>
    /// Slowly increases the health bars visuals. Also tests if the enemy is going to die, if they are it calls Die.
    /// Also returns control to the correct autority if no one died
    /// </summary>
    /// <param name="startScale">The localScale of the redHealthBar before calling this function</param>
    /// <param name="targetScale">The localScale the redHealthBar needs to get to</param>
    /// <param name="startPos">The localPosition of the redHealthBar before calling this function</param>
    /// <param name="targetPos">The localPosition the redHealthBar needs to get to</param>
    /// <param name="speed">scalar for how fast the health bar moves. Defaults to 1</param>
    /// <param name="healing">If the health is being healed (true) or damage is being taken (false)</param>
    /// <returns>Returns an IEnumerator</returns>
    private IEnumerator UpdateHealth(Vector3 startScale, Vector3 targetScale, Vector3 startPos, Vector3 targetPos, bool healing, float speed = 1f)
    {
        // Signal MoveAttack that a character's health bar is currently being updated.
        // We will remove it here after this ends or in Ascend if the damage is fatal
        MoveAttack.AddOngoingAction();

        while (Vector3.Distance(_redHealthBar.transform.localPosition, targetPos) > 0.01f)
        {
            Vector3 posIncrement = targetPos - startPos;
            _redHealthBar.transform.localPosition += posIncrement * Time.deltaTime * speed;

            Vector3 scaleIncrement = targetScale - startScale;
            _redHealthBar.transform.localScale += scaleIncrement * Time.deltaTime * speed;

            yield return null;
        }

        _redHealthBar.transform.localPosition = targetPos;
        _redHealthBar.transform.localScale = targetScale;

        

        // If the character died. We call it here so that health goes down first
        if (_curHP == 0)
            Die();
        // Signal MoveAttack that a character's health bar is finished being updated
        else
            MoveAttack.RemoveOngoingAction();

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
        if (_redHealthBar != null && _frameHealthBar != null)
        {
            UpdateHealthDisplay();
        }
    }

    /// <summary>
    /// Increments curHP by healAmount
    /// </summary>
    /// <param name="healAmount">the amount to heal</param>
    /// <returns>Returns true if the unit was healed. False otherwise (they had fullHP or a negative value was passed in)</returns>
    public bool Heal(int healAmount)
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
        if (_redHealthBar != null && _frameHealthBar != null)
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
