using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Push : Skill
{
    // The node that we are attacking
    private Node _attackNode;
    // How fast we push the charctet
    [SerializeField] private float _pushSpeed = 2f;

    // Called before start
    // Set the skill specific variables
    private new void Awake()
    {
        base.Awake();
        // Set skillNum to be the skillNumber of push: 4
        _skillNum = 4;
        _range = 1;
        _cooldown = 0;
    }

    /// <summary>
    /// Initializes some things for children
    /// Set damage so that it looks correct upon investigating the skill
    /// </summary>
    protected override void Initialize()
    {
        _damage = _statsRef.GetStrength();
    }

    /// <summary>
    /// Starts the skills animation and gets a reference to the enemy that will be hit.
    /// </summary>
    /// <param name="attackNodesPos">The position of the node that will be attacked</param>
    override public void StartSkill(Vector2Int attackNodePos)
    {
        // Initialize the list of targets
        _enemiesHP = new List<Health>();
        // Get the damage
        _damage = _statsRef.GetStrength();

        // We have to set the enemy to attack, we just need to validate a bit first
        _attackNode = _mAContRef.GetNodeAtPosition(attackNodePos);
        
        if (_attackNode != null)
        {
            MoveAttack charToAttack = _mAContRef.GetCharacterMAByNode(_attackNode);
            if (charToAttack != null)
            {
                // Add the one enemy we will be pushing
                _enemiesHP.Add(charToAttack.GetComponent<Health>());
                if (_enemiesHP[0] == null)
                    Debug.Log("Character to push does not have a Health script attached to it");

                // Start the skill's animation
                StartSkillAnimation(attackNodePos);
            }
            else
                Debug.Log("Enemy to attack does not have a MoveAttack script attached to it");
        }
        // This occurs when an enemy isn't close enough to an ally to attack. Call end attack and don't start playing an animation
        else
        {
            EndSkill();
            return;
        }
    }

    /// <summary>
    /// Ends the skills animaton and pushes the enemy one space back.
    /// If the enemy being pushed cannot move that one space back, they will take damage instead.
    /// </summary>
    override public void EndSkill()
    {
        // Validate that we have an enemy to attack
        if (_enemiesHP != null && _enemiesHP.Count > 0 && _enemiesHP[0] != null && _attackNode != null)
        {
            //Debug.Log("Ending attack on " + enemiesHP[0].name);
            //sound effect
            _audManRef.PlaySound("Bump");
            
            // End the skills animation
            EndSkillAnimation();

            // Peak at the done "behind" the character to attack. If it is open, push the enemy
            // If it is not open, deal some damage
            Node standingNode = _mAContRef.GetNodeByWorldPosition(this.transform.position);
            Vector2Int direction = _attackNode.Position - standingNode.Position;
            Node pushToNode = _mAContRef.GetNodeAtPosition(_attackNode.Position + direction);
            if (pushToNode != null)
            {
                // If the node is empty, we push the character there
                if (pushToNode.Occupying == CharacterType.None)
                {
                    StartCoroutine(PushCharacter(_enemiesHP[0].transform, pushToNode));
                }
                // If they can't be pushed, deal damage
                else
                {
                    _enemiesHP[0].TakeDamage(_damage, this.GetComponent<Stats>());
                }
            }
            // If they cannot be pushed there, deal damage
            else
            {
                _enemiesHP[0].TakeDamage(_damage, this.GetComponent<Stats>());
            }


            // Reduce the special skill's cooldown by 1
            // If this is an ally
            if (_maRef.WhatAmI == CharacterType.Ally)
            {
                // Get the AllySkillController
                AllySkillController allySkillContRef = _maRef.GetComponent<AllySkillController>();
                if (allySkillContRef == null)
                {
                    Debug.Log("There is no AllySkillController attached to " + this.name);
                }
                // Increment the cooldown of special skill
                if (allySkillContRef.SpecialSkill != null)
                    allySkillContRef.SpecialSkill.IncrementCooldownTimer();
            }
        }
        // If we have no enemy to attack, give back control to the proper authority
        else
        {
            // We should not attack anything, so set attack animation to 0
            _anime.SetInteger("AttackDirection", 0);
        }

        // Get rid of the references of the enemies to hit, so that we do not hit them again on accident
        // after the next time this enemy moves
        _enemiesHP = new List<Health>();

        if (_maRef.WhatAmI == CharacterType.Ally)
            GoOnCooldown();
    }

    /// <summary>
    /// Increments the position of the charToPush by a little bit
    /// </summary>
    /// <param name="charToPush">The transform we will be changing</param>
    /// <param name="pushToNode">The node we will be pushing the character to</param>
    /// <returns>IEnumerator</returns>
    private IEnumerator PushCharacter(Transform charToPush, Node pushToNode)
    {
        // Add an ongoing action
        MoveAttack.AddOngoingAction();

        // Get the starting node so that we can change what's "there" after the push
        Node startNode = _mAContRef.GetNodeByWorldPosition(charToPush.transform.position);
        Vector2Int roundedPos = new Vector2Int(Mathf.RoundToInt(charToPush.transform.position.x),
            Mathf.RoundToInt(charToPush.transform.position.y));
        Vector2Int pushDirVect = new Vector2Int(0, 0);
        // Determine direction to push the character, technically, it should only be one of them
        // X factor
        if (roundedPos.x < pushToNode.Position.x)
            pushDirVect.x = 1;
        else if (roundedPos.x > pushToNode.Position.x)
            pushDirVect.x = -1;
        else
            pushDirVect.x = 0;
        // Y factor
        if (roundedPos.y < pushToNode.Position.y)
            pushDirVect.y = 1;
        else if (roundedPos.y > pushToNode.Position.y)
            pushDirVect.y = -1;
        else
            pushDirVect.y = 0;
        // Get the amount we will increment by each push
        Vector3 incrementVect = new Vector3(pushDirVect.x, pushDirVect.y, 0) * Time.deltaTime * _pushSpeed;
        float distance = Mathf.Abs(charToPush.transform.position.x - pushToNode.Position.x +
            charToPush.transform.position.y - pushToNode.Position.y);
        float lastDist = distance;
        // Start pushing the character that way
        // When the distance between the tiles starts to grow, stop incrementing the character
        while (lastDist >= distance)
        {
            // Move the character a little
            charToPush.transform.position += incrementVect;

            // Set the last distance
            lastDist = distance;
            // Recalculate the distance
            distance = Mathf.Abs(charToPush.transform.position.x - pushToNode.Position.x +
                charToPush.transform.position.y - pushToNode.Position.y);

            yield return null;
        }

        // Fix the character in place
        charToPush.transform.position = new Vector3(pushToNode.Position.x, pushToNode.Position.y, 0);
        // Update the node it came from and where it ended
        pushToNode.Occupying = startNode.Occupying;
        startNode.Occupying = CharacterType.None;

        // Remove the ongoing action
        MoveAttack.RemoveOngoingAction();

        yield return null;
    }
}
