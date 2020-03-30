﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;

public enum RoomType { NORMAL, HALLWAY, START, END, SAFE}

public class Room : MonoBehaviour
{
    // A list of the lights that this room is broadcasting to other rooms
    [SerializeField] private List<Light2D> broadcastLights = null;
    public List<Light2D> BroadcastLights
    {
        get { return broadcastLights; }
    }
    // A list of the lights that are shining into this room
    [SerializeField] private List<Light2D> receiveLights = null;
    public List<Light2D> ReceiveLights
    {
        get { return receiveLights; }
    }
    // A list of room adjacent to this one
    [SerializeField] private List<Room> adjacentRooms = null;
    public List<Room> AdjacentRooms
    {
        get { return adjacentRooms; }
    }
    // The light that will illuminate the room
    private Light2D _roomLight = null;
    // The characters currently in the room
    private List<MoveAttack> _alliesInRoom;
    // A list of the enemies in the room
    private List<MoveAttack> _enemiesInRoom;
    // If the room is on or off
    private bool _isRoomActive;
    // The current light level of the room
    private float _currentLightIntensity;
    // If all enemies in the room have been defeated
    private bool _clear;
    // Represents how far the room is away from the starting room
    private int _roomWeight;
    public int RoomWeight
    {
        get { return _roomWeight; }
        set { _roomWeight = value; }
    }
    // What kind of room this is
    private RoomType _myRoomType;
    public RoomType MyRoomType
    {
        set { _myRoomType = value; }
        get { return _myRoomType; }
    }
    // The difficulty of the room
    private int _roomDifficulty;
    public int RoomDifficulty
    {
        set { _roomDifficulty = value; }
        get { return _roomDifficulty; }
    }

    // Events
    // When a room is activated
    public delegate void RoomActivated(List<MoveAttack> enemiesInTheRoom);
    public static event RoomActivated OnRoomActivate;


    // Called when this component is set active
    // Subscribe to events
    private void OnEnable()
    {
        // When generation is done, show the first room
        ProceduralGenerationController.OnFinishGenerationNoParam += ShowStartRoom;
    }

    // Called when this component is toggled off
    // Unsubscribe from ALL events
    private void OnDisable()
    {
        // When generation is done, show the first room
        ProceduralGenerationController.OnFinishGenerationNoParam -= ShowStartRoom;
    }

