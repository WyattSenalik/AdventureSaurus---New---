using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCam : MonoBehaviour
{
    // Reference to the map camera
    private Camera _camToWorkOn;

    // Called when the component is toggled active
    // Subscribe to events
    private void OnEnable()
    {
        // When the grid finishes calculating, initialize the map camera size
        MoveAttackController.OnGridFinishedCalculating += Initialize;
    }

    // Called when the component is toggled off
    // Unsubscribe from events
    private void OnDisable()
    {
        MoveAttackController.OnGridFinishedCalculating -= Initialize;
    }

    // Called when the gameobject is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        MoveAttackController.OnGridFinishedCalculating -= Initialize;
    }

    // Called before Start
    // Set references to itself
    private void Awake()
    {
        // Get the camera we will work on
        _camToWorkOn = this.GetComponent<Camera>();
        if (_camToWorkOn == null)
            Debug.Log("Could not find Camera attached to " + this.name);
    }

    /// <summary>
    /// Called from MoveAttackController by the OnGridFinishedCalculating event
    /// Gets the camera to work on and sets its orthographic size.
    /// </summary>
    /// <param name="topLeft">Top left of the grid</param>
    /// <param name="botRight">Bot right of the grid</param>
    private void Initialize(Vector2Int topLeft, Vector2Int botRight)
    {
        // Set its size
        int xDist = botRight.x - topLeft.x;
        int yDist = topLeft.y - botRight.y;
        if (xDist > yDist)
            _camToWorkOn.orthographicSize = xDist;
        else
            _camToWorkOn.orthographicSize = yDist;
    }
}
