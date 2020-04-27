using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapSaving : MonoBehaviour
{
    // All the tilesets
    [SerializeField] private TileSet[] _allTileSets = null;

    // Reference to the tilemap to save
    private Tilemap _tilemap;

    // The corners of the tilemap
    private Vector2Int _gridBotLeft;
    private Vector2Int _gridTopRight;


    // Called when the component is set active
    // Subscribe to events
    private void OnEnable()
    {
        // When generation is finished, intialize some references
        ProceduralGenerationController.OnFinishGenerationNoParam += Initialize;
        // When the grid is finished being calculated set the corners of the tilemap
        MoveAttackController.OnGridFinishedCalculating += SetCorners;
        // When the game saves, save the tilemap
        SaveSystem.OnSave += SaveTilemap;
    }
    // Called when the component is toggled off
    // Unsubscribe from events
    private void OnDisable()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
        MoveAttackController.OnGridFinishedCalculating -= SetCorners;
        SaveSystem.OnSave -= SaveTilemap;
    }
    // Called when the component is destroyed
    // Unsubscribe from ALL events
    private void OnDestroy()
    {
        ProceduralGenerationController.OnFinishGenerationNoParam -= Initialize;
        MoveAttackController.OnGridFinishedCalculating -= SetCorners;
        SaveSystem.OnSave -= SaveTilemap;
    }

    // Called before Start
    private void Awake()
    {
        // Create the hashtables that will be used for saving the tilemap
        if (_allTileSets != null)
            SaveTilemapController.BuildHashtables(_allTileSets);
    }

    /// <summary>
    /// Initializes the references to things that did not exist prior to generation
    /// </summary>
    private void Initialize()
    {
        GameObject tilemapObj = GameObject.FindWithTag("Tilemap");
        if (tilemapObj != null)
            _tilemap = tilemapObj.GetComponent<Tilemap>();
        else
            Debug.LogError("No GameObject with the tag Tilemap was found");
    }

    /// <summary>
    /// Sets the bot left and top right of the tilemap
    /// </summary>
    /// <param name="gridTopLeft">Top left corner of the tilemap</param>
    /// <param name="gridBotRight">Bot right corner of the tilemap</param>
    private void SetCorners(Vector2Int gridTopLeft, Vector2Int gridBotRight)
    {
        _gridBotLeft = new Vector2Int(gridTopLeft.x, gridBotRight.y);
        _gridTopRight = new Vector2Int(gridBotRight.x, gridTopLeft.y);
    }


    private void SaveTilemap()
    {
        // Save the bounds of the tilemap
        SaveSystem.SaveTilemapBounds(_gridBotLeft, _gridTopRight);

        // Save each of the tiles
        // Iterate over one column of the tilemap (going up)
        for (int col = _gridBotLeft.y; col <= _gridTopRight.y; ++col)
        {
            // Iterate over one row of the tilemap (going right)
            for (int row = _gridBotLeft.x; row <= _gridTopRight.x; ++row)
            {
                // Position of the tile (3D)
                Vector3Int tilePos = new Vector3Int(row, col, 0);

                // Key of the tile (-1 means no tile at that location)
                int key = -1;
                // Pull off the TileBase at the current position
                TileBase tileBase = _tilemap.GetTile(tilePos);
                // If its not null, use the hashtable to get the corresponding key
                if (tileBase != null)
                    key = SaveTilemapController.GetKey(tileBase);

                // Save the current tile's data
                SaveSystem.SaveTile(tilePos, key);
            }
        }
    }
}
