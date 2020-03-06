using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveAttackGUIController : MonoBehaviour
{
    // Reference to the Buttons of the endTurnButton, character Portraits, etc. These are turned on and off when the user has control
    [SerializeField] private List<Button> buttonsToTurnOff = null;

    private MoveAttackController mAContRef = null;  // Reference to the MoveAttackController script
    private InputController inpContRef = null;  // Reference to the InputController script
    private MoveAttack charSelected;    // A reference to the selected character's MoveAttack script 

    private bool canSelect; // If the user can select things right now
    private Node nodeToAttack;  // Used with MoveAndAttack to keep track of who we should be attacking

    // For CamFollow
    private CamFollow camFollowRef;
    public bool areSelected1,areSelected2,areSelected3;
    
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

        GameObject mainCam = GameObject.FindWithTag("MainCamera");
        if (mainCam == null)
            Debug.Log("Could not find a GameObject with the tag MainCamera");
        else
        {
            camFollowRef = mainCam.GetComponent<CamFollow>();
            if (camFollowRef == null)
                Debug.Log("Could not find CamFollow attached to " + mainCam.name);
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        charSelected = null;
        ToggleSelect(true);
    }

    // Update is called once per frame
    // We use it to test for input and take the appropriate action
    private void Update()
    {
        CheckForSelection();
    }

    /// <summary>
    /// Called every frame, logics out what should happen upon a selection from the user
    /// </summary>
    private void CheckForSelection()
    {
        // If the user can select things right now and they tried to select something
        if (canSelect && inpContRef.SelectClick())
        {
            // Get the node that was just selected
            Node selectedNode = GetSelectedNode();
            // Deselect whatever charcter we had select if the user clicked somewhere that is not a node
            if (selectedNode == null)
            {
                Deselect();
                // Don't execute any more of this iteration, since the selected node is null
                return;
            }

            // If the user has not already selected some character
            if (charSelected == null)
            {
                AttemptSelect(selectedNode);
            }
            // Otherwise, they have a character selected already
            else
            {
                // If the selected node is the one housing the currently selected character
                MoveAttack mARef = mAContRef.GetCharacterMAByNode(selectedNode);
                if (mARef != null && mARef == charSelected)
                {
                    // If we aren't display stats, we want to do that
                    if (!mARef.MyStats.AreStatsDisplayed())
                    {
                        mARef.MyStats.DisplayStats(true);
                    }
                    // If we are, we want to stop
                    else
                    {
                        mARef.MyStats.DisplayStats(false);
                    }
                }
                // If the selected node contains an ally, deselect the current selected character, and select the new character
                // Or if the selected node contains an enemy and we have an enemy selected
                else if (selectedNode.occupying == CharacterType.Ally ||
                    (selectedNode.occupying == CharacterType.Enemy && charSelected.WhatAmI == CharacterType.Enemy))
                {
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
                        // Try to move/attack with them. If it fails (since the node was an invalid one to move/attack), try to select an enemy if its there
                        if (!AttemptMoveOrAttack(selectedNode))
                            AttemptSelect(selectedNode);
                        // If it was successful, we need to hide their stats
                        else
                            charSelected.MyStats.DisplayStats(false);
                    }
                    // If the character has moved, but not attacked yet
                    else if (!charSelected.HasAttacked)
                    {
                        // Try to attack, if successful, just deselect
                        if (AttemptAttack(selectedNode))
                            Deselect(); // Deselect the already selected character
                        // Otherwise, the node was invalid to attack, so try to select a character if there is one there
                        else
                        {
                            Deselect();
                            AttemptSelect(selectedNode);
                        }
                    }
                    // If they can do neither just deselect (this would probably never happen)
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
        if (charSelected == null)
            return;
        // Make sure this character is active before doing anything else
        if (!charSelected.gameObject.activeInHierarchy)
            return;
        // If it has one and hasn't moved this turn yet or hasn't attacked this turn
        if (!(charSelected.HasMoved && charSelected.HasAttacked))
        {
            mAContRef.SetActiveVisuals(charSelected);    // Set the visuals of it to be on
        }

        // Camera stuff
        // Update ally camera
        if (charSelected.WhatAmI == CharacterType.Ally)
        {
            if (charSelected.name == "Ally (1)")
            {
                areSelected1 = true;
            }
            else if (charSelected.name == "Ally (2)")
            {
                areSelected2 = true;
            }
            else if (charSelected.name == "Ally (3)")
            {
                areSelected3 = true;
            }
        }
        // Update enemy camera
        else if (charSelected.WhatAmI == CharacterType.Enemy)
        {
            camFollowRef.FollowEnemy(charSelected.transform);
        }

        /*
        // If the character has already moved, we dont really want to keep them selected, so deselect them
        if (charSelected.WhatAmI == CharacterType.Ally && charSelected.HasMoved && charSelected.HasAttacked)
        {
            Deselect();
        }
        */
    }
    
    /// <summary>
    /// Tries to start moving the selected character to the node that was just selected
    /// or tries to go to hit the enemy at the selected node
    /// </summary>
    /// <param name="selNode">The node that was just selected</param>
    /// <returns>Returns true if the character will do an action, false if they just got deselected</returns>
    private bool AttemptMoveOrAttack(Node selNode)
    {
        MoveAttack charAtNode = mAContRef.GetCharacterMAByNode(selNode);
        // If the current character can move there
        if (charSelected.MoveTiles.Contains(selNode) && selNode.occupying == CharacterType.None)
        {
            //Debug.Log("AttemptMove");
            ToggleSelect(false);    // Make it so that the player cannot select whilst something is moving
            // Calculate the pathing
            mAContRef.ResetPathing();
            Node startNode = mAContRef.GetNodeByWorldPosition(charSelected.transform.position);
            mAContRef.Pathing(startNode, selNode, charSelected.WhatAmI);
            // Start moving the character
            charSelected.StartMove();
            mAContRef.TurnOffVisuals(charSelected);

            return true;
        }
        // If the current character can attack there, and there is an (active) enemy there.
        // Then we want the current character to walk to the closest node to there and attack
        else if (charSelected.AttackTiles.Contains(selNode) && charAtNode != null && 
            charAtNode.WhatAmI == CharacterType.Enemy && charAtNode.gameObject.activeInHierarchy)
        {
            AttemptMoveAndAttack(selNode);
            return true;
        }
        // If neither, just deselect them
        else
        {
            Deselect();
            return false;
        }
    }

    /// <summary>
    /// Tries to attack something near itself
    /// </summary>
    /// <param name="selNode">The node that was selected</param>
    /// <returns>Returns true if the attack was successful, false otherwise</returns>
    private bool AttemptAttack(Node selNode)
    {
        // If the current character can attack there
        MoveAttack charToAttack = mAContRef.GetCharacterMAByNode(selNode);
        if (charSelected.AttackTiles.Contains(selNode) && selNode.occupying == CharacterType.Enemy && charToAttack != null && charToAttack.gameObject.activeInHierarchy)
        {
            ToggleSelect(false);    // Make it so that the player cannot select whilst something is attacking
            charSelected.StartAttack(selNode.position); // Start the attack
            return true;
        }
        return false;
    }

    /// <summary>
    /// Turns the visuals of the character off and no longer selects them
    /// </summary>
    private void Deselect()
    {
        if (charSelected != null)
        {
            mAContRef.TurnOffVisuals(charSelected);
            charSelected.MyStats.DisplayStats(false);
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
                ToggleSelect(false);    // Make it so that the player cannot select whilst something is attacking
                charSelected.StartAttack(nodeToAttack.position);
                // Unselect and untarget everything that we saved for this move attack
                nodeToAttack = null;
                Deselect();
            }
            // If the character isn't supposed to attack someone, show their visuals
            else
            {
                ToggleSelect(true);
                mAContRef.SetActiveVisuals(charSelected);
            }
        }
        else
        {
            ToggleSelect(true);
        }
    }

    /// <summary>
    /// Called from TurnSystem. Gets rid of the user's ability to interact with their characters while it is not their turn
    /// </summary>
    public void DenySelect()
    {
        ToggleSelect(false);    // Don't let the user select anything
        Deselect(); // Deselect anything that was selected
        // To be safe, reset any held references
        nodeToAttack = null;
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
        Node nodeToMoveTo = null; // The node that will be moved to
        int distToMoveNode = int.MaxValue; // The distance to the closest node
        Node charSelectedNode = mAContRef.GetNodeByWorldPosition(charSelected.transform.position); // Node the ally is on
        // Cross reference them against the nodes this character can move to until a match is found
        foreach (Node testNode in potNodes)
        {
            // We haven't done pathing, so we can't compare Fs, so we will just calculate it by actual distance
            int testNodeDist = Mathf.Abs(testNode.position.x - charSelectedNode.position.x) +
                Mathf.Abs(testNode.position.y - charSelectedNode.position.y);
            if (nodeToMoveTo != null)
                Debug.Log("Seeing if node at " + testNode.position + " is closer than " + nodeToMoveTo.position + "from " + charSelectedNode.position);
            else
                Debug.Log("Seeing if node at " + testNode.position + " is closer than null from " + charSelectedNode.position);
            // If the ally can move there or is already there and that node is closer than the closer than the current nodeToMoveTo
            if ((charSelected.MoveTiles.Contains(testNode) || testNode == charSelectedNode) && testNodeDist < distToMoveNode)
            {
                if (nodeToMoveTo != null)
                    Debug.Log("It was! Node at " + testNode.position + " was closer than " + nodeToMoveTo.position + " from " + charSelectedNode.position + " by " +
                        testNodeDist + " compared to " + distToMoveNode);
                else
                    Debug.Log("It was! Node at " + testNode.position + " was closer than null from " + charSelectedNode.position + " by " +
                        testNodeDist + " compared to " + distToMoveNode);
                nodeToMoveTo = testNode;
                distToMoveNode = testNodeDist;
            }
        }

        // Just make sure that the node exists
        if (nodeToMoveTo == null)
        {
            Debug.Log("Big trouble in MoveAttackGUIController. There was an enemy in range, but no valid tiles to attack them from");
            Deselect();
            return;
        }

        ToggleSelect(false);    // Make it so that the player cannot select whilst something is moving
        // Calculate the pathing
        mAContRef.ResetPathing();
        mAContRef.Pathing(charSelectedNode, nodeToMoveTo, charSelected.WhatAmI);
        // Start moving the character
        charSelected.StartMove();
        mAContRef.TurnOffVisuals(charSelected);

        // The attack will be started in AllowSelect, so that we don't attack until we actually reach the tile we were going to
    }

    /// <summary>
    /// Swaps canSelect to true or false. Also toggles the endTurnButton on and off
    /// </summary>
    /// <param name="onOff">Whether to let the user select or not</param>
    private void ToggleSelect(bool onOff)
    {
        canSelect = onOff;
        // Switch the buttons on or off
        foreach (Button butt in buttonsToTurnOff)
        {
            if (butt.enabled)
                butt.interactable = onOff;
        }
    }
}
