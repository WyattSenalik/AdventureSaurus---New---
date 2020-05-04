using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smite : Skill
{
    // The prefab that displays the animation
    private GameObject _spawnPref = null;
    // Reference tot he spawned prefab, for deletion purposes
    private GameObject _activeSkillAnimObj = null;

    // Called before start
    // Set the skill specific variables
    private new void Awake()
    {
        // Call the base version
        base.Awake();
        // Set skillNum to be the skillNumber of smite: 2
        _skillNum = 2;

        // These will change. Here are their base values
        _cooldown = 4;
        _range = 2;
        _damage = 2;

        // Load the spawn predfab
        _spawnPref = Resources.Load<GameObject>("SmitePrefab");
    }

    /// <summary>
    /// Starts the skills animation and gets a reference to the enemy that will be hit.
    /// </summary>
    /// <param name="attackNodePos">The position of the enemy being hit with smite</param>
    public override void StartSkill(Vector2Int attackNodePos)
    {
        // Initialize the list of targets
        _enemiesHP = new List<Health>();

        // Get the node to attack
        Node nodeToAttack = _mAContRef.GetNodeAtPosition(attackNodePos);
        // Make sure there is a node there
        if (nodeToAttack != null)
        {
            // Try to get the move attack script of the character to attack at that node
            MoveAttack charToAttack = _mAContRef.GetCharacterMAByNode(nodeToAttack);
            if (charToAttack != null)
            {
                // Try to get the health script off the character to attack
                Health charToAtkHPScript = charToAttack.GetComponent<Health>();
                if (charToAtkHPScript != null)
                    _enemiesHP.Add(charToAttack.GetComponent<Health>());
                else
                    Debug.Log(charToAttack.name + " has no Health script for smite to attack");

                // Start the skill's animation
                StartSkillAnimation(attackNodePos);

                //sound effect
                AudioManager smite = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
                smite.PlaySound("Smite");
            }
            else
                Debug.Log("Enemy to attack does not have a MoveAttack script attached to it");
        }
        else
        {
            Debug.Log("No node at " + attackNodePos + " for smite to be cast towards");
            EndSkill();
        }
    }

    /// <summary>
    /// Ends the skills animaton
    /// </summary>
    public override void EndSkill()
    {
        if (_enemiesHP != null)
        {
            // Deal the damage to each enemy
            for (int i = 0; i < _enemiesHP.Count; i++)
            {
                if (_enemiesHP[i] != null)
                {
                    _enemiesHP[i].TakeDamage(_damage, this.GetComponent<Stats>());
                }
            }
            // Get rid of the references of the enemies to hit, so that we do not hit them again on accident
            // after the next time this enemy moves
            _enemiesHP = new List<Health>();

            if (_maRef.WhatAmI == CharacterType.Ally)
                GoOnCooldown();
        }
    }

    /// <summary>
    /// Creates the prefab to do the smite
    /// Called from the animation
    /// </summary>
    public override void SpawnSkillAddition()
    {
        try
        {
            // Create the prefab to animate
            _activeSkillAnimObj = Instantiate(_spawnPref);
            // Center it on the character to attack
            _activeSkillAnimObj.transform.SetParent(_enemiesHP[0].transform);
            _activeSkillAnimObj.transform.localPosition = Vector3.zero;
            // Get the SpecialAttackSpawn script attached to it and set its Spawner
            SpecialAttackSpawn smiteSpawn = _activeSkillAnimObj.transform.GetChild(0).GetComponent<SpecialAttackSpawn>();
            smiteSpawn.SetSpawner(_maRef);
        }
        catch
        {
            Debug.LogError("Failed to create the SmiteSpawn " + _activeSkillAnimObj.transform.GetChild(0).name);
        }

        // Start the end skill animation
        EndSkillAnimation();
    }
}
