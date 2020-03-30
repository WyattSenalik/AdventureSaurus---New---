using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamZoom : MonoBehaviour
{
    [SerializeField] private float _maxZoomSize = 8f;
    [SerializeField] private float _minZoomSize = 2f;
    [SerializeField] private float _zoomSpeed = 1f;
    private InputController _inpContRef; // Reference to the input controller
    private Camera _mainCam; // Reference to the main camera that this script will be controlling

    // Set references
    private void Awake()
    {
        GameObject gameContObj = GameObject.FindWithTag("GameController");
        if (gameContObj == null)
            Debug.Log("Could not find any GameObject with the tag GameController");
        else
        {
            _inpContRef = gameContObj.GetComponent<InputController>();
            if (_inpContRef == null)
                Debug.Log("There was no InputController attached to " + gameContObj.name);
        }

        GameObject mainCamObj = GameObject.FindWithTag("MainCamera");
        if (mainCamObj == null)
            Debug.Log("Could not find any GameObject with the tag MainCamera");
        else
        {
            _mainCam = mainCamObj.GetComponent<Camera>();
            if (_mainCam == null)
                Debug.Log("There was no Camera attached to " + mainCamObj.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Get the zoom amount
        float zoomAmount = _inpContRef.ZoomCommand();
        // If we are not past our limits
        if ((_mainCam.orthographicSize > _minZoomSize && zoomAmount < 0) || (_mainCam.orthographicSize < _maxZoomSize && zoomAmount > 0))
        {
            _mainCam.orthographicSize += zoomAmount * _zoomSpeed;
            // If we've surpassed the limits
            if (_mainCam.orthographicSize > _maxZoomSize)
                _mainCam.orthographicSize = _maxZoomSize;
            else if (_mainCam.orthographicSize < _minZoomSize)
                _mainCam.orthographicSize = _minZoomSize;
        }
    }
}
