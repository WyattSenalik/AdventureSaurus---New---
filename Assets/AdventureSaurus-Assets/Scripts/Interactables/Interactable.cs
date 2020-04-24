using UnityEngine;

public class Interactable : MonoBehaviour
{
    // If the object can currently be interacted with
    protected bool _canInteractWith = true;
    public bool GetCanInteractWith() { return _canInteractWith; }
    public void SetCanInteractWith(bool newCanInteractWith) { _canInteractWith = newCanInteractWith; }
    // Events
    // When this item is finished being interacted with
    public delegate void FinishInteraction();
    public static event FinishInteraction OnFinishInteraction;


    // Called when the component is set active
    // Subscribe to events
    private void OnEnable()
    {
        SaveSystem.OnSave += SaveInteractable;
    }
    // Called when the component is toggled off
    // Unsubscribe from events
    private void OnDisable()
    {
        SaveSystem.OnSave -= SaveInteractable;
    }
    // Called when the component is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        SaveSystem.OnSave -= SaveInteractable;
    }

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

    /// <summary>
    /// Saves this interactables data in a binary file
    /// </summary>
    private void SaveInteractable()
    {
        SaveSystem.SaveInteractable(this);
    }
}
