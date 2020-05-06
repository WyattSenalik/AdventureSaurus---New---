using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour
{
    // Reference to game controller scripts
    protected MoveAttackController _mAContRef;

    // Reference to the selected character's MoveAttack script
    protected MoveAttack _maRef;
    // The animator attached to this character
    protected Animator _anime = null;
    // The spriterenderer attached to this cahracter
    protected SpriteRenderer _sprRendRef = null;
    // Stats attached to this character
    protected Stats _statsRef = null;

    // The skill's unique number
    protected int _skillNum = -1;
    public int GetSkillNum(){ return _skillNum; }
    // If this skill hits diagonals
    protected bool _diagnols = false;
    // If this skill heals
    protected bool _healing = false;
    // A list of the enemies that will take damage from this skill
    // Or a list of the allyies that will be healed from this skill
    protected List<Health> _enemiesHP;
    // The cooldown of the skill
    protected int _cooldown = 0;
    public int GetCoolDown(){ return _cooldown; }
    // The amount of turns progressed towards the cooldown
    protected int _cooldownTimer;
    public int GetCooldownTimer(){ return _cooldownTimer; }
    // The amount of damage this skill does / the amount this skill heals
    protected int _damage = 1;
    public int GetDamage(){ return _damage; }
    // The distance this skill can cover
    protected int _range = 1;
    public int GetRange() { return _range; }

    protected AudioManager _audManRef;

    // Events
    // When the cooldown changes
    public delegate void CooldownChange();
    public static event CooldownChange OnCooldownChange;
    // When the cooldown starts
    public delegate void CooldownStart();
    public static event CooldownStart OnCooldownStart;

    // Called when the component is set active
    // Subscribe to events
    private void OnEnable()
    {
        // When generation is done, do some initialization
        ProceduralGenerationController.OnFinishGenerationNoParam += SetReferences;
        ProceduralGenerationController.OnFinishGenerationNoParam += Initialize;
    }

    // Called when the component is toggled off
    // Unsubscribe from events
    private void OnDisable()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= SetReferences;
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
    }

    // Called when the gameobject is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= SetReferences;
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
    }

    // Called before start
    protected void Awake()
    {
        // These will have to be set multiple times [allies only]
        SetReferences();
        // These will only need to be set once, since they are attached to this game object
        _sprRendRef = this.GetComponent<SpriteRenderer>();
        if (_sprRendRef == null)
        {
            Debug.Log("Could not find SpriteRenderer attached to " + this.name);
        }
        _anime = this.GetComponent<Animator>();
        if (_anime == null)
        {
            Debug.Log("Could not find Animator attached to " + this.name);
        }
        _maRef = this.GetComponent<MoveAttack>();
        if (_maRef == null)
        {
            Debug.Log("Could not find MoveAttack attached to " + this.name);
        }
        _statsRef = this.GetComponent<Stats>();
        if (_statsRef == null)
        {
            Debug.Log("Could not find Stats attached to " + this.name);
        }
    }

    /// <summary>
    /// Sets references to foreign scripts.
    /// </summary>
    private void SetReferences()
    {
        GameObject gameControllerObj = GameObject.FindWithTag("GameController");
        if (gameControllerObj == null)
        {
            Debug.Log("Could not find any GameObject with the tag GameController");
        }
        else
        {
            _mAContRef = gameControllerObj.GetComponent<MoveAttackController>();
            if (_mAContRef == null)
            {
                Debug.Log("Could not find MoveAttackController attached to " + gameControllerObj.name);
            }
        }

        // Get the audio manager
        try
        {
            _audManRef = GameObject.FindWithTag("AudioManager").GetComponent<AudioManager>();
        }
        catch
        {
            Debug.Log("Could not get AudioManager");
        }
    }

    /// <summary>
    /// Initializes some things for children
    /// </summary>
    protected virtual void Initialize()
    {
        // TODO in children if needed
    }

    // Initialize some variables
    private void Start()
    {
        _cooldownTimer = 0;
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
            _sprRendRef.flipX = false;
            attackDirection = 1;
        }
        // If I am right the node I am striking, I should attack left
        if (this.transform.position.x - attackNodePos.x > 0)
        {
            _sprRendRef.flipX = true;
            attackDirection = 2;
        }
        // If I am left the node I am striking, I should attack right
        if (attackNodePos.x - this.transform.position.x > 0)
        {
            _sprRendRef.flipX = false;
            attackDirection = 3;
        }
        // If I am above the node I am striking, I should attack down
        if (this.transform.position.y - attackNodePos.y > 0)
        {
            _sprRendRef.flipX = false;
            attackDirection = 4;
        }

        _anime.SetInteger("SkillNum", _skillNum);
        //Debug.Log("Start Attack");
        _anime.SetInteger("AttackDirection", attackDirection);
    }

    /// <summary>
    /// This ends the animation of the skill.
    /// </summary>
    protected void EndSkillAnimation()
    {
        _anime.SetInteger("AttackDirection", -_anime.GetInteger("AttackDirection"));
        _anime.SetInteger("SkillNum", -1);


        // Also puts the skill on cooldown
        // Not anymore, we do this in end skill now
        //GoOnCooldown();
    }

    /// <summary>
    /// Makes the skill go on cooldown.
    /// </summary>
    protected void GoOnCooldown()
    {
        if (_cooldown != 0)
        {
            // Sets cooldown timer to cooldown. When cooldown timer reaches 0, we can use the skill again
            _cooldownTimer = _cooldown;

            // If the current character is an ally, we also want them to reset their skill to basic attack
            if (_maRef.WhatAmI == CharacterType.Ally)
            {
                // Get the ally skill controller
                AllySkillController allyskillContRef = _maRef.GetComponent<AllySkillController>();
                if (allyskillContRef == null)
                {
                    Debug.Log("No AllySkillController was attached to " + _maRef.name);
                    return;
                }

                // De activate the special skill (if it was one)
                allyskillContRef.DeactivateSkill();
            }

            OnCooldownStart?.Invoke();
        }
    }

    /// <summary>
    /// Increments the cooldown 1 closer to being off cooldown.
    /// Called from BasicAttack.EndSkill and TODO
    /// </summary>
    public void IncrementCooldownTimer()
    {
        // 0 is refreshed so lower the cooldown timer
        if (--_cooldownTimer < 0)
            _cooldownTimer = 0;
    }

    /// <summary>
    /// Decrements the cooldown 1 farther to being off cooldown.
    /// </summary>
    private void DecrementCooldownTimer()
    {
        // Make the cooldown longer. If for some reason, it would make the timer
        // longer than the cooldown, just make it the cooldown
        if (++_cooldownTimer > _cooldown)
            _cooldownTimer = _cooldown;
    }

    /// <summary>
    /// Decrements cooldown by 1 so that it takes 1 less turn to get the skill back
    /// </summary>
    public void UpgradeCooldown()
    {
        // Make the cooldown lower
        --_cooldown;
        // Also increment the timer
        IncrementCooldownTimer();

        // Call the on cooldown change event
        OnCooldownChange?.Invoke();
    }

    /// <summary>
    /// Increases the damage of the skill by 1
    /// </summary>
    public void UpgradeDamage()
    {
        ++_damage;
    }

    /// <summary>
    /// Increases the range of the skill by 1
    /// </summary>
    public void UpgradeRange()
    {
        ++_range;
        if (_maRef.SkillRef == this)
            _maRef.AttackRange = _range;
    }

    /// <summary>
    /// Increments cooldown by 1 so that it takes 1 more turn to get the skill back
    /// </summary>
    public void DowngradeCooldown()
    {
        // Make the cooldown higher
        ++_cooldown;
        // Also decrement the timer
        DecrementCooldownTimer();

        // Call the on cooldown change event
        OnCooldownChange?.Invoke();
    }

    /// <summary>
    /// Decreases the damage of the skill by 1
    /// </summary>
    public void DowngradeDamage()
    {
        --_damage;
    }

    /// <summary>
    /// Decreases the range of the skill by 1
    /// </summary>
    public void DowngradeRange()
    {
        --_range;
    }

    /// <summary>
    /// Sets the attack range of MoveAttack
    /// Called when the skill is equipped
    /// </summary>
    public void EquipSkill()
    {
        // Set the range
        _maRef.AttackRange = _range;
        Debug.Log(this.name + " has attack range " + _maRef.AttackRange);

        // Set if the skill is friendly or deadly
        _maRef.TargetFriendly = _healing;

        // Update the attack tiles
        if (_maRef.AttackTiles != null)
        {
            _maRef.CalculateAllTiles();
        }
    }

    /// <summary>
    /// Allows for it to overriden in other functions that spawn another
    /// prefab to play an animation
    /// </summary>
    public virtual void SpawnSkillAddition() { }

    /// <summary>
    /// Called if this skill is attached to an enemy from the EnemySkillController.
    /// Sets cooldown to 0
    /// </summary>
    public void SetAsEnemySkill()
    {
        _cooldown = 0;
    }
}
