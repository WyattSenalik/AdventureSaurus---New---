using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Campfire : Interactable
{
    // The lit version of the campfire. Will be set in procedural generation
    private AnimatedTile _litCampfire;
    public AnimatedTile LitCampfire
    {
        set { _litCampfire = value; }
    }

    // Reference to the tilemap. Will also be set in procedural generation
    private Tilemap _tilemapRef;
    public Tilemap TilemapRef
    {
        set { _tilemapRef = value; }
    }
  

    /// <summary>
    /// Starts the interaction with the campfire.
    /// Restore health to all ally characters
    /// </summary>
    public override void StartInteract()
    {
        // Get the players
        GameObject[] allyObjs = GameObject.FindGameObjectsWithTag("Player");

        // Pull off their health scripts and heal them by half their max hp
        for (int i = 0; i < allyObjs.Length; ++i)
        {
            AllyHealth healthRef = allyObjs[i].GetComponent<AllyHealth>();
            if (healthRef != null)
                healthRef.Heal(healthRef.MaxHP / 2);
        }

        /// Turn on the campfire and make it non interactable
        // Get the position of this as a Vector3Int
        Vector3Int tilemapPos = new Vector3Int(Mathf.RoundToInt(this.transform.position.x),
            Mathf.RoundToInt(this.transform.position.y), 0);
        // Set the tile at that spot to the active campfire
        _tilemapRef.SetTile(tilemapPos, _litCampfire);

        // Make it so that this interactable cannot be interacted with again
        _canInteractWith = false;

        // Call the base last, since it calls the event.
        base.StartInteract();
    }
}
