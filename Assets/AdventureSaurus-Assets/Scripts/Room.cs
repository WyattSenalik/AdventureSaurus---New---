using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;

public class Room : MonoBehaviour
{
    [SerializeField] private List<Light2D> broadcastLights = null;  // A list of the lights that this room is broadcasting to other rooms
    [SerializeField] private List<Room> broadcastToRooms = null;    // A list of the rooms the broadcastLights are broadcasting to. Indices line up
    [SerializeField] private List<Light2D> receiveLights = null;    // A list of the lights that are shining into this room
    [SerializeField] private List<Room> receiveFromRooms = null;    // A list of the rooms the receiveLights are coming from. Indices line up
    [SerializeField] private List<Room> adjacentRooms = null;   // A list of room adjacent to this one
    private Light2D roomLight = null;    // The light that will illuminate the room
    private EnemyMoveAttackAI enMAAIRef;    // Reference to the EnemyMoveAttackAI script
    private List<MoveAttack> alliesInRoom;  // The characters currently in the room
    private List<MoveAttack> enemiesInRoom; // A list of the enemies in the room
    private bool isRoomActive;  // If the room is on or off
    private float currentLightIntensity;    // The current light level of the room
    private bool clear; // If all enemies in the room have been defeated

    // Set References
    // Awake is called before Start
    private void Awake()
    {
        GameObject gameController = GameObject.FindWithTag("GameController");
        // Make sure a GameController exists
        if (gameController == null)
            Debug.Log("Could not find any GameObject with the tag GameController");
        else
        {
            enMAAIRef = gameController.GetComponent<EnemyMoveAttackAI>();
            if (enMAAIRef == null)
            {
                Debug.Log("Could not find EnemyMoveAttackAI attached to " + gameController.name);
            }
        }

        roomLight = this.GetComponent<Light2D>();
        if (roomLight == null)
            Debug.Log("Could not find Light attached to " + this.name);
    }

    // Initialize variables
    private void Start()
    {
        roomLight.intensity = 0;
        currentLightIntensity = 0;
        alliesInRoom = new List<MoveAttack>();
        enemiesInRoom = new List<MoveAttack>();
        isRoomActive = false;
        clear = true;
    }

