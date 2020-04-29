using UnityEngine;

public class PersistantInfinite : PersistantController
{
    /// <summary>
    /// Checks if we should swap the tileset to a different tileset.
    /// Pick a random tile set
    /// </summary>
    protected override void CheckSwapTileset()
    {
        int randIndex = Random.Range(0, _tileSets.Length);
        _activeTileSet = _tileSets[randIndex];
    }
}
