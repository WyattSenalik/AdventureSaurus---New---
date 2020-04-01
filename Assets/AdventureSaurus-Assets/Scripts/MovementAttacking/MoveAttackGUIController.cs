using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveAttackGUIController : MonoBehaviour
{
    // Reference to the Buttons of the endTurnButton, character Portraits, etc. These are turned on and off when the user has control
    [SerializeField] private List<Button> _buttonsToTurnOff = null;
    // Refernce to the side hp and exp bars. These are also turned on and off when the user has control
    [SerializeField] private Image[] _imagesToTurnOff = null;

    // Reference to the MoveAttackController script
    private MoveAttackController _mAContRef = null;
    // Reference to the InputController script
    private InputController _inpContRef = null;
    // A reference to the selected character's MoveAttack script 
    private MoveAttack _charSelected;
    public MoveAttack CharSelectedMA
    {
        get { return _charSelected; }
    }

    // If the user can select things right now
    private bool _canSelect;
    // Used with MoveAndAttack to keep track of who we should be attacking
    private Node _nodeToAttack;

    // Events
    // Event for when a character is selected
    public delegate void CharacterSelect(MoveAttack charMARef);
    public static event CharacterSelect OnCharacterSelect;
    // Event for when a character is deslected
    public delegate void CharacterDeselect(MoveAttack charMARef);
    public static event CharacterDeselect OnCharacterDeselect;
    // Event for when the player is allowed to select again
    public delegate void PlayerAllowedSelected();
    public static event PlayerAllowedSelected OnPlayerAllowedSelect;

    // Called when the gameobject is toggled on
    // Subscribe to events
    private void OnEnable()
    {
        // When the player's turn begins, allow them to select things
        TurnSystem.OnBeginPlayerTurn += AllowSelect;
        // When the player's turn ends, deselect the selected character
        TurnSystem.OnFinishPlayerTurn += Deselect;
        // When the enemy's turn begins, deny the user from selecting
        TurnSystem.OnBeginEnemyTurn += DenySelect;

        // When the game is paused, disable this script
        Pause.OnPauseGame += HideScript;
        // Unsubscribe to the unpause event (since if this is active, the game is unpaused)
        Pause.OnUnpauseGame -= ShowScript;
    }

    // Called when the gameobject is toggled off
    // Unsubscribe to events
    private void OnDisable()
    {
        TurnSystem.OnBeginPlayerTurn -= AllowSelect;
        TurnSystem.OnFinishPlayerTurn -= Deselect;
        TurnSystem.OnBeginEnemyTurn -= DenySelect;

        // Unsubscribe to the pause event (since if this is inactive, the game is paused)
        Pause.OnPauseGame -= HideScript;
        // When the game is unpaused, re-enable this script
        Pause.OnUnpauseGame += ShowScript;
    }

    // Called when the gameobject is destroyed
    // Unsubscribe to ALL events
    private void OnDestroy()
    {
        TurnSystem.OnBeginPlayerTurn -= AllowSelect;
        TurnSystem.OnBeginEnemyTurn -= DenySelect;
        TurnSystem.OnFinishPlayerTurn -= Deselect;
        Pause.OnPauseGame -= HideScript;
        Pause.OnUnpauseGame -= ShowScript;
        MoveAttack.OnCharacterFinishedMoving -= ReturnControlAfterMove;
        MoveAttack.OnCharacterFinishedMoving -= BeginAttackAfterMove;
        MoveAttack.OnCharacterFinishedAction -= ReturnControlAfterAction;
    }

    // Set references
    private void Awake()
    {
        _mAContRef = this.GetComponent<MoveAttackController>();
        if (_mAContRef == null)
        {
            Debug.Log("Could not find MoveAttackController attached to " + this.name);
        }

        _inpContRef = this.GetComponent<InputController>();
        if (_inpContRef == null)
        {
            Debug.Log("Could not find InputController attached to " + this.name);
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        _charSelected = null;
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
        if (_canSelect && _inpContRef.SelectClick())
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
            if (_charSelected == null)
            {
                AttemptSelect(selectedNode);
            }
            // Otherwise, they have a character selected already
            else
            {
                // If the selected node is the one housing the currently selected character
                MoveAttack mARef = _mAContRef.GetCharacterMAByNode(selectedNode);
                if (mARef != null && mARef == _charSelected)
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
                else if (selectedNode.Occupying == CharacterType.Ally ||
                    (selectedNode.Occupying == CharacterType.Enemy && _charSelected.WhatAmI == CharacterType.Enemy))
                {
                    if (mARef == null)
                        Debug.Log("There was no MoveAttack script associated with this node, yet it was occupied by an Ally");
                    // If that ally is not the currently selected ally, attempt to select them
                    if (_charSelected == null || mARef.gameObject != _charSelected.gameObject)
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
                else if (_charSelected.WhatAmI == CharacterType.Ally)
                {
                    // If the character has not moved yet
                    if (!_charSelected.HasMoved)
                    {
                        // Try to move/attack with them. If it fails (since the node was an invalid one to move/attack), try to select an enemy if its there
                        if (!AttemptMoveOrAttack(selectedNode))
                            AttemptSelect(selectedNode);
                        // If it was successful, we need to hide their stats
                        else
                            _charSelected.MyStats.DisplayStats(false);
                    }
                    // If the character has moved, but not attacked yet
                    else if (!_charSelected.HasAttacked)
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
        _charSelected = _mAContRef.GetCharacterMAByNode(selNode);
        // Make sure the MoveAttack script is valid and that the character is active in the hierarchy
        if (_charSelected != null && _charSelected.gameObject.activeInHierarchy)
        {
            // If it hasn't moved this turn yet or hasn't attacked this turn
            if (!(_charSelected.HasMoved && _charSelected.HasAttacked))
            {
                //Debug.Log("Select character");
                // Calculate its visuals
                _charSelected.CalcMoveTiles();
                _charSelected.CalcAttackTiles();
                // Set the visuals of it to be on
                _mAContRef.SetActiveVisuals(_charSelected);
            }

            // Call the character selected event
            if (OnCharacterSelect != null)
                OnCharacterSelect(_charSelected);
        }
    }
    
    /// <summary>
    /// Tries to start moving the selected character to the node that was just selected
    /// or tries to go to hit the enemy at the selected node
    /// </summary>
    /// <param name="selNode">The node that was just selected</param>
    /// <returns>Returns true if the character will do an action, false if they just got deselected</returns>
    private bool AttemptMoveOrAttack(Node selNode)
    {
        MoveAttack charAtNode = _mAContRef.GetCharacterMAByNode(selNode);
        // If the current character can move there
        if (_charSelected.MoveTiles.Contains(selNode) && selNode.Occupying == CharacterType.None)
        {
            // Start moving the ally
            Node startNode = _mAContRef.GetNodeByWorldPosition(_charSelected.transform.position);
            DoMove(startNode, selNode);

            // We want the user to be able to select after moving, so
            // when the ally finishes moving, return control
            MoveAttack.OnCharacterFinishedMoving += ReturnControlAfterMove;

            return true;
        }
        // If the current character can attack there, and there is an (active) enemy there.
        // Then we want the current character to walk to the closest node to there and attack
        else if (_charSelected.AttackTiles.Contains(selNode) && charAtNode != null && 
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
        MoveAttack charToAttack = _mAContRef.GetCharacterMAByNode(selNode);
        if (_charSelected.AttackTiles.Contains(selNode) && selNode.Occupying == CharacterType.Enemy
            && charToAttack != null && charToAttack.gameObject.activeInHierarchy)
        {
            // Set the node to attack
            _nodeToAttack = selNode;

            // Have the ally attack that node
            DoAttack();

            // When the ally finishes their attack, return control to the user
            MoveAttack.OnCharacterFinishedAction += ReturnControlAfterAction;

            return true;
        }
        return false;
    }


    /// <summary>
    /// When the user has a ally selected and tries to select an enemy in range
    /// </summary>
    /// <param name="selNode">The node that the ally is trying to attack</param>
    private void AttemptMoveAndAttack(Node selNode)
    {
        // Set the node to attack
        _nodeToAttack = selNode;

        // Find out the node attackRange away from selNode that is closest to the charSelected
        // Get the viable nodes
        List<Node> potNodes = _mAContRef.GetNodesDistFromNode(selNode, _charSelected.AttackRange);
        Node nodeToMoveTo = null; // The node that will be moved to
        int distToMoveNode = int.MaxValue; // The distance to the closest node
        Node charSelectedNode = _mAContRef.GetNodeByWorldPosition(_charSelected.transform.position); // Node the ally is on
        // Cross reference them against the nodes this character can move to until a match is found
        foreach (Node testNode in potNodes)
        {
            // We haven't done pathing, so we can't compare Fs, so we will just calculate it by actual distance
            int testNodeDist = Mathf.Abs(testNode.Position.x - charSelectedNode.Position.x) +
                Mathf.Abs(testNode.Position.y - charSelectedNode.Position.y);
            //if (nodeToMoveTo != null)
            //    Debug.Log("Seeing if node at " + testNode.Position + " is closer than " + nodeToMoveTo.Position + "from " + charSelectedNode.Position);
            //else
            //    Debug.Log("Seeing if node at " + testNode.Position + " is closer than null from " + charSelectedNode.Position);
            // If the ally can move there or is already there and that node is closer than the closer than the current nodeToMoveTo
            if ((_charSelected.MoveTiles.Contains(testNode) || testNode == charSelectedNode) && testNodeDist < distToMoveNode)
            {
                //if (nodeToMoveTo != null)
                //    Debug.Log("It was! Node at " + testNode.Position + " was closer than " + nodeToMoveTo.Position + " from " + charSelectedNode.Position + " by " +
                //        testNodeDist + " compared to " + distToMoveNode);
                //else
                //    Debug.Log("It was! Node at " + testNode.Position + " was closer than null from " + charSelectedNode.Position + " by " +
                //        testNodeDist + " compared to " + distToMoveNode);
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

        // Move the character
        DoMove(charSelectedNode, nodeToMoveTo);

        // When the ally finished moving, attack
        MoveAttack.OnCharacterFinishedMoving += BeginAttackAfterMove;
    }

    /// <summary>
    /// Begins the selected ally moving
    /// </summary>
    /// <param name="startNode">Node to start moving from</param>
    /// <param name="endNode">Node to move to</param>
    private void DoMove(Node startNode, Node endNode)
    {
        // Make it so that the player cannot select whilst something is moving
        ToggleSelect(false);
        // Calculate the pathing
        _mAContRef.Pathing(startNode, endNode, _charSelected.WhatAmI);
        // Start moving the character
        _charSelected.StartMove();
        _mAContRef.TurnOffVisuals(_charSelected);
    }

    /// <summary>
    /// Begins the selected ally attacking
    /// </summary>
    private void DoAttack()
    {
        // Make it so that the player cannot select whilst something is attacking
        ToggleSelect(false);
        // Start the attack
        _charSelected.StartAttack(_nodeToAttack.Position);
        // Unselect and untarget everything that we saved for this move attack
        _nodeToAttack = null;
        Deselect();
    }

    /// <summary>
    /// Begins the selected character's attack after they move
    /// Called by OnCharacterFinishedMoving event in MoveAttack
    /// </summary>
    private void BeginAttackAfterMove()
    {
        // Remove itself from the OnCharacterFinishedMoving event
        MoveAttack.OnCharacterFinishedMoving -= BeginAttackAfterMove;

        // Do the attack
        DoAttack();

        // When the ally finishes their attack, return control to the user
        MoveAttack.OnCharacterFinishedAction += ReturnControlAfterAction;
    }

    /// <summary>
    /// Returns control to the user after an ally finishes its action
    /// Called by OnCharacterFinishedAction event in MoveAttack
    /// </summary>
    private void ReturnControlAfterAction()
    {
        // Remove itself from the OnCharacterFinishedAction event
        MoveAttack.OnCharacterFinishedAction -= ReturnControlAfterAction;

        ToggleSelect(true);
        AllowSelect();
    }

    /// <summary>
    /// Returns control to the user after an ally finishes moving
    /// Called by OnCharacterFinishedMoving event in MoveAttack
    /// </summary>
    private void ReturnControlAfterMove()
    {
        // Remove itself from the OnCharacterFinishedMoving event
        MoveAttack.OnCharacterFinishedMoving -= ReturnControlAfterMove;

        ToggleSelect(true);
        AllowSelect();
    }

    /// <summary>
    /// Turns the visuals of the character off and no longer selects them
    /// </summary>
    private void Deselect()
    {
        if (_charSelected != null)
        {
            _mAContRef.TurnOffVisuals(_charSelected);
            _charSelected.MyStats.DisplayStats(false);
            _charSelected = null;
        }

        // Call the character selected event
        if (OnCharacterDeselect != null)
            OnCharacterDeselect(null);
    }

    /// <summary>
    /// Gets the node at the current selection location
    /// </summary>
    /// <returns>Returns the node at the current selection location</returns>
    private Node GetSelectedNode()
    {
        Vector2Int pos = _inpContRef.SelectToGridPoint();
        return _mAContRef.GetNodeAtPosition(pos);
    }

    /// <summary>
    /// Allows the user to select again and recalculates all the visual tiles, since they have now changed
    /// Called by the TurnSystem.OnBeginPlayerTurn event
    /// Called when an ally stops moving (and they wont be taking action) or their action ends
    /// </summary>
    private void AllowSelect()
    {
        // Call the event for when the player is allowed to select
        if (OnPlayerAllowedSelect != null)
            OnPlayerAllowedSelect();

        // Allow them to hit buttons
        ToggleSelect(true);
        // If the user still has someone selected, show their new active visuals
        if (_charSelected != null)
        {
            _mAContRef.TurnOffVisuals(_charSelected);
            _mAContRef.SetActiveVisuals(_charSelected);
        }
    }

    /// <summary>
    /// Gets rid of the user's ability to interact with their characters while it is not their turn.
    /// Called by the TurnSystem.OnBeginEnemyTurn event
    /// </summary>
    private void DenySelect()
    {
        //Debug.Log("DenySelect");
        // Don't let the user select anything
        ToggleSelect(false);
        // Deselect anything that was selected
        Deselect();
        // To be safe, reset any held references
        _nodeToAttack = null;
    }

    /// <summary>
    /// Swaps canSelect to true or false. Also toggles the endTurnButton on and off
    /// </summary>
    /// <param name="onOff">Whether to let the user select or not</param>
    private void ToggleSelect(bool onOff)
    {
        _canSelect = onOff;
        // Switch the buttons on or off
        foreach (Button butt in _buttonsToTurnOff)
        {
            if (butt.enabled)
                butt.interactable = onOff;
        }
        // Switch the images on or off
        foreach (Image img in _imagesToTurnOff)
        {
            // If the image is enabled in the hierarchy
            if (img.enabled)
            {
                // If turning on
                if (onOff)
                    img.color = new Color(img.color.r, img.color.g, img.color.b, 1);
                // If turning off
                else
                    img.color = new Color(img.color.r, img.color.g, img.color.b, 0.5f);
            }
                
        }
    }

    /// <summary>
    /// Toggles off this script
    /// </summary>
    private void HideScript()
    {
        this.enabled = false;
    }

    /// <summary>
    /// Toggles on this script
    /// </summary>
    private void ShowScript()
    {
        this.enabled = true;
    }
}
