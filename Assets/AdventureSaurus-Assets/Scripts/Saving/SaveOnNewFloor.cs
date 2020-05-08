using UnityEngine;

public class SaveOnNewFloor : MonoBehaviour
{
    // Called before Start
    // Subscribe to events
    private void Awake()
    {
        // When a new floor is generated, save it
        ProceduralGenerationController.OnPostFinishGeneration += SaveSystem.SaveGame;
    }
    // Called when the component is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        ProceduralGenerationController.OnPostFinishGeneration -= SaveSystem.SaveGame;
    }

}
