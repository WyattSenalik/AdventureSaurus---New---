using UnityEngine;

public class EnemyGrimoire : Grimoire
{
    // Reference to the enemy's EnemySkillController
    private EnemySkillController _enemySkillContRef;


    // Called when the component is set active
    // Subscribe to events
    private void OnEnable()
    {
        // When generation is done, initialize
        ProceduralGenerationController.OnFinishGenerationNoParam += Initialize;
    }
    // Called when the component is toggled off
    // Unsubscribe from events
    private void OnDisable()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
    }
    // Called when the component is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
    }

    // Called before start
    // Set references to itself
    protected override void Awake()
    {
        base.Awake();

        _enemySkillContRef = this.GetComponent<EnemySkillController>();
        if (_enemySkillContRef == null)
            Debug.Log("There was no AllySkillController attached to " + this.name);
    }

    // Initialize some things after generation
    private void Initialize()
    {
        // See if we should increase magic
        MagicIncrease();
    }

    /// <summary>
    /// Increases the damage of the skill by one
    /// </summary>
    protected override void IncreaseDamage()
    {
        if (_enemySkillContRef.GetSpecialSkill() != null)
            _enemySkillContRef.GetSpecialSkill().UpgradeDamage();
    }

    /// <summary>
    /// Increases the range fo the skill by one
    /// </summary>
    protected override void IncreaseRange()
    {
        if (_enemySkillContRef.GetSpecialSkill() != null)
            _enemySkillContRef.GetSpecialSkill().UpgradeRange();
    }

    /// <summary>
    /// Lowers the cooldown of the skill by one
    /// </summary>
    protected override void LowerCooldown()
    {
        if (_enemySkillContRef.GetSpecialSkill() != null)
            _enemySkillContRef.GetSpecialSkill().UpgradeCooldown();
    }

    /// <summary>
    /// Grants the skill
    /// </summary>
    protected override void GainSkill()
    {
        Debug.LogError("Gain Skill not supported for EnemyGrimoire");
    }
}
