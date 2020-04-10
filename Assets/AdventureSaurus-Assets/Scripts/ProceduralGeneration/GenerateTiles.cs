using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GenerateTiles : MonoBehaviour
{
    // The object that will be spawned that contains an empty tile map
    [SerializeField] private GameObject _tileMapPrefab = null;
    // Chance to spawn one-tiled decorations on a tile
    [SerializeField] private int _singleTileDecorationChance = 15;
    // Chance to spawn two-tiled decorations on a tile
    [SerializeField] private int _twoTileDecorationChance = 50;
    // TileSet to be used
    [SerializeField] private TileSet _tileSet = null;
    public TileSet ActiveTileSet
    {
        get { return _tileSet; }
    }

    // Reference to the tile map
    private Tilemap _tileMapRef;

    // Called before start
    private void Awake()
    {
        // Validation
        if (_tileMapPrefab == null)
            Debug.Log("tileMapPrefab was not set correctly in GenerateTiles attached to " + this.name);
        if (_tileSet == null)
            Debug.Log("tileSet was not set correctly in GenerateTiles attached to " + this.name);
    }

    /// <summary>
    /// Places tiles down at rooms that are children of roomParent, walls that are children of wallParent, and stairs at the
    /// position of stairsTrans. Also randomly places dinosaur bones in the rooms.
    /// </summary>
    /// <param name="roomParent">The parent of all the rooms (position and size)</param>
    /// <param name="wallParent">The parent of all the walls (position)</param>
    /// <param name="stairsTrans">Transform of the stairs (position)</param>
    /// <returns>Returns the newly created Tilemap</returns>
    public Tilemap SpawnTileMap(Transform roomParent, Transform wallParent, Transform stairsTrans)
    {
        // Initialize the list of potential positions to spawn the one-tiled decor
        List<Vector2Int> oneTiledDecor = new List<Vector2Int>();
        // Initialize the list of potential position to spawn the two-tiled decor
        List<Vector2Int> twoTiledDecor = new List<Vector2Int>();

        // Create the tilemap
        GameObject tileMapObj = Instantiate(_tileMapPrefab, new Vector3(-0.5f, -0.5f, 0), Quaternion.identity);
        tileMapObj.name = "Grid";
        _tileMapRef = tileMapObj.transform.GetChild(0).GetComponent<Tilemap>();
        if (_tileMapRef == null)
            Debug.Log("There is no Tilemap attached to the child of " + tileMapObj.name);

        // Clear any tiles on the map
        _tileMapRef.ClearAllTiles();

        // Add floor tiles to the rooms
        // Iterate over each room
        foreach (Transform curRoom in roomParent)
        {
            Vector2Int topLeft = new Vector2Int(Mathf.RoundToInt(curRoom.position.x - (curRoom.localScale.x - 1) / 2f),
                Mathf.RoundToInt(curRoom.position.y + (curRoom.localScale.y - 1) / 2f));
            int amRows = Mathf.RoundToInt(curRoom.localScale.y);
            int amCols = Mathf.RoundToInt(curRoom.localScale.x);
            // Iterate over the rows of the room
            for (int curRow = 0; curRow < amRows; ++curRow)
            {
                // Iterate over the columns of the room
                for (int curCol = 0; curCol < amCols; ++curCol)
                {
                    // Choose a random floor tile
                    int randomIndex = Random.Range(0, _tileSet.SingleFloorTilesLength());
                    // Calculate the current location
                    Vector2Int curLocation = topLeft + new Vector2Int(curCol, -curRow);

                    // Give a chance for this tile to be convered in single-tiled decor
                    // Only if there are any single-tiled decorations
                    if (_tileSet.SingleTiledDecorationTilesLength() > 0 &&
                        Random.Range(0, _singleTileDecorationChance) == 0)
                    {
                        oneTiledDecor.Add(curLocation);
                    }
                    // Give a chance for this tile to be covered in a two-tiled decor
                    // Only if there are any double-tiled decorations
                    else if (_tileSet.TwoTiledDecorationTilesLength() > 0 &&
                        Random.Range(0, _twoTileDecorationChance) == 0)
                    {
                        twoTiledDecor.Add(curLocation);
                    }

                    // Add a single room tile at the current location
                    AddTile(curLocation, _tileSet.GetSingleFloorTile(randomIndex));
                }
            }
        }

        // Add walls
        // Iterate over the walls
        foreach (Transform curWall in wallParent)
        {
            // The type of wall the current wall is
            // If (-1, 0) - wall is left; (0, -1) - wall is close
            // (1, 0) - wall is right; (0, 1) - wall is far
            // (-1, -1) - wall is closeLeftOuterCorner; (1, -1) - wall is closeRightOuterCorner
            // (-1, 1) - wall is farLeftOuterCorner; (1, 1) - wall is farRightOuterCorner
            Vector2Int curWallTypeVect = Vector2Int.zero;
            Vector2Int innerWallTypeVect = Vector2Int.zero;
            // Determine what time of tile this is by looking at its neighbors
            BoundsInt aroundBounds = new BoundsInt(-1, -1, 0, 3, 3, 1);
            foreach (Vector3Int curPos in aroundBounds.allPositionsWithin)
            {
                // If its the position of the current tile
                if (curPos.x == 0 && curPos.y == 0) continue;
                // Get the tile on the tile map at that locaiton
                Vector3Int curWallPos = new Vector3Int(Mathf.RoundToInt(curWall.position.x), Mathf.RoundToInt(curWall.position.y), 0);
                Sprite curTileSpr = _tileMapRef.GetSprite(curPos + curWallPos);
                // Test if the sprite exists, if it doesn't we can determine somethings about this wall
                if (curTileSpr == null)
                {
                    // These should be called max once per iteration, since a wall will never have both left and right empty
                    // or both close and far tiles empty
                    if (curPos.x != 0 && curPos.y == 0)
                        curWallTypeVect.x = curPos.x;
                    else if (curPos.y != 0 && curPos.x == 0)
                        curWallTypeVect.y = curPos.y;
                    // If, its neither, then it may be an inner wall, so save the current position
                    else
                    {
                        innerWallTypeVect.x = curPos.x;
                        innerWallTypeVect.y = curPos.y;
                    }
                }
            }

            // If the wall is a left wall
            if (curWallTypeVect.x == -1 && curWallTypeVect.y == 0)
            {
                // Choose a random tile
                int randomIndex = Random.Range(0, _tileSet.LeftWallTilesLength());
                AddTile(curWall.position, _tileSet.GetLeftWallTile(randomIndex));
            }

            // If the wall is a right wall
            else if (curWallTypeVect.x == 1 && curWallTypeVect.y == 0)
            {
                // Choose a random tile
                int randomIndex = Random.Range(0, _tileSet.RightWallTilesLength());
                AddTile(curWall.position, _tileSet.GetRightWallTile(randomIndex));
            }

            // If the wall is a close wall
            else if (curWallTypeVect.x == 0 && curWallTypeVect.y == -1)
            {
                // Choose a random tile
                int randomIndex = Random.Range(0, _tileSet.CloseWallTilesLength());
                AddTile(curWall.position, _tileSet.GetCloseWallTile(randomIndex));
            }

            // If the wall is a far wall
            else if (curWallTypeVect.x == 0 && curWallTypeVect.y == 1)
            {
                // Choose a random tile
                int randomIndex = Random.Range(0, _tileSet.FarWallTilesLength());
                AddTile(curWall.position, _tileSet.GetFarWallTile(randomIndex));
            }

            // Outer corners
            // If the wall is a close left outer corner
            else if (curWallTypeVect.x == -1 && curWallTypeVect.y == -1)
            {
                // Choose a random tile
                int randomIndex = Random.Range(0, _tileSet.CloseLeftOuterCornerWallTilesLength());
                AddTile(curWall.position, _tileSet.GetCloseLeftOuterCornerWallTile(randomIndex));
            }

            // If the wall is a close right outer corner
            else if (curWallTypeVect.x == 1 && curWallTypeVect.y == -1)
            {
                // Choose a random tile
                int randomIndex = Random.Range(0, _tileSet.CloseRightOuterCornerWallTilesLength());
                AddTile(curWall.position, _tileSet.GetCloseRightOuterCornerWallTile(randomIndex));
            }

            // If the wall is a far left outer corner
            else if (curWallTypeVect.x == -1 && curWallTypeVect.y == 1)
            {
                // Choose a random tile
                int randomIndex = Random.Range(0, _tileSet.FarLeftOuterCornerWallTilesLength());
                AddTile(curWall.position, _tileSet.GetFarLeftOuterCornerWallTile(randomIndex));
            }

            // If the wall is a far right outer corner
            else if (curWallTypeVect.x == 1 && curWallTypeVect.y == 1)
            {
                // Choose a random tile
                int randomIndex = Random.Range(0, _tileSet.FarRightOuterCornerWallTilesLength());
                AddTile(curWall.position, _tileSet.GetFarRightOuterCornerWallTile(randomIndex));
            }

            // Inner corners
            // If the wall is a close left inner corner
            else if (innerWallTypeVect.x == -1 && innerWallTypeVect.y == -1)
            {
                // Choose a random tile
                int randomIndex = Random.Range(0, _tileSet.CloseLeftInnerCornerWallTilesLength());
                AddTile(curWall.position, _tileSet.GetCloseLeftInnerCornerWallTile(randomIndex));
            }

            // If the wall is a close right inner corner
            else if (innerWallTypeVect.x == 1 && innerWallTypeVect.y == -1)
            {
                // Choose a random tile
                int randomIndex = Random.Range(0, _tileSet.CloseRightInnerCornerWallTilesLength());
                AddTile(curWall.position, _tileSet.GetCloseRightInnerCornerWallTile(randomIndex));
            }

            // If the wall is a far left inner corner
            else if (innerWallTypeVect.x == -1 && innerWallTypeVect.y == 1)
            {
                // Choose a random tile
                int randomIndex = Random.Range(0, _tileSet.FarLeftInnerCornerWallTilesLength());
                AddTile(curWall.position, _tileSet.GetFarLeftInnerCornerWallTile(randomIndex));
            }

            // If the wall is a far right inner corner
            else if (innerWallTypeVect.x == 1 && innerWallTypeVect.y == 1)
            {
                // Choose a random tile
                int randomIndex = Random.Range(0, _tileSet.FarRightInnerCornerWallTilesLength());
                AddTile(curWall.position, _tileSet.GetFarRightInnerCornerWallTile(randomIndex));
            }

            // If the wall is a completely different kind of tile
            else
            {
                Debug.Log("Strange wall encountered CurWallTypeVect: " + curWallTypeVect + ". InnerWallTypeVect: " + innerWallTypeVect);
                AddTile(curWall.position, _tileSet.GetSingleFloorTile(0));
            }
        }

        // Add the stairs to the tilemap
        // Choose a random tile
        int randomStairIndex = Random.Range(0, _tileSet.StairTilesLength());
        AddTile(stairsTrans.position, _tileSet.GetStairTile(randomStairIndex));

        // Iterate over the tiles we said we would spawn single-tiled decor at and try to spawn some
        foreach (Vector2Int tilePos in oneTiledDecor)
        {
            AttemptPlaceSingleTileDecoration(tilePos);
        }
        // Iterate over the tiles we said we would spawn two-tiled decor at and try to spawn some
        foreach (Vector2Int tilePos in twoTiledDecor)
        {
            AttemptPlaceTwoTileDecoration(tilePos);
        }

        // Give the tilemap
        return _tileMapRef;
    }

    /// <summary>
    /// Adds a tile at the given world position with the specified tile
    /// </summary>
    /// <param name="worldPos">The position to spawn the tile at</param>
    /// <param name="tileToAdd">The tile to spawn</param>
    private void AddTile(Vector3 worldPos, Tile tileToAdd)
    {
        Vector3Int spawnPos = new Vector3Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y), 0);
        _tileMapRef.SetTile(spawnPos, tileToAdd);
    }

    /// <summary>
    /// Adds a tile at the given grid position with the specified tile
    /// </summary>
    /// <param name="gridPos">The position to spawn the tile at</param>
    /// <param name="tileToAdd">The tile to spawn</param>
    private void AddTile(Vector2Int gridPos, Tile tileToAdd)
    {
        Vector3Int spawnPos = new Vector3Int(gridPos.x, gridPos.y, 0);
        _tileMapRef.SetTile(spawnPos, tileToAdd);
    }

    /// <summary>
    /// Trys to create a one-tiled decoration at a position
    /// </summary>
    /// <param name="testPos">The position on the tilemap we are trying to place the single-tiled decor</param>
    /// <returns>Returns true if successful</returns>
    private bool AttemptPlaceSingleTileDecoration(Vector2Int testPos)
    {
        // Get the sprite on the current tile
        Sprite curTileSpr = _tileMapRef.GetSprite(new Vector3Int(testPos.x, testPos.y, 0));
        // Test if the current tile is a floor tile or not
        // Assume it isn't a floor tile
        bool isFloorTile = false;
        // Iterate over the floor tiles
        for (int i = 0; i < _tileSet.SingleFloorTilesLength(); ++i)
        {
            Tile curFloorTile = _tileSet.GetSingleFloorTile(i);
            // If we find a floor tile with the same sprite, this is a floor tile
            if (curFloorTile != null && curFloorTile.sprite == curTileSpr)
            {
                isFloorTile = true;
                break;
            }
        }
        // If this was not a floor tile, we can't spawn the decir
        if (!isFloorTile)
            return false;

        // If we made it all the way here, we can spawn decor
        // Pick a random decor
        int randomIndex = Random.Range(0, _tileSet.SingleTiledDecorationTilesLength());
        // Add the tile
        AddTile(testPos, _tileSet.GetSingleTiledDecorationTile(randomIndex));

        return true;
    }

    /// <summary>
    /// Trys to create a two-tiled decoration at a position
    /// </summary>
    /// <param name="testLeftPos">The position on the tilemap we are trying to place the left part of the two-tiled decor</param>
    /// <returns>Returns true if successful</returns>
    private bool AttemptPlaceTwoTileDecoration(Vector2Int testLeftPos)
    {
        //Debug.Log("Trying to spawn dino at " + testBonePos);
        // Iterate over this tile and its neighbors to find out if any of the adjacent tiles make it so we can't the decoration
        BoundsInt aroundLeft = new BoundsInt(-1, -1, 0, 3, 3, 1);
        foreach (Vector3Int curPos in aroundLeft.allPositionsWithin)
        {
            // If its one of the directly adjacent tiles, or this tile
            if (curPos.x == 0 || curPos.y == 0)
            {
                // Test if the tile at the current position if a floor tile, if it isn't we can't spawn the decor
                // Get the sprite on the current tile
                Sprite curTileSpr = _tileMapRef.GetSprite(curPos + new Vector3Int(testLeftPos.x, testLeftPos.y, 0));
                // Assume it isn't a floor tile
                bool isFloorTile = false;
                // Iterate over the floor tiles
                for (int i = 0; i < _tileSet.SingleFloorTilesLength(); ++i)
                {
                    Tile curFloorTile = _tileSet.GetSingleFloorTile(i);
                    // If we find a floor tile with the same sprite, this is a floor tile
                    if (curFloorTile != null && curFloorTile.sprite == curTileSpr)
                    {
                        isFloorTile = true;
                        break;
                    }
                }
                // If this was not a floor tile, we can't spawn the decir
                if (!isFloorTile)
                    return false;
            }
        }

        // If we made it all the way here, we can spawn decor
        // Pick a random left tile for the decor (gives an even index, which is the left tile)
        int randomLeftIndex = Random.Range(0, _tileSet.TwoTiledDecorationTilesLength() / 2) * 2;
        // Add the left tile
        AddTile(testLeftPos, _tileSet.GetTwoTiledDecorationTile(randomLeftIndex));
        // Add the right tile
        AddTile(testLeftPos + new Vector2Int(1, 0), _tileSet.GetTwoTiledDecorationTile(randomLeftIndex + 1));

        return true;
    }
}
