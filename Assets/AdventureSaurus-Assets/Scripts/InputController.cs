using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputController : MonoBehaviour
{
    // Whether input will be excepted or not
    private bool _canInput;

    // For touch zoom
    // How fast the zoom is
    [SerializeField] private float zoomSpeed = 0.03f;

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
    // Unsubscribe from events
    private void OnDisable()
    {
        // Unsubscribe to the pause event (since if this is inactive, the game is paused)
        Pause.OnPauseGame -= HideScript;
        // When the game is unpaused, re-enable this script
        Pause.OnUnpauseGame += ShowScript;
    }

    // Called when the gameobject is destroyed
    // Unsubscribe form ALL events
    private void OnDestroy()
    {
        Pause.OnPauseGame -= HideScript;
        Pause.OnUnpauseGame -= ShowScript;
    }


    // Initialize variables
    private void Start()
    {
        _canInput = true;
    }

    /// <summary>
    /// Returns true if the player has issued the "select/click" command.
    /// Left mouse click for now
    /// </summary>
    /// <returns>Returns true on the frame the command was issued, false otherwise</returns>
    public bool SelectClick()
    {
        //Vector3 pos = Input.mousePosition;

        // If we can input and did not click a button
        if (_canInput /*&& !WasButtonClick(pos)*/)
            return Input.GetMouseButtonDown(0);
        return false;
    }

    /// <summary>
    /// Gets the location data of where a select would have occured
    /// Currently converts the mouse position from screen to world point then world point to grid point
    /// </summary>
    /// <returns>The grid position of where a select command occured</returns>
    public Vector2Int SelectToGridPoint()
    {
        if (_canInput)
        {
            Vector3 pos = Input.mousePosition;

            pos.z = 20;
            pos = Camera.main.ScreenToWorldPoint(pos);


            return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
        }
        return new Vector2Int(int.MinValue, int.MinValue);
    }

    /// <summary>
    /// Returns the amount to zoom in or out
    /// Currently uses the mouse wheel
    /// </summary>
    /// <returns>Zoom amount. Positive is in. Negative is out</returns>
    public float ZoomCommand()
    {
        if (_canInput)
        {
            // The input of the mouse (in the case we are playing on pc)
            float mouseInput = -Input.GetAxis("Mouse ScrollWheel");

            // The input of the touch controls (in case we are playing mobile)
            float touchInput = 0;
            if (Input.touchCount == 2)
            {
                // Get the touches
                Touch firstTouch = Input.GetTouch(0);
                Touch secondTouch = Input.GetTouch(1);
                // Get the positions of the touches
                Vector2 firstTouchPrevPos = firstTouch.position - firstTouch.deltaPosition;
                Vector2 secondTouchPrevPos = secondTouch.position - secondTouch.deltaPosition;
                // Get the previous and current differences in position
                float touchesPrevPosDiff = (firstTouchPrevPos - secondTouchPrevPos).magnitude;
                float touchesCurPosDiff = (firstTouch.position - secondTouch.position).magnitude;
                // Set the touch input
                touchInput = (firstTouch.deltaPosition - secondTouch.deltaPosition).magnitude * zoomSpeed;

                // If the previous is less than the current we want to zoom in, so make this negative
                if (touchesPrevPosDiff < touchesCurPosDiff)
                    touchInput = -touchInput;
            }


            if (Mathf.Abs(mouseInput) >= Mathf.Abs(touchInput))
                return mouseInput;
            else
                return touchInput;
        }
        return 0;
    }

    /// <summary>
    /// Sets canInput to true
    /// </summary>
    public void AllowInput()
    {
        _canInput = true;
    }

    /// <summary>
    /// Sets canInput to false
    /// </summary>
    public void DenyInput()
    {
        _canInput = false;
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
