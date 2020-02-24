using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeSixtySwing : Skill
{
    private void Awake()
    {
        skillNum = 360;
        rangeTiles = new List<Vector2Int>();
        rangeTiles.Add(Vector2Int.down);
        rangeTiles.Add(Vector2Int.up);
        rangeTiles.Add(Vector2Int.right);
        rangeTiles.Add(Vector2Int.left);
        rangeTiles.Add(new Vector2Int(1, 1));
        rangeTiles.Add(new Vector2Int(-1, 1));
        rangeTiles.Add(new Vector2Int(1, -1));
        rangeTiles.Add(new Vector2Int(-1, -1));

    }

    
}
