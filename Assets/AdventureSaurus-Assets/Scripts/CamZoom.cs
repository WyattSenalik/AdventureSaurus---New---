using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamZoom : MonoBehaviour
{
    [SerializeField] private float maxZoomSize = 8f;
    [SerializeField] private float minZoomSize = 2f;
    [SerializeField] private float zoomSpeed = 1f;
    private InputController inpContRef; // Reference to the input controller
    private Camera mainCam; // Reference to the main camera that this script will be controlling

    // Set references
    private void Awake()
    {
        GameObject gameContObj = GameObject.FindWithTag("GameController");
        if (gameContObj == null)
            Debug.Log("Could not find any GameObject with the tag GameController");
        else
        {
            inpContRef = gameContObj.GetComponent<InputController>();
            if (inpContRef == null)
                Debug.Log("There was no InputController attached to " + gameContObj.name);
        }

        GameObject mainCamObj = GameObject.FindWithTag("MainCamera");
        if (mainCamObj == null)
            Debug.Log("Could not find any GameObject with the tag MainCamera");
        else
        {
            mainCam = mainCamObj.GetComponent<Camera>();
            if (mainCam == null)
                Debug.Log("There was no Camera attached to " + mainCamObj.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Get the zoom amount
        float zoomAmount = inpContRef.ZoomCommand();
        // If we are not past our limits
        if ((mainCam.orthographicSize > minZoomSize && zoomAmount < 0) || (mainCam.orthographicSize < maxZoomSize && zoomAmount > 0))
        {
            mainCam.orthographicSize += zoomAmount * zoomSpeed;
            // If we've surpassed the limits
            if (mainCam.orthographicSize > maxZoomSize)
                mainCam.orthographicSize = maxZoomSize;
            else if (mainCam.orthographicSize < minZoomSize)
                mainCam.orthographicSize = minZoomSize;
        }
    }
}
