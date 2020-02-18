using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAttackGUIController : MonoBehaviour
{
    private MoveAttackController mAContRef = null;  // Reference to the MoveAttackController script
    private InputController inpContRef = null;  // Reference to the InputController script
    private MoveAttack charSelected;    // A reference to the selected character's MoveAttack script 
    private bool canSelect; // If the user can select things right now
    private bool awaitingChoice;    // Whether the user must make a choice on where to move after saying they want to attack without moving first
    private Node nodeToAttack;  // Used with MoveAndAttack to keep track of who we should be attacking

    // Set references
    private void Awake()
    {
        mAContRef = this.GetComponent<MoveAttackController>();
        if (mAContRef == null)
        {
            Debug.Log("Could not find MoveAttackController attached to " + this.name);
        }

        inpContRef = this.GetComponent<InputController>();
        if (inpContRef == null)
        {
            Debug.Log("Could not find InputController attached to " + this.name);
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        charSelected = null;
        canSelect = true;
    }

    // Update is called once per frame
    private void Update()
    {
        // If the user can select things right now
        if (canSelect)
        {
            // If the user has tried to select something
            if (inpContRef.SelectClick())
            {
                // If we are awaiting a choice about where to move, then attack with the currently selected character
                if (awaitingChoice)
                {
                    AttemptMoveAndAttack();
                    awaitingChoice = false;
                }
                else
                {
                    // If the user has not already selected some character
                    if (charSelected == null)
                    {
                        AttemptSelect();
                    }
                    // Otherwise, they have a character selected already
                    else
                    {
                        // If the selected character is an ally, try to move them to the location the user just selected
                        if (charSelected.WhatAmI == CharacterType.Ally)
                        {
                            // If the character has not moved yet
                            if (!charSelected.HasMoved)
                            {
                                AttemptMoveOrAttack();
                            }
                            // If the character has moved, but not attacked yet
                            else if (!charSelected.HasAttacked)
                            {
                                AttemptAttack();
                                Deselect(); // Deselect the already selected character
                            }
                        }
                        // If the character is not an ally, just deselect them
                        else
                        {
                            Deselect();
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Tries to select a character at the grid location the user just clicked on. If successful, show that characters move/attack ranges
    /// </summary>
    private void AttemptSelect()
    {
        Node selNode = GetSelectedNode();   // Try to select the node
        // If it exists
        if (selNode != null)
        {
            // Try to get the MoveAttack script off the character
            charSelected = mAContRef.GetCharacterMAByNode(selNode);
            // If it has one and hasn't moved this turn yet or hasn't attacked this turn
            if (charSelected != null && (!charSelected.HasMoved || !charSelected.HasAttacked))
            {
                charSelected.SetActiveVisuals(true);    // Set the visuals of it to be on
            }

            // If the character has already moved, we dont really want to keep them selected, so deselect them
            if (charSelected != null && charSelected.WhatAmI == CharacterType.Ally && charSelected.HasMoved && charSelected.HasAttacked)
            {
                Deselect();
            }
        }
    }
    
    /// <summary>
    /// Tries to start moving the selected character to the node that was just selected
    /// or tries to go to hit the enemy at the selected node
    /// </summary>
    private void AttemptMoveOrAttack()
    {
        Node selNode = GetSelectedNode();   // Try to select the node
        // If it exists
        if (selNode != null)
        {
            // Make sure that the player did not select the node the character is currently on, if they did, they shouldnt move
            // Get the character at the node, there shouldn't be one if they can move
            MoveAttack charAtNode = mAContRef.GetCharacterMAByNode(selNode);
            // If the charAtNode is the selected character
            if (charAtNode == charSelected)
            {
                Deselect();
                return;
            }

            // If the current character can move there
            if (charSelected.MoveTiles.Contains(selNode))
            {
                canSelect = false;  // Make it so that the player cannot select whilst something is moving
                // Calculate the pathing
                mAContRef.ResetPathing();
                mAContRef.Pathing(selNode, charSelected.WhatAmI);
                // Start moving the character
                charSelected.StartMove();
                charSelected.SetActiveVisuals(false);
            }
            // If the current character can attack there, and there is an enemy there.
            // Then we want to off the user a choice of the available places to move,
            // Once they select, move the character there and make them attack the enemy
            else if (charSelected.AttackTiles.Contains(selNode) && charAtNode != null && charAtNode.WhatAmI == CharacterType.Enemy)
            {
                Vector2Int attackPos = new Vector2Int(Mathf.RoundToInt(charAtNode.gameObject.transform.position.x), Mathf.RoundToInt(charAtNode.gameObject.transform.position.y));
                nodeToAttack = mAContRef.GetNodeAtPosition(attackPos);
                OfferMoveAttackOption(selNode);
            }
            // If neither, just deselect them
            else
            {
                Deselect();
            }
        }
        // If the node does not exist, just deselect
        else
        {
            Deselect();
        }
    }

    /// <summary>
    /// Tries to attack something near itself
    /// </summary>
    private void AttemptAttack()
    {
        Node selNode = GetSelectedNode();   // Try to select the node
        // If it exists
        if (selNode != null)
        {
            // If the current character can attack there
            if (charSelected.AttackTiles.Contains(selNode) && selNode.occupying == CharacterType.Enemy)
            {
                canSelect = false;  // Make it so that the player cannot select whilst something is attacking
                charSelected.StartAttack(selNode.position); // Start the attack
            }
        }
    }

    /// <summary>
    /// Turns the visuals of the character off and no longer selects them
    /// </summary>
    private void Deselect()
    {
        if (charSelected != null)
        {
            charSelected.SetActiveVisuals(false);
            charSelected = null;
        }
    }

    /// <summary>
    /// Gets the node at the current selection location
    /// </summary>
    /// <returns>Returns the node at the current selection location</returns>
    private Node GetSelectedNode()
    {
        Vector2Int pos = inpContRef.SelectToGridPoint();
        return mAContRef.GetNodeAtPosition(pos);
    }

    /// <summary>
    /// Called from MoveAttack by Allies after they finish moving.
    /// Allows the user to select again and recalculates all the visual tiles, since they have now changed
    /// </summary>
    public void AllowSelect()
    {
        // Recalculate all the visual tiles for the characters
        mAContRef.CreateAllVisualTiles();

        // If the user still has someone selected, show their new active visuals
        if (charSelected != null)
        {
            charSelected.SetActiveVisuals(true);

            // If the character just moved, and now must start attacking someone
            if (nodeToAttack != null)
            {
                canSelect = false;  // Make it so that the player cannot select whilst something is attacking
                charSelected.StartAttack(nodeToAttack.position);
                // Unselect and untarget everything that we saved for this move attack
                nodeToAttack = null;
                Deselect();
            }
        }

        canSelect = true;
    }

    /// <summary>
    /// Called from TurnSystem. Gets rid of the user's ability to interact with their characters while it is not their turn
    /// </summary>
    public void DenySelect()
    {
        canSelect = false;  // Don't let the user select anything
        Deselect(); // Deselect anything that was selected
        // To be safe, reset any held references
        nodeToAttack = null;
        awaitingChoice = false;
    }

    /// <summary>
    /// Resets what the character can move to to be the places they can attack the intended target
    /// </summary>
    private void OfferMoveAttackOption(Node selNode)
    {
        // Find what tiles can reach tile with an attack
        List<Node> reachTiles = mAContRef.GetNodesDistFromNode(selNode, charSelected.AttackRange);
        // Cross reference these tiles with the character's movement tiles and the resulting list is what the player can move to to attack
        List<Node> attackMoveTiles = new List<Node>();
        foreach (Node node in reachTiles)
        {
            if (charSelected.MoveTiles.Contains(node))
            {
                attackMoveTiles.Add(node);
            }
        }
        // Make these tiles what the character can move to, don't show their attack
        // and recalculate that character's movement tiles, displaying them afterwards
        charSelected.MoveTiles = attackMoveTiles;
        charSelected.AttackTiles = new List<Node>();
        charSelected.CreateVisualTiles(true);
        awaitingChoice = true;  // Used in Update
    }

    /// <summary>
    /// When the user has a ally selected and tries to select an enemy in range
    /// </summary>
    private void AttemptMoveAndAttack()
    {
        Node selNode = GetSelectedNode();   // Try to select the node

        // If the select exists and was a valid one
        if (selNode != null && charSelected.MoveTiles.Contains(selNode))
        {
            canSelect = false;  // Make it so that the player cannot select whilst something is moving
            // Calculate the pathing
            mAContRef.ResetPathing();
            mAContRef.Pathing(selNode, charSelected.WhatAmI);
            // Start moving the character
            charSelected.StartMove();
            charSelected.SetActiveVisuals(false);

            // The attack will be started in AllowSelect, so that we don't attack until we actually reach the tile we were going to
        }
        // If the node doesn't exists or is not valid, we need to reset all the tiles of that character, get rid of
        else
        {
            // Recalculate where the unit can move and attack, then reapply those changes to the unit
            charSelected.CalcMoveTiles();
            charSelected.CalcAttackTiles();
            charSelected.CreateVisualTiles(false);
            // Get rid of the enemy's node we were trying to attack
            nodeToAttack = null;
            // Deselect the ally unit
            Deselect();
        }
    }
}
