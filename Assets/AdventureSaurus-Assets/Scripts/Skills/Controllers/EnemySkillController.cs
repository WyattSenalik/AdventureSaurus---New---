using UnityEngine;

// Controls what skill is active for an enemy
// Attached to every enemy
public class EnemySkillController : CharacterSkills
{
    // The (non-basic attack) skill of this character
    private Skill _specialSkill;
    public Skill GetSpecialSkill() { return _specialSkill; }

    // Called before the first frame
    // Initializes skill
    protected new void Start()
    {
        base.Start();
        // We want to set the enemies skill to the one skill they have
        Skill mySkill = this.GetComponent<Skill>();
        if (mySkill != null)
        {
            SetSkill(mySkill);
            mySkill.EquipSkill();
            _availableSkills.Add(mySkill);
        }
        if (mySkill.SkillNum != SkillHolder.BASIC_ATTACK)
            _specialSkill = mySkill;
        // See if we should increase magic at all
        EnemyGrimoire enGrim = this.GetComponent<EnemyGrimoire>();
        if (enGrim != null)
        {
            enGrim.MagicIncrease();
        }
    }
}
