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
        curHP = maxHP;
    }

    // Resizes the health bar to fit inside the empty health bar
    private void UpdateHealthBar()
    {
        // Change the localScale of the redHealthBar so that it is startingHP/curHP of its "full health" size
        redHealthBar.transform.localScale = new Vector3((frameHealthBar.localScale.x - 2*redBarOffset) * curHP / maxHP, redHealthBar.localScale.y, redHealthBar.localScale.z);

        // Change the localPosition of the redHealthBar so that it looks like it is coming from the left of the frame
        // -(spritePixelSize / pixelsPerUnit) * blankBar.localScale.x / 2 + (spritePixelSize / pixelsPerUnit) * redBar.localScale.x / 2 + blankBar.localPosition.x
        // ((spritePixelSize / pixelsPerUnit) / 2) * (redBar.localScale.x - blankBar.localScale.x) + blankBar.localPosition.x
        float pixelsPerUnit = redHealthBar.gameObject.GetComponent<SpriteMask>().sprite.pixelsPerUnit;
        float spritePixelSize = redHealthBar.gameObject.GetComponent<SpriteMask>().sprite.rect.size.x;
        float xComponent = ((spritePixelSize / pixelsPerUnit) / 2) * (redHealthBar.localScale.x - frameHealthBar.localScale.x) + frameHealthBar.localPosition.x;
        redHealthBar.transform.localPosition = new Vector3(xComponent + redBarOffset, redHealthBar.localPosition.y, redHealthBar.localPosition.z);
    }

    // Decrements curHP by dmgToTake
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
            Die();
        }
        // Otherwise do it normally
        else
        {
            curHP -= dmgToTake;
        }

        // If this unit has a health bar, update it to properly display the new health information
        if (redHealthBar != null && frameHealthBar != null)
        {
            UpdateHealthBar();
        }
    }

    // Increments curHP by healAmount
    // Returns true if the unit was healed. False otherwise (they had fullHP or a negative value was passed in)
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
            UpdateHealthBar();
        }
        return true;
    }

    // Starts the death animation. From that death animation, ascend should be called to destroy the object
    private void Die()
    {
        animRef.SetBool("Dead", true);
    }

    // Actually destroys the object. Should only be called by an animation event
    private void Ascend()
    {
        // Since this is done, we need to let other character move to where this just was
        Node myNode = mAContRef.GetNodeByWorldPosition(this.transform.position);
        myNode.occupying = CharacterType.None;
        // We need to move this character out from the character's parent before we recalculate the visuals, or this will be included in those calculaiton
        GameObject graveyard = new GameObject("Graveyard");
        this.transform.parent = graveyard.transform;
        // We then need to recreate all the visuals, so that the user can see they can move over the dead body
        mAContRef.CreateAllVisualTiles();

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

        //Debug.Log(this.gameObject.name + " has died");
        Destroy(graveyard);
        Destroy(this.gameObject);
    }
}
