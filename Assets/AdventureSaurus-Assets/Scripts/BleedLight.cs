using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BleedLight : MonoBehaviour
{
    // Reference to the receiving room and broadcasting room
    private Room _broadcastRoom;
    public Room BroadcastRoom
    {
        set { _broadcastRoom = value; }
    }
    private Room _receiveRoom;
    public Room ReceiveRoom
    {
        set { _receiveRoom = value; }
    }

    // SpriteRenderer of the semi-circle
    [SerializeField] private SpriteRenderer _fillCirc = null;
    // SpriteRenderer of the inverse of the semi-circle
    [SerializeField] private SpriteRenderer _borderCirc = null;


    /// <summary>
    /// Updates the appropriate alpha values of the fill and border
    /// </summary>
    public void UpdateBleedLight()
    {
        UpdateFillAlpha();
        UpdateBorderAlpha();

        // If both the fill circle and the border are off, hide their objects
        // If both the fill circle and the border circle are off, show the cover
        if (_fillCirc.color.a == 1 && _borderCirc.color.a == 1)
        {
            _fillCirc.gameObject.SetActive(false);
            _borderCirc.gameObject.SetActive(false);
        }
        else
        {
            _fillCirc.gameObject.SetActive(true);
            _borderCirc.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Set the alpha value of the _fillCirc to the maximum lighting (minimum alpha) of the
    /// broadcast room and receive room
    /// </summary>
    private void UpdateFillAlpha()
    {
        // Get the most lit room's alpha
        float alphaVal = Mathf.Min(_receiveRoom.GetRoomAlpha(), _broadcastRoom.GetRoomAlpha());
        // Set the fill circle's alpha
        SetAlpha(alphaVal, _fillCirc);
    }

    /// <summary>
    /// Set the alpha value of the _borderCirc to the lighting of the receive room
    /// </summary>
    private void UpdateBorderAlpha()
    {
        // Get the lighting of the receieve room
        float alphaVal = _receiveRoom.GetRoomAlpha();
        // Set the border circle's alpha
        SetAlpha(alphaVal, _borderCirc);
    }

    /// <summary>
    /// Sets the alpha value of the passed in sprite renderer
    /// </summary>
    /// <param name="alphaVal">Alpha value to set [0, 1] float</param>
    /// <param name="sprRend">SpriteRenderer to edit the alpha of</param>
    private void SetAlpha(float alphaVal, SpriteRenderer sprRend)
    {
        // Cast the float to [0, 1] if it is outside the bounds
        if (alphaVal > 1)
            alphaVal = 1;
        else if (alphaVal < 0)
            alphaVal = 0;

        // Create a color that only changes the alpha
        Color col = sprRend.color;
        col.a = alphaVal;

        // Set the new color
        sprRend.color = col;
    }
}
