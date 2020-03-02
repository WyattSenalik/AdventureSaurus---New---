using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;

public class Room : MonoBehaviour
{
    [SerializeField] private List<Light2D> broadcastLights = null;  // A list of the lights that this room is broadcasting to other rooms
    [SerializeField] private List<Light2D> receiveLights = null;    // A list of the lights that are shining into this room
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
            StartCoroutine(ChangeIntensity(this));
            return;
        }
        //Debug.Log("otherRoom is " + otherRoom.name);
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
        StartCoroutine(ChangeIntensity(this));
        // Change the lighting of the other room
        StartCoroutine(ChangeIntensity(otherRoom));
    }


    /// <summary>
    /// Called when the last ally in a room dies. Turns off the room
    /// </summary>
    /// <param name="allyWhoExited">The enemy who just exited the room (died)</param>
    private void CalmRoom(MoveAttack allyWhoExited)
    {
        // We want to turn the room off
        currentLightIntensity = 0f;
        // Change the intensity of this room
        StartCoroutine(ChangeIntensity(this));
        // Update lighting of the adjacent rooms
        foreach (Room adjRoom in adjacentRooms)
        {
            StartCoroutine(ChangeIntensity(adjRoom));
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

            // If we haven't activated the enemies in the room yet, do that
            if (!isRoomActive)
            {
                enMAAIRef.ActivateRoom(enemiesInRoom);
                isRoomActive = true;
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
            alliesInRoom.Remove(mARef);
            // If it was the last ally and that ally is not moving (they died)
            if (alliesInRoom.Count == 0 && !mARef.transition)
                CalmRoom(mARef);
        }
        // If it was an enemy and the room is active
        else if (mARef.WhatAmI == CharacterType.Enemy && isRoomActive)
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
    /// Turns on all the broadcast lights of enter room little by little.
    /// This is supposed to be called from ChangeIntensity at each interval
    /// </summary>
    /// <param name="enterRoom">The room who owns the broadcast lights. It was just entered by an ally</param>
    private void UpdateBroadcastLights(Room enterRoom)
    {
        // We are going to be setting the intensity of each broadcast light coming from this room to go to other rooms.
        // The resulting overlap of intensities should equal the enterRoom's light's intensity at this moment (not currentIntensity)
        float targetIntensity = enterRoom.roomLight.intensity;
        for (int i = 0; i < enterRoom.broadcastLights.Count; ++i)
        {
            // We have to make it so the sum of the room this broadcast light is 
            // broadcasting into's light plus this light is the target intensity.
            // The room this light is broadcasting into is the broadcastToRooms at the same index.
            float actualIntensity = targetIntensity - enterRoom.adjacentRooms[i].roomLight.intensity;
            // Make sure this intensity is not negative, if it is, we just want to turn it off
            if (actualIntensity < 0f)
            {
                actualIntensity = 0f;
            }
            // Set the value
            enterRoom.broadcastLights[i].intensity = actualIntensity;
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
            float targetIntensity = enterRoom.adjacentRooms[i].roomLight.intensity;
            // We have to make it so the sum of enterRoom's light's intensity and the receiving light's intensity
            // equals the target intensity
            float actualIntensity = targetIntensity - enterRoom.roomLight.intensity;
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
    /// Slowly changes the room's light intensity to be the room's currentItensity
    /// </summary>
    /// <param name="roomToChange">The room whose light will be changed</param>
    /// <returns>IEnumerator</returns>
    public IEnumerator ChangeIntensity(Room roomToChange)
    {
        // If its dimmer than its supposed to be, make it brighter
        while (roomToChange.roomLight.intensity < roomToChange.currentLightIntensity)
        {
            roomToChange.roomLight.intensity += Time.deltaTime;
            UpdateBroadcastLights(roomToChange);
            UpdateReceivingLights(roomToChange);
            yield return null;
        }
        // If its brighter than its supposed to be, make it dimmer
        while (roomToChange.roomLight.intensity > roomToChange.currentLightIntensity)
        {
            roomToChange.roomLight.intensity -= Time.deltaTime;
            UpdateBroadcastLights(roomToChange);
            UpdateReceivingLights(roomToChange);
            yield return null;
        }
        // Its close enough, so finish setting it
        roomToChange.roomLight.intensity = roomToChange.currentLightIntensity;
        UpdateBroadcastLights(roomToChange);
        UpdateReceivingLights(roomToChange);
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
