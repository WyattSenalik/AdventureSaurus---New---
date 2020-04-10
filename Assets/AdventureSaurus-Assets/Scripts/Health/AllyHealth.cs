using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllyHealth : Health
{
    // References to itself
    private AllySkillController _allySkillCont;

    // The side health bar info. Will be set from pause menu
    private Slider _sideSlider;
    public Slider SideSlider
    {
        set
        {
            _sideSlider = value;
            UpdateSideHealth();
        }
    }


    // Called when the component is set active
    // Subscribe to events
    private void OnEnable()
    {
        // When generation is done, do some initialization
        ProceduralGenerationController.OnFinishGenerationNoParam += SetReferences;
        ProceduralGenerationController.OnFinishGenerationNoParam += UpdateHealthDisplay;
        ProceduralGenerationController.OnFinishGenerationNoParam += UpdateSideHealth;
    }

    // Called when the component is toggled off
    // Unsubscribe from events
    private void OnDisable()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= SetReferences;
        ProceduralGenerationController.OnFinishGenerationNoParam -= UpdateHealthDisplay;
        ProceduralGenerationController.OnFinishGenerationNoParam -= UpdateSideHealth;
    }

    // Called when the gameobject is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= SetReferences;
        ProceduralGenerationController.OnFinishGenerationNoParam -= UpdateHealthDisplay;
        ProceduralGenerationController.OnFinishGenerationNoParam -= UpdateSideHealth;
    }

    // Called before Start
    // Set references
    private new void Awake()
    {
        // Call the base's version
        base.Awake();

        // Get the ally's stats
        _allySkillCont = this.GetComponent<AllySkillController>();
    }

    /// <summary>
    /// Decrements curHP by dmgToTake.
    /// Updates the side health slider value
    /// </summary>
    /// <param name="dmgToTake">Amount curHP will go down by</param>
    /// <param name="dmgDealer">The Stats of the character who dealt damage</param>
    public override void TakeDamage(int dmgToTake, Stats dmgDealer)
    {
        // Call the base's version
        base.TakeDamage(dmgToTake, dmgDealer);

        // Increment the cooldown of the ally's skill by one
        if (_allySkillCont.SpecialSkill != null)
            _allySkillCont.SpecialSkill.IncrementCooldownTimer();

        // Update the health bar on the side
        UpdateSideHealth();
    }

    /// <summary>
    /// Updates the value of the side health bar
    /// </summary>
    private void UpdateSideHealth()
    {
        if (_sideSlider != null)
            _sideSlider.value = ((float)CurHP) / MaxHP;
    }
    /// <summary>
    /// Consumes a charge of potion and heals player for 50% of max health
    /// </summary>
    public void ConsumePotion()
    {
        base.Heal(base.MaxHP / 2);

        // Update both health bars
        UpdateHealthDisplay();
        UpdateSideHealth();
    }
}
