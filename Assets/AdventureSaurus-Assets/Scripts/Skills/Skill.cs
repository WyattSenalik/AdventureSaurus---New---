using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{
    // Reference to game controller scripts
    // Need turn system to call isPlayerDone after using Skill
    protected TurnSystem turnSysRef;
    protected MoveAttackGUIController mAGUIContRef;
    protected MoveAttackController mAContRef;
    protected EnemyMoveAttackAI enMAAIRef;

    // Reference to the selected character's MoveAttack script
    protected MoveAttack maRef;
    // The skill's unique number
    protected int skillNum = -1;
    public int SkillNum
    {
        get { return skillNum; }
    }
    // If this skill hits diagonals
    protected bool diagnols = false;
    // If this skill heals
    protected bool healing = false;
    // A list of the enemies that will take damage from this skill
    // Or a list of the allyies that will be healed from this skill
    protected List<Health> enemiesHP;
    // The cooldown of the skill
    protected int cooldown = 0;
    // The amount of turns progressed towards the cooldown
    protected int cooldownTimer;
    public int CooldownTimer
    {
        get { return cooldownTimer; }
    }
    // The amount of damage this skill does / the amount this skill heals
    protected int damage = 1;
    // The distance this skill can cover
    protected int range = 1;
    // The animator attached to this character
    protected Animator anime = null;//damage to deal, amount to heal ,and amount to buff
    // The spriterenderer attached to this cahracter
    protected SpriteRenderer sprRendRef = null;
    // Stats attached to this character
    protected Stats statsRef = null;

    /// <summary>
    /// Sets references to foreign scripts.
    /// Called from Awake and from PersistantController
    /// </summary>
    public void SetReferences()
    {
        GameObject gameControllerObj = GameObject.FindWithTag("GameController");
        if (gameControllerObj == null)
        {
            Debug.Log("Could not find any GameObject with the tag GameController");
        }
        else
        {
            turnSysRef = gameControllerObj.GetComponent<TurnSystem>();
            if (turnSysRef == null)
            {
                Debug.Log("Could not find TurnSystem attached to " + gameControllerObj.name);
            }
            mAGUIContRef = gameControllerObj.GetComponent<MoveAttackGUIController>();
            if (mAGUIContRef == null)
            {
                Debug.Log("Could not find MoveAttackGUIController attached to " + gameControllerObj.name);
            }
            mAContRef = gameControllerObj.GetComponent<MoveAttackController>();
            if (mAContRef == null)
            {
                Debug.Log("Could not find MoveAttackController attached to " + gameControllerObj.name);
            }
            enMAAIRef = gameControllerObj.GetComponent<EnemyMoveAttackAI>();
            if (enMAAIRef == null)
            {
                Debug.Log("Could not find EnemyMoveAttackAI attached to " + gameControllerObj.name);
            }
        }
    }

    // Called before start
    protected void Awake()
    {
        // These will have to be set multiple times [allies only]
        SetReferences();
        // These will only need to be set once, since they are attached to this game object
        sprRendRef = this.GetComponent<SpriteRenderer>();
        if (sprRendRef == null)
        {
            Debug.Log("Could not find SpriteRenderer attached to " + this.name);
        }
        anime = this.GetComponent<Animator>();
        if (anime == null)
        {
            Debug.Log("Could not find Animator attached to " + this.name);
        }
        maRef = this.GetComponent<MoveAttack>();
        if (maRef == null)
        {
            Debug.Log("Could not find MoveAttack attached to " + this.name);
        }
        statsRef = this.GetComponent<Stats>();
        if (statsRef == null)
        {
            Debug.Log("Could not find Stats attached to " + this.name);
        }
    }

    // Initialize some variables
    private void Start()
    {
        cooldownTimer = 0;
    }


    /// <summary>
    /// Calls StartSkillAnimation and gets reference to the enemies damaged by this skill
    /// </summary>
    /// <param name="attackNodesPos">Grid position of the node at the center of the skill</param>
    virtual public void StartSkill(Vector2Int attackNodePos)
    {
        Debug.Log("StartAttack not implemented");
    }

    /// <summary>
    /// Stops the skill animation and deals damage to any enemy that was hit and if no enemies hit gives control back to the appropriate authority.
    /// </summary>
    virtual public void EndSkill()
    {
        Debug.Log("EndAttack not implemented");

    }

    /// <summary>
    /// This starts the animation of the skill.
    /// </summary>
    /// <param name="attackNodePos">This represents the direction the charcter will strike.</param>
    protected void StartSkillAnimation(Vector2Int attackNodePos)
    {
        int attackDirection = -1;
        // If I am below the node I am striking, I should attack up
        if (attackNodePos.y - this.transform.position.y > 0)
        {
            sprRendRef.flipX = false;
            attackDirection = 1;
        }
        // If I am right the node I am striking, I should attack left
        if (this.transform.position.x - attackNodePos.x > 0)
        {
            sprRendRef.flipX = true;
            attackDirection = 2;
        }
        // If I am left the node I am striking, I should attack right
        if (attackNodePos.x - this.transform.position.x > 0)
        {
            sprRendRef.flipX = false;
            attackDirection = 3;
        }
        // If I am above the node I am striking, I should attack down
        if (this.transform.position.y - attackNodePos.y > 0)
        {
            sprRendRef.flipX = false;
            attackDirection = 4;
        }

        anime.SetInteger("SkillNum", skillNum);
        //Debug.Log("Start Attack");
        anime.SetInteger("AttackDirection", attackDirection);
    }

    /// <summary>
    /// This ends the animation of the skill.
    /// </summary>
    protected void EndSkillAnimation()
    {
        anime.SetInteger("AttackDirection", -anime.GetInteger("AttackDirection"));
        anime.SetInteger("SkillNum", -1);

        // Also puts the skill on cooldown
        GoOnCooldown();
    }

    /// <summary>
    /// Makes the skill go on cooldown.
    /// </summary>
    protected void GoOnCooldown()
    {
        if (cooldown != 0)
        {
            // Sets cooldown timer to cooldown. When cooldown timer reaches 0, we can use the skill again
            cooldownTimer = cooldown;

            // If the current character is an ally, we also want them to reset their skill to basic attack
            if (maRef.WhatAmI == CharacterType.Ally)
            {
                // Get the ally skill controller
                AllySkillController allyskillContRef = maRef.GetComponent<AllySkillController>();
                if (allyskillContRef == null)
                {
                    Debug.Log("No AllySkillController was attached to " + maRef.name);
                    return;
                }

                // De activate the special skill (if it was one)
                allyskillContRef.DeactivateSkill();
            }
        }
    }

    /// <summary>
    /// Increments the cooldown 1 closer to being off cooldown.
    /// Called from BasicAttack.EndSkill and TODO
    /// </summary>
    public void IncrementCooldownTimer()
    {
        // 0 is refreshed so lower the cooldown timer
        if (--cooldownTimer < 0)
            cooldownTimer = 0;
    }

    /// <summary>
    /// Decrements cooldown by 1 so that it takes 1 less turn to get the skill back
    /// </summary>
    public void UpgradeCooldown()
    {
        // Make the cooldown lower
        --cooldown;
        // Also increment the timer
        IncrementCooldownTimer();
    }

    /// <summary>
    /// Increases the damage of the skill by 1
    /// </summary>
    public void UpgradeDamage()
    {
        ++damage;
    }

    /// <summary>
    /// Increases the range of the skill by 1
    /// </summary>
    public void UpgradeRange()
    {
        ++range;
    }

    /// <summary>
    /// Sets the attack range of MoveAttack
    /// Called when the skill is equipped
    /// </summary>
    public void EquipSkill()
    {
        // Set the range
        maRef.AttackRange = range;
    }
}
