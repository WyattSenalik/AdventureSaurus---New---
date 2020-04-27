using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCam : MonoBehaviour
{
    //Camera Variables
    [SerializeField] private float _dragSpeed = 3f;

    // Reference to the map camera
    private Camera _camToWorkOn;
   

    private List<Transform> _allies;
    // Camera Velocity
    private Vector3 _camVel;

    //Mouse Position
    private Vector3 _mouseOrigin;
    private bool _isDragging;

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
 
    private void LateUpdate()
    {
        
        //finds the mapmenu and checks if the mapmenu is on
        GameObject mapOn = GameObject.FindWithTag("MapMenu");
        Rect bounds = new Rect(100, 35, 750, 800);
        if (mapOn)
        {
            if (Input.GetMouseButtonDown(0) && mapOn.activeInHierarchy && bounds.Contains(Input.mousePosition))
            {
                //right click was pressed    
                _mouseOrigin = Input.mousePosition;
                _isDragging = true;
            }
        }
        //checks when right click isn't pressed
        if (!Input.GetMouseButton(0))
        {
            _isDragging = false;
        }
        //if right click is pressed it will drag the cam around
        if (_isDragging)
        {
            Vector3 targetPosition = _camToWorkOn.ScreenToViewportPoint(Input.mousePosition - _mouseOrigin);

            // update x and y but not z
            Vector3 move = new Vector3(-targetPosition.x * _dragSpeed, -targetPosition.y * _dragSpeed, 0);

            _camToWorkOn.transform.Translate(move, Space.Self);
        }
        
    }
}
    