    // Called when this gameobject is destroyed
    // Unsubscribe from events
    private void OnDestroy()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= ShowStartRoom;
    }

    // Set References
    // Awake is called before Start
    private void Awake()
    {
        _roomLight = this.GetComponent<Light2D>();
        if (_roomLight == null)
            Debug.Log("Could not find Light attached to " + this.name);

        // Initialize my lists that will be set from Procedural Generation
        // only if I do not set them in editor
        if (broadcastLights == null)
            broadcastLights = new List<Light2D>();
        if (receiveLights == null)
            receiveLights = new List<Light2D>();
        if (adjacentRooms == null)
            adjacentRooms = new List<Room>();

        // Initialize variables
        _alliesInRoom = new List<MoveAttack>();
        _enemiesInRoom = new List<MoveAttack>();
        _roomLight.intensity = 0;
        _currentLightIntensity = 0;
        _isRoomActive = false;
        _clear = true;
    }

    /// <summary>
    /// Called by ProceduralGenerationController.OnFinishGenerationNoParam event
    /// If this room is the start room, turn it on
    /// </summary>
    private void ShowStartRoom()
    {
        // If this room is the first room
        if (_myRoomType == RoomType.START)
        {
            // Since we just walked into the room, we want to turn it on
            _currentLightIntensity = 1f;
            // Turn on the starting room's lights
            this.BeginChangeIntensity();
        }
    }

    /// <summary>
    /// Called every time a character enters the room
    /// </summary>
    /// <param name="allyWhoEntered">MoveAttack script attached to the ally who entered the room</param>
    private void TriggerRoom(MoveAttack allyWhoEntered)
    {
        // The room the character just came from
        Room otherRoom = GetAdjacentRoomByAlly(allyWhoEntered);

        // Since we just walked into the room, we want to turn it on
        _currentLightIntensity = 1f;

        // Test if there is another room, there may not be, if the character starts in that room
        if (otherRoom == null)
        {
            // Turn on the starting room's lights and thats it
            this.BeginChangeIntensity();
            return;
        }
        //Debug.Log("otherRoom is " + otherRoom.name);
        int amountAlliesInOtherRoom = otherRoom._alliesInRoom.Count; // The number of allies in the other room

        // If the room the character came from has only that character in it
        if (amountAlliesInOtherRoom == 1)
        {
            // Turn off the room that was came from's light
            // If that room has been cleared, we dim it, otherwise we turn it completely off
            if (otherRoom._clear)
            {
                // Change the otherRoom's light
                otherRoom._currentLightIntensity = 0.5f;
            }
            // If the room has not been cleared, we turn it off completely
            else
            {
                // Change the otherRoom's light
                otherRoom._currentLightIntensity = 0f;
            }
        }

        // Actually update the lighting of everything after determining what it should be
        // Turn on the lights of this room
        this.BeginChangeIntensity();
        // Change the lighting of the other room
        otherRoom.BeginChangeIntensity();
    }


    /// <summary>
    /// Called when the last ally in a room dies. Turns off the room
    /// </summary>
    /// <param name="allyWhoExited">The enemy who just exited the room (died)</param>
    private void CalmRoom(MoveAttack allyWhoExited)
    {
        Debug.Log("Calming the room");
        // We want to turn the room off
        _currentLightIntensity = 0f;
        // Change the intensity of this room
        this.BeginChangeIntensity();
        // Update lighting of the adjacent rooms
        foreach (Room adjRoom in adjacentRooms)
        {
            adjRoom.BeginChangeIntensity();
        }
    }


    /// <summary>
    /// When this object collides with the player for the first time, we trigger the room. Also adds characters to what is currently in the room
    /// </summary>
    /// <param name="collision">The collider2d of the object that was collided with</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Try to get the movement script from what we collided with, and tests if it exists
        MoveAttack mARef = collision.GetComponent<MoveAttack>();
        if (mARef == null)
        {
            return;
        }

        // If we collide with an enemy
        if (mARef.WhatAmI == CharacterType.Enemy)
        {
            // If they aren't already a part of the room's enemies, add them to it
            if (!_enemiesInRoom.Contains(mARef))
            {
                _clear = false;
                _enemiesInRoom.Add(mARef);
            }
            // Also, if their room isn't active hide them (transition is used to test if it is the beginning of the game or not, since we don't
            // want an enemy to disappear when they walk between rooms, for which transition will always be true
            if (!_isRoomActive && mARef.Transition == false)
            {
                mARef.gameObject.SetActive(false);
            }
        }
        // If we collide with a player
        else if (mARef.WhatAmI == CharacterType.Ally)
        {
            // Add them to the allies in room
            // Make sure they are not already in the list
            if (!_alliesInRoom.Contains(mARef))
                _alliesInRoom.Add(mARef);
            // Turn on the room
            TriggerRoom(mARef);

            // If we haven't activated the enemies in the room yet, do that
            if (!_isRoomActive)
            {
                _isRoomActive = true;
                // Call the room activate event
                if (OnRoomActivate != null)
                    OnRoomActivate(_enemiesInRoom);
            }
        }
    }

    /// <summary>
    /// When the last character in the room leaves, un trigger the room
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        // If we collide with something that has a MoveAttack script attached, remove it from characters currently in the room
        MoveAttack mARef = collision.GetComponent<MoveAttack>();
        if (mARef == null)
            return;
        // If it was an ally
        if (mARef.WhatAmI == CharacterType.Ally)
        {
            _alliesInRoom.Remove(mARef);
            // If it was the last ally and that ally dead
            if (_alliesInRoom.Count == 0 && mARef.GetComponent<Health>().CurHP == 0)
                CalmRoom(mARef);
        }
        // If it was an enemy and the room is active
        else if (mARef.WhatAmI == CharacterType.Enemy && _isRoomActive)
        {
            _enemiesInRoom.Remove(mARef);
            // If it was the last enemy
            if (_enemiesInRoom.Count == 0)
                _clear = true;
        }
    }

    /// <summary>
    /// Returns the intensity of the room's main light
    /// </summary>
    /// <returns>float that is the intensity of the room light</returns>
    public float GetIntensity()
    {
        return _currentLightIntensity;
    }

    /// <summary>
    /// Turns on all the broadcast lights of enter room little by little.
    /// This is supposed to be called from ChangeIntensity at each interval
    /// </summary>
    /// <param name="enterRoom">The room who owns the broadcast lights. It was just entered by an ally</param>
    private void UpdateBroadcastLights(Room enterRoom)
    {
        // We are going to be setting the intensity of each broadcast light coming from this room to go to other rooms.
        // The resulting overlap of intensities should equal the enterRoom's light's intensity at this moment (not currentIntensity)
        float targetIntensity = enterRoom._roomLight.intensity;
        for (int i = 0; i < enterRoom.broadcastLights.Count; ++i)
        {
            // We have to make it so the sum of the room this broadcast light is 
            // broadcasting into's light plus this light is the target intensity.
            // The room this light is broadcasting into is the broadcastToRooms at the same index.
            float actualIntensity = targetIntensity - enterRoom.adjacentRooms[i]._roomLight.intensity;
            // Make sure this intensity is not negative, if it is, we just want to turn it off
            if (actualIntensity < 0f)
            {
                actualIntensity = 0f;
            }
            // Set the value
            enterRoom.broadcastLights[i].intensity = actualIntensity;
            //Debug.Log("Target intensity for " + enterRoom.broadcastLights[i].name + " is " + targetIntensity);
            //Debug.Log("Actual intensity of " + enterRoom.broadcastLights[i].name + " is " + actualIntensity);
        }
    }

    /// <summary>
    /// Turns off all the broadcast lights of enter room little by little
    /// This is supposed to be called from ChangeIntensity at each interval
    /// </summary>
    /// <param name="enterRoom">The room who is receiving the receiveLights. It was just entered by an ally</param>
    private void UpdateReceivingLights(Room enterRoom)
    {
        for (int i = 0; i < enterRoom.receiveLights.Count; ++i)
        {
            // We are going to be setting the intensity of each receiving light coming into etner room from other rooms.
            // The resulting overlap of intensities should equal the room this light is being broadcast from's light's 
            // intensity at this moment (not currentIntensity)
            // The light we are on has the same index as the room it is receiving from
            float targetIntensity = enterRoom.adjacentRooms[i]._roomLight.intensity;
            // We have to make it so the sum of enterRoom's light's intensity and the receiving light's intensity
            // equals the target intensity
            float actualIntensity = targetIntensity - enterRoom._roomLight.intensity;
            // Make sure this intensity is not negative, if it is, we just want to turn it off
            if (actualIntensity < 0f)
            {
                actualIntensity = 0f;
            }
            // Set the value
            enterRoom.receiveLights[i].intensity = actualIntensity;
        }
    }

    /// <summary>
    /// Stops other coroutines and starts a new one to change the intensity
    /// </summary>
    public void BeginChangeIntensity()
    {
        // Stop any other change intensity coroutines running before starting this one
        StopAllCoroutines();
        // Start the coroutine to change the intensity
        StartCoroutine(ChangeIntensity());
    }

    /// <summary>
    /// Slowly changes the room's light intensity to be the room's currentItensity
    /// </summary>
    /// <returns>IEnumerator</returns>
    private IEnumerator ChangeIntensity()
    {
        // If its dimmer than its supposed to be, make it brighter
        while (_roomLight.intensity < _currentLightIntensity)
        {
            _roomLight.intensity += Time.deltaTime;
            UpdateBroadcastLights(this);
            UpdateReceivingLights(this);
            yield return null;
        }
        // If its brighter than its supposed to be, make it dimmer
        while (_roomLight.intensity > _currentLightIntensity)
        {
            _roomLight.intensity -= Time.deltaTime;
            UpdateBroadcastLights(this);
            UpdateReceivingLights(this);
            yield return null;
        }
        // Its close enough, so finish setting it
        _roomLight.intensity = _currentLightIntensity;
        UpdateBroadcastLights(this);
        UpdateReceivingLights(this);
        yield return null;
    }

    /// <summary>
    /// Returns a room, that is not this room, that contains the given ally
    /// </summary>
    /// <param name="ally">The ally coming from the room we will return</param>
    /// <returns>Room adjacent to this room where ally came from</returns>
    private Room GetAdjacentRoomByAlly(MoveAttack ally)
    {
        // Iterate over the adjacent rooms until we get the room this ally is in
        foreach (Room adjRoom in adjacentRooms)
        {
            if (adjRoom._alliesInRoom.Contains(ally))
            {
                return adjRoom;
            }
        }
        // If we don't find it, something went wrong, or the game just started
        if (ally.Transition)
            Debug.Log("Could not find adjacent room with " + ally.name + " in it");
        return null;
    }

    /// <summary>
    /// Sorts the adjacent rooms based on the lists broadcastLights and receiveLights (both correspond to two halves of the same circle)
    /// </summary>
    public void SortAdjacentRooms()
    {
        // Iterate over the broadcast lights to compare each adjacent room to them
        for (int i = 0; i < broadcastLights.Count; ++i)
        {
            // Get the positon of the light
            Vector3 curLightWorldPos = broadcastLights[i].transform.position;
            // Closest distance to the light and reference to the room that is the closest
            float closestDist = float.MaxValue;
            Room closestRoom = null;
            int closestIndex = 0;
            // Iterate over each adjacent room to see which one is the adjacent room corresponding to the light
            for (int k = i; k < adjacentRooms.Count; ++k)
            {
                // Get the position of the current room
                Vector3 curRoomWorldPos = adjacentRooms[k].transform.position;
                // Calculate the distance between the room and the current light
                float curDist = (curLightWorldPos - curRoomWorldPos).magnitude;
                // If this room is closer to the light than the last
                if (curDist < closestDist)
                {
                    closestDist = curDist;
                    closestRoom = adjacentRooms[k];
                    closestIndex = k;
                }
            }
            // Swap the two rooms, as the closer room is closer to the current light, so it is the one that corresponds ot it
            adjacentRooms[closestIndex] = adjacentRooms[i];
            adjacentRooms[i] = closestRoom;
        }
    }
}
