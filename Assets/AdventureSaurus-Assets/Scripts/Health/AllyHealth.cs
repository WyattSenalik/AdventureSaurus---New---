﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllyHealth : Health
{
    // The side health bar info. Will be set from pause menu
    private Slider _sideSlider;
    public Slider SideSlider
    {
        set { _sideSlider = value; }
    }


    // Called when the component is set active
    // Subscribe to events
    private void OnEnable()
    {
        // When generation is done, do some initialization
        ProceduralGenerationController.OnFinishGenerationNoParam += SetReferences;
        ProceduralGenerationController.OnFinishGenerationNoParam += UpdateHealthDisplay;
    }

    // Called when the component is toggled off
    // Unsubscribe from events
    private void OnDisable()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= SetReferences;
        ProceduralGenerationController.OnFinishGenerationNoParam -= UpdateHealthDisplay;
    }

    // Called when the gameobject is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= SetReferences;
        ProceduralGenerationController.OnFinishGenerationNoParam -= UpdateHealthDisplay;
    }

    // Called before Start
    // Set references
    private new void Awake()
    {
        // Call the base's version
        base.Awake();

        // These will need to be set a few times [allies only]
        SetReferences();
    }

    /// <summary>
    /// Set References
    /// </summary>
    private void SetReferences()
    {
        GameObject gameManagerObj = GameObject.FindWithTag("GameController");
        if (gameManagerObj != null)
        {
            _mAContRef = gameManagerObj.GetComponent<MoveAttackController>();
            if (_mAContRef == null)
                Debug.Log("Could not find MoveAttackController attached to " + gameManagerObj.name);
        }
        else
            Debug.Log("Could not find any GameObject with the tag GameController");
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

        // Will be null for enemies
        if (_sideSlider != null)
            _sideSlider.value = ((float)CurHP) / MaxHP;
    }
}
