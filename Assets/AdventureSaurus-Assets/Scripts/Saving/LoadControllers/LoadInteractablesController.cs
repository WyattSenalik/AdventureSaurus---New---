using UnityEngine;

public static class LoadInteractablesController
{
    /// <summary>
    /// Creates interactable objects based on the stored interactable data
    /// </summary>
    /// <param name="interactableParent">Transform that will serve as the parent for the loaded interactables</param>
    public static void LoadInteractables(Transform interactableParent)
    {
        // Get the amount of interactables
        ChildAmountData interactAmData = SaveSystem.LoadInteractableAmount();
        int amountInteractables = interactAmData.GetChildAmount();

        // Load each interactable
        for (int i = 0; i < amountInteractables; ++i)
        {
            // Get the data for the interactable
            InteractableData interactData = SaveSystem.LoadInteractable(i);
            // Get the key to which prefab to spawn off the interactData
            int key = interactData.GetPrefabKey();
            // Use the key to get the prefab to load
            GameObject interactPref = SaveInteractablesController.GetInteractablePrefab(key);
            // Spawn the prefab as a child of the interactable parent
            GameObject interactObj = Object.Instantiate(interactPref, interactableParent);

            // Set its transform components
            interactObj.transform.position = interactData.GetPosition();

            // Get its Interact script
            Interactable interactScriptRef = interactObj.GetComponent<Interactable>();
            if (interactScriptRef == null)
            {
                Debug.LogError("No Interactable script was attached to " + interactObj.name);
                return;
            }
            // Set all the saved variables from that script
            interactScriptRef.SetCanInteractWith(interactData.GetCanInteractWith());
        }
    }
}
