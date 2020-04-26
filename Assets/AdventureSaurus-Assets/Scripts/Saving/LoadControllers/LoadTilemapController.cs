using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class LoadTilemapController
{
    /// <summary>
    /// Creates a new grid and loads the saved tiles into its tilemap
    /// </summary>
    /// <param name="gridPrefab">Prefab of the grid which holds the tilemap as a child of it</param>
    /// <returns>Tilemap. Reference to the loaded tilemap</returns>
    public static Tilemap LoadTilemap(GameObject gridPrefab)
    {
        // Create the grid and get the tilemap
        Tilemap tilemap = Object.Instantiate(gridPrefab).transform.GetChild(0).GetComponent<Tilemap>();

        // Load the bounds for the tilemap
        TilemapBoundsData tilemapBoundsData = SaveSystem.LoadTilemapBounds();
        Vector2Int botLeft = tilemapBoundsData.GetBotLeftCorner();
        Vector2Int topRight = tilemapBoundsData.GetTopRightCorner();

        // Load each of the tiles
        // Iterate over one column of the tilemap (going up)
        for (int col = botLeft.y; col <= topRight.y; ++col)
        {
            // Iterate over one row of the tilemap (going right)
            for (int row = botLeft.x; row <= topRight.x; ++row)
            {
                // Position of the tile (3D)
                Vector3Int tilePos = new Vector3Int(row, col, 0);

                // Load the data of the tile there
                TileSaveData tileData = SaveSystem.LoadTile(tilePos);
                // Get the key of the tile
                int key = tileData.GetKey();
                // If the key is valid, there is a saved tile there
                if (key >= 0)
                {
                    // Get the tile by the saved key
                    Tile tile = SaveTilemapController.GetTile(key);
                    // Set the tile at the position to the correct one
                    tilemap.SetTile(tilePos, tile);
                }
            }
        }

        return tilemap;
    }
}
