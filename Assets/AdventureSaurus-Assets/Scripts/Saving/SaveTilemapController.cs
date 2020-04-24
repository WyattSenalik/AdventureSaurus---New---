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
    /// Get a Tile by its key
    /// </summary>
    /// <param name="key">int key for the Tile</param>
    /// <returns>Tile</returns>
    public static Tile GetTile(int key) { return _tileHash[key] as Tile; }
    /// <summary>
    /// Get an int (key) by the Tile's Sprite
    /// </summary>
    /// <param name="spr">Sprite of the Tile with a given int (key)</param>
    /// <returns>int (key)</returns>
    public static int GetKey(Sprite spr) { return (int) _keyHash[spr]; }

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
                Tile curTile = singleSet.GetSingleFloorTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile.sprite, currentKey);

                ++currentKey;
            }
            //// Walls
            // Close wall tiles
            for (int i = 0; i < singleSet.CloseWallTilesLength(); ++i)
            {
                Tile curTile = singleSet.GetCloseWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile.sprite, currentKey);

                ++currentKey;
            }
            // Far wall tiles
            for (int i = 0; i < singleSet.FarWallTilesLength(); ++i)
            {
                Tile curTile = singleSet.GetFarWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile.sprite, currentKey);

                ++currentKey;
            }
            // Left wall tiles
            for (int i = 0; i < singleSet.LeftWallTilesLength(); ++i)
            {
                Tile curTile = singleSet.GetLeftWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile.sprite, currentKey);

                ++currentKey;
            }
            // Right wall tiles
            for (int i = 0; i < singleSet.RightWallTilesLength(); ++i)
            {
                Tile curTile = singleSet.GetRightWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile.sprite, currentKey);

                ++currentKey;
            }
            //// Corner tiles for walls
            /// Outer Corners
            // Close Left Outer tiles
            for (int i = 0; i < singleSet.CloseLeftOuterCornerWallTilesLength(); ++i)
            {
                Tile curTile = singleSet.GetCloseLeftOuterCornerWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile.sprite, currentKey);

                ++currentKey;
            }
            // Close Right Outer tiles
            for (int i = 0; i < singleSet.CloseRightOuterCornerWallTilesLength(); ++i)
            {
                Tile curTile = singleSet.GetCloseRightOuterCornerWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile.sprite, currentKey);

                ++currentKey;
            }
            // Far Left Outer tiles
            for (int i = 0; i < singleSet.FarLeftOuterCornerWallTilesLength(); ++i)
            {
                Tile curTile = singleSet.GetFarLeftOuterCornerWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile.sprite, currentKey);

                ++currentKey;
            }
            // Far Right Outer tiles
            for (int i = 0; i < singleSet.FarRightOuterCornerWallTilesLength(); ++i)
            {
                Tile curTile = singleSet.GetFarRightOuterCornerWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile.sprite, currentKey);

                ++currentKey;
            }
            /// Inner Corners
            // Close Left Inner tiles
            for (int i = 0; i < singleSet.CloseLeftInnerCornerWallTilesLength(); ++i)
            {
                Tile curTile = singleSet.GetCloseLeftInnerCornerWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile.sprite, currentKey);

                ++currentKey;
            }
            // Close Right Inner tiles
            for (int i = 0; i < singleSet.CloseRightInnerCornerWallTilesLength(); ++i)
            {
                Tile curTile = singleSet.GetCloseRightInnerCornerWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile.sprite, currentKey);

                ++currentKey;
            }
            // Far Left Inner tiles
            for (int i = 0; i < singleSet.FarLeftInnerCornerWallTilesLength(); ++i)
            {
                Tile curTile = singleSet.GetFarLeftInnerCornerWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile.sprite, currentKey);

                ++currentKey;
            }
            // Far Right Inner tiles
            for (int i = 0; i < singleSet.FarRightInnerCornerWallTilesLength(); ++i)
            {
                Tile curTile = singleSet.GetFarRightInnerCornerWallTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile.sprite, currentKey);

                ++currentKey;
            }
            //// Stair tiles
            for (int i = 0; i < singleSet.StairTilesLength(); ++i)
            {
                Tile curTile = singleSet.GetStairTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile.sprite, currentKey);

                ++currentKey;
            }
            //// Campfire
            // Unlit
            Tile unlitTile = singleSet.GetUnlitCampfire();

            _tileHash.Add(currentKey, unlitTile);
            _keyHash.Add(unlitTile.sprite, currentKey);

            ++currentKey;
            // Lit
            AnimatedTile litTile = singleSet.GetLitCampfire();

            _tileHash.Add(currentKey, litTile);
            foreach (Sprite singleFrame in litTile.m_AnimatedSprites)
                _keyHash.Add(singleFrame, currentKey);

            ++currentKey;
            //// Decorations
            // Single tiled decorations
            for (int i = 0; i < singleSet.SingleTiledDecorationTilesLength(); ++i)
            {
                Tile curTile = singleSet.GetSingleTiledDecorationTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile.sprite, currentKey);

                ++currentKey;
            }
            // Two tiled decorations
            for (int i = 0; i < singleSet.TwoTiledDecorationTilesLength(); ++i)
            {
                Tile curTile = singleSet.GetTwoTiledDecorationTile(i);

                _tileHash.Add(currentKey, curTile);
                _keyHash.Add(curTile.sprite, currentKey);

                ++currentKey;
            }
        }
    }
}
