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

    
    // Called on the first frame update
    private void Start()
    {
        try
        {
            GameObject genContObj = GameObject.FindWithTag("GenerationController");
            TileSet activeTileSet = genContObj.GetComponent<GenerateTiles>().GetActiveTileSet();
            _litCampfire = activeTileSet.GetLitCampfire();
        }
        catch
        {
            Debug.LogError("Error setting lit campfire");
        }

        _tilemapRef = GameObject.FindWithTag("Tilemap").GetComponent<Tilemap>();
    }

    /// <summary>
    /// Starts the interaction with the campfire.
    /// Restore health to all ally characters
    /// </summary>
    public override void StartInteract()
    {
        // Assume we will be starting the fire
        bool shouldActivate = true;

        // Test if there are any active enemies. If there are, don't let the campfire activate
        try
        {
            GameObject gameContObj = GameObject.FindWithTag("GameController");
            EnemyTurnController enTurnCont = gameContObj.GetComponent<EnemyTurnController>();
            if (enTurnCont.GetAmountEnemies() > 0)
                shouldActivate = false;
        }
        catch
        {
            Debug.LogError("Could not get the EnemyTurnController");
        }

        if (shouldActivate) {
            // Get the players
            GameObject[] allyObjs = GameObject.FindGameObjectsWithTag("Player");
            // Pull off their health scripts and heal them by half their max hp
            for (int i = 0; i < allyObjs.Length; ++i)
            {
                AllyHealth healthRef = allyObjs[i].GetComponent<AllyHealth>();
                if (healthRef != null)
                    healthRef.Heal(healthRef.MaxHP / 2);
            }

            //Refills Potion
            GameObject refill = GameObject.FindGameObjectWithTag("PotionHolder");
            refill.GetComponent<HealthPotion>().RefillPotion();
            
            /// Turn on the campfire and make it non interactable
            // Get the position of this as a Vector3Int
            Vector3Int tilemapPos = new Vector3Int(Mathf.RoundToInt(this.transform.position.x),
                Mathf.RoundToInt(this.transform.position.y), 0);
            // Set the tile at that spot to the active campfire
            _tilemapRef.SetTile(tilemapPos, _litCampfire);

            // Make it so that this interactable cannot be interacted with again
            _canInteractWith = false;

            // Save a checkpoint
            SaveSystem.SaveGame();
        }

        // Call the base last, since it calls the event.
        base.StartInteract();
    }
}
