using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    // The Bars are assumed to have the same sized sprite attached
    [SerializeField] private Transform redHealthBar = null;     // The transform that holds the sprite of the health
    [SerializeField] private Transform frameHealthBar = null;   // The transform that holds the sprite of the outline of the health
    [SerializeField] private GameObject rightRedBit = null;     // The rid bit on the right side of the health bar
    [SerializeField] private GameObject leftRedBit = null;      // The rid bit on the left side of the health bar
    [SerializeField] private float redBarOffset = 0.0625f;      // The offset for the red health bar to be at
    private int maxHP;  // Maximum health of the character
    public int MaxHP
    {
        set { maxHP = value; }
    }
    private int curHP;  // The current health of the character
    public int CurHP
    {
        get { return curHP; }
    }
    private Animator animRef;   // Reference to the animator attached to this gameObject

    // Movement references
    private MoveAttackGUIController mAGUIContRef;   // A reference to the MoveAttackGUIController, used to give back control to the user after death
    private MoveAttackController mAContRef; // A reference to the MoveAttackController script, used to recalculate movement after death
    private CharacterType whatAmI;  // What type of character this health script is attached to

    // Enemy AI references
    private EnemyMoveAttackAI enMAAIRef;    // A reference to the EnemyMoveAttackAI, used to alert the enemy AI it should move the next unit after death

    // For turns
    private TurnSystem turnSysRef;  // Reference to the TurnSystem script

    // Set References
    private void Awake()
    {
        animRef = this.gameObject.GetComponent<Animator>();
        if (animRef == null)
        {
            Debug.Log("Could not find Animator attached to " + this.name);
        }

        GameObject gameManagerObj = GameObject.FindWithTag("GameController");
        if (gameManagerObj != null)
        {
            mAGUIContRef = gameManagerObj.GetComponent<MoveAttackGUIController>();
            if (mAGUIContRef == null)
                Debug.Log("Could not find MoveAttackGUIController attached to " + gameManagerObj.name);
            mAContRef = gameManagerObj.GetComponent<MoveAttackController>();
            if (mAContRef == null)
                Debug.Log("Could not find MoveAttackController attached to " + gameManagerObj.name);
            enMAAIRef = gameManagerObj.GetComponent<EnemyMoveAttackAI>();
            if (enMAAIRef == null)
                Debug.Log("Could not find EnemyMoveAttackAI attached to " + gameManagerObj.name);
            turnSysRef = gameManagerObj.GetComponent<TurnSystem>();
            if (turnSysRef == null)
                Debug.Log("Could not find TurnSystem attached to " + gameManagerObj.name);
        }
        else
            Debug.Log("Could not find any GameObject with the tag GameController");

        MoveAttack mARef = this.GetComponent<MoveAttack>();
        if (mARef == null)
        {
            Debug.Log("Could not find MoveAttack attached to " + this.name);
        }
        else
        {
            whatAmI = mARef.WhatAmI;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        curHP = maxHP;  // Needs to be in start because maxHP is set in the Awake function of the Stats script
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
        startScale = redHealthBar.transform.localScale;
        targetScale = new Vector3((frameHealthBar.localScale.x - 2*redBarOffset) * curHP / maxHP, redHealthBar.localScale.y, redHealthBar.localScale.z);

        // Change the localPosition of the redHealthBar so that it looks like it is coming from the left of the frame
        // -(spritePixelSize / pixelsPerUnit) * blankBar.localScale.x / 2 + (spritePixelSize / pixelsPerUnit) * redBar.localScale.x / 2 + blankBar.localPosition.x
        // ((spritePixelSize / pixelsPerUnit) / 2) * (redBar.localScale.x - blankBar.localScale.x) + blankBar.localPosition.x
        startPos = redHealthBar.transform.localPosition;
        float pixelsPerUnit = redHealthBar.gameObject.GetComponent<SpriteMask>().sprite.pixelsPerUnit;
        float spritePixelSize = redHealthBar.gameObject.GetComponent<SpriteMask>().sprite.rect.size.x;
        float xComponent = ((spritePixelSize / pixelsPerUnit) / 2) * (targetScale.x - frameHealthBar.localScale.x) + frameHealthBar.localPosition.x;
        targetPos = new Vector3(xComponent + redBarOffset, redHealthBar.localPosition.y, redHealthBar.localPosition.z);
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
    /// <returns>Returns an IEnumerator</returns>
    private IEnumerator UpdateHealth(Vector3 startScale, Vector3 targetScale, Vector3 startPos, Vector3 targetPos, float speed=1f)
    {
        while (Vector3.Distance(redHealthBar.transform.localPosition, targetPos) > 0.01f)
        {
            Vector3 posIncrement = targetPos - startPos;
            redHealthBar.transform.localPosition += posIncrement * Time.deltaTime * speed;

            Vector3 scaleIncrement = targetScale - startScale;
            redHealthBar.transform.localScale += scaleIncrement * Time.deltaTime * speed;

            yield return null;
        }

        redHealthBar.transform.localPosition = targetPos;
        redHealthBar.transform.localScale = targetScale;

        // If the character died. We call it here so that health goes down first
        if (curHP == 0)
            Die();
        // If the character is not going to begin dying, give the appropriate authority control again
        else
            GiveBackControl();

        yield return null;
    }

    /// <summary>
    /// Decrements curHP by dmgToTake
    /// </summary>
    /// <param name="dmgToTake">Amount curHP will go down by</param>
    public void TakeDamage(int dmgToTake)
    {
        // Make sure the input is valid. If its not, print a debug and stop the function
        if (dmgToTake < 0)
        {
            Debug.Log("A negative value was passed into Health.TakeDamage() attached to " + this.gameObject.name + "\nPlease do not try to heal the unit using this function");
            return;
        }

        // If damage will deal more than needed to take health to 0, just reduce health to 0
        else if (curHP - dmgToTake <= 0)
        {
            curHP = 0;
        }
        // Otherwise do it normally
        else
        {
            curHP -= dmgToTake;
        }

        // If this unit has a health bar, update it to properly display the new health information
        if (redHealthBar != null && frameHealthBar != null)
        {
            // Calculate where the health bar should go
            Vector3 startScale = Vector3.zero, targetScale = Vector3.zero, startPos = Vector3.zero, targetPos = Vector3.zero;
            CalculateHealthBar(ref startScale, ref targetScale, ref startPos, ref targetPos);
            // Update the health bar
            // If the character died, we call it will be called in the update health coroutine so that their hp goes down before they die
            // If the character didn't die, we return control to the proper authority
            StartCoroutine(UpdateHealth(startScale, targetScale, startPos, targetPos));
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
        else if (curHP == maxHP)
        {
            return false;
        }
        // If they will get healed up to full
        else if (curHP + healAmount >= maxHP)
        {
            curHP = maxHP;
        }
        // Otherwise we just heal normally
        else
        {
            curHP += healAmount;
        }

        // If this unit has a health bar, update it to properly display the new health information
        if (redHealthBar != null && frameHealthBar != null)
        {
            Vector3 startScale = Vector3.zero, targetScale = Vector3.zero, startPos = Vector3.zero, targetPos = Vector3.zero;
            CalculateHealthBar(ref startScale, ref targetScale, ref startPos, ref targetPos);
            StartCoroutine(UpdateHealth(startScale, targetScale, startPos, targetPos));
        }
        return true;
    }

    /// <summary>
    /// Starts the death animation. From that death animation, ascend should be called to destroy the object
    /// </summary>
    private void Die()
    {
        animRef.SetBool("Dead", true);
    }

    /// <summary>
    /// Actually destroys the object. Should only be called by an animation event
    /// </summary>
    private void Ascend()
    {
        // Since this is done, we need to let other character move to where this just was
        Node myNode = mAContRef.GetNodeByWorldPosition(this.transform.position);
        myNode.occupying = CharacterType.None;
        // We need to move this character out from the character's parent before we recalculate the visuals, or this will be included in those calculaiton
        GameObject graveyard = new GameObject("Graveyard");
        this.transform.parent = graveyard.transform;
        // We then need to recreate all the visuals, so that the user can see they can move over the dead body
        //NEEDTOFIXmAContRef.CreateAllVisualTiles();

        // Give either the user or the ai control of their stuff
        GiveBackControl();

        //Debug.Log(this.gameObject.name + " has died");
        Destroy(graveyard);
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Gives conrol back to the appropriate authority, either the enemy AI or the user. Called from UpdateHealth if the 
    /// character won't die. Called from Ascend if the character will die
    /// </summary>
    private void GiveBackControl()
    {
        // If the character is an ally, that means an enemy killed it, so we have to tell the enemy AI script to move the next enemy
        if (whatAmI == CharacterType.Ally)
        {
            enMAAIRef.NextEnemy();
            //Debug.Log("Dead ally");
        }
        // If the character is an enemy, that means an ally killed it, so we have to allow the user to select again
        if (whatAmI == CharacterType.Enemy)
        {
            //Debug.Log("Dead enemy");
            mAGUIContRef.AllowSelect();
            turnSysRef.IsPlayerDone();
        }
    }
}
