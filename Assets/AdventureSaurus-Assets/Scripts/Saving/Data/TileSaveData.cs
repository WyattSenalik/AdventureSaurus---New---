using UnityEngine;

[System.Serializable]
public class TileSaveData
{
    /// Tile's position of the tilemap
    private int[] position;
    /// Reference to the actual tile at that location
    private int key;

    /// Getters
    public Vector3Int GetPosition() { return new Vector3Int(position[0], position[1], position[2]); }
    public int GetKey() { return key; }

    public TileSaveData(Vector3Int tilePos, int tileKey)
    {
        position = new int[3];
        position[0] = tilePos.x;
        position[1] = tilePos.y;
        position[2] = tilePos.z;

        key = tileKey;
    }
}
