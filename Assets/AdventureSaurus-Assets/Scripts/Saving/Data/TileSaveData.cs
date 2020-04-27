using UnityEngine;

[System.Serializable]
public class TileSaveData
{
    /// Tile's position of the tilemap
    private int[] _position;
    /// Reference to the actual tile at that location
    private int _key;

    /// Getters
    public Vector3Int GetPosition() { return new Vector3Int(_position[0], _position[1], _position[2]); }
    public int GetKey() { return _key; }

    public TileSaveData(Vector3Int tilePos, int tileKey)
    {
        _position = new int[3];
        _position[0] = tilePos.x;
        _position[1] = tilePos.y;
        _position[2] = tilePos.z;

        _key = tileKey;
    }
}