    /// <summary>
    /// Called every time a character enters the room
    /// </summary>
    /// <param name="allyWhoEntered">MoveAttack script attached to the ally who entered the room</param>
    private void TriggerRoom(MoveAttack allyWhoEntered)
    {
        Room otherRoom = GetAdjacentRoomByAlly(allyWhoEntered); // The room the character just came from

        // Since we just walked into the room, we want to turn it on
        currentLightIntensity = 1f;

        // Test if there is another room, there may not be, if the character starts in that room
        if (otherRoom == null)
        {
            // Turn on the starting room's lights and thats it
            StartCoroutine(ChangeIntensity(this, true));
            return;
        }
        int amountAlliesInOtherRoom = otherRoom.alliesInRoom.Count; // The number of allies in the other room

        // If the room the character came from has only that character in it
        if (amountAlliesInOtherRoom == 1)
        {
            // Turn off the room that was came from's light
            // If that room has been cleared, we dim it, otherwise we turn it completely off
            if (otherRoom.clear)
            {
                // Change the otherRoom's light
                otherRoom.currentLightIntensity = 0.5f;
            }
            // If the room has not been cleared, we turn it off completely
            else
            {
                // Change the otherRoom's light
                otherRoom.currentLightIntensity = 0f;
            }
        }

        // Actually update the lighting of everything after determining what it should be
        // Turn on the lights of this room
        StartCoroutine(ChangeIntensity(this, true));
        // Change the lighting of the other room
        StartCoroutine(ChangeIntensity(otherRoom, false));
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
            if (!enemiesInRoom.Contains(mARef))
            {
                clear = false;
                enemiesInRoom.Add(mARef);
            }
            // Also, if their room isn't active hide them (transition is used to test if it is the beginning of the game or not, since we don't
            // want an enemy to disappear when they walk between rooms, for which transition will always be true
            if (!isRoomActive && mARef.transition == false)
            {
                mARef.gameObject.SetActive(false);
            }
        }
        // If we collide with a player
        else if (mARef.WhatAmI == CharacterType.Ally)
        {
            // Add them to the allies in room
            // Make sure they are not already in the list
            if (!alliesInRoom.Contains(mARef))
                alliesInRoom.Add(mARef);
            // Turn on the room
            TriggerRoom(mARef);
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
            alliesInRoom.Remove(mARef);
        }
        // If it was an enemy
        else if (mARef.WhatAmI == CharacterType.Enemy)
        {
            enemiesInRoom.Remove(mARef);
            // If it was the last enemy
            if (enemiesInRoom.Count == 0)
                clear = true;
        }
    }

    /// <summary>
    /// Returns the intensity of the room's main light
    /// </summary>
    /// <returns>float that is the intensity of the room light</returns>
    public float GetIntensity()
    {
        return currentLightIntensity;
    }

    /// <summary>
    /// Turns all the broadcast lights of the past-in room to the target intensity (kinda)
    /// This is supposed to be called from ChangeIntensity
    /// </summary>
    /// <param name="sharedRoom">The room that will have its broadcast lights updated</param>
    /// <param name="targetIntensity">The target intensity for the lights to be set to. Defaults to 1</param>
    public void UpdateBroadcastLights(Room sharedRoom = null, float targetIntensity = 1)
    {
        // If no room was passed in, default it to this room
        if (sharedRoom == null)
            sharedRoom = this;

        // We need to determine the actual intensity we should set the light to. We want the brightest part to
        // be the same intensity as the target intensity, but we are broadcasting into another room, so we need to determine
        // what room we are broadcasting into and find out its intensity
        for (int i = 0; i < broadcastLights.Count; ++i)
        {
            float actualIntensity = targetIntensity - broadcastToRooms[i].roomLight.intensity;
            broadcastLights[i].intensity = actualIntensity;
        }
    }

    /// <summary>
    /// Turns all the receiving lights of the past-in room to the target intensity (kinda)
    /// </summary>
    /// <param name="sharedRoom">The room that will have its receiving lights updated</param>
    /// <param name="targetIntensity">The target intensity for the lights to be set to. Defaults to 1</param>
    private void UpdateReceivingLights(Room sharedRoom = null, float targetIntensity = 1)
    {
        // If no room was passed in, default it to this room
        if (sharedRoom == null)
            sharedRoom = this;

        // We need to determine the actual intensity we should set the light to. We want the brightest part to
        // be the same intensity as the target intensity, but we are broadcasting into another room, so we need to determine
        // what room we are broadcasting into and find out its intensity
        for (int i = 0; i < receiveLights.Count; ++i)
        {
            float actualIntensity = receiveFromRooms[i].roomLight.intensity - this.roomLight.intensity;
            if (actualIntensity < 0)
            {
                actualIntensity = 0;
            }
            receiveLights[i].intensity = actualIntensity;
        }
    }

    /// <summary>
    /// Slowly changes the light intensity to be targetIntensity
    /// </summary>
    /// <param name="lightToChange">The light that will be changed</param>
    /// <param name="targetIntensity">The intensity the light will be set to</param>
    /// <returns>IEnumerator</returns>
    public IEnumerator ChangeIntensity(Room roomToChange, bool enteringRoom)
    {
        // If its dimmer than its supposed to be, make it brighter
        while (roomToChange.roomLight.intensity < roomToChange.currentLightIntensity)
        {
            roomToChange.roomLight.intensity += Time.deltaTime;
            if (enteringRoom)
            {
                UpdateBroadcastLights(roomToChange, roomToChange.roomLight.intensity);
                UpdateReceivingLights(roomToChange, roomToChange.roomLight.intensity);
            }
            yield return null;
        }
        // If its brighter than its supposed to be, make it dimmer
        while (roomToChange.roomLight.intensity > roomToChange.currentLightIntensity)
        {
            roomToChange.roomLight.intensity -= Time.deltaTime;
            if (enteringRoom)
            {
                UpdateBroadcastLights(roomToChange, roomToChange.roomLight.intensity);
                UpdateReceivingLights(roomToChange, roomToChange.roomLight.intensity);
            }
            yield return null;
        }
        // Its close enough, so finish setting it
        roomToChange.roomLight.intensity = roomToChange.currentLightIntensity;
        if (enteringRoom)
        {
            UpdateBroadcastLights(roomToChange, roomToChange.roomLight.intensity);
            UpdateReceivingLights(roomToChange, roomToChange.roomLight.intensity);
        }
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
            if (adjRoom.alliesInRoom.Contains(ally))
            {
                return adjRoom;
            }
        }
        // If we don't find it, something went wrong
        Debug.Log("Could not find adjacent room with " + ally.name + " in it");
        return null;
    }
}
