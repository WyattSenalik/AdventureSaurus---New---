using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    // The UI elements that will be turned off by the pause
    [SerializeField] private GameObject[] _uIElementsToTurnOff = null;
    // Holds if each element of uIElementsToTurnOff was on or off before the game was paused
    private bool[] _uIElementsActiveStatus;
    // The things we want to turn on when the game is paused
    [SerializeField] private GameObject[] _pauseMenuElements = null;

    // The timeScale before the pause
    private float _prevTime;
    // If the game is paused or not
    private bool _isPaused;

    // Events
    // When the game is paused
    public delegate void GamePause();
    public static event GamePause OnPauseGame;
    // When the game is unpaused
    public delegate void GameUnpause();
    public static event GameUnpause OnUnpauseGame;

    // Start is called before the first frame update
    void Start()
    {
        _prevTime = Time.timeScale;
        _isPaused = false;

        // Initialize whether the uIElementsToTurnOff are active or not
        _uIElementsActiveStatus = new bool[_uIElementsToTurnOff.Length];
        for (int i = 0; i < _uIElementsToTurnOff.Length; ++i)
        {
            _uIElementsActiveStatus[i] = _uIElementsToTurnOff[i].activeSelf;
        }
    }

    /// <summary>
    /// Pauses or unpauses the game. Called from a menu button
    /// </summary>
    public void PauseGame()
    {
        // If the game is current not paused, pause the game
        if (!_isPaused)
        {
            // Change the time scale
            _prevTime = Time.timeScale;
            Time.timeScale = 0;
            //if (Time.fixedDeltaTime - prevTime < 0)
                //Time.fixedDeltaTime = 0;
            //else
                //Time.fixedDeltaTime -= prevTime;
            
            // Pause the game
            if (OnPauseGame != null)
                OnPauseGame();
        }
        else
        {
            // Revert the time
            Time.timeScale = _prevTime;
            //Time.fixedDeltaTime += prevTime;

            // Unpause the game
            if (OnUnpauseGame != null)
                OnUnpauseGame();
        }
        // Turn on/off game-state ui elements
        ToggleUIElementsActivity(_isPaused);

        // Flip the pause state of the game
        _isPaused = !_isPaused;

        // Turn off/on the pause menu
        TogglePauseMenu(_isPaused);
    }

    /// <summary>
    /// Turns on or off ui elements
    /// </summary>
    /// <param name="onOff">Whether the ui elements should be turned on or off</param>
    private void ToggleUIElementsActivity(bool onOff)
    {
        // We have to deal with the UI elements that need to be turned off when we pause
        for (int i = 0; i < _uIElementsToTurnOff.Length; ++i)
        {
            if (_uIElementsToTurnOff[i] != null)
            {
                // If we are to be pausing
                if (!onOff)
                {
                    // Update the uIElementsActiveStatus for when we unpause and turn everything off
                    _uIElementsActiveStatus[i] = _uIElementsToTurnOff[i].activeSelf;
                    _uIElementsToTurnOff[i].SetActive(onOff);
                }
                // If we are to be unpausing, revert the on status back to what it was before the pause
                else
                {
                    _uIElementsToTurnOff[i].SetActive(_uIElementsActiveStatus[i]);
                }
            }
        }
    }

    /// <summary>
    /// Enables/Disables the pause menu
    /// </summary>
    /// <param name="onOff">Whether to turn on or off the pause menu</param>
    private void TogglePauseMenu(bool onOff)
    {
        foreach (GameObject element in _pauseMenuElements)
        {
            if (element == null)
            {
                Debug.Log("An element in pauseMenuElements is null. Check the predefined elements in the editor.");
                continue;
            }
            element.SetActive(onOff);
        }
    }
}