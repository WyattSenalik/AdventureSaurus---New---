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
            StartUpdateSideHealth();
        }
    }


    // Called when the component is set active
    // Subscribe to events
    private void OnEnable()
    {
        // When generation is done, do some initialization
        ProceduralGenerationController.OnFinishGenerationNoParam += SetReferences;
        ProceduralGenerationController.OnFinishGenerationNoParam += UpdateHealthDisplay;
        ProceduralGenerationController.OnFinishGenerationNoParam += StartUpdateSideHealth;
    }

    // Called when the component is toggled off
    // Unsubscribe from events
    private void OnDisable()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= SetReferences;
        ProceduralGenerationController.OnFinishGenerationNoParam -= UpdateHealthDisplay;
        ProceduralGenerationController.OnFinishGenerationNoParam -= StartUpdateSideHealth;
    }

    // Called when the gameobject is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= SetReferences;
        ProceduralGenerationController.OnFinishGenerationNoParam -= UpdateHealthDisplay;
        ProceduralGenerationController.OnFinishGenerationNoParam -= StartUpdateSideHealth;
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
        StartUpdateSideHealth();
    }

    /// <summary>
    /// Increments curHP by healAmount.
    /// Also updates the heal bars
    /// </summary>
    /// <param name="healAmount">the amount to heal</param>
    /// <returns>Returns true if the unit was healed. False otherwise (they had fullHP or a negative value was passed in)</returns>
    public override bool Heal(int healAmount)
    {
        // If the heal was succesful update the side health bar and return true
        if (base.Heal(healAmount))
        {
            StartUpdateSideHealth();
            return true;
        }
        // If the heal was not successful, return false
        else
            return false;
    }

    /// <summary>
    /// Starts the coroutine which updates the value of the side health bar
    /// </summary>
    private void StartUpdateSideHealth()
    {
        if (_sideSlider != null)
            StartCoroutine(UpdateSideHealth());
    }

    /// <summary>
    /// Updates the value of the side health bar
    /// </summary>
    private IEnumerator UpdateSideHealth()
    {
        // Add the ongoing action
        MoveAttack.AddOngoingAction();
        // Get the target amount to work towards
        float targetAm = ((float)CurHP) / MaxHP;
        // If we are lower than the current amount
        while (_sideSlider.value < targetAm)
        {
            _sideSlider.value += Time.deltaTime;
            yield return null;
        }
        // If we are higher than the current amount
        while (_sideSlider.value > targetAm)
        {
            _sideSlider.value -= Time.deltaTime;
            yield return null;
        }

        // Just set it to what it is supposed to be
        _sideSlider.value = targetAm;

        // Remove the ongoing action
        MoveAttack.RemoveOngoingAction();

        yield return null;
    }
    /// <summary>
    /// Consumes a charge of potion and heals player for 50% of max health
    /// </summary>
    public void ConsumePotion()
    {
        Heal(base.MaxHP / 2);
    }
}
