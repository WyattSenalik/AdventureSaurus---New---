using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPauseGameScript : MonoBehaviour
{
    // Called when the component is toggled active
    // Subscribe to events
    private void OnEnable()
    {
        // When the game is paused, disable this script
        Pause.OnPauseGame += HideScript;
        // Unsubscribe to the unpause event (since if this is active, the game is unpaused)
        Pause.OnUnpauseGame -= ShowScript;
    }

    // Called when the component is toggled inactive
    // Unsubscribe to events
    private void OnDisable()
    {
        // Unsubscribe to the pause event (since if this is inactive, the game is paused)
        Pause.OnPauseGame -= HideScript;
        // When the game is unpaused, re-enable this script
        Pause.OnUnpauseGame += ShowScript;
    }

    // Called when the gameobject is destroyed
    // Unsubscribe to ALL events
    private void OnDestroy()
    {
        Pause.OnPauseGame -= HideScript;
        Pause.OnUnpauseGame -= ShowScript;
    }

    /// <summary>
    /// Toggles off this script
    /// </summary>
    private void HideScript()
    {
        this.enabled = false;
    }

    /// <summary>
    /// Toggles on this script
    /// </summary>
    private void ShowScript()
    {
        this.enabled = true;
    }
}
