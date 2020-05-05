using System.Collections.Generic;
using UnityEngine;

public class ThreeSixtySwing : Skill
{
    // The prefab that displays the animation
    private GameObject _spawnPref;

    // Called before start
    // Set the skill specific variables
    private new void Awake()
    {
        base.Awake();
        // Set skillNum to be the skillNumber of threesixtyswing: 3
        _skillNum = 3;
        _diagnols = true;
        _cooldown = 4;

        _spawnPref = Resources.Load<GameObject>("ThreeSixtyPrefab");
    }

    /// <summary>
    /// Starts the animation for 360. Also gets refences to the enemies that got hit by skill, so that they can be damaged in endSkill.
    /// </summary>
    /// <param name="attackNodePos">The point the player used to start the skill. Dictates direction of animations.</param>
    override public void StartSkill(Vector2Int attackNodePos)
    {
        // Gets the primary node to attack
        Node nodeToAttack = _mAContRef.GetNodeAtPosition(attackNodePos);

        // If we have a node to attack
        if (nodeToAttack != null)
        {
            // Gets a initial reference point at the position of this charcter
            Vector2Int center = new Vector2Int(Mathf.RoundToInt(this.transform.position.x), Mathf.RoundToInt(this.transform.position.y));

            // List of the 8 tiles around character
            List<Node> areaOfAttack = new List<Node>();

            // 4 Adjacent tiles
            areaOfAttack.Add(_mAContRef.GetNodeAtPosition(center + Vector2Int.up));
            areaOfAttack.Add(_mAContRef.GetNodeAtPosition(center + Vector2Int.right));
            areaOfAttack.Add(_mAContRef.GetNodeAtPosition(center + Vector2Int.down));
            areaOfAttack.Add(_mAContRef.GetNodeAtPosition(center + Vector2Int.left));

            // These are the diagnol tiles
            areaOfAttack.Add(_mAContRef.GetNodeAtPosition(center + Vector2Int.up + Vector2Int.left));
            areaOfAttack.Add(_mAContRef.GetNodeAtPosition(center + Vector2Int.up + Vector2Int.right));
            areaOfAttack.Add(_mAContRef.GetNodeAtPosition(center + Vector2Int.down + Vector2Int.left));
            areaOfAttack.Add(_mAContRef.GetNodeAtPosition(center + Vector2Int.down + Vector2Int.right));

            // Initialize the list of enemies
            _enemiesHP = new List<Health>();

            // Iterates through the area of attack to test for enemies
            foreach (Node attack in areaOfAttack){

                MoveAttack charToAttack = _mAContRef.GetCharacterMAByNode(attack);
                // Checks if character exist on node
                if (charToAttack != null)
                {
                    //checks if charcter is an enemy
                    if (charToAttack.WhatAmI == CharacterType.Enemy)
                    {
                        // Get the health script from that enemy
                        Health charToAtkHPScript = charToAttack.GetComponent<Health>();

                        // Make sure they have a health script
                        if (charToAtkHPScript == null)
                            Debug.Log("Enemy to attack does not have a Health script attached to it");
                        else
                            _enemiesHP.Add(charToAttack.GetComponent<Health>());
                    }
                }
            }

            // If we have at least 1 enemy to attack
            if (_enemiesHP.Count > 0)
            {
                // Start the animation of the character
                StartSkillAnimation(attackNodePos);
            }
            else
                Debug.Log("There were no enemies for ThreeSixtySwing to hit");
        }
        else
        {
            //Debug.Log("Node to attack does not exist");
            EndSkill();
            return;
        }
    }

    /// <summary>
    /// Ends the skill animation. Deals damage to all participating enemies.
    /// </summary>
    public override void EndSkill()
    {
        // Validate that we have an enemy to attack
        if (_enemiesHP != null)
        {
            //Debug.Log("EndAnimation");
            // End the skills animation
            _audManRef.StopSound("360");
            EndSkillAnimation();


            //Debug.Log("Deal damage");
            // Deal the damage to each enemy
            for (int i = 0; i < _enemiesHP.Count; i++)
            {
                if (_enemiesHP[i] != null)
                {
                    _enemiesHP[i].TakeDamage(_damage, this.GetComponent<Stats>());
                }
            }
            //Debug.Log("Finished doing damage");
        }
        else
        {
            // We should not attack anything, so set attack animation to 0
            _anime.SetInteger("AttackDirection", 0);
        }

        // Get rid of the references of the enemies to hit, so that we do not hit them again on accident
        // after the next time this enemy moves
        _enemiesHP = new List<Health>();

        // Go on cooldown
        if (_maRef.WhatAmI == CharacterType.Ally)
            GoOnCooldown();
    }

    /// <summary>
    /// Creates the prefab to do the animation
    /// Called from the animation
    /// </summary>
    public override void SpawnSkillAddition()
    {
        // Create the prefabs to animate
        foreach (Health enemyHPRef in _enemiesHP)
        {
            Transform enTrans = enemyHPRef.transform;
            // Create the prefab
            GameObject curPref = Instantiate(_spawnPref);
            // Center it on the character
            curPref.transform.SetParent(enTrans);
            curPref.transform.localPosition = Vector3.zero;
        }
    }
}
