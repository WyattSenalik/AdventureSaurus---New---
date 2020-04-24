using UnityEngine;

[System.Serializable]
public class TilemapBoundsData
{
    /// Positions of the corners of the tilemap
    int[] botleftCorner;
    int[] toprightCorner;

    /// Getters
    public Vector2Int GetBotLeftCorner() { return new Vector2Int(botleftCorner[0], botleftCorner[1]); }
    public Vector2Int GetTopRightCorner() { return new Vector2Int(toprightCorner[0], toprightCorner[1]); }

    public TilemapBoundsData(Vector2Int botLeft, Vector2Int topRight)
    {
        botleftCorner = new int[2];
        botleftCorner[0] = Mathf.RoundToInt(botLeft.x);
        botleftCorner[1] = Mathf.RoundToInt(botLeft.y);

        toprightCorner = new int[2];
        toprightCorner[0] = Mathf.RoundToInt(topRight.x);
        toprightCorner[1] = Mathf.RoundToInt(topRight.y);
    }
}
