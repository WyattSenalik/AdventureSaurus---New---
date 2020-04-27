using UnityEngine;
using UnityEngine.UI;

public class TestForCheckpoint : MonoBehaviour
{
    // The button that takes the player to the most recent checkpoint
    [SerializeField] private Button _checkpointButton = null;

    // Start is called before the first frame update
    private void Start()
    {
        // Check if there is any saved checkpoint.
        // If there isn't we don't want the player to be able to press the checkpoint button
        if (!SaveSystem.CheckForCheckpoint())
            _checkpointButton.interactable = false;
        else
            _checkpointButton.interactable = true;
    }
}
