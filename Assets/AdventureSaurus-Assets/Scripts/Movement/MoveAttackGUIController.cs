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
    // We use it to test for input and take the appropriate action
    private void Update()
    {
        // If the user can select things right now and they tried to select something
        if (canSelect && inpContRef.SelectClick())
        {
            // Get the node that was just selected
            Node selectedNode = GetSelectedNode();
            // Deselect whatever was selected
            if (selectedNode == null)
            {
                Deselect();
                // Don't execute any more of this iteration, since the selected node is null
                return;
            }

            // If the user has not already selected some character
            if (charSelected == null)
            {
                Debug.Log("Attempt Select");
                AttemptSelect(selectedNode);
            }
            // Otherwise, they have a character selected already
            else
            {
                // If the selected node contains an ally, deselect the current selected character, and select the new character
                if (selectedNode.occupying == CharacterType.Ally)
                {
                    MoveAttack mARef = mAContRef.GetCharacterMAByNode(selectedNode);
                    if (mARef == null)
                        Debug.Log("There was no MoveAttack script associated with this node, yet it was occupied by an Ally");
                    // If that ally is not the currently selected ally, attempt to select them
                    if (charSelected == null || mARef.gameObject != charSelected.gameObject)
                    {
                        Deselect();
                        AttemptSelect(selectedNode);
                    }
                    // Otherwise, just deselect
                    else
                    {
                        Deselect();
                    }
                }
                // Otherwise, If the currently selected character is an ally, try to move them to the location the user just selected or attack something there
                else if (charSelected.WhatAmI == CharacterType.Ally)
                {
                    // If the character has not moved yet
                    if (!charSelected.HasMoved)
                    {
                        AttemptMoveOrAttack(selectedNode);
                    }
                    // If the character has moved, but not attacked yet
                    else if (!charSelected.HasAttacked)
                    {
                        AttemptAttack(selectedNode);
                        Deselect(); // Deselect the already selected character
                    }
                    // If they can do neither, just deselect
                    else
                    {
                        Deselect();
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

    /// <summary>
    /// Tries to select a character at the grid location the user just clicked on. If successful, show that characters move/attack ranges
    /// </summary>
    /// <param name="selNode">The node that was just selected</param>
    private void AttemptSelect(Node selNode)
    {
        // Try to get the MoveAttack script off the character
        charSelected = mAContRef.GetCharacterMAByNode(selNode);
        // If it has one and hasn't moved this turn yet or hasn't attacked this turn
        if (charSelected != null && !(charSelected.HasMoved && charSelected.HasAttacked))
        {
            mAContRef.SetActiveVisuals(charSelected);    // Set the visuals of it to be on
        }

        // If the character has already moved, we dont really want to keep them selected, so deselect them
        if (charSelected != null && charSelected.WhatAmI == CharacterType.Ally && charSelected.HasMoved && charSelected.HasAttacked)
        {
            Deselect();
        }
    }
    
    /// <summary>
    /// Tries to start moving the selected character to the node that was just selected
    /// or tries to go to hit the enemy at the selected node
    /// </summary>
    /// <param name="selNode">The node that was just selected</param>
    private void AttemptMoveOrAttack(Node selNode)
    {
        MoveAttack charAtNode = mAContRef.GetCharacterMAByNode(selNode);
        // If the current character can move there
        if (charSelected.MoveTiles.Contains(selNode) && selNode.occupying == CharacterType.None)
        {
            canSelect = false;  // Make it so that the player cannot select whilst something is moving
            // Calculate the pathing
            mAContRef.ResetPathing();
            mAContRef.Pathing(selNode, charSelected.WhatAmI);
            // Start moving the character
            charSelected.StartMove();
            mAContRef.TurnOffVisuals(charSelected);
        }
        // If the current character can attack there, and there is an enemy there.
        // Then we want the current character to walk to the closest node to there and attack
        else if (charSelected.AttackTiles.Contains(selNode) && charAtNode != null && charAtNode.WhatAmI == CharacterType.Enemy)
        {
            AttemptMoveAndAttack(selNode);
        }
        // If neither, just deselect them
        else
        {
            Deselect();
        }
    }

    /// <summary>
    /// Tries to attack something near itself
    /// </summary>
    /// <param name="selNode">The node that was selected</param>
    private void AttemptAttack(Node selNode)
    {
        // If the current character can attack there
        if (charSelected.AttackTiles.Contains(selNode) && selNode.occupying == CharacterType.Enemy)
        {
            canSelect = false;  // Make it so that the player cannot select whilst something is attacking
            charSelected.StartAttack(selNode.position); // Start the attack
        }
    }

    /// <summary>
    /// Turns the visuals of the character off and no longer selects them
    /// </summary>
    private void Deselect()
    {
        if (charSelected != null)
        {
            mAContRef.TurnOffVisuals(charSelected);
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
        // Recalculate all the moveattack tiles for the characters
        mAContRef.RecalculateAllMovementAttackTiles();

        // If the user still has someone selected, show their new active visuals
        if (charSelected != null)
        {
            // If the character just moved, and now must start attacking someone
            if (nodeToAttack != null)
            {
                canSelect = false;  // Make it so that the player cannot select whilst something is attacking
                charSelected.StartAttack(nodeToAttack.position);
                // Unselect and untarget everything that we saved for this move attack
                nodeToAttack = null;
                Deselect();
            }
            // If the character isn't supposed to select someone, show their visuals
            else
            {
                mAContRef.SetActiveVisuals(charSelected);
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
    /// When the user has a ally selected and tries to select an enemy in range
    /// </summary>
    /// <param name="selNode">The node that the ally is trying to attack</param>
    private void AttemptMoveAndAttack(Node selNode)
    {
        // Set the node to attack
        nodeToAttack = selNode;

        // Find out the node attackRange away from selNode that is closest to the charSelected
        // Get the viable nodes
        List<Node> potNodes = mAContRef.GetNodesDistFromNode(selNode, charSelected.AttackRange);
        Node nodeToMoveTo = null;
        Node charSelectedNode = mAContRef.GetNodeByWorldPosition(charSelected.transform.position);
        // Cross reference them against the nodes this character can move to until a match is found
        foreach (Node testNode in potNodes)
        {
            if (charSelected.MoveTiles.Contains(testNode) || testNode == charSelectedNode)
            {
                nodeToMoveTo = testNode;
                break;
            }
        }

        // Just make sure that the node exists
        if (nodeToMoveTo == null)
        {
            Debug.Log("Big trouble in MoveAttackGUIController. There was an enemy in range, but no valid tiles to attack them from");
            Deselect();
            return;
        }

        canSelect = false;  // Make it so that the player cannot select whilst something is moving
        // Calculate the pathing
        mAContRef.ResetPathing();
        mAContRef.Pathing(nodeToMoveTo, charSelected.WhatAmI);
        // Start moving the character
        charSelected.StartMove();
        mAContRef.TurnOffVisuals(charSelected);

        // The attack will be started in AllowSelect, so that we don't attack until we actually reach the tile we were going to
    }
}
