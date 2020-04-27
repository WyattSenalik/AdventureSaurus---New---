using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class SaveTilemapController
{
    // The hashtable for converting from int to Tile
    private static Hashtable _tileHash;
    // The hastable for converting from Sprite to int (primary key to a Tile)
    private static Hashtable _keyHash;


    //// Getters
    /// <summary>
    /// Get a TileBase by its key
    /// </summary>
    /// <param name="key">int key for the TileBase</param>
    /// <returns>TileBase</returns>
    public static TileBase GetTile(int key) { return _tileHash[key] as TileBase; }
    /// <summary>
    /// Get an int (key) by the TileBase
    /// </summary>
    /// <param name="tileBase">TileBase with a given int (key)</param>
    /// <returns>int (key)</returns>
    public static int GetKey(TileBase tileBase) { return (int) _keyHash[tileBase]; }

    /// <summary>
    /// Creates the two hashtables to correspond to the given tile sets
    /// </summary>
    /// <param name="allTileSets">The TileSets to base the hashtables on</param>
    public static void BuildHashtables(TileSet[] allTileSets)
    {
        _tileHash = new Hashtable();
        _keyHash = new Hashtable();
        int currentKey = 0;
        foreach (TileSet singleSet in allTileSets)
        {
            // Single floor tiles
            for (int i = 0; i < singleSet.SingleFloorTilesLength(); ++i)
            {
                TileBase curTile = singleSet.GetSingleFloorTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile, currentKey);

                ++currentKey;
            }
            //// Walls
            // Close wall tiles
            for (int i = 0; i < singleSet.CloseWallTilesLength(); ++i)
            {
                TileBase curTile = singleSet.GetCloseWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile, currentKey);

                ++currentKey;
            }
            // Far wall tiles
            for (int i = 0; i < singleSet.FarWallTilesLength(); ++i)
            {
                TileBase curTile = singleSet.GetFarWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile, currentKey);

                ++currentKey;
            }
            // Left wall tiles
            for (int i = 0; i < singleSet.LeftWallTilesLength(); ++i)
            {
                TileBase curTile = singleSet.GetLeftWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile, currentKey);

                ++currentKey;
            }
            // Right wall tiles
            for (int i = 0; i < singleSet.RightWallTilesLength(); ++i)
            {
                TileBase curTile = singleSet.GetRightWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile, currentKey);

                ++currentKey;
            }
            //// Corner tiles for walls
            /// Outer Corners
            // Close Left Outer tiles
            for (int i = 0; i < singleSet.CloseLeftOuterCornerWallTilesLength(); ++i)
            {
                TileBase curTile = singleSet.GetCloseLeftOuterCornerWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile, currentKey);

                ++currentKey;
            }
            // Close Right Outer tiles
            for (int i = 0; i < singleSet.CloseRightOuterCornerWallTilesLength(); ++i)
            {
                TileBase curTile = singleSet.GetCloseRightOuterCornerWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile, currentKey);

                ++currentKey;
            }
            // Far Left Outer tiles
            for (int i = 0; i < singleSet.FarLeftOuterCornerWallTilesLength(); ++i)
            {
                TileBase curTile = singleSet.GetFarLeftOuterCornerWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile, currentKey);

                ++currentKey;
            }
            // Far Right Outer tiles
            for (int i = 0; i < singleSet.FarRightOuterCornerWallTilesLength(); ++i)
            {
                TileBase curTile = singleSet.GetFarRightOuterCornerWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile, currentKey);

                ++currentKey;
            }
            /// Inner Corners
            // Close Left Inner tiles
            for (int i = 0; i < singleSet.CloseLeftInnerCornerWallTilesLength(); ++i)
            {
                TileBase curTile = singleSet.GetCloseLeftInnerCornerWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile, currentKey);

                ++currentKey;
            }
            // Close Right Inner tiles
            for (int i = 0; i < singleSet.CloseRightInnerCornerWallTilesLength(); ++i)
            {
                TileBase curTile = singleSet.GetCloseRightInnerCornerWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile, currentKey);

                ++currentKey;
            }
            // Far Left Inner tiles
            for (int i = 0; i < singleSet.FarLeftInnerCornerWallTilesLength(); ++i)
            {
                TileBase curTile = singleSet.GetFarLeftInnerCornerWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile, currentKey);

                ++currentKey;
            }
            // Far Right Inner tiles
            for (int i = 0; i < singleSet.FarRightInnerCornerWallTilesLength(); ++i)
            {
                TileBase curTile = singleSet.GetFarRightInnerCornerWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile, currentKey);

                ++currentKey;
            }
            //// Stair tiles
            for (int i = 0; i < singleSet.StairTilesLength(); ++i)
            {
                TileBase curTile = singleSet.GetStairTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile, currentKey);

                ++currentKey;
            }
            //// Campfire
            // Unlit
            TileBase unlitTile = singleSet.GetUnlitCampfire();

            _tileHash.Add(currentKey, unlitTile);
            _keyHash.Add(unlitTile, currentKey);

            ++currentKey;
            // Lit
            TileBase litTile = singleSet.GetLitCampfire();

            _tileHash.Add(currentKey, litTile);
            _keyHash.Add(litTile, currentKey);

            ++currentKey;
            //// Decorations
            // Single tiled decorations
            for (int i = 0; i < singleSet.SingleTiledDecorationTilesLength(); ++i)
            {
                TileBase curTile = singleSet.GetSingleTiledDecorationTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile, currentKey);

                ++currentKey;
            }
            // Two tiled decorations
            for (int i = 0; i < singleSet.TwoTiledDecorationTilesLength(); ++i)
            {
                TileBase curTile = singleSet.GetTwoTiledDecorationTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile, currentKey);

                ++currentKey;
            }
        }
    }
}
