﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    // Camera variables
    [SerializeField] private float _smoothTime = 0.3f;
    [SerializeField] private float _dragSpeed = 0.3f;
    //[SerializeField] private float _cameraDistance = 30.0f;

    // All allies
    private Transform _allyParent;

    // Index of last ally followed
    private Transform _lastAllyFollowed;

    // The transform of the character to follow
    private Transform _charToFollow;

    // Camera Velocity
    private Vector3 _camVel;

    //Mouse Position
    private Vector3 _mouseOrigin;

    // If who we are following is an enemy
    private bool _isOnEnemy;
    // If we finished panning to the enemy
    private bool _panFinished;
    // If the camera is being dragged around
    private bool _isDragging;

    // Events
    // When the camera finishes panning to an enemy
    public delegate void FinishEnemyPan();
    public static event FinishEnemyPan OnFinishEnemyPan;



    // Called when the gameobject is toggled on
    // Subscribe to events
    private void OnEnable()
    {
        // Subscribe to when a character is clicked, so that we can move to them
        MoveAttackGUIController.OnCharacterSelect += FollowCharacter;
        // When an enemy starts their turn, have the camera start to pan over to them
        EnemyTurnController.OnPreSingleEnemyTurn += FollowEnemy;
        // When the enemy's turn is over, put the camera on an ally
        EnemyTurnController.OnEndEnemyTurn += FollowLastAlly;
        // When the generation is done, initialize this script
        ProceduralGenerationController.OnFinishGenerationNoParam += Initialize;
    }

    // Called when the gameobject is toggled off
    // Unsubscribe from events
    private void OnDisable()
    {
        MoveAttackGUIController.OnCharacterSelect -= FollowCharacter;
        EnemyTurnController.OnPreSingleEnemyTurn -= FollowEnemy;
        EnemyTurnController.OnEndEnemyTurn -= FollowLastAlly;
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
    }

    // Called when the gameobject is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        MoveAttackGUIController.OnCharacterSelect -= FollowCharacter;
        EnemyTurnController.OnPreSingleEnemyTurn -= FollowEnemy;
        EnemyTurnController.OnEndEnemyTurn -= FollowLastAlly;
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
    }


    // Called before the first frame update
    private void Start()
    {
        // We haven't finished panning before starting
        _panFinished = false;
    }

    /// <summary>
    /// Initializes things for this script.
    /// Called from the FinishGenerating event
    /// </summary>
    private void Initialize()
    {
        // Get the allies
        _allyParent = GameObject.Find(ProceduralGenerationController.ALLY_PARENT_NAME).transform;

        // Default to following the player first
        _isOnEnemy = false;
        FollowFirstAlly();
    }

    // Move the camera to the correct positions
    // We use late update to move camera
    private void LateUpdate()
    {
        //Checks if pan finished and if it is enemy turn before allowing free cam
        if (_panFinished && !_isOnEnemy)
        {
            //gets mouse position on right click
            if (Input.GetMouseButtonDown(1))
            {
                //right click was pressed    
                _mouseOrigin = Input.mousePosition;
                _isDragging = true;
                _charToFollow = null;
            }
            //checks when right click isn't pressed
            if (!Input.GetMouseButton(1))
            {
                _isDragging = false;
            }
            //if right click is pressed it will drag the cam around
            if (_isDragging)
            {
                Vector3 targetPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition - _mouseOrigin);

                // update x and y but not z
                Vector3 move = new Vector3(-targetPosition.x * _dragSpeed, -targetPosition.y * _dragSpeed, 0);

                Camera.main.transform.Translate(move, Space.Self);
            }
        }
       

        // If we have a character to follow
        if (_charToFollow != null)
        {
            // Follow the character
            Vector3 targetPosition = _charToFollow.TransformPoint(new Vector3(0, 0, -10));
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _camVel, _smoothTime);

            // If we should be following an enemy and we haven't finished panning yet
            if (_isOnEnemy && !_panFinished)
            {
                // If we have panned over to the enemy
                if (Mathf.Abs(transform.position.x - _charToFollow.position.x) + 
                    Mathf.Abs(transform.position.y - _charToFollow.position.y) <= 1)
                {
                    if (OnFinishEnemyPan != null)
                        OnFinishEnemyPan();

                    _panFinished = true;
                }
            }
        }
        

    }

    /// <summary>
    /// Called from MoveAttackGUIController.OnCharacterSelect event
    /// Sets the character to follow
    /// </summary>
    /// <param name="charMA"></param>
    private void FollowCharacter(MoveAttack charMA)
    {
        // It is not the enemy's turn
        _isOnEnemy = false;
        _panFinished = true;

        // Set the char to follow
        if (charMA != null)
        {
            _charToFollow = charMA.transform;

            // If they are an ally
            if (charMA.WhatAmI == CharacterType.Ally)
            {
                _lastAllyFollowed = charMA.transform;
            }
        }
        else
            _charToFollow = null;
    }

    /// <summary>
    /// Called from EnemyTurnController.OnPreSingleEnemyTurn event
    /// Sets the camera to go to the enemy specified by enemyTrans
    /// </summary>
    /// <param name="enemyTrans">The transform of the enemy to follow</param>
    private void FollowEnemy(Transform enemyTrans)
    {
        // It is the enemy's turn
        _isOnEnemy = true;
        _panFinished = false;

        // Set the char to follow
        if (enemyTrans != null)
            _charToFollow = enemyTrans.transform;
        else
            _charToFollow = null;
    }

    /// <summary>
    /// Defaults camera to player position
    /// </summary>
    private void FollowFirstAlly()
    {
        // Find the first enemy and follow it
        if (_allyParent.childCount > 0)
        {
            _charToFollow = _allyParent.GetChild(0);
            _lastAllyFollowed = _charToFollow;
        }
    }

    /// <summary>
    /// Sets the camera on the ally with the given index.
    /// Called from the side GUI Buttons
    /// </summary>
    /// <param name="allyIndex">Index of the ally to follow</param>
    public void FollowAlly(int allyIndex)
    {
        // It is not the enemy's turn
        _isOnEnemy = false;
        _panFinished = true;

        if (allyIndex < _allyParent.childCount)
        {
            _charToFollow = _allyParent.GetChild(allyIndex);
            _lastAllyFollowed = _charToFollow;
        }
    }

    /// <summary>
    /// Sets the camera on the given ally
    /// </summary>
    /// <param name="allyToFollow">Transform of ally to follow</param>
    private void FollowAlly(Transform allyToFollow)
    {
        // It is not the enemy's turn
        _isOnEnemy = false;
        _panFinished = true;

        if (allyToFollow != null)
        {
            _charToFollow = allyToFollow;
            _lastAllyFollowed = allyToFollow;
        }
    }

    /// <summary>
    /// Puts the camera back on the last ally the camera was following
    /// </summary>
    private void FollowLastAlly()
    {
        if (_lastAllyFollowed != null)
        {
            FollowAlly(_lastAllyFollowed);
        }
        else
        {
            FollowFirstAlly();
        }
    }
}