﻿using System.Collections;
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
    private SpriteRenderer _fillCirc = null;


    // Called before start
    // Set references
    private void Awake()
    {
        _fillCirc = this.GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Updates the appropriate alpha values of the fill
    /// </summary>
    public void UpdateBleedLight()
    {
        UpdateFillAlpha();
    }

    /// <summary>
    /// Set the alpha value of the _fillCirc to the maximum lighting (minimum alpha) of the
    /// broadcast room and receive room
    /// </summary>
    private void UpdateFillAlpha()
    {
        // Get the most lit room's alpha
        float alphaVal = Mathf.Max(_receiveRoom.GetRoomAlpha(), _broadcastRoom.GetRoomAlpha());
        // Set the fill circle's alpha
        SetAlpha(alphaVal, _fillCirc);
    }


    /// <summary>
    /// Sets the alpha value of the passed in sprite renderer
    /// </summary>
    /// <param name="alphaVal">Alpha value to set [0, 1] float</param>
    /// <param name="sprRend">SpriteRenderer to edit the alpha of</param>
    private void SetAlpha(float alphaVal, SpriteRenderer sprRend)
    {
        // Clamps the float to [0, 1] if it is outside the bounds
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
