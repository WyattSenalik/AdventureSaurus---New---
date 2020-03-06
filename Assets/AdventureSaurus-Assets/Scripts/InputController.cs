using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    private bool canInput;  // Whether input will be excepted or not

    // Initialize variables
    private void Start()
    {
        canInput = true;
    }

    /// <summary>
    /// Returns true if the player has issued the "select/click" command.
    /// Left mouse click for now
    /// </summary>
    /// <returns>Returns true on the frame the command was issued, false otherwise</returns>
    public bool SelectClick()
    {
        if (canInput)
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
        if (canInput)
        {
            Vector3 pos = Input.mousePosition;
            pos.z = 20;
            pos = Camera.main.ScreenToWorldPoint(pos);

            return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
        }
        return new Vector2Int(0, 0);
    }

    /// <summary>
    /// Returns the amount to zoom in or out
    /// Currently uses the mouse wheel
    /// </summary>
    /// <returns>Zoom amount. Positive is in. Negative is out</returns>
    public float ZoomCommand()
    {
        if (canInput)
        {
            return -Input.GetAxis("Mouse ScrollWheel");
        }
        return 0;
    }

    /// <summary>
    /// Sets canInput to true
    /// </summary>
    public void AllowInput()
    {
        canInput = true;
    }

    /// <summary>
    /// Sets canInput to false
    /// </summary>
    public void DenyInput()
    {
        canInput = false;
    }
}
