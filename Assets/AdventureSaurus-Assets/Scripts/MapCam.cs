using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCam : MonoBehaviour
{
    private Camera camToWorkOn;

    /// <summary>
    /// Called from Procedural Generation after everything is created.
    /// Gets the camera to work on and sets its orthographic size.
    /// </summary>
    /// <param name="topLeft">Top left of the grid</param>
    /// <param name="botRight">Bot right of the grid</param>
    public void Initialize(Vector2Int topLeft, Vector2Int botRight)
    {
        // Get the camera we will work on
        camToWorkOn = this.GetComponent<Camera>();
        if (camToWorkOn == null)
            Debug.Log("Could not find Camera attached to " + this.name);

        // Set its size
        int xDist = botRight.x - topLeft.x;
        int yDist = topLeft.y - botRight.y;
        if (xDist > yDist)
            camToWorkOn.orthographicSize = xDist;
        else
            camToWorkOn.orthographicSize = yDist;
    }
}
