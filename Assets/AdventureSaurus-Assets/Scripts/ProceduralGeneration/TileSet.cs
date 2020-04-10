using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New TileSet", menuName = "TileSet")] 
public class TileSet : ScriptableObject
{
    // Floor tiles that can be used
    [SerializeField] private Tile[] _singleFloorTiles = null;
    public Tile GetSingleFloorTile(int index)
    {
        if (index < _singleFloorTiles.Length && index >= 0)
            return _singleFloorTiles[index];
        else
            return null;
    }
    public int SingleFloorTilesLength()
    {
        return _singleFloorTiles.Length;
    }
    //// Wall tiles
    // Close wall
    [SerializeField] private Tile[] _closeWallTiles = null;
    public Tile GetCloseWallTile(int index)
    {
        if (index < _closeWallTiles.Length && index >= 0)
            return _closeWallTiles[index];
        else
            return null;
    }
    public int CloseWallTilesLength()
    {
        return _closeWallTiles.Length;
    }
    // Far Wall
    [SerializeField] private Tile[] _farWallTiles = null;
    public Tile GetFarWallTile(int index)
    {
        if (index < _farWallTiles.Length && index >= 0)
            return _farWallTiles[index];
        else
            return null;
    }
    public int FarWallTilesLength()
    {
        return _farWallTiles.Length;
    }
    // Left Wall
    [SerializeField] private Tile[] _leftWallTiles = null;
    public Tile GetLeftWallTile(int index)
    {
        if (index < _leftWallTiles.Length && index >= 0)
            return _leftWallTiles[index];
        else
            return null;
    }
    public int LeftWallTilesLength()
    {
        return _leftWallTiles.Length;
    }
    // Right Wall
    [SerializeField] private Tile[] _rightWallTiles = null;
    public Tile GetRightWallTile(int index)
    {
        if (index < _rightWallTiles.Length && index >= 0)
            return _rightWallTiles[index];
        else
            return null;
    }
    public int RightWallTilesLength()
    {
        return _rightWallTiles.Length;
    }
    //// Corner tiles for walls
    /// Outer Corners
    // Close Left Outer
    [SerializeField] private Tile[] _closeLeftOuterCornerWallTiles = null;
    public Tile GetCloseLeftOuterCornerWallTile(int index)
    {
        if (index < _closeLeftOuterCornerWallTiles.Length && index >= 0)
            return _closeLeftOuterCornerWallTiles[index];
        else
            return null;
    }
    public int CloseLeftOuterCornerWallTilesLength()
    {
        return _closeLeftOuterCornerWallTiles.Length;
    }
    // Close Right Outer
    [SerializeField] private Tile[] _closeRightOuterCornerWallTiles = null;
    public Tile GetCloseRightOuterCornerWallTile(int index)
    {
        if (index < _closeRightOuterCornerWallTiles.Length && index >= 0)
            return _closeRightOuterCornerWallTiles[index];
        else
            return null;
    }
    public int CloseRightOuterCornerWallTilesLength()
    {
        return _closeRightOuterCornerWallTiles.Length;
    }
    // Far Left Outer
    [SerializeField] private Tile[] _farLeftOuterCornerWallTiles = null;
    public Tile GetFarLeftOuterCornerWallTile(int index)
    {
        if (index < _farLeftOuterCornerWallTiles.Length && index >= 0)
            return _farLeftOuterCornerWallTiles[index];
        else
            return null;
    }
    public int FarLeftOuterCornerWallTilesLength()
    {
        return _farLeftOuterCornerWallTiles.Length;
    }
    // Far Right Outer
    [SerializeField] private Tile[] _farRightOuterCornerWallTiles = null;
    public Tile GetFarRightOuterCornerWallTile(int index)
    {
        if (index < _farRightOuterCornerWallTiles.Length && index >= 0)
            return _farRightOuterCornerWallTiles[index];
        else
            return null;
    }
    public int FarRightOuterCornerWallTilesLength()
    {
        return _farRightOuterCornerWallTiles.Length;
    }
    /// Inner Corners
    // Close Left Inner
    [SerializeField] private Tile[] _closeLeftInnerCornerWallTiles = null;
    public Tile GetCloseLeftInnerCornerWallTile(int index)
    {
        if (index < _closeLeftInnerCornerWallTiles.Length && index >= 0)
            return _closeLeftInnerCornerWallTiles[index];
        else
            return null;
    }
    public int CloseLeftInnerCornerWallTilesLength()
    {
        return _closeLeftInnerCornerWallTiles.Length;
    }
    // Close Right Inner
    [SerializeField] private Tile[] _closeRightInnerCornerWallTiles = null;
    public Tile GetCloseRightInnerCornerWallTile(int index)
    {
        if (index < _closeRightInnerCornerWallTiles.Length && index >= 0)
            return _closeRightInnerCornerWallTiles[index];
        else
            return null;
    }
    public int CloseRightInnerCornerWallTilesLength()
    {
        return _closeRightInnerCornerWallTiles.Length;
    }
    // Far Left Inner
    [SerializeField] private Tile[] _farLeftInnerCornerWallTiles = null;
    public Tile GetFarLeftInnerCornerWallTile(int index)
    {
        if (index < _farLeftInnerCornerWallTiles.Length && index >= 0)
            return _farLeftInnerCornerWallTiles[index];
        else
            return null;
    }
    public int FarLeftInnerCornerWallTilesLength()
    {
        return _farLeftInnerCornerWallTiles.Length;
    }
    // Far Right Inner
    [SerializeField] private Tile[] _farRightInnerCornerWallTiles = null;
    public Tile GetFarRightInnerCornerWallTile(int index)
    {
        if (index < _farRightInnerCornerWallTiles.Length && index >= 0)
            return _farRightInnerCornerWallTiles[index];
        else
            return null;
    }
    public int FarRightInnerCornerWallTilesLength()
    {
        return _farRightInnerCornerWallTiles.Length;
    }
    // Stair tiles
    [SerializeField] private Tile[] _stairTiles = null;
    public Tile GetStairTile(int index)
    {
        if (index < _stairTiles.Length && index >= 0)
            return _stairTiles[index];
        else
            return null;
    }
    public int StairTilesLength()
    {
        return _stairTiles.Length;
    }

    //// Campfire
    // Unlit
    [SerializeField] private Tile _unlitCampfire = null;
    public Tile GetUnlitCampfire()
    {
        return _unlitCampfire;
    }
    // Lit
    [SerializeField] private AnimatedTile _litCampfire = null;
    public AnimatedTile GetLitCampfire()
    {
        return _litCampfire;
    }

    //// Decorations
    // Single tiled decorations
    [SerializeField] private Tile[] _singleTiledDecorationTiles = null;
    public Tile GetSingleTiledDecorationTile(int index)
    {
        if (index < _singleTiledDecorationTiles.Length && index >= 0)
            return _singleTiledDecorationTiles[index];
        else
            return null;
    }
    public int SingleTiledDecorationTilesLength()
    {
        return _singleTiledDecorationTiles.Length;
    }
    // Floor tiles that take up multiple tiles that can be used
    // For in a line of length 2 from left to right
    [SerializeField] private Tile[] _twoTiledDecorationTiles = null;
    public Tile GetTwoTiledDecorationTile(int index)
    {
        if (index < _twoTiledDecorationTiles.Length && index >= 0)
            return _twoTiledDecorationTiles[index];
        else
            return null;
    }
    public int TwoTiledDecorationTilesLength()
    {
        return _twoTiledDecorationTiles.Length;
    }
}
