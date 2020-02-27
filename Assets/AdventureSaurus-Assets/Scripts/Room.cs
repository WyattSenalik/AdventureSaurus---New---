using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;

public class Room : MonoBehaviour
{
    [SerializeField] private List<MoveAttack> enemiesInRoom = null; // A list of the enemies in the room
    [SerializeField] private List<SharedRoomLight> lightsToShare = null;    // A list of the lights this room must share
    public List<SharedRoomLight> LightsToShare
    {
        get { return lightsToShare; }
    }
    private Light2D roomLight = null;    // The light that will illuminate the room
    private EnemyMoveAttackAI enMAAIRef;    // Reference to the EnemyMoveAttackAI script
    private bool roomHasBeenActivated;  // Whether or not this room has been walked into or not
    private List<MoveAttack> charactersInRoom;  // The characters currently in the room

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
        roomHasBeenActivated = false;
        roomLight.intensity = 0;
        charactersInRoom = new List<MoveAttack>();
    }

    /// <summary>
    /// Called when an ally enters this room for the first time. Sends the enemies in this room information to the EnemyMoveAttackAI script
    /// to be added to the active enemies list and be activated
    /// </summary>
    private void TriggerRoomForFirstTime()
    {
        roomHasBeenActivated = true;
        enMAAIRef.ActivateRoom(enemiesInRoom);
    }

    /// <summary>
    /// Called every time a character enters the room
    /// </summary>
    private void TriggerRoom()
    {
        roomLight.intensity = 1f;
        ActivateSharedRoomLights();
        // When this room is turned on we should check if we should turn off the shared lights that should broadcast a light into this room
        foreach (SharedRoomLight sharedRoomLight in lightsToShare)
        {
            // Check the room lights of the rooms we share lights with to turn on their room lights
            foreach (SharedRoomLight potentialThisRoomLight in sharedRoomLight.RoomToShareWith.LightsToShare)
            {
                // If we found one that shares a light with this room, we want to turn off that room's lights
                if (potentialThisRoomLight.RoomToShareWith == this)
                {
                    // Only turn off the light that bleeds into this room
                    sharedRoomLight.RoomToShareWith.DeactivateSharedRoomLights(this);
                }
            }
        }
    }

    /// <summary>
    /// Called when the last character in the room leaves. Makes the room light faded
    /// </summary>
    private void SleepRoom()
    {
        roomLight.intensity = 0.4f;
        DeactivateSharedRoomLights();
        // When this room is turned off we should check if we should turn on the shared lights that should broadcast a light into this room
        foreach (SharedRoomLight sharedRoomLight in lightsToShare)
        {
            // Check the room lights of the rooms we share lights with to turn on their room lights
            foreach (SharedRoomLight potentialThisRoomLight in sharedRoomLight.RoomToShareWith.LightsToShare)
            {
                // If we found one that shares a light with this room and that other room is on, we want to turn on that room's light
                if (potentialThisRoomLight.RoomToShareWith == this && potentialThisRoomLight.RoomToBroadcastFrom.roomLight.intensity == 1)
                {
                    // Only turn on the light that bleeds into this room
                    sharedRoomLight.RoomToShareWith.ActivateSharedRoomLights(this);
                }
            }
        }
    }

    /// <summary>
    /// When this object collides with the player for the first time, we trigger the room. Also adds characters to what is currently in the room
    /// </summary>
    /// <param name="collision">The collider2d of the object that was collided with</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If we collide with a player
        if (collision.gameObject.tag == "Player")
        {
            // If this is first time colliding, turn on the room
            if (!roomHasBeenActivated)
            {
                TriggerRoomForFirstTime();
            }
            TriggerRoom();
        }
        // If we collide with something that has a MoveAttack script attached, add it to the characters currently in the room
        MoveAttack mARef = collision.GetComponent<MoveAttack>();
        if (mARef != null)
        {
            charactersInRoom.Add(mARef);
        }
    }

    /// <summary>
    /// When the last character in the room leaves, un trigger the room
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        // If we collide with something that has a MoveAttack script attached, add it to the characters currently in the room
        MoveAttack mARef = collision.GetComponent<MoveAttack>();
        if (mARef != null)
        {
            charactersInRoom.Remove(mARef);
            // If it was the last character
            if (charactersInRoom.Count == 0)
                SleepRoom();
        }
    }

    /// <summary>
    /// Returns the intensity of the room's main light
    /// </summary>
    /// <returns>float that is the intensity of the room light</returns>
    public float GetIntensity()
    {
        return roomLight.intensity;
    }

    /// <summary>
    /// Turns on all the shared lights of this room that are shared with the passed in room. If no room it passed in, it turns on all
    /// </summary>
    /// /// <param name="sharedRoom">The room that shares a room with the lights we want to turn on</param>
    public void ActivateSharedRoomLights(Room sharedRoom = null)
    {
        // When this room is triggered we turn all the shared lights on such that the light intensity does not exceed 1
        foreach (SharedRoomLight sharedRoomLight in lightsToShare)
        {
            // If we found the right room
            if (sharedRoom == null || sharedRoomLight.RoomToShareWith == sharedRoom)
            {
                float shineIntensity = 1 - sharedRoomLight.RoomToShareWith.GetIntensity();
                sharedRoomLight.SharedLight.intensity = shineIntensity;
            }
        }
    }

    /// <summary>
    /// Turns off the shared lights of this room that share a room with the passed in room. If no room is passed in, it turns off all 
    /// shared lights in the room
    /// </summary>
    /// <param name="sharedRoom">The room that shares a room with the lights we want to turn off</param>
    public void DeactivateSharedRoomLights(Room sharedRoom = null)
    {
        // When this room is turned off we turn off all the shared lights it is showing into other rooms
        foreach (SharedRoomLight sharedRoomLight in lightsToShare)
        {
            if (sharedRoom == null || sharedRoomLight.RoomToShareWith == sharedRoom)
                sharedRoomLight.SharedLight.intensity = 0;
        }
    }
}
