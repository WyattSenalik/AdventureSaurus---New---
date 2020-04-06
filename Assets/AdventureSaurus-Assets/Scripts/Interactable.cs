using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    // Events
    // When this item is finished being interacted with
    public delegate void FinishInteraction();
    public static event FinishInteraction OnFinishInteraction;

    /// <summary>
    /// Starts the interaction with this objects
    /// </summary>
    public void StartInteract()
    {
        Debug.Log("Default Interaction");

        // Call the finish interaction event
        if (OnFinishInteraction != null)
            OnFinishInteraction();
    }
}
