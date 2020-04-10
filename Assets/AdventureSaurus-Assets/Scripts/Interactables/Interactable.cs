using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    // If the object can currently be interacted with
    protected bool _canInteractWith = true;
    public bool CanInteractWith
    {
        get { return _canInteractWith; }
    }
    // Events
    // When this item is finished being interacted with
    public delegate void FinishInteraction();
    public static event FinishInteraction OnFinishInteraction;

    /// <summary>
    /// Starts the interaction with this object.
    /// Meant to be overriden in child classes.
    /// Should be called at the end of the child class
    /// </summary>
    public virtual void StartInteract()
    {
        //Debug.Log("Default Interaction");

        // Call the finish interaction event
        if (OnFinishInteraction != null)
            OnFinishInteraction();
    }
}
