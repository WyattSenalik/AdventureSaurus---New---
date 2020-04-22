using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentSaving : MonoBehaviour
{
    // Parent of rooms
    private Transform _roomParent;


    // Called when the component is set active
    // Subscribe to events
    private void OnEnable()
    {
        // When we finish generating initialize
        ProceduralGenerationController.OnFinishGenerationNoParam += Initialize;
        // When the game saves, save the amount of rooms we currently have
        SaveSystem.OnSave += SaveRoomAmount;
    }
    // Called when the component is toggled off
    // Unsubscribe from events
    private void OnDisable()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
        SaveSystem.OnSave -= SaveRoomAmount;
    }
    // Called when the component is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
        SaveSystem.OnSave -= SaveRoomAmount;
    }

    /// <summary>
    /// Initializes the references to things that did not exist prior to generation
    /// </summary>
    private void Initialize()
    {
        // Set the room parent
        _roomParent = GameObject.Find(ProceduralGenerationController.roomParentName).transform;
    }

    /// <summary>
    /// Saves the amount of rooms we have to a binary file
    /// </summary>
    private void SaveRoomAmount()
    {
        SaveSystem.SaveRoomAmount(_roomParent);
    }
}
