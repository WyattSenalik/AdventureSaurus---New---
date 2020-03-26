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

    private MoveAttackController _mAContRef = null;  // Reference to the MoveAttackController script
    private InputController _inpContRef = null;  // Reference to the InputController script
    private MoveAttack _charSelected;    // A reference to the selected character's MoveAttack script 
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

    // Called when the gameobject is toggled on
    // Subscribe to events
    private void OnEnable()
    {
        // When the player's turn begins, allow them to select things
        TurnSystem.OnBeginPlayerTurn += AllowSelect;
        // When the enemy's turn begins, deny the user from selecting
        TurnSystem.OnBeginEnemyTurn += DenySelect;
    }

    // Called when the gameobject is toggled off
    // Unsubscribe to events
    private void OnDisable()
    {
        TurnSystem.OnBeginPlayerTurn -= AllowSelect;
        TurnSystem.OnBeginEnemyTurn -= DenySelect;
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
                else if (selectedNode.occupying == CharacterType.Ally ||
                    (selectedNode.occupying == CharacterType.Enemy && _charSelected.WhatAmI == CharacterType.Enemy))
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
        if (_charSelected == null)
            return;
        // Make sure this character is active before doing anything else
        if (!_charSelected.gameObject.activeInHierarchy)
            return;
        // If it has one and hasn't moved this turn yet or hasn't attacked this turn
        if (!(_charSelected.HasMoved && _charSelected.HasAttacked))
        {
            // Set the visuals of it to be on
            _mAContRef.SetActiveVisuals(_charSelected);
        }

        // Call the character selected event
        if (OnCharacterSelect != null)
            OnCharacterSelect(_charSelected);
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
        if (_charSelected.MoveTiles.Contains(selNode) && selNode.occupying == CharacterType.None)
        {
            //Debug.Log("AttemptMove");
            ToggleSelect(false);    // Make it so that the player cannot select whilst something is moving
            // Calculate the pathing
            _mAContRef.ResetPathing();
            Node startNode = _mAContRef.GetNodeByWorldPosition(_charSelected.transform.position);
            _mAContRef.Pathing(startNode, selNode, _charSelected.WhatAmI);
            // Start moving the character
            _charSelected.StartMove();
            _mAContRef.TurnOffVisuals(_charSelected);

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
        if (_charSelected.AttackTiles.Contains(selNode) && selNode.occupying == CharacterType.Enemy && charToAttack != null && charToAttack.gameObject.activeInHierarchy)
        {
            ToggleSelect(false);    // Make it so that the player cannot select whilst something is attacking
            _charSelected.StartAttack(selNode.position); // Start the attack
            return true;
        }
        return false;
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
    /// Called from TurnSystem.OnBeginPlayerTurn event.
    /// Called from MoveAttack by Allies after they finish moving.
    /// Allows the user to select again and recalculates all the visual tiles, since they have now changed
    /// </summary>
    public void AllowSelect()
    {
        // Recalculate all the moveattack tiles for the characters
        _mAContRef.RecalculateAllMovementAttackTiles();

        // If the user still has someone selected, show their new active visuals
        if (_charSelected != null)
        {
            // If the character just moved, and now must start attacking someone
            if (_nodeToAttack != null)
            {
                ToggleSelect(false);    // Make it so that the player cannot select whilst something is attacking
                _charSelected.StartAttack(_nodeToAttack.position);
                // Unselect and untarget everything that we saved for this move attack
                _nodeToAttack = null;
                Deselect();
            }
            // If the character isn't supposed to attack someone, show their visuals
            else
            {
                ToggleSelect(true);
                _mAContRef.SetActiveVisuals(_charSelected);
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
        _nodeToAttack = null;
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
            int testNodeDist = Mathf.Abs(testNode.position.x - charSelectedNode.position.x) +
                Mathf.Abs(testNode.position.y - charSelectedNode.position.y);
            if (nodeToMoveTo != null)
                Debug.Log("Seeing if node at " + testNode.position + " is closer than " + nodeToMoveTo.position + "from " + charSelectedNode.position);
            else
                Debug.Log("Seeing if node at " + testNode.position + " is closer than null from " + charSelectedNode.position);
            // If the ally can move there or is already there and that node is closer than the closer than the current nodeToMoveTo
            if ((_charSelected.MoveTiles.Contains(testNode) || testNode == charSelectedNode) && testNodeDist < distToMoveNode)
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
        _mAContRef.ResetPathing();
        _mAContRef.Pathing(charSelectedNode, nodeToMoveTo, _charSelected.WhatAmI);
        // Start moving the character
        _charSelected.StartMove();
        _mAContRef.TurnOffVisuals(_charSelected);

        // The attack will be started in AllowSelect, so that we don't attack until we actually reach the tile we were going to
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
}
