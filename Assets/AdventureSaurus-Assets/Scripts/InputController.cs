using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputController : MonoBehaviour
{
    // Buttons that if the player clicks on, we shouldn't return the gridpoint
    [SerializeField] private RectTransform[] _buttonTransforms = null;
    [SerializeField] private Canvas _canvasRef = null;
    private bool _canInput;  // Whether input will be excepted or not

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
        Vector3 pos = Input.mousePosition;

        // If we can input and did not click a button
        if (_canInput && !WasButtonClick(pos))
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
            return -Input.GetAxis("Mouse ScrollWheel");
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
    /// Checks if the last input was a button click and not a grid click
    /// </summary>
    /// <param name="clickPos">Where was clicked in screen point</param>
    /// <returns>True if a button was clicked. False is it was a grid click</returns>
    private bool WasButtonClick(Vector2 clickPos)
    {
        // Convert the click location to a 0-1 viewport position
        Vector2 canvasPos = Camera.main.ScreenToViewportPoint(clickPos);
        // Make sure we did not click where a button is
        // Iterate over each button
        foreach (RectTransform buttRectTrans in _buttonTransforms)
        {
            // Get the center
            float centerX = buttRectTrans.anchoredPosition.x;
            float centerY = buttRectTrans.anchoredPosition.y;
            // Go over the parents as they help determine the center
            RectTransform curRectTrans = buttRectTrans.parent.GetComponent<RectTransform>();
            while (curRectTrans.gameObject != _canvasRef.gameObject)
            {
                centerX += curRectTrans.anchoredPosition.x;
                centerY += curRectTrans.anchoredPosition.y;
                curRectTrans = curRectTrans.parent.GetComponent<RectTransform>();
            }
            // Calculate the bounds of the button
            float left = (centerX - buttRectTrans.rect.width * 0.5f * buttRectTrans.localScale.x) * _canvasRef.scaleFactor / _canvasRef.pixelRect.xMax;
            float right = (centerX + buttRectTrans.rect.width * 0.5f * buttRectTrans.localScale.x) * _canvasRef.scaleFactor / _canvasRef.pixelRect.xMax;
            float bot = (centerY - buttRectTrans.rect.height * 0.5f * buttRectTrans.localScale.y) * _canvasRef.scaleFactor / _canvasRef.pixelRect.yMax;
            float top = (centerY + buttRectTrans.rect.height * 0.5f * buttRectTrans.localScale.y) * _canvasRef.scaleFactor / _canvasRef.pixelRect.yMax;

            // Get the bottom left and top right rectangle positions
            //Debug.Log(buttRectTrans.name + " has position (" + ((left + right) / 2) + ", " + ((bot + top) / 2) + ")");
            Vector2 botLeftCorner = new Vector2(left, bot);
            Vector2 topRightCorner = new Vector2(right, top);

            // If we clicked on where a button is
            if (canvasPos.x >= botLeftCorner.x && canvasPos.x <= topRightCorner.x &&
                canvasPos.y >= botLeftCorner.y && canvasPos.y <= topRightCorner.y)
            {
                //Debug.Log("Clicked on " + buttRectTrans.name + ". Click was at " + canvasPos.x + ", " + canvasPos.y +
                //    "). Button is at left corner: " + botLeftCorner +
                //    "right corner: " + topRightCorner);
                return true;
            }
            //Debug.Log("Did not click on " + buttRectTrans.name + ". Click was at " + canvasPos.x + ", " + canvasPos.y +
            //        "). Button is at left corner: " + botLeftCorner +
            //        " right corner: " + topRightCorner);
        }
        // If we made it here, we did not click on a button
        return false;
    }
}
