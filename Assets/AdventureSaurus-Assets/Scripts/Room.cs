using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;

public class Room : MonoBehaviour
{
    [SerializeField] private List<MoveAttack> enemiesInRoom = null; // An list of the enemies in the room
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
    private void TriggerRoom()
    {
        roomHasBeenActivated = true;
        enMAAIRef.ActivateRoom(enemiesInRoom);
    }

    /// <summary>
    /// CAlled when the last character in the room leaves. Makes the room light faded
    /// </summary>
    private void SleepRoom()
    {
        roomLight.intensity = 0.4f;
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
            roomLight.intensity = 1f;
            // If this is first time colliding, turn on the room
            if (!roomHasBeenActivated)
            {
                TriggerRoom();
            }
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
}
