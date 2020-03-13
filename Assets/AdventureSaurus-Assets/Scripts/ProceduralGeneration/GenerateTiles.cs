using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GenerateTiles : MonoBehaviour
{
    // The object that will be spawned that contains an empty tile map
    [SerializeField] private GameObject tileMapPrefab = null;
    // Chance to spawn dinosaur bones on a tile
    [SerializeField] private int dinoBonesChance = 12;
    // Floor tiles that can be used
    [SerializeField] private List<Tile> singleFloorTiles = null;
    // Floor tiles that take up multiple tiles that can be used
    [SerializeField] private List<Tile> twoTileDinoBones = null;
    // Wall tiles
    [SerializeField] private List<Tile> closeWallTiles = null;
    [SerializeField] private List<Tile> farWallTiles = null;
    [SerializeField] private List<Tile> leftWallTiles = null;
    [SerializeField] private List<Tile> rightWallTiles = null;
    // Corner tiles for walls
    // Outer Corners
    [SerializeField] private Tile closeLeftOuterCornerWallTile = null;
    [SerializeField] private Tile closeRightOuterCornerWallTile = null;
    [SerializeField] private Tile farLeftOuterCornerWallTile = null;
    [SerializeField] private Tile farRightOuterCornerWallTile = null;
    // Inner Corners
    [SerializeField] private Tile closeLeftInnerCornerWallTile = null;
    [SerializeField] private Tile closeRightInnerCornerWallTile = null;
    [SerializeField] private Tile farLeftInnerCornerWallTile = null;
    [SerializeField] private Tile farRightInnerCornerWallTile = null;
    // Stair tiles
    [SerializeField] private Tile stairTile = null;

    // Reference to the tile map
    private Tilemap tileMapRef;

    // Called before start
    private void Awake()
    {
        // Validation
        if (tileMapPrefab == null)
            Debug.Log("tileMapPrefab was not set correctly in GenerateStairs attached to " + this.name);
        if (singleFloorTiles == null)
            Debug.Log("singleFloorTiles was not set correctly in GenerateStairs attached to " + this.name);
        if (twoTileDinoBones == null)
            Debug.Log("twoTileDinoBones was not set correctly in GenerateStairs attached to " + this.name);
        if (closeWallTiles == null)
            Debug.Log("closeWallTiles was not set correctly in GenerateStairs attached to " + this.name);
        if (farWallTiles == null)
            Debug.Log("farWallTiles was not set correctly in GenerateStairs attached to " + this.name);
        if (leftWallTiles == null)
            Debug.Log("leftWallTiles was not set correctly in GenerateStairs attached to " + this.name);
        if (rightWallTiles == null)
            Debug.Log("rightWallTiles was not set correctly in GenerateStairs attached to " + this.name);
        if (closeLeftOuterCornerWallTile == null)
            Debug.Log("closeLeftOuterCornerWallTile was not set correctly in GenerateStairs attached to " + this.name);
        if (closeRightOuterCornerWallTile == null)
            Debug.Log("closeRightOuterCornerWallTile was not set correctly in GenerateStairs attached to " + this.name);
        if (farLeftOuterCornerWallTile == null)
            Debug.Log("farLeftOuterCornerWallTile was not set correctly in GenerateStairs attached to " + this.name);
        if (farRightOuterCornerWallTile == null)
            Debug.Log("farRightOuterCornerWallTile was not set correctly in GenerateStairs attached to " + this.name);
        if (closeLeftInnerCornerWallTile == null)
            Debug.Log("closeLeftInnerCornerWallTile was not set correctly in GenerateStairs attached to " + this.name);
        if (closeRightInnerCornerWallTile == null)
            Debug.Log("closeRightInnerCornerWallTile was not set correctly in GenerateStairs attached to " + this.name);
        if (farLeftInnerCornerWallTile == null)
            Debug.Log("farLeftInnerCornerWallTile was not set correctly in GenerateStairs attached to " + this.name);
        if (farRightInnerCornerWallTile == null)
            Debug.Log("farRightInnerCornerWallTile was not set correctly in GenerateStairs attached to " + this.name);
        if (stairTile == null)
            Debug.Log("stairTile was not set correctly in GenerateStairs attached to " + this.name);
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
        // Initilize the list of potential position to spawn the dinosaur bones
        List<Vector2Int> dinoBonePos = new List<Vector2Int>();

        // Create the tilemap
        GameObject tileMapObj = Instantiate(tileMapPrefab, new Vector3(-0.5f, -0.5f, 0), Quaternion.identity);
        tileMapObj.name = "Grid";
        tileMapRef = tileMapObj.transform.GetChild(0).GetComponent<Tilemap>();
        if (tileMapRef == null)
            Debug.Log("There is no Tilemap attached to the child of " + tileMapObj.name);

        // Clear any tiles on the map
        tileMapRef.ClearAllTiles();

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
                    int randomIndex = Random.Range(0, singleFloorTiles.Count);
                    // Calculate the current location
                    Vector2Int curLocation = topLeft + new Vector2Int(curCol, -curRow);

                    // Give a chance for this tile to be covered in dino bones
                    if (Random.Range(0, dinoBonesChance) == 0)
                        dinoBonePos.Add(curLocation);

                    // Add a single room tile at the current location
                    AddTile(curLocation, singleFloorTiles[randomIndex]);
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
                Sprite curTileSpr = tileMapRef.GetSprite(curPos + curWallPos);
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
                AddRandomTile(curWall.position, leftWallTiles);

            // If the wall is a right wall
            else if (curWallTypeVect.x == 1 && curWallTypeVect.y == 0)
                AddRandomTile(curWall.position, rightWallTiles);

            // If the wall is a close wall
            else if (curWallTypeVect.x == 0 && curWallTypeVect.y == -1)
                AddRandomTile(curWall.position, closeWallTiles);

            // If the wall is a far wall
            else if (curWallTypeVect.x == 0 && curWallTypeVect.y == 1)
                AddRandomTile(curWall.position, farWallTiles);

            // Outer corners
            // If the wall is a close left outer corner
            else if (curWallTypeVect.x == -1 && curWallTypeVect.y == -1)
                AddTile(curWall.position, closeLeftOuterCornerWallTile);

            // If the wall is a close right outer corner
            else if (curWallTypeVect.x == 1 && curWallTypeVect.y == -1)
                AddTile(curWall.position, closeRightOuterCornerWallTile);

            // If the wall is a far left outer corner
            else if (curWallTypeVect.x == -1 && curWallTypeVect.y == 1)
                AddTile(curWall.position, farLeftOuterCornerWallTile);

            // If the wall is a far right outer corner
            else if (curWallTypeVect.x == 1 && curWallTypeVect.y == 1)
                AddTile(curWall.position, farRightOuterCornerWallTile);

            // Inner corners
            // If the wall is a close left outer corner
            else if (innerWallTypeVect.x == -1 && innerWallTypeVect.y == -1)
                AddTile(curWall.position, closeLeftInnerCornerWallTile);

            // If the wall is a close right outer corner
            else if (innerWallTypeVect.x == 1 && innerWallTypeVect.y == -1)
                AddTile(curWall.position, closeRightInnerCornerWallTile);

            // If the wall is a far left outer corner
            else if (innerWallTypeVect.x == -1 && innerWallTypeVect.y == 1)
                AddTile(curWall.position, farLeftInnerCornerWallTile);

            // If the wall is a far right outer corner
            else if (innerWallTypeVect.x == 1 && innerWallTypeVect.y == 1)
                AddTile(curWall.position, farRightInnerCornerWallTile);

            // If the wall is a completely different kind of tile
            else
            {
                Debug.Log("Strange wall encountered " + curWallTypeVect);
                AddTile(curWall.position, singleFloorTiles[0]);
            }
        }

        // Add the stairs to the tilemap
        AddTile(stairsTrans.position, stairTile);

        // Add the fireplace


        // Iterate over the tiles we said we would spawn dino bones at and try to spawn dino bones
        foreach (Vector2Int tilePos in dinoBonePos)
        {
            AttemptPlaceDinoBones(tilePos);
        }

        // Give the tilemap
        return tileMapRef;
    }

    /// <summary>
    /// Adds a tile at the given world position with the specified tile
    /// </summary>
    /// <param name="worldPos">The position to spawn the tile at</param>
    /// <param name="tileToAdd">The tile to spawn</param>
    private void AddTile(Vector3 worldPos, Tile tileToAdd)
    {
        Vector3Int spawnPos = new Vector3Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y), 0);
        tileMapRef.SetTile(spawnPos, tileToAdd);
    }

    /// <summary>
    /// Adds a tile at the given grid position with the specified tile
    /// </summary>
    /// <param name="gridPos">The position to spawn the tile at</param>
    /// <param name="tileToAdd">The tile to spawn</param>
    private void AddTile(Vector2Int gridPos, Tile tileToAdd)
    {
        Vector3Int spawnPos = new Vector3Int(gridPos.x, gridPos.y, 0);
        tileMapRef.SetTile(spawnPos, tileToAdd);
    }

    /// <summary>
    /// Spawns a random Tile in the given list at the given world position
    /// </summary>
    /// <param name="worldPos">The position to spawn the tile at</param>
    /// <param name="tileChoices">The list of potential tiles to spawn</param>
    private void AddRandomTile(Vector3 worldPos, List<Tile> tileChoices)
    {
        // Choose a random tile
        int randomIndex = Random.Range(0, tileChoices.Count);
        AddTile(worldPos, tileChoices[randomIndex]);
    }

    /// <summary>
    /// Spawns a random Tile in the given list at the given grid position
    /// </summary>
    /// <param name="gridPos">The position to spawn the tile at</param>
    /// <param name="tileChoices">The list of potential tiles to spawn</param>
    private void AddRandomTile(Vector2Int gridPos, List<Tile> tileChoices)
    {
        // Choose a random tile
        int randomIndex = Random.Range(0, tileChoices.Count);
        AddTile(gridPos, tileChoices[randomIndex]);
    }

    /// <summary>
    /// Trys to create dino bones at a position
    /// </summary>
    /// <param name="testBonePos">The position on the tilemap we are trying to place dino bones</param>
    /// <returns>Returns true if successful</returns>
    private bool AttemptPlaceDinoBones(Vector2Int testBonePos)
    {
        //Debug.Log("Trying to spawn dino at " + testBonePos);
        // Iterate over this tile and its neighbors to find out if any of the adjacent tiles make it so we can't spawn dino bones
        BoundsInt aroundBounds = new BoundsInt(-1, -1, 0, 3, 3, 1);
        foreach (Vector3Int curPos in aroundBounds.allPositionsWithin)
        {
            // If its one of the directly adjacent tiles, or this tile
            if (curPos.x == 0 || curPos.y == 0)
            {
                // Test if the tile at the current position if a floor tile, if it isn't we can't spawn the bones
                // Get the sprite on the current tile
                Sprite curTileSpr = tileMapRef.GetSprite(curPos + new Vector3Int(testBonePos.x, testBonePos.y, 0));
                // Assume it isn't a floor tile
                bool isFloorTile = false;
                // Iterate over the floor tiles
                foreach (Tile curFloorTile in singleFloorTiles)
                {
                    // If we find a floor tile with the same sprite, this is a floor tile
                    if (curFloorTile.sprite == curTileSpr)
                    {
                        isFloorTile = true;
                        break;
                    }
                }
                // If this was not a floor tile, we can't spawn the bones
                if (!isFloorTile)
                    return false;
            }
        }

        // If we made it all the way here, we can spawn dino bones
        // Pick a random left tile for the dino bones (gives an even index, which is the left tile)
        int randomLeftIndex = Random.Range(0, twoTileDinoBones.Count / 2) * 2;
        // Add the left tile
        AddTile(testBonePos, twoTileDinoBones[randomLeftIndex]);
        // Add the right tile
        AddTile(testBonePos + new Vector2Int(1, 0), twoTileDinoBones[randomLeftIndex + 1]);

        return true;
    }
